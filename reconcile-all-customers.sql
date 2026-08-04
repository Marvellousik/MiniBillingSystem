USE InternBillingDB;
GO

PRINT 'Starting auto-reconciliation of existing account credits against unpaid bills...';

DECLARE @CustomerID INT, @Credit DECIMAL(10,2);
DECLARE @BillID INT, @AmountDue DECIMAL(10,2);
DECLARE @SettleAmount DECIMAL(10,2);

-- Cursor for customers with positive credit and unpaid bills
DECLARE customer_cursor CURSOR FOR 
SELECT DISTINCT c.CustomerID, c.AccountBalance 
FROM Customers c
JOIN Bills b ON c.CustomerID = b.CustomerID
WHERE c.AccountBalance > 0 AND b.IsPaid = 0;

OPEN customer_cursor;
FETCH NEXT FROM customer_cursor INTO @CustomerID, @Credit;

WHILE @@FETCH_STATUS = 0
BEGIN
    DECLARE bill_cursor CURSOR FOR 
    SELECT BillID, AmountDue 
    FROM Bills 
    WHERE CustomerID = @CustomerID AND IsPaid = 0 
    ORDER BY DueDate ASC, BillID ASC;

    OPEN bill_cursor;
    FETCH NEXT FROM bill_cursor INTO @BillID, @AmountDue;

    WHILE @@FETCH_STATUS = 0 AND @Credit > 0
    BEGIN
        IF @Credit >= @AmountDue
        BEGIN
            SET @SettleAmount = @AmountDue;
            SET @Credit = @Credit - @SettleAmount;

            UPDATE Bills SET IsPaid = 1, AmountDue = 0 WHERE BillID = @BillID;
            INSERT INTO Payments (BillID, AmountPaid, PaymentDate, PaymentMethod) 
            VALUES (@BillID, @SettleAmount, GETDATE(), 'Account Credit Auto-Reconciliation');
        END
        ELSE
        BEGIN
            SET @SettleAmount = @Credit;
            UPDATE Bills SET AmountDue = AmountDue - @SettleAmount WHERE BillID = @BillID;
            INSERT INTO Payments (BillID, AmountPaid, PaymentDate, PaymentMethod) 
            VALUES (@BillID, @SettleAmount, GETDATE(), 'Account Credit Auto-Reconciliation');
            SET @Credit = 0;
        END

        FETCH NEXT FROM bill_cursor INTO @BillID, @AmountDue;
    END

    CLOSE bill_cursor;
    DEALLOCATE bill_cursor;

    UPDATE Customers SET AccountBalance = @Credit WHERE CustomerID = @CustomerID;

    FETCH NEXT FROM customer_cursor INTO @CustomerID, @Credit;
END

CLOSE customer_cursor;
DEALLOCATE customer_cursor;

PRINT 'Auto-reconciliation completed successfully!';
GO
