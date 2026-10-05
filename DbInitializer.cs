using System;
using System.IO;
using Microsoft.Data.Sqlite;

namespace MiniBillingSystem
{
    public static class DbInitializer
    {
        private static readonly object _lock = new();
        private static bool _initialized = false;

        public static void Initialize(string connectionString)
        {
            if (_initialized) return;

            lock (_lock)
            {
                if (_initialized) return;

                // Ensure directory exists if path is specified
                try
                {
                    var builder = new SqliteConnectionStringBuilder(connectionString);
                    if (!string.IsNullOrEmpty(builder.DataSource) && builder.DataSource != ":memory:")
                    {
                        var dir = Path.GetDirectoryName(Path.GetFullPath(builder.DataSource));
                        if (!string.IsNullOrEmpty(dir) && !Directory.Exists(dir))
                        {
                            Directory.CreateDirectory(dir);
                        }
                    }
                }
                catch
                {
                    // Ignore connection string parse failure, let open handle it
                }

                using var conn = new SqliteConnection(connectionString);
                conn.Open();

                using (var pragmaCmd = conn.CreateCommand())
                {
                    pragmaCmd.CommandText = "PRAGMA journal_mode = WAL; PRAGMA foreign_keys = ON; PRAGMA busy_timeout = 5000;";
                    pragmaCmd.ExecuteNonQuery();
                }

                string createTablesSql = @"
                    CREATE TABLE IF NOT EXISTS Customers (
                        CustomerID INTEGER PRIMARY KEY AUTOINCREMENT,
                        FullName TEXT NOT NULL,
                        Address TEXT NOT NULL,
                        PhoneNumber TEXT NOT NULL,
                        Email TEXT NOT NULL UNIQUE,
                        AccountBalance NUMERIC NOT NULL DEFAULT 0.00
                    );

                    CREATE TABLE IF NOT EXISTS Bills (
                        BillID INTEGER PRIMARY KEY AUTOINCREMENT,
                        CustomerID INTEGER NOT NULL,
                        Amount NUMERIC NOT NULL CHECK (Amount >= 0),
                        AmountDue NUMERIC NOT NULL CHECK (AmountDue >= 0),
                        DueDate TEXT NOT NULL,
                        IsPaid INTEGER NOT NULL DEFAULT 0,
                        Status TEXT NOT NULL DEFAULT 'Unpaid' CHECK (Status IN ('Unpaid','PartiallyPaid','Paid')),
                        FOREIGN KEY (CustomerID) REFERENCES Customers(CustomerID)
                    );

                    CREATE TABLE IF NOT EXISTS Payments (
                        PaymentID INTEGER PRIMARY KEY AUTOINCREMENT,
                        BillID INTEGER NOT NULL,
                        AmountPaid NUMERIC NOT NULL CHECK (AmountPaid > 0),
                        PaymentDate TEXT NOT NULL DEFAULT (datetime('now')),
                        PaymentMethod TEXT NOT NULL DEFAULT 'Cash',
                        FOREIGN KEY (BillID) REFERENCES Bills(BillID)
                    );
                ";

                using (var cmd = conn.CreateCommand())
                {
                    cmd.CommandText = createTablesSql;
                    cmd.ExecuteNonQuery();
                }

                // Check if customers table is empty, seed if empty
                using (var checkCmd = conn.CreateCommand())
                {
                    checkCmd.CommandText = "SELECT COUNT(*) FROM Customers";
                    long count = Convert.ToInt64(checkCmd.ExecuteScalar());

                    if (count == 0)
                    {
                        SeedInitialData(conn);
                    }
                }

                _initialized = true;
            }
        }

        private static void SeedInitialData(SqliteConnection conn)
        {
            using var tx = conn.BeginTransaction();
            try
            {
                string seedSql = @"
                    INSERT INTO Customers (CustomerID, FullName, Address, PhoneNumber, Email, AccountBalance) VALUES
                    (1, 'Emeka Okafor', 'Plot 142, Aminu Kano Crescent, Wuse II, Abuja', '08033124590', 'emeka.okafor@gmail.com', 5000.00),
                    (2, 'Hauwa Mohammed Bello', 'No. 8, Crescent 4, Kado Estate, Abuja', '08124567891', 'hmbello@yahoo.com', 0.00),
                    (3, 'Adebayo Chukwuma Adeleke', 'Block 12, Flat 3, FHA Estate, Gwarinpa, Abuja', '07061234567', 'bayo.adeleke@outlook.com', 12500.50),
                    (4, 'Fatima Garba Aliyu', 'Shop 15, Area 1 Shopping Complex, Garki, Abuja', '08059876543', 'fatima.aliyu@garki-biz.ng', 0.00),
                    (5, 'Chidiebere Nnamdi Okonkwo', 'No. 24, Obafemi Awolowo Way, Jabi, Abuja', '08187654321', 'c.okonkwo@gmail.com', 2500.00),
                    (6, 'Aisha Ibrahim Danjuma', 'Suite B12, Banex Plaza, Wuse 2, Abuja', '08023456789', 'aisha.danjuma@hotmail.com', 0.00),
                    (7, 'Babatunde Olumide Johnson', 'House 7, 3rd Avenue, Model City, Lugbe, Abuja', '08139988776', 'babs.johnson@gmail.com', 0.00),
                    (8, 'Nkechi Blessing Ezekwesili', 'Plot 402, Cadastral Zone B06, Mabushi, Abuja', '07031122334', 'nkechi.blessing@yahoo.com', 7500.00),
                    (9, 'Umar Farouq Yakubu', 'No. 5, Shehu Shagari Way, Maitama, Abuja', '08094455667', 'uf.yakubu@amaccouncil.gov.ng', 0.00),
                    (10, 'Grace Ifeoma Nwachukwu', 'Corner Shop 4, Karu Market Road, Karu, Abuja', '08162233445', 'grace.nwachukwu@gmail.com', 1500.00);

                    INSERT INTO Bills (BillID, CustomerID, AmountDue, DueDate, IsPaid, Amount, Status) VALUES
                    (1, 1, 0.00, '2026-06-15', 1, 18500.00, 'Paid'),
                    (2, 1, 24500.00, '2026-08-15', 0, 24500.00, 'Unpaid'),
                    (3, 2, 0.00, '2026-05-30', 1, 15000.00, 'Paid'),
                    (4, 2, 12000.00, '2026-07-30', 0, 12000.00, 'Unpaid'),
                    (5, 3, 0.00, '2026-06-01', 1, 35000.00, 'Paid'),
                    (6, 3, 0.00, '2026-07-01', 1, 27500.00, 'Paid'),
                    (7, 4, 45000.00, '2026-07-28', 0, 45000.00, 'Unpaid'),
                    (8, 5, 0.00, '2026-06-20', 1, 16000.00, 'Paid'),
                    (9, 5, 18500.00, '2026-08-10', 0, 18500.00, 'Unpaid'),
                    (10, 6, 65000.00, '2026-07-25', 0, 65000.00, 'Unpaid'),
                    (11, 7, 9500.00, '2026-07-31', 0, 9500.00, 'Unpaid'),
                    (12, 8, 0.00, '2026-06-10', 1, 22000.00, 'Paid'),
                    (13, 9, 85000.00, '2026-08-30', 0, 85000.00, 'Unpaid'),
                    (14, 10, 8000.00, '2026-07-29', 0, 8000.00, 'Unpaid');

                    INSERT INTO Payments (PaymentID, BillID, AmountPaid, PaymentDate, PaymentMethod) VALUES
                    (1, 1, 18500.00, '2026-06-12 10:14:22', 'Bank Transfer (GTBank)'),
                    (2, 3, 15000.00, '2026-05-28 14:30:05', 'POS Terminal'),
                    (3, 5, 35000.00, '2026-05-31 09:45:12', 'Remita Web Pay'),
                    (4, 6, 40000.00, '2026-07-01 16:20:00', 'Bank Transfer (Access Bank)'),
                    (5, 8, 16000.00, '2026-06-18 11:05:44', 'USSD (*737#)'),
                    (6, 12, 22000.00, '2026-06-08 15:10:30', 'Bank Transfer (Zenith Bank)');
                ";

                using (var seedCmd = conn.CreateCommand())
                {
                    seedCmd.Transaction = tx;
                    seedCmd.CommandText = seedSql;
                    seedCmd.ExecuteNonQuery();
                }

                tx.Commit();
            }
            catch
            {
                tx.Rollback();
                throw;
            }
        }
    }
}
