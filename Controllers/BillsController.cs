using Microsoft.AspNetCore.Mvc;
using Microsoft.Data.Sqlite;
using MiniBillingSystem.Models;

namespace MiniBillingSystem.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class BillsController : ControllerBase
    {
        private readonly string _connStr;

        public BillsController(IConfiguration configuration)
        {
            _connStr = configuration.GetConnectionString("DefaultConnection") 
                ?? "Data Source=billing.db;Cache=Shared";
        }

        [HttpGet]
        public IActionResult GetBills([FromQuery] int page = 1, [FromQuery] int pageSize = 50, [FromQuery] string sortBy = "recent_created")
        {
            if (page < 1) page = 1;
            if (pageSize < 1 || pageSize > 100) pageSize = 50;
            int offset = (page - 1) * pageSize;

            var bills = new List<BillDto>();
            int totalCount = 0;

            string countSql = "SELECT COUNT(*) FROM Bills";

            string orderByClause = sortBy?.ToLower() switch
            {
                "recent_activity" => "ORDER BY COALESCE((SELECT MAX(p.PaymentDate) FROM Payments p WHERE p.BillID = b.BillID), b.DueDate) DESC, b.BillID DESC",
                "due_date" => "ORDER BY b.DueDate ASC, b.BillID DESC",
                _ => "ORDER BY b.BillID DESC"
            };

            string sql = $@"
                SELECT b.BillID, b.CustomerID, c.FullName, b.Amount, b.AmountDue, b.DueDate, b.IsPaid, b.Status 
                FROM Bills b 
                JOIN Customers c ON b.CustomerID = c.CustomerID
                {orderByClause} LIMIT @PageSize OFFSET @Offset";

            try
            {
                using var conn = new SqliteConnection(_connStr);
                conn.Open();

                using (var countCmd = new SqliteCommand(countSql, conn))
                {
                    totalCount = Convert.ToInt32(countCmd.ExecuteScalar());
                }

                using var cmd = new SqliteCommand(sql, conn);
                cmd.Parameters.AddWithValue("@Offset", offset);
                cmd.Parameters.AddWithValue("@PageSize", pageSize);

                using var reader = cmd.ExecuteReader();
                while (reader.Read())
                {
                    bills.Add(new BillDto
                    {
                        BillID = Convert.ToInt32(reader["BillID"]),
                        CustomerID = Convert.ToInt32(reader["CustomerID"]),
                        FullName = reader["FullName"].ToString() ?? "",
                        AmountDue = Convert.ToDecimal(reader["AmountDue"]),
                        DueDate = reader["DueDate"].ToString() ?? "",
                        IsPaid = Convert.ToInt32(reader["IsPaid"]) == 1
                    });
                }

                return Ok(new { data = bills, page, pageSize, totalCount, sortBy });
            }
            catch
            {
                return StatusCode(500, new { error = "Oops! We couldn't load the bills right now." });
            }
        }

        [HttpPost]
        public IActionResult GenerateBill([FromBody] CreateBillRequest request)
        {
            if (request == null)
            {
                return BadRequest(new { error = "Please enter valid bill details." });
            }

            if (request.CustomerID <= 0)
            {
                return BadRequest(new { error = "Please enter a valid positive Customer ID." });
            }

            if (request.AmountDue < 0)
            {
                return BadRequest(new { error = "Please enter a valid positive number (e.g., 1500 or 1500.50). Letters and symbols are not allowed." });
            }

            if (!DateTime.TryParse(request.DueDate, out DateTime validDueDate))
            {
                return BadRequest(new { error = "That date format wasn't recognized. Please use YYYY-MM-DD (e.g., 2026-08-01)." });
            }

            string formattedDueDate = validDueDate.ToString("yyyy-MM-dd");
            decimal billAmount = request.AmountDue; // Amount parameter represents original total bill

            using var conn = new SqliteConnection(_connStr);
            conn.Open();
            using SqliteTransaction transaction = conn.BeginTransaction();

            try
            {
                SqliteCommand checkCreditCmd = new SqliteCommand(
                    "SELECT AccountBalance FROM Customers WHERE CustomerID = @CustID", 
                    conn, 
                    transaction);
                checkCreditCmd.Parameters.AddWithValue("@CustID", request.CustomerID);

                object? result = checkCreditCmd.ExecuteScalar();
                if (result == null || result == DBNull.Value)
                {
                    transaction.Rollback();
                    return NotFound(new { error = $"We couldn't find a customer with ID '{request.CustomerID}'. Please double-check the ID and try again." });
                }

                decimal accountBalance = Convert.ToDecimal(result);
                decimal finalAmountDue = billAmount;
                int isPaid = 0;
                string status = "Unpaid";
                string outcomeMsg = "";
                string outcomeType = "";

                if (accountBalance >= billAmount)
                {
                    finalAmountDue = 0;
                    isPaid = 1;
                    status = "Paid";

                    SqliteCommand deductCmd = new SqliteCommand(
                        "UPDATE Customers SET AccountBalance = AccountBalance - @BillAmount WHERE CustomerID = @CustID", 
                        conn, 
                        transaction);
                    deductCmd.Parameters.AddWithValue("@BillAmount", billAmount);
                    deductCmd.Parameters.AddWithValue("@CustID", request.CustomerID);
                    deductCmd.ExecuteNonQuery();

                    outcomeType = "FULLY_COVERED";
                    outcomeMsg = $"Great news! The customer had enough credit (₦{accountBalance:N2}) to cover this. The bill was generated and is already marked as paid.";
                }
                else if (accountBalance > 0)
                {
                    finalAmountDue = billAmount - accountBalance;
                    isPaid = 0;
                    status = "PartiallyPaid";

                    SqliteCommand drainCmd = new SqliteCommand(
                        "UPDATE Customers SET AccountBalance = 0 WHERE CustomerID = @CustID", 
                        conn, 
                        transaction);
                    drainCmd.Parameters.AddWithValue("@CustID", request.CustomerID);
                    drainCmd.ExecuteNonQuery();

                    outcomeType = "PARTIALLY_COVERED";
                    outcomeMsg = $"We applied the customer's available credit (₦{accountBalance:N2}). The new remaining bill amount is ₦{finalAmountDue:N2}.";
                }
                else
                {
                    finalAmountDue = billAmount;
                    isPaid = 0;
                    status = "Unpaid";
                    outcomeType = "NO_CREDIT_APPLIED";
                    outcomeMsg = "Bill successfully generated!";
                }

                SqliteCommand insertBillCmd = new SqliteCommand(
                    @"INSERT INTO Bills (CustomerID, Amount, AmountDue, DueDate, IsPaid, Status) 
                      VALUES (@CustID, @OriginalAmount, @AmountDue, @DueDate, @IsPaid, @Status)", 
                    conn, 
                    transaction);
                insertBillCmd.Parameters.AddWithValue("@CustID", request.CustomerID);
                insertBillCmd.Parameters.AddWithValue("@OriginalAmount", billAmount);
                insertBillCmd.Parameters.AddWithValue("@AmountDue", finalAmountDue);
                insertBillCmd.Parameters.AddWithValue("@DueDate", formattedDueDate);
                insertBillCmd.Parameters.AddWithValue("@IsPaid", isPaid);
                insertBillCmd.Parameters.AddWithValue("@Status", status);
                insertBillCmd.ExecuteNonQuery();

                transaction.Commit();

                return Ok(new BillCreationResultDto
                {
                    Message = outcomeMsg,
                    Outcome = outcomeType,
                    FinalAmountDue = finalAmountDue,
                    IsPaid = isPaid == 1
                });
            }
            catch
            {
                transaction.Rollback();
                return StatusCode(500, new { error = "Sorry, we ran into an issue generating that bill. Please try again." });
            }
        }
    }
}
