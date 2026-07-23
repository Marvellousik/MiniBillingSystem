using Microsoft.AspNetCore.Mvc;
using Microsoft.Data.SqlClient;
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
                ?? "Server=.\\SQLEXPRESS;Database=InternBillingDB;Integrated Security=True;TrustServerCertificate=True;";
        }

        [HttpGet]
        public IActionResult GetCustomers([FromQuery] int page = 1, [FromQuery] int pageSize = 50)
        {
            if (page < 1) page = 1;
            if (pageSize < 1 || pageSize > 100) pageSize = 50;
            int offset = (page - 1) * pageSize;

            var customers = new List<CustomerDto>();
            int totalCount = 0;

            string countSql = "SELECT COUNT(*) FROM Customers";
            string sql = @"
                SELECT c.CustomerID, c.FullName, c.AccountBalance AS Credit, ISNULL(SUM(b.AmountDue), 0) AS TotalOwed
                FROM Customers c
                LEFT JOIN Bills b ON c.CustomerID = b.CustomerID AND b.IsPaid = 0
                GROUP BY c.CustomerID, c.FullName, c.AccountBalance
                ORDER BY c.CustomerID OFFSET @Offset ROWS FETCH NEXT @PageSize ROWS ONLY";

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
                    customers.Add(new CustomerDto
                    {
                        CustomerID = Convert.ToInt32(reader["CustomerID"]),
                        FullName = reader["FullName"].ToString() ?? "",
                        AccountBalance = Convert.ToDecimal(reader["Credit"]),
                        TotalOwed = Convert.ToDecimal(reader["TotalOwed"])
                    });
                }

                return Ok(new { data = customers, page, pageSize, totalCount });
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
                OUTPUT INSERTED.CustomerID 
                VALUES (@Name, @Address, @Phone, @Email)";

            try
            {
                using var conn = new SqlConnection(_connStr);
                conn.Open();
                using var cmd = new SqlCommand(sql, conn);
                cmd.Parameters.AddWithValue("@Name", name);
                cmd.Parameters.AddWithValue("@Address", address);
                cmd.Parameters.AddWithValue("@Phone", phone);
                cmd.Parameters.AddWithValue("@Email", email);

                int newId = Convert.ToInt32(cmd.ExecuteScalar());
                return Ok(new { message = "Customer successfully registered!", customerID = newId });
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
                using var conn = new SqlConnection(_connStr);
                conn.Open();

                string infoSql = @"
                    SELECT c.CustomerID, c.FullName, c.Address, c.PhoneNumber, c.Email, c.AccountBalance,
                           ISNULL((SELECT SUM(b.AmountDue) FROM Bills b WHERE b.CustomerID = c.CustomerID AND b.IsPaid = 0), 0) AS TotalOwed
                    FROM Customers c WHERE c.CustomerID = @ID";

                CustomerDetailDto? detail = null;
                using (var infoCmd = new SqlCommand(infoSql, conn))
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
                using (var billCmd = new SqlCommand(billSql, conn))
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
                            DueDate = Convert.ToDateTime(billReader["DueDate"]).ToString("yyyy-MM-dd"),
                            IsPaid = Convert.ToBoolean(billReader["IsPaid"])
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

                using (var payCmd = new SqlCommand(paySql, conn))
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
                            PaymentDate = Convert.ToDateTime(payReader["PaymentDate"]).ToString("yyyy-MM-dd")
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
    }
}
