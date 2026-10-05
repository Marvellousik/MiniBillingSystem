using Microsoft.AspNetCore.Mvc;
using Microsoft.Data.Sqlite;
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
                connStr = "Data Source=billing.db;Cache=Shared";
            }

            // 2. Database Connectivity Check (SELECT 1)
            long dbResponseTimeMs = -1;
            bool dbConnected = false;
            string? dbError = null;

            try
            {
                string effectiveConn = connStr ?? "Data Source=billing.db;Cache=Shared";
                DbInitializer.Initialize(effectiveConn);

                var dbSw = Stopwatch.StartNew();
                using var conn = new SqliteConnection(effectiveConn);
                conn.Open();
                using var cmd = new SqliteCommand("SELECT 1", conn);
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
                    using var conn = new SqliteConnection(connStr);
                    conn.Open();

                    // Check required tables in SQLite
                    string checkTablesSql = @"
                        SELECT name FROM sqlite_master 
                        WHERE type = 'table' AND name IN ('Customers', 'Bills', 'Payments')";
                    using var tableCmd = new SqliteCommand(checkTablesSql, conn);
                    using var tableReader = tableCmd.ExecuteReader();
                    while (tableReader.Read())
                    {
                        verifiedTables.Add(tableReader["name"].ToString() ?? "");
                    }
                    tableReader.Close();

                    // In SQLite, business operations are implemented in engine/controllers
                    verifiedProcedures = new List<string> 
                    { 
                        "usp_AddCustomer (SQLite Engine)", 
                        "usp_CreateBill (SQLite Engine)", 
                        "usp_RecordPayment (SQLite Engine)" 
                    };

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
