using Microsoft.AspNetCore.Mvc;
using Microsoft.Data.Sqlite;
using MiniBillingSystem.Models;

namespace MiniBillingSystem.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class CustomersController : ControllerBase
    {
        private readonly string _connStr;

        public CustomersController(IConfiguration configuration)
        {
            _connStr = configuration.GetConnectionString("DefaultConnection") 
                ?? "Data Source=billing.db;Cache=Shared";
        }

        [HttpGet]
        public IActionResult GetCustomers([FromQuery] int page = 1, [FromQuery] int pageSize = 50, [FromQuery] string sortBy = "recent_created")
        {
            if (page < 1) page = 1;
            if (pageSize < 1 || pageSize > 100) pageSize = 50;
            int offset = (page - 1) * pageSize;

            var customers = new List<CustomerDto>();
            int totalCount = 0;

            string countSql = "SELECT COUNT(*) FROM Customers";

            string orderByClause = sortBy?.ToLower() switch
            {
                "recent_activity" => "ORDER BY COALESCE(MAX(p.PaymentDate), MAX(b.DueDate), '1970-01-01') DESC, c.CustomerID DESC",
                "name" => "ORDER BY c.FullName ASC, c.CustomerID DESC",
                _ => "ORDER BY c.CustomerID DESC"
            };

            string sql = $@"
                SELECT c.CustomerID, c.FullName, c.AccountBalance AS Credit, COALESCE(SUM(b.AmountDue), 0) AS TotalOwed
                FROM Customers c
                LEFT JOIN Bills b ON c.CustomerID = b.CustomerID AND b.IsPaid = 0
                LEFT JOIN Payments p ON b.BillID = p.BillID
                GROUP BY c.CustomerID, c.FullName, c.AccountBalance
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
                    customers.Add(new CustomerDto
                    {
                        CustomerID = Convert.ToInt32(reader["CustomerID"]),
                        FullName = reader["FullName"].ToString() ?? "",
                        AccountBalance = Convert.ToDecimal(reader["Credit"]),
                        TotalOwed = Convert.ToDecimal(reader["TotalOwed"])
                    });
                }

                return Ok(new { data = customers, page, pageSize, totalCount, sortBy });
            }
            catch
            {
                return StatusCode(500, new { error = "Oops! We couldn't load the customer list right now." });
            }
        }

        [HttpPost]
        public IActionResult RegisterCustomer([FromBody] CreateCustomerRequest request)
        {
            if (request == null)
            {
                return BadRequest(new { error = "This field cannot be left blank. Please enter a value." });
            }

            string name = (request.FullName ?? "").Trim();
            string address = (request.Address ?? "").Trim();
            string phone = (request.PhoneNumber ?? "").Trim();
            string email = (request.Email ?? "").Trim();

            if (string.IsNullOrEmpty(name))
                return BadRequest(new { error = "Full Name cannot be left blank. Please enter a value." });
            if (name.Length > 100)
                return BadRequest(new { error = "Full Name is a bit too long! Please shorten it to 100 characters or less." });

            if (string.IsNullOrEmpty(address))
                return BadRequest(new { error = "Address cannot be left blank. Please enter a value." });
            if (address.Length > 255)
                return BadRequest(new { error = "Address is a bit too long! Please shorten it to 255 characters or less." });

            if (string.IsNullOrEmpty(phone))
                return BadRequest(new { error = "Phone Number cannot be left blank. Please enter a value." });
            if (phone.Length > 20)
                return BadRequest(new { error = "Phone Number is a bit too long! Please shorten it to 20 characters or less." });

            if (string.IsNullOrEmpty(email))
                return BadRequest(new { error = "Email cannot be left blank. Please enter a value." });
            if (email.Length > 100)
                return BadRequest(new { error = "Email is a bit too long! Please shorten it to 100 characters or less." });

            string sql = @"
                INSERT INTO Customers (FullName, Address, PhoneNumber, Email) 
                VALUES (@Name, @Address, @Phone, @Email)
                RETURNING CustomerID;";

            try
            {
                using var conn = new SqliteConnection(_connStr);
                conn.Open();
                using var cmd = new SqliteCommand(sql, conn);
                cmd.Parameters.AddWithValue("@Name", name);
                cmd.Parameters.AddWithValue("@Address", address);
                cmd.Parameters.AddWithValue("@Phone", phone);
                cmd.Parameters.AddWithValue("@Email", email);

                int newId = Convert.ToInt32(cmd.ExecuteScalar());
                return Ok(new { message = "Customer successfully registered!", customerID = newId });
            }
            catch (SqliteException ex) when (ex.SqliteErrorCode == 19)
            {
                return BadRequest(new { error = "A customer with this email already exists." });
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { error = "Oops! Something went wrong saving the customer: " + ex.Message });
            }
        }

        [HttpGet("{id}")]
        public IActionResult GetCustomerHistory(int id)
        {
            try
            {
                using var conn = new SqliteConnection(_connStr);
                conn.Open();

                string infoSql = @"
                    SELECT c.CustomerID, c.FullName, c.Address, c.PhoneNumber, c.Email, c.AccountBalance,
                           COALESCE((SELECT SUM(b.AmountDue) FROM Bills b WHERE b.CustomerID = c.CustomerID AND b.IsPaid = 0), 0) AS TotalOwed
                    FROM Customers c WHERE c.CustomerID = @ID";

                CustomerDetailDto? detail = null;
                using (var infoCmd = new SqliteCommand(infoSql, conn))
                {
                    infoCmd.Parameters.AddWithValue("@ID", id);
                    using var reader = infoCmd.ExecuteReader();
                    if (reader.Read())
                    {
                        detail = new CustomerDetailDto
                        {
                            CustomerID = Convert.ToInt32(reader["CustomerID"]),
                            FullName = reader["FullName"].ToString() ?? "",
                            Address = reader["Address"].ToString() ?? "",
                            PhoneNumber = reader["PhoneNumber"].ToString() ?? "",
                            Email = reader["Email"].ToString() ?? "",
                            AccountBalance = Convert.ToDecimal(reader["AccountBalance"]),
                            TotalOwed = Convert.ToDecimal(reader["TotalOwed"])
                        };
                    }
                }

                if (detail == null)
                {
                    return NotFound(new { error = $"We couldn't find a customer with ID '{id}'." });
                }

                // Fetch Bills
                string billSql = "SELECT BillID, AmountDue, DueDate, IsPaid FROM Bills WHERE CustomerID = @ID ORDER BY BillID DESC";
                using (var billCmd = new SqliteCommand(billSql, conn))
                {
                    billCmd.Parameters.AddWithValue("@ID", id);
                    using var billReader = billCmd.ExecuteReader();
                    while (billReader.Read())
                    {
                        detail.Bills.Add(new BillDto
                        {
                            BillID = Convert.ToInt32(billReader["BillID"]),
                            CustomerID = id,
                            FullName = detail.FullName,
                            AmountDue = Convert.ToDecimal(billReader["AmountDue"]),
                            DueDate = billReader["DueDate"].ToString() ?? "",
                            IsPaid = Convert.ToInt32(billReader["IsPaid"]) == 1
                        });
                    }
                }

                // Fetch Payments
                string paySql = @"
                    SELECT p.PaymentID, p.BillID, p.AmountPaid, p.PaymentDate, p.PaymentMethod 
                    FROM Payments p 
                    JOIN Bills b ON p.BillID = b.BillID 
                    WHERE b.CustomerID = @ID 
                    ORDER BY p.PaymentID DESC";

                using (var payCmd = new SqliteCommand(paySql, conn))
                {
                    payCmd.Parameters.AddWithValue("@ID", id);
                    using var payReader = payCmd.ExecuteReader();
                    while (payReader.Read())
                    {
                        detail.Payments.Add(new PaymentDto
                        {
                            PaymentID = Convert.ToInt32(payReader["PaymentID"]),
                            BillID = Convert.ToInt32(payReader["BillID"]),
                            CustomerID = id,
                            FullName = detail.FullName,
                            AmountPaid = Convert.ToDecimal(payReader["AmountPaid"]),
                            PaymentMethod = payReader["PaymentMethod"].ToString() ?? "",
                            PaymentDate = payReader["PaymentDate"].ToString() ?? ""
                        });
                    }
                }

                return Ok(detail);
            }
            catch
            {
                return StatusCode(500, new { error = "Sorry, we ran into an issue pulling up this customer's dashboard." });
            }
        }

        [HttpPut("{id}")]
        public IActionResult UpdateCustomer(int id, [FromBody] UpdateCustomerRequest request)
        {
            if (request == null)
            {
                return BadRequest(new { error = "This field cannot be left blank. Please enter a value." });
            }

            string name = (request.FullName ?? "").Trim();
            string address = (request.Address ?? "").Trim();
            string phone = (request.PhoneNumber ?? "").Trim();
            string email = (request.Email ?? "").Trim();

            if (string.IsNullOrEmpty(name))
                return BadRequest(new { error = "Full Name cannot be left blank. Please enter a value." });
            if (name.Length > 100)
                return BadRequest(new { error = "Full Name is a bit too long! Please shorten it to 100 characters or less." });

            if (string.IsNullOrEmpty(address))
                return BadRequest(new { error = "Address cannot be left blank. Please enter a value." });
            if (address.Length > 255)
                return BadRequest(new { error = "Address is a bit too long! Please shorten it to 255 characters or less." });

            if (string.IsNullOrEmpty(phone))
                return BadRequest(new { error = "Phone Number cannot be left blank. Please enter a value." });
            if (phone.Length > 20)
                return BadRequest(new { error = "Phone Number is a bit too long! Please shorten it to 20 characters or less." });

            if (string.IsNullOrEmpty(email))
                return BadRequest(new { error = "Email cannot be left blank. Please enter a value." });
            if (email.Length > 100)
                return BadRequest(new { error = "Email is a bit too long! Please shorten it to 100 characters or less." });

            try
            {
                using var conn = new SqliteConnection(_connStr);
                conn.Open();
                using var cmd = new SqliteCommand(@"
                    UPDATE Customers 
                    SET FullName = @FullName, Address = @Address, PhoneNumber = @PhoneNumber, Email = @Email 
                    WHERE CustomerID = @CustomerID", conn);
                cmd.Parameters.AddWithValue("@CustomerID", id);
                cmd.Parameters.AddWithValue("@FullName", name);
                cmd.Parameters.AddWithValue("@Address", address);
                cmd.Parameters.AddWithValue("@PhoneNumber", phone);
                cmd.Parameters.AddWithValue("@Email", email);

                int rows = cmd.ExecuteNonQuery();
                if (rows == 0)
                {
                    return NotFound(new { error = $"We couldn't find a customer with ID '{id}' to update." });
                }

                return Ok(new { message = "Customer details successfully updated." });
            }
            catch (SqliteException ex) when (ex.SqliteErrorCode == 19)
            {
                return BadRequest(new { error = "A customer with this email already exists." });
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { error = "Oops! Something went wrong updating the customer: " + ex.Message });
            }
        }

        [HttpDelete("{id}")]
        public IActionResult DeleteCustomer(int id)
        {
            try
            {
                using var conn = new SqliteConnection(_connStr);
                conn.Open();

                // Check for existing bills or payments
                using (var checkCmd = new SqliteCommand("SELECT COUNT(*) FROM Bills WHERE CustomerID = @CustomerID", conn))
                {
                    checkCmd.Parameters.AddWithValue("@CustomerID", id);
                    long billCount = Convert.ToInt64(checkCmd.ExecuteScalar());
                    if (billCount > 0)
                    {
                        return BadRequest(new { error = "Cannot delete customer because they have existing bills or payment records. Delete or settle their bills first." });
                    }
                }

                using var cmd = new SqliteCommand("DELETE FROM Customers WHERE CustomerID = @CustomerID", conn);
                cmd.Parameters.AddWithValue("@CustomerID", id);

                int rows = cmd.ExecuteNonQuery();
                if (rows == 0)
                {
                    return NotFound(new { error = $"We couldn't find a customer with ID '{id}' to delete." });
                }

                return Ok(new { message = "Customer successfully deleted." });
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { error = "Oops! Something went wrong deleting the customer: " + ex.Message });
            }
        }
    }
}
