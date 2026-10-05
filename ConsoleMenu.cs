using System;
using System.Collections.Generic;
using System.Data;
using System.IO;
using System.Linq;
using Microsoft.Data.Sqlite;
using Microsoft.Extensions.Configuration;

namespace MiniBillingSystem
{
    public static class ConsoleMenu
    {
        public static string ConnStr { get; set; } = "Data Source=billing.db;Cache=Shared";

        public static void Initialize(IConfiguration configuration)
        {
            string? configConn = configuration.GetConnectionString("DefaultConnection");
            if (!string.IsNullOrWhiteSpace(configConn))
            {
                ConnStr = configConn;
            }
            DbInitializer.Initialize(ConnStr);
        }

        public static void LogError(string context, Exception ex)
        {
            string message = $"[{DateTime.Now}] {context}: {ex.Message}";
            Console.WriteLine(message);
            try
            {
                File.AppendAllText("errors.log", message + Environment.NewLine);
            }
            catch (Exception logEx)
            {
                Console.WriteLine($"[Logging Failed] {logEx.Message}");
            }
        }

        public static List<Models.Customer> GetAllCustomers()
        {
            var customers = new List<Models.Customer>();
            try
            {
                DbInitializer.Initialize(ConnStr);
                using (SqliteConnection conn = new SqliteConnection(ConnStr))
                {
                    conn.Open();
                    using (SqliteCommand cmd = new SqliteCommand("SELECT CustomerID, FullName, Address, PhoneNumber, Email FROM Customers", conn))
                    {
                        using (SqliteDataReader reader = cmd.ExecuteReader())
                        {
                            while (reader.Read())
                            {
                                int id = Convert.ToInt32(reader["CustomerID"]);
                                string name = reader["FullName"].ToString() ?? "";
                                string address = reader.HasColumn("Address") ? (reader["Address"]?.ToString() ?? "") : "";
                                string phone = reader["PhoneNumber"].ToString() ?? "";
                                string email = reader["Email"].ToString() ?? "";

                                customers.Add(new Models.Customer(id, name, address, phone, email));
                            }
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                LogError("GetAllCustomers", ex);
            }
            return customers;
        }

        private static bool HasColumn(this SqliteDataReader reader, string columnName)
        {
            for (int i = 0; i < reader.FieldCount; i++)
            {
                if (reader.GetName(i).Equals(columnName, StringComparison.OrdinalIgnoreCase))
                    return true;
            }
            return false;
        }

        public static void ViewCustomers()
        {
            List<Models.Customer> customers = GetAllCustomers();
            Console.WriteLine($"\n{"ID",-5} {"Name",-25} {"Phone",-16} Email");
            Console.WriteLine(new string('-', 70));
            foreach (Models.Customer c in customers)
            {
                Console.WriteLine(c);
            }
            Console.WriteLine($"Total Customers: {customers.Count}");
        }

        public static void SearchCustomerByName()
        {
            Console.Write("Search name contains: ");
            string? term = Console.ReadLine();
            if (string.IsNullOrWhiteSpace(term))
            {
                Console.WriteLine("Search term cannot be empty.");
                return;
            }

            List<Models.Customer> matches = GetAllCustomers()
                .Where(c => c.FullName.ToLower().Contains(term.ToLower()))
                .OrderBy(c => c.FullName)
                .ToList();

            Console.WriteLine($"\n{matches.Count} match(es) found:");
            Console.WriteLine($"{"ID",-5} {"Name",-25} {"Phone",-16} Email");
            Console.WriteLine(new string('-', 70));
            foreach (Models.Customer c in matches)
            {
                Console.WriteLine(c);
            }
        }

        public static void AddCustomer()
        {
            Console.Write("Full Name:    "); string? name = Console.ReadLine();
            Console.Write("Address:      "); string? address = Console.ReadLine();
            Console.Write("Phone Number: "); string? phone = Console.ReadLine();
            Console.Write("Email:        "); string? email = Console.ReadLine();

            try
            {
                DbInitializer.Initialize(ConnStr);
                using (SqliteConnection conn = new SqliteConnection(ConnStr))
                {
                    conn.Open();
                    using (SqliteCommand cmd = new SqliteCommand("INSERT INTO Customers (FullName, Address, PhoneNumber, Email) VALUES (@FullName, @Address, @PhoneNumber, @Email)", conn))
                    {
                        cmd.Parameters.AddWithValue("@FullName", name ?? "");
                        cmd.Parameters.AddWithValue("@Address", address ?? "");
                        cmd.Parameters.AddWithValue("@PhoneNumber", phone ?? "");
                        cmd.Parameters.AddWithValue("@Email", email ?? "");

                        cmd.ExecuteNonQuery();
                        Console.WriteLine("Customer successfully added.");
                    }
                }
            }
            catch (Exception ex)
            {
                LogError("AddCustomer", ex);
            }
        }

        public static void UpdateCustomer()
        {
            Console.Write("Customer ID to update: ");
            if (!int.TryParse(Console.ReadLine(), out int id))
            {
                Console.WriteLine("Invalid ID.");
                return;
            }
            Console.Write("New Full Name: "); string? name = Console.ReadLine();
            Console.Write("New Address:   "); string? address = Console.ReadLine();
            Console.Write("New Phone:     "); string? phone = Console.ReadLine();
            Console.Write("New Email:     "); string? email = Console.ReadLine();

            try
            {
                DbInitializer.Initialize(ConnStr);
                using (SqliteConnection conn = new SqliteConnection(ConnStr))
                {
                    conn.Open();
                    using (SqliteCommand cmd = new SqliteCommand("UPDATE Customers SET FullName = @FullName, Address = @Address, PhoneNumber = @PhoneNumber, Email = @Email WHERE CustomerID = @CustomerID", conn))
                    {
                        cmd.Parameters.AddWithValue("@CustomerID", id);
                        cmd.Parameters.AddWithValue("@FullName", name ?? "");
                        cmd.Parameters.AddWithValue("@Address", address ?? "");
                        cmd.Parameters.AddWithValue("@PhoneNumber", phone ?? "");
                        cmd.Parameters.AddWithValue("@Email", email ?? "");

                        int rows = cmd.ExecuteNonQuery();
                        Console.WriteLine(rows > 0 ? "Customer updated." : "No customer found with that ID.");
                    }
                }
            }
            catch (Exception ex)
            {
                LogError("UpdateCustomer", ex);
            }
        }

        public static void DeleteCustomer()
        {
            Console.Write("Customer ID to delete: ");
            if (!int.TryParse(Console.ReadLine(), out int id))
            {
                Console.WriteLine("Invalid ID.");
                return;
            }

            try
            {
                DbInitializer.Initialize(ConnStr);
                using (SqliteConnection conn = new SqliteConnection(ConnStr))
                {
                    conn.Open();
                    using (SqliteCommand cmd = new SqliteCommand("DELETE FROM Customers WHERE CustomerID = @CustomerID", conn))
                    {
                        cmd.Parameters.AddWithValue("@CustomerID", id);

                        int rows = cmd.ExecuteNonQuery();
                        Console.WriteLine(rows > 0 ? "Customer deleted." : "No customer found with that ID.");
                    }
                }
            }
            catch (Exception ex)
            {
                LogError("DeleteCustomer", ex);
            }
        }

        public static void CreateBill()
        {
            Console.Write("Customer ID: ");
            if (!int.TryParse(Console.ReadLine(), out int custId))
            {
                Console.WriteLine("Invalid Customer ID.");
                return;
            }
            Console.Write("Amount: ");
            if (!decimal.TryParse(Console.ReadLine(), out decimal amount))
            {
                Console.WriteLine("Invalid Amount.");
                return;
            }
            Console.Write("Due Date (YYYY-MM-DD): ");
            if (!DateTime.TryParse(Console.ReadLine(), out DateTime dueDate))
            {
                Console.WriteLine("Invalid Date.");
                return;
            }

            try
            {
                DbInitializer.Initialize(ConnStr);
                using (SqliteConnection conn = new SqliteConnection(ConnStr))
                {
                    conn.Open();
                    using (SqliteCommand cmd = new SqliteCommand(@"
                        INSERT INTO Bills (CustomerID, Amount, AmountDue, DueDate, IsPaid, Status) 
                        VALUES (@CustomerID, @Amount, @Amount, @DueDate, 0, 'Unpaid')", conn))
                    {
                        cmd.Parameters.AddWithValue("@CustomerID", custId);
                        cmd.Parameters.AddWithValue("@Amount", amount);
                        cmd.Parameters.AddWithValue("@DueDate", dueDate.ToString("yyyy-MM-dd"));

                        cmd.ExecuteNonQuery();
                        Console.WriteLine("Bill created successfully.");
                    }
                }
            }
            catch (Exception ex)
            {
                LogError("CreateBill", ex);
            }
        }

        public static void ViewBills()
        {
            try
            {
                DbInitializer.Initialize(ConnStr);
                using (SqliteConnection conn = new SqliteConnection(ConnStr))
                {
                    conn.Open();
                    using (SqliteCommand cmd = new SqliteCommand(@"
                        SELECT b.BillID, c.FullName, b.Amount, b.AmountDue, b.DueDate, b.IsPaid, b.Status
                        FROM Bills b
                        JOIN Customers c ON b.CustomerID = c.CustomerID
                        ORDER BY b.DueDate", conn))
                    {
                        using (SqliteDataReader reader = cmd.ExecuteReader())
                        {
                            Console.WriteLine($"\n{"Bill ID",-8} {"Customer",-25} {"Amount",-12} {"Amount Due",-12} {"Due Date",-12} {"Status",-12}");
                            Console.WriteLine(new string('-', 85));
                            while (reader.Read())
                            {
                                int billId = Convert.ToInt32(reader["BillID"]);
                                string name = reader["FullName"].ToString() ?? "";
                                decimal amount = Convert.ToDecimal(reader["Amount"]);
                                decimal due = Convert.ToDecimal(reader["AmountDue"]);
                                string dueDate = reader["DueDate"].ToString() ?? "";
                                string status = reader["Status"]?.ToString() ?? (Convert.ToInt32(reader["IsPaid"]) == 1 ? "Paid" : "Unpaid");

                                Console.WriteLine($"{billId,-8} {name,-25} ₦{amount,-11:N2} ₦{due,-11:N2} {dueDate,-12} {status,-12}");
                            }
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                LogError("ViewBills", ex);
            }
        }

        public static void RecordPayment()
        {
            Console.Write("Bill ID: ");
            if (!int.TryParse(Console.ReadLine(), out int billId))
            {
                Console.WriteLine("Invalid Bill ID.");
                return;
            }
            Console.Write("Amount Paid: ");
            if (!decimal.TryParse(Console.ReadLine(), out decimal amountPaid))
            {
                Console.WriteLine("Invalid Amount.");
                return;
            }

            try
            {
                DbInitializer.Initialize(ConnStr);
                using (SqliteConnection conn = new SqliteConnection(ConnStr))
                {
                    conn.Open();
                    using (SqliteTransaction tx = conn.BeginTransaction())
                    {
                        using (SqliteCommand cmd = new SqliteCommand(@"
                            INSERT INTO Payments (BillID, AmountPaid, PaymentMethod) VALUES (@BillID, @AmountPaid, 'Cash');
                            UPDATE Bills SET AmountDue = MAX(0, AmountDue - @AmountPaid),
                                             IsPaid = CASE WHEN AmountDue - @AmountPaid <= 0 THEN 1 ELSE 0 END,
                                             Status = CASE WHEN AmountDue - @AmountPaid <= 0 THEN 'Paid' ELSE 'PartiallyPaid' END
                            WHERE BillID = @BillID;", conn, tx))
                        {
                            cmd.Parameters.AddWithValue("@BillID", billId);
                            cmd.Parameters.AddWithValue("@AmountPaid", amountPaid);

                            cmd.ExecuteNonQuery();
                        }
                        tx.Commit();
                        Console.WriteLine("Payment recorded successfully.");
                    }
                }
            }
            catch (Exception ex)
            {
                LogError("RecordPayment", ex);
            }
        }

        public static void ViewSummaryReport()
        {
            try
            {
                DbInitializer.Initialize(ConnStr);
                using (SqliteConnection conn = new SqliteConnection(ConnStr))
                {
                    conn.Open();
                    using (SqliteCommand cmd = new SqliteCommand(@"
                        SELECT
                            (SELECT COUNT(*) FROM Customers) AS TotalCustomers,
                            (SELECT COUNT(*) FROM Bills) AS TotalBills,
                            (SELECT COUNT(*) FROM Bills WHERE IsPaid = 0) AS UnpaidBills,
                            (SELECT COALESCE(SUM(Amount), 0) FROM Bills) AS TotalBilled,
                            (SELECT COALESCE(SUM(AmountPaid), 0) FROM Payments) AS TotalCollected,
                            (SELECT COALESCE(SUM(AmountDue), 0) FROM Bills WHERE IsPaid = 0) AS Outstanding", conn))
                    {
                        using (SqliteDataReader reader = cmd.ExecuteReader())
                        {
                            if (reader.Read())
                            {
                                Console.WriteLine("\n=== SYSTEM SUMMARY REPORT ===");
                                Console.WriteLine($"Total Customers:    {reader["TotalCustomers"]}");
                                Console.WriteLine($"Total Bills:        {reader["TotalBills"]}");
                                Console.WriteLine($"Unpaid Bills:       {reader["UnpaidBills"]}");
                                Console.WriteLine($"Total Billed:       ₦{Convert.ToDecimal(reader["TotalBilled"]):N2}");
                                Console.WriteLine($"Total Collected:    ₦{Convert.ToDecimal(reader["TotalCollected"]):N2}");
                                Console.WriteLine($"Total Outstanding:  ₦{Convert.ToDecimal(reader["Outstanding"]):N2}");
                                Console.WriteLine("=============================");
                            }
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                LogError("ViewSummaryReport", ex);
            }
        }

        public static void ShowMenu()
        {
            Console.WriteLine("\n=== Mini Billing System ===");
            Console.WriteLine("1.  Add Customer");
            Console.WriteLine("2.  View All Customers");
            Console.WriteLine("3.  Search Customer by Name");
            Console.WriteLine("4.  Update Customer");
            Console.WriteLine("5.  Delete Customer");
            Console.WriteLine("6.  Create Bill");
            Console.WriteLine("7.  View All Bills");
            Console.WriteLine("8.  Record Payment");
            Console.WriteLine("9.  View Summary Report");
            Console.WriteLine("10. Exit");
            Console.Write("Choose an option: ");
        }

        public static void Run(string[] args)
        {
            Console.WriteLine("Starting Mini Customer Billing System Console Application...");
            bool exit = false;
            while (!exit)
            {
                ShowMenu();
                string? input = Console.ReadLine();
                switch (input?.Trim())
                {
                    case "1": AddCustomer(); break;
                    case "2": ViewCustomers(); break;
                    case "3": SearchCustomerByName(); break;
                    case "4": UpdateCustomer(); break;
                    case "5": DeleteCustomer(); break;
                    case "6": CreateBill(); break;
                    case "7": ViewBills(); break;
                    case "8": RecordPayment(); break;
                    case "9": ViewSummaryReport(); break;
                    case "10":
                    case "exit":
                        exit = true;
                        Console.WriteLine("Exiting Mini Billing System. Goodbye!");
                        break;
                    default:
                        Console.WriteLine("Invalid option. Please enter a number between 1 and 10.");
                        break;
                }
            }
        }
    }
}
