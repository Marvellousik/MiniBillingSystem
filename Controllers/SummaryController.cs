using Microsoft.AspNetCore.Mvc;
using Microsoft.Data.Sqlite;
using MiniBillingSystem.Models;

namespace MiniBillingSystem.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class SummaryController : ControllerBase
    {
        private readonly string _connStr;

        public SummaryController(IConfiguration configuration)
        {
            _connStr = configuration.GetConnectionString("DefaultConnection") 
                ?? "Data Source=billing.db;Cache=Shared";
        }

        [HttpGet]
        public IActionResult GetSummary()
        {
            try
            {
                using var conn = new SqliteConnection(_connStr);
                conn.Open();

                int totalCustomers = 0;
                decimal totalOutstanding = 0;
                int billsDueThisWeek = 0;
                decimal totalCollected = 0;

                using (var cmd = new SqliteCommand("SELECT COUNT(*) FROM Customers", conn))
                {
                    totalCustomers = Convert.ToInt32(cmd.ExecuteScalar());
                }

                using (var cmd = new SqliteCommand("SELECT COALESCE(SUM(AmountDue), 0) FROM Bills WHERE IsPaid = 0", conn))
                {
                    totalOutstanding = Convert.ToDecimal(cmd.ExecuteScalar());
                }

                // Bills due between today and 7 days from now (unpaid)
                using (var cmd = new SqliteCommand(@"
                    SELECT COUNT(*) FROM Bills 
                    WHERE IsPaid = 0 AND date(DueDate) >= date('now') AND date(DueDate) <= date('now', '+7 days')", conn))
                {
                    billsDueThisWeek = Convert.ToInt32(cmd.ExecuteScalar());
                }

                using (var cmd = new SqliteCommand("SELECT COALESCE(SUM(AmountPaid), 0) FROM Payments", conn))
                {
                    totalCollected = Convert.ToDecimal(cmd.ExecuteScalar());
                }

                return Ok(new SummaryDto
                {
                    TotalCustomers = totalCustomers,
                    TotalOutstanding = totalOutstanding,
                    BillsDueThisWeek = billsDueThisWeek,
                    TotalCollected = totalCollected
                });
            }
            catch
            {
                return StatusCode(500, new { error = "Unable to load summary metrics right now." });
            }
        }
    }
}
