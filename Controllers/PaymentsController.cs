using Microsoft.AspNetCore.Mvc;
using Microsoft.Data.SqlClient;
using MiniBillingSystem.Models;

namespace MiniBillingSystem.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class PaymentsController : ControllerBase
    {
        private readonly string _connStr;

        public PaymentsController(IConfiguration configuration)
        {
            _connStr = configuration.GetConnectionString("DefaultConnection") 
                ?? "Server=.\\SQLEXPRESS;Database=InternBillingDB;Integrated Security=True;TrustServerCertificate=True;";
        }

        [HttpGet]
        public IActionResult GetPayments([FromQuery] int page = 1, [FromQuery] int pageSize = 50)
        {
            if (page < 1) page = 1;
            if (pageSize < 1 || pageSize > 100) pageSize = 50;
            int offset = (page - 1) * pageSize;

            var payments = new List<PaymentDto>();
            int totalCount = 0;

            string countSql = "SELECT COUNT(*) FROM Payments";
            string sql = @"
                SELECT p.PaymentID, c.FullName, b.BillID, b.CustomerID, p.AmountPaid, p.PaymentDate, p.PaymentMethod 
                FROM Payments p 
                JOIN Bills b ON p.BillID = b.BillID 
                JOIN Customers c ON b.CustomerID = c.CustomerID
                ORDER BY p.PaymentID DESC OFFSET @Offset ROWS FETCH NEXT @PageSize ROWS ONLY";

            try
            {
                using var conn = new SqlConnection(_connStr);
                conn.Open();

                using (var countCmd = new SqlCommand(countSql, conn))
                {
                    totalCount = Convert.ToInt32(countCmd.ExecuteScalar());
                }

                using var cmd = new SqlCommand(sql, conn);
                cmd.Parameters.AddWithValue("@Offset", offset);
                cmd.Parameters.AddWithValue("@PageSize", pageSize);

                using var reader = cmd.ExecuteReader();
                while (reader.Read())
                {
                    payments.Add(new PaymentDto
                    {
                        PaymentID = Convert.ToInt32(reader["PaymentID"]),
                        CustomerID = Convert.ToInt32(reader["CustomerID"]),
                        FullName = reader["FullName"].ToString() ?? "",
                        BillID = Convert.ToInt32(reader["BillID"]),
                        AmountPaid = Convert.ToDecimal(reader["AmountPaid"]),
                        PaymentMethod = reader["PaymentMethod"].ToString() ?? "",
                        PaymentDate = Convert.ToDateTime(reader["PaymentDate"]).ToString("yyyy-MM-dd HH:mm")
                    });
                }

                return Ok(new { data = payments, page, pageSize, totalCount });
            }
            catch
            {
                return StatusCode(500, new { error = "Oops! We couldn't load the payment history right now." });
            }
        }

        [HttpPost]
        public IActionResult RecordPayment([FromBody] RecordPaymentRequest request)
        {
            if (request == null)
            {
                return BadRequest(new { error = "Please enter valid payment details." });
            }

            if (request.BillID <= 0)
            {
                return BadRequest(new { error = "Please enter a valid positive Bill ID." });
            }

            if (request.AmountPaid < 0)
            {
                return BadRequest(new { error = "Please enter a valid positive number (e.g., 1500 or 1500.50). Letters and symbols are not allowed." });
            }

            string method = (request.PaymentMethod ?? "").Trim();
            if (string.IsNullOrEmpty(method))
            {
                return BadRequest(new { error = "Payment method cannot be left blank. Please select or enter a method." });
            }

            using var conn = new SqlConnection(_connStr);
            conn.Open();
            using SqlTransaction transaction = conn.BeginTransaction();

            try
            {
                decimal amountDue = 0;
                int customerId = 0;
                bool isPaid = false;

                SqlCommand checkCmd = new SqlCommand(
                    "SELECT AmountDue, CustomerID, IsPaid FROM Bills WITH (UPDLOCK) WHERE BillID = @BillID", 
                    conn, 
                    transaction);
                checkCmd.Parameters.AddWithValue("@BillID", request.BillID);

                using (SqlDataReader reader = checkCmd.ExecuteReader())
                {
                    if (reader.Read())
                    {
                        amountDue = Convert.ToDecimal(reader["AmountDue"]);
                        customerId = Convert.ToInt32(reader["CustomerID"]);
                        isPaid = Convert.ToBoolean(reader["IsPaid"]);
                    }
                    else
                    {
                        transaction.Rollback();
                        return NotFound(new { error = "We couldn't find that Bill ID. Please double-check it and try again." });
                    }
                }

                if (isPaid)
                {
                    transaction.Rollback();
                    return BadRequest(new { error = "Good news! This bill has already been fully paid." });
                }

                // Record initial payment entry
                SqlCommand insertCmd = new SqlCommand(
                    "INSERT INTO Payments (BillID, AmountPaid, PaymentMethod) VALUES (@BillID, @Amount, @Method)", 
                    conn, 
                    transaction);
                insertCmd.Parameters.AddWithValue("@BillID", request.BillID);
                insertCmd.Parameters.AddWithValue("@Amount", request.AmountPaid);
                insertCmd.Parameters.AddWithValue("@Method", method);
                insertCmd.ExecuteNonQuery();

                string outcomeMsg = "";
                string outcomeType = "";
                decimal remainingBalance = 0;

                if (request.AmountPaid >= amountDue)
                {
                    SqlCommand updateBillCmd = new SqlCommand(
                        "UPDATE Bills SET IsPaid = 1, AmountDue = 0, Status = 'Paid' WHERE BillID = @BillID", 
                        conn, 
                        transaction);
                    updateBillCmd.Parameters.AddWithValue("@BillID", request.BillID);
                    updateBillCmd.ExecuteNonQuery();

                    decimal overpayment = request.AmountPaid - amountDue;
                    if (overpayment > 0)
                    {
                        SqlCommand creditCmd = new SqlCommand(
                            "UPDATE Customers SET AccountBalance = AccountBalance + @Overpayment WHERE CustomerID = @CustID", 
                            conn, 
                            transaction);
                        creditCmd.Parameters.AddWithValue("@Overpayment", overpayment);
                        creditCmd.Parameters.AddWithValue("@CustID", customerId);
                        creditCmd.ExecuteNonQuery();

                        outcomeType = "OVERPAYMENT";
                        outcomeMsg = $"Payment successful! The target bill is fully paid. Overpayment of ₦{overpayment:N2} was processed.";
                    }
                    else
                    {
                        outcomeType = "FULLY_PAID";
                        outcomeMsg = "Payment successful! The bill is now fully paid.";
                    }
                    remainingBalance = 0;
                }
                else
                {
                    remainingBalance = amountDue - request.AmountPaid;
                    SqlCommand updatePartialCmd = new SqlCommand(
                        "UPDATE Bills SET AmountDue = @Remaining, IsPaid = 0, Status = 'PartiallyPaid' WHERE BillID = @BillID", 
                        conn, 
                        transaction);
                    updatePartialCmd.Parameters.AddWithValue("@Remaining", remainingBalance);
                    updatePartialCmd.Parameters.AddWithValue("@BillID", request.BillID);
                    updatePartialCmd.ExecuteNonQuery();

                    outcomeType = "PARTIAL_PAYMENT";
                    outcomeMsg = $"Partial payment accepted. The customer still owes ₦{remainingBalance:N2} on this bill.";
                }

                // Automatic Reconciliation: Apply any available customer credit against other unpaid bills
                int autoSettledCount = ReconcileCustomerCreditAndBills(customerId, conn, transaction);

                if (autoSettledCount > 0)
                {
                    outcomeMsg += $" Automatic reconciliation applied available credit to settle {autoSettledCount} other pending bill(s).";
                }

                transaction.Commit();

                return Ok(new PaymentResultDto
                {
                    Message = outcomeMsg,
                    Outcome = outcomeType,
                    RemainingAmountDue = remainingBalance
                });
            }
            catch (Exception ex)
            {
                transaction.Rollback();
                return StatusCode(500, new { error = "Sorry, the payment failed to process: " + ex.Message });
            }
        }

        /// <summary>
        /// Automatically reconciles available customer account credit against any outstanding unpaid bills.
        /// </summary>
        public static int ReconcileCustomerCreditAndBills(int customerId, SqlConnection conn, SqlTransaction transaction)
        {
            int autoSettledCount = 0;

            // 1. Get current AccountBalance for Customer
            SqlCommand balanceCmd = new SqlCommand(
                "SELECT AccountBalance FROM Customers WITH (UPDLOCK) WHERE CustomerID = @CustID", 
                conn, 
                transaction);
            balanceCmd.Parameters.AddWithValue("@CustID", customerId);
            object? res = balanceCmd.ExecuteScalar();

            if (res == null || res == DBNull.Value) return 0;
            decimal credit = Convert.ToDecimal(res);
            if (credit <= 0) return 0;

            // 2. Fetch all unpaid bills for this customer ordered by DueDate ASC, BillID ASC
            string unpaidBillsSql = @"
                SELECT BillID, AmountDue FROM Bills WITH (UPDLOCK) 
                WHERE CustomerID = @CustID AND IsPaid = 0 
                ORDER BY DueDate ASC, BillID ASC";

            var unpaidBills = new List<(int BillID, decimal AmountDue)>();
            using (var billsCmd = new SqlCommand(unpaidBillsSql, conn, transaction))
            {
                billsCmd.Parameters.AddWithValue("@CustID", customerId);
                using var reader = billsCmd.ExecuteReader();
                while (reader.Read())
                {
                    unpaidBills.Add((Convert.ToInt32(reader["BillID"]), Convert.ToDecimal(reader["AmountDue"])));
                }
            }

            // 3. Process unpaid bills using available credit
            foreach (var bill in unpaidBills)
            {
                if (credit <= 0) break;

                decimal appliedAmount = 0;
                if (credit >= bill.AmountDue)
                {
                    appliedAmount = bill.AmountDue;
                    credit -= appliedAmount;

                    // Mark bill fully paid
                    SqlCommand payBillCmd = new SqlCommand(
                        "UPDATE Bills SET IsPaid = 1, AmountDue = 0, Status = 'Paid' WHERE BillID = @BillID", 
                        conn, 
                        transaction);
                    payBillCmd.Parameters.AddWithValue("@BillID", bill.BillID);
                    payBillCmd.ExecuteNonQuery();

                    autoSettledCount++;
                }
                else
                {
                    appliedAmount = credit;
                    decimal newRemaining = bill.AmountDue - credit;
                    credit = 0;

                    // Update partial bill balance
                    SqlCommand partialCmd = new SqlCommand(
                        "UPDATE Bills SET AmountDue = @NewRemaining, IsPaid = 0, Status = 'PartiallyPaid' WHERE BillID = @BillID", 
                        conn, 
                        transaction);
                    partialCmd.Parameters.AddWithValue("@NewRemaining", newRemaining);
                    partialCmd.Parameters.AddWithValue("@BillID", bill.BillID);
                    partialCmd.ExecuteNonQuery();
                }

                // Record auto-reconciliation payment entry
                SqlCommand autoPayCmd = new SqlCommand(
                    "INSERT INTO Payments (BillID, AmountPaid, PaymentMethod) VALUES (@BillID, @Amount, 'Account Credit Auto-Reconciliation')", 
                    conn, 
                    transaction);
                autoPayCmd.Parameters.AddWithValue("@BillID", bill.BillID);
                autoPayCmd.Parameters.AddWithValue("@Amount", appliedAmount);
                autoPayCmd.ExecuteNonQuery();
            }

            // 4. Update final customer AccountBalance
            SqlCommand updateCustCmd = new SqlCommand(
                "UPDATE Customers SET AccountBalance = @FinalCredit WHERE CustomerID = @CustID", 
                conn, 
                transaction);
            updateCustCmd.Parameters.AddWithValue("@FinalCredit", credit);
            updateCustCmd.Parameters.AddWithValue("@CustID", customerId);
            updateCustCmd.ExecuteNonQuery();

            return autoSettledCount;
        }
    }
}
