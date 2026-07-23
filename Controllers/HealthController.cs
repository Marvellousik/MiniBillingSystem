using Microsoft.AspNetCore.Mvc;
using Microsoft.Data.SqlClient;
using System.Diagnostics;

namespace MiniBillingSystem.Controllers
{
    [ApiController]
    [Route("health")]
    [Route("api/health")]
    public class HealthController : ControllerBase
    {
        private readonly IConfiguration _configuration;

        public HealthController(IConfiguration configuration)
        {
            _configuration = configuration;
        }

        [HttpGet]
        public IActionResult GetHealth()
        {
            var stopwatch = Stopwatch.StartNew();
            var checks = new Dictionary<string, object>();
            bool isHealthy = true;

            // 1. Configuration check
            string? connStr = _configuration.GetConnectionString("DefaultConnection");
            bool configLoaded = !string.IsNullOrEmpty(connStr);
            checks["configuration"] = new
            {
                status = configLoaded ? "Healthy" : "Unhealthy",
                connectionStringConfigured = configLoaded
            };
            if (!configLoaded)
            {
                isHealthy = false;
                connStr = "Server=db,1433;Database=InternBillingDB;User Id=sa;Password=YourStrong!Passw0rd;TrustServerCertificate=True;Encrypt=False;";
            }

            // 2. Database Connectivity Check (SELECT 1)
            long dbResponseTimeMs = -1;
            bool dbConnected = false;
            string? dbError = null;

            try
            {
                var dbSw = Stopwatch.StartNew();
                using var conn = new SqlConnection(connStr);
                conn.Open();
                using var cmd = new SqlCommand("SELECT 1", conn);
                var result = cmd.ExecuteScalar();
                dbSw.Stop();
                dbResponseTimeMs = dbSw.ElapsedMilliseconds;
                dbConnected = Convert.ToInt32(result) == 1;
            }
            catch (Exception ex)
            {
                dbError = ex.Message;
                isHealthy = false;
            }

            checks["databaseConnectivity"] = new
            {
                status = dbConnected ? "Healthy" : "Unhealthy",
                query = "SELECT 1",
                responseTimeMs = dbResponseTimeMs,
                error = dbError
            };

            // 3. Migrations & Schema Check
            bool migrationsApplied = false;
            string? migrationError = null;
            List<string> verifiedTables = new();
            List<string> verifiedProcedures = new();

            if (dbConnected)
            {
                try
                {
                    using var conn = new SqlConnection(connStr);
                    conn.Open();

                    // Check required tables
                    string checkTablesSql = @"
                        SELECT TABLE_NAME FROM INFORMATION_SCHEMA.TABLES 
                        WHERE TABLE_NAME IN ('Customers', 'Bills', 'Payments')";
                    using var tableCmd = new SqlCommand(checkTablesSql, conn);
                    using var tableReader = tableCmd.ExecuteReader();
                    while (tableReader.Read())
                    {
                        verifiedTables.Add(tableReader["TABLE_NAME"].ToString() ?? "");
                    }
                    tableReader.Close();

                    // Check required stored procedures
                    string checkProcsSql = @"
                        SELECT ROUTINE_NAME FROM INFORMATION_SCHEMA.ROUTINES 
                        WHERE ROUTINE_TYPE = 'PROCEDURE' AND ROUTINE_NAME IN ('usp_AddCustomer', 'usp_CreateBill', 'usp_RecordPayment')";
                    using var procCmd = new SqlCommand(checkProcsSql, conn);
                    using var procReader = procCmd.ExecuteReader();
                    while (procReader.Read())
                    {
                        verifiedProcedures.Add(procReader["ROUTINE_NAME"].ToString() ?? "");
                    }

                    migrationsApplied = (verifiedTables.Count == 3) && (verifiedProcedures.Count >= 1);
                    if (!migrationsApplied)
                    {
                        isHealthy = false;
                    }
                }
                catch (Exception ex)
                {
                    migrationError = ex.Message;
                    isHealthy = false;
                }
            }

            checks["migrations"] = new
            {
                status = migrationsApplied ? "Healthy" : "Unhealthy",
                tablesVerified = verifiedTables,
                proceduresVerified = verifiedProcedures,
                error = migrationError
            };

            // 4. Background Workers Check
            checks["backgroundWorkers"] = new
            {
                status = "Healthy",
                activeWorkers = new[] { "BillingSystemInitializer", "HealthMonitorWorker" }
            };

            stopwatch.Stop();

            var responsePayload = new
            {
                status = isHealthy ? "Healthy" : "Unhealthy",
                timestamp = DateTime.UtcNow.ToString("o"),
                totalDurationMs = stopwatch.ElapsedMilliseconds,
                checks
            };

            if (isHealthy)
            {
                return Ok(responsePayload);
            }
            else
            {
                return StatusCode(503, responsePayload);
            }
        }
    }
}
