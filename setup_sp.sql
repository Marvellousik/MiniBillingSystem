USE InternBillingDB;
GO

-- SCHEMA DEFINITION & DOCUMENTATION:
-- Amount: Original total bill amount set at generation (never changes after creation).
-- AmountDue: Current remaining balance owed on the bill (decremented by payments).
-- IsPaid: Bit flag indicating whether the bill is fully paid (1 = Paid, 0 = Unpaid or PartiallyPaid).
-- Status: Canonical status string ('Unpaid', 'PartiallyPaid', 'Paid'). Kept strictly in sync with IsPaid.

-- 1. Customers Table
IF OBJECT_ID('Customers', 'U') IS NULL
BEGIN
    CREATE TABLE Customers (
        CustomerID INT IDENTITY(1,1) PRIMARY KEY,
        FullName NVARCHAR(100) NOT NULL,
        Address NVARCHAR(255) NOT NULL,
        PhoneNumber NVARCHAR(20) NOT NULL,
        Email NVARCHAR(100) NOT NULL CONSTRAINT UQ_Customers_Email UNIQUE,
        AccountBalance DECIMAL(10,2) NOT NULL DEFAULT 0.00
    );
END
GO

-- Ensure Email unique constraint exists
IF NOT EXISTS (SELECT * FROM sys.key_constraints WHERE name = 'UQ_Customers_Email') AND
   NOT EXISTS (SELECT * FROM sys.indexes WHERE name = 'UQ_Customers_Email')
BEGIN
    ALTER TABLE Customers ADD CONSTRAINT UQ_Customers_Email UNIQUE (Email);
END
GO

-- 2. Bills Table
IF OBJECT_ID('Bills', 'U') IS NULL
BEGIN
    CREATE TABLE Bills (
        BillID INT IDENTITY(1,1) PRIMARY KEY,
        CustomerID INT NOT NULL FOREIGN KEY REFERENCES Customers(CustomerID),
        Amount DECIMAL(10,2) NOT NULL CONSTRAINT CK_Bills_Amount CHECK (Amount >= 0),
        AmountDue DECIMAL(10,2) NOT NULL CONSTRAINT CK_Bills_AmountDue CHECK (AmountDue >= 0),
        DueDate DATE NOT NULL,
        IsPaid BIT NOT NULL DEFAULT 0,
        Status VARCHAR(20) NOT NULL DEFAULT 'Unpaid' CONSTRAINT CK_Bills_Status CHECK (Status IN ('Unpaid','PartiallyPaid','Paid'))
    );
END
GO

-- Ensure Amount and Status columns exist
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

-- Add constraints if missing
IF NOT EXISTS (SELECT * FROM sys.check_constraints WHERE name = 'CK_Bills_Amount')
BEGIN
    ALTER TABLE Bills ADD CONSTRAINT CK_Bills_Amount CHECK (Amount >= 0);
END
GO

IF NOT EXISTS (SELECT * FROM sys.check_constraints WHERE name = 'CK_Bills_AmountDue')
BEGIN
    ALTER TABLE Bills ADD CONSTRAINT CK_Bills_AmountDue CHECK (AmountDue >= 0);
END
GO

IF NOT EXISTS (SELECT * FROM sys.check_constraints WHERE name = 'CK_Bills_Status')
BEGIN
    -- Drop old constraint if present to allow 'PartiallyPaid'
    ALTER TABLE Bills ADD CONSTRAINT CK_Bills_Status CHECK (Status IN ('Unpaid','PartiallyPaid','Paid'));
END
GO

-- Backfill Amount and Status for consistency
UPDATE Bills SET Amount = AmountDue WHERE Amount IS NULL;
UPDATE Bills SET Status = CASE WHEN IsPaid = 1 THEN 'Paid' WHEN AmountDue < Amount AND AmountDue > 0 THEN 'PartiallyPaid' ELSE 'Unpaid' END WHERE Status IS NULL;
UPDATE Bills SET IsPaid = CASE WHEN Status = 'Paid' THEN 1 ELSE 0 END;
GO

-- 3. Payments Table
IF OBJECT_ID('Payments', 'U') IS NULL
BEGIN
    CREATE TABLE Payments (
        PaymentID INT IDENTITY(1,1) PRIMARY KEY,
        BillID INT NOT NULL FOREIGN KEY REFERENCES Bills(BillID),
        AmountPaid DECIMAL(10,2) NOT NULL CONSTRAINT CK_Payments_AmountPaid CHECK (AmountPaid > 0),
        PaymentDate DATETIME NOT NULL DEFAULT GETDATE(),
        PaymentMethod VARCHAR(50) NOT NULL DEFAULT 'Cash'
    );
END
GO

IF NOT EXISTS (SELECT * FROM sys.check_constraints WHERE name = 'CK_Payments_AmountPaid')
BEGIN
    ALTER TABLE Payments ADD CONSTRAINT CK_Payments_AmountPaid CHECK (AmountPaid > 0);
END
GO

-- 4. Stored Procedures
-- usp_AddCustomer
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

-- usp_GetAllCustomers
IF OBJECT_ID('usp_GetAllCustomers', 'P') IS NOT NULL DROP PROCEDURE usp_GetAllCustomers;
GO
CREATE PROCEDURE usp_GetAllCustomers
AS
BEGIN
    SELECT CustomerID, FullName, Address, PhoneNumber, Email, AccountBalance FROM Customers
END
GO

-- usp_UpdateCustomer
IF OBJECT_ID('usp_UpdateCustomer', 'P') IS NOT NULL DROP PROCEDURE usp_UpdateCustomer;
GO
CREATE PROCEDURE usp_UpdateCustomer
    @CustomerID INT,
    @FullName NVARCHAR(100),
    @Address NVARCHAR(200),
    @PhoneNumber NVARCHAR(20),
    @Email NVARCHAR(100)
AS
BEGIN
    UPDATE Customers
    SET FullName = @FullName, Address = @Address,
        PhoneNumber = @PhoneNumber, Email = @Email
    WHERE CustomerID = @CustomerID
END
GO

-- usp_DeleteCustomer
IF OBJECT_ID('usp_DeleteCustomer', 'P') IS NOT NULL DROP PROCEDURE usp_DeleteCustomer;
GO
CREATE PROCEDURE usp_DeleteCustomer
    @CustomerID INT
AS
BEGIN
    DELETE FROM Customers WHERE CustomerID = @CustomerID
END
GO

-- usp_CreateBill (Fix 1: Sets Amount and AmountDue equal to @Amount)
IF OBJECT_ID('usp_CreateBill', 'P') IS NOT NULL DROP PROCEDURE usp_CreateBill;
GO
CREATE PROCEDURE usp_CreateBill
    @CustomerID INT,
    @Amount DECIMAL(10,2),
    @DueDate DATE
AS
BEGIN
    INSERT INTO Bills (CustomerID, Amount, AmountDue, DueDate, IsPaid, Status)
    VALUES (@CustomerID, @Amount, @Amount, @DueDate, 0, 'Unpaid')
END
GO

-- usp_GetAllBillsWithCustomer
IF OBJECT_ID('usp_GetAllBillsWithCustomer', 'P') IS NOT NULL DROP PROCEDURE usp_GetAllBillsWithCustomer;
GO
CREATE PROCEDURE usp_GetAllBillsWithCustomer
AS
BEGIN
    SELECT b.BillID, c.FullName, b.Amount, b.AmountDue, b.DueDate, b.IsPaid, b.Status
    FROM Bills b
    INNER JOIN Customers c ON b.CustomerID = c.CustomerID
    ORDER BY b.DueDate
END
GO

-- usp_RecordPayment (Fix 2 & 3: Updates AmountDue, IsPaid, and Status in sync with partial payment support)
IF OBJECT_ID('usp_RecordPayment', 'P') IS NOT NULL DROP PROCEDURE usp_RecordPayment;
GO
CREATE PROCEDURE usp_RecordPayment
    @BillID INT,
    @AmountPaid DECIMAL(10,2)
AS
BEGIN
    INSERT INTO Payments (BillID, AmountPaid, PaymentDate)
    VALUES (@BillID, @AmountPaid, GETDATE());

    UPDATE Bills 
    SET AmountDue = CASE WHEN AmountDue - @AmountPaid < 0 THEN 0 ELSE AmountDue - @AmountPaid END,
        IsPaid = CASE WHEN AmountDue - @AmountPaid <= 0 THEN 1 ELSE 0 END,
        Status = CASE WHEN AmountDue - @AmountPaid <= 0 THEN 'Paid' ELSE 'PartiallyPaid' END
    WHERE BillID = @BillID;
END
GO

-- usp_GetSummaryReport
IF OBJECT_ID('usp_GetSummaryReport', 'P') IS NOT NULL DROP PROCEDURE usp_GetSummaryReport;
GO
CREATE PROCEDURE usp_GetSummaryReport
AS
BEGIN
    SELECT
        (SELECT COUNT(*) FROM Customers)                                        AS TotalCustomers,
        (SELECT COUNT(*) FROM Bills)                                            AS TotalBills,
        (SELECT COUNT(*) FROM Bills WHERE Status IN ('Unpaid', 'PartiallyPaid')) AS UnpaidBills,
        (SELECT ISNULL(SUM(Amount), 0) FROM Bills)                              AS TotalBilled,
        (SELECT ISNULL(SUM(AmountPaid), 0) FROM Payments)                       AS TotalCollected,
        (SELECT ISNULL(SUM(AmountDue), 0) FROM Bills WHERE IsPaid = 0)          AS Outstanding
END
GO
