USE InternBillingDB;
GO

-- Ensure Amount and Status columns exist on Bills table
IF NOT EXISTS (SELECT * FROM sys.columns WHERE object_id = OBJECT_ID('Bills') AND name = 'Amount')
BEGIN
    ALTER TABLE Bills ADD Amount DECIMAL(10,2) NULL;
END
GO

IF NOT EXISTS (SELECT * FROM sys.columns WHERE object_id = OBJECT_ID('Bills') AND name = 'Status')
BEGIN
    ALTER TABLE Bills ADD Status VARCHAR(20) NULL;
END
GO

-- Add default constraint for AmountDue if needed so INSERT without AmountDue succeeds
IF NOT EXISTS (SELECT * FROM sys.default_constraints WHERE name = 'DF_Bills_AmountDue')
BEGIN
    ALTER TABLE Bills ADD CONSTRAINT DF_Bills_AmountDue DEFAULT 0.00 FOR AmountDue;
END
GO

-- Backfill data for existing rows
UPDATE Bills SET Amount = AmountDue WHERE Amount IS NULL;
UPDATE Bills SET Status = CASE WHEN IsPaid = 1 THEN 'Paid' ELSE 'Unpaid' END WHERE Status IS NULL;
GO

-- 1. Create usp_AddCustomer procedure
IF OBJECT_ID('usp_AddCustomer', 'P') IS NOT NULL DROP PROCEDURE usp_AddCustomer;
GO
CREATE PROCEDURE usp_AddCustomer
    @FullName NVARCHAR(100),
    @Address NVARCHAR(200),
    @PhoneNumber NVARCHAR(20),
    @Email NVARCHAR(100)
AS
BEGIN
    INSERT INTO Customers (FullName, Address, PhoneNumber, Email)
    VALUES (@FullName, @Address, @PhoneNumber, @Email)
END
GO

-- 2. Create usp_GetAllCustomers procedure
IF OBJECT_ID('usp_GetAllCustomers', 'P') IS NOT NULL DROP PROCEDURE usp_GetAllCustomers;
GO
CREATE PROCEDURE usp_GetAllCustomers
AS
BEGIN
    SELECT CustomerID, FullName, PhoneNumber, Email FROM Customers
END
GO

-- 3. Create usp_CreateBill procedure
IF OBJECT_ID('usp_CreateBill', 'P') IS NOT NULL DROP PROCEDURE usp_CreateBill;
GO
CREATE PROCEDURE usp_CreateBill
    @CustomerID INT,
    @Amount DECIMAL(10,2),
    @DueDate DATE
AS
BEGIN
    INSERT INTO Bills (CustomerID, Amount, DueDate, Status)
    VALUES (@CustomerID, @Amount, @DueDate, 'Unpaid')
END
GO

-- 4. Create usp_GetAllBillsWithCustomer procedure
IF OBJECT_ID('usp_GetAllBillsWithCustomer', 'P') IS NOT NULL DROP PROCEDURE usp_GetAllBillsWithCustomer;
GO
CREATE PROCEDURE usp_GetAllBillsWithCustomer
AS
BEGIN
    SELECT b.BillID, c.FullName, b.Amount, b.DueDate, b.Status
    FROM Bills b
    INNER JOIN Customers c ON b.CustomerID = c.CustomerID
    ORDER BY b.DueDate
END
GO

-- 5. Create usp_RecordPayment procedure
IF OBJECT_ID('usp_RecordPayment', 'P') IS NOT NULL DROP PROCEDURE usp_RecordPayment;
GO
CREATE PROCEDURE usp_RecordPayment
    @BillID INT,
    @AmountPaid DECIMAL(10,2)
AS
BEGIN
    INSERT INTO Payments (BillID, AmountPaid, PaymentDate)
    VALUES (@BillID, @AmountPaid, GETDATE())

    UPDATE Bills SET Status = 'Paid' WHERE BillID = @BillID
END
GO

-- 6. Create usp_GetSummaryReport procedure (including stretch goal UnpaidBills count)
IF OBJECT_ID('usp_GetSummaryReport', 'P') IS NOT NULL DROP PROCEDURE usp_GetSummaryReport;
GO
CREATE PROCEDURE usp_GetSummaryReport
AS
BEGIN
    SELECT
        (SELECT COUNT(*) FROM Customers)                              AS TotalCustomers,
        (SELECT COUNT(*) FROM Bills)                                  AS TotalBills,
        (SELECT COUNT(*) FROM Bills WHERE Status = 'Unpaid')          AS UnpaidBills,
        (SELECT ISNULL(SUM(Amount), 0) FROM Bills)                    AS TotalBilled,
        (SELECT ISNULL(SUM(AmountPaid), 0) FROM Payments)             AS TotalCollected,
        (SELECT ISNULL(SUM(Amount), 0) FROM Bills WHERE Status = 'Unpaid') AS Outstanding
END
GO
