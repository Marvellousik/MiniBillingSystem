USE InternBillingDB;
GO

-- 1. Clear existing data
DELETE FROM Payments;
DELETE FROM Bills;
DELETE FROM Customers;
GO

-- 2. Insert Authentic AMAC Customers with explicit IDs
SET IDENTITY_INSERT Customers ON;

INSERT INTO Customers (CustomerID, FullName, Address, PhoneNumber, Email, AccountBalance) VALUES
(1, N'Emeka Okafor', N'Plot 142, Aminu Kano Crescent, Wuse II, Abuja', N'08033124590', N'emeka.okafor@gmail.com', 5000.00),
(2, N'Hauwa Mohammed Bello', N'No. 8, Crescent 4, Kado Estate, Abuja', N'08124567891', N'hmbello@yahoo.com', 0.00),
(3, N'Adebayo Chukwuma Adeleke', N'Block 12, Flat 3, FHA Estate, Gwarinpa, Abuja', N'07061234567', N'bayo.adeleke@outlook.com', 12500.50),
(4, N'Fatima Garba Aliyu', N'Shop 15, Area 1 Shopping Complex, Garki, Abuja', N'08059876543', N'fatima.aliyu@garki-biz.ng', 0.00),
(5, N'Chidiebere Nnamdi Okonkwo', N'No. 24, Obafemi Awolowo Way, Jabi, Abuja', N'08187654321', N'c.okonkwo@gmail.com', 2500.00),
(6, N'Aisha Ibrahim Danjuma', N'Suite B12, Banex Plaza, Wuse 2, Abuja', N'08023456789', N'aisha.danjuma@hotmail.com', 0.00),
(7, N'Babatunde Olumide Johnson', N'House 7, 3rd Avenue, Model City, Lugbe, Abuja', N'08139988776', N'babs.johnson@gmail.com', 0.00),
(8, N'Nkechi Blessing Ezekwesili', N'Plot 402, Cadastral Zone B06, Mabushi, Abuja', N'07031122334', N'nkechi.blessing@yahoo.com', 7500.00),
(9, N'Umar Farouq Yakubu', N'No. 5, Shehu Shagari Way, Maitama, Abuja', N'08094455667', N'uf.yakubu@amaccouncil.gov.ng', 0.00),
(10, N'Grace Ifeoma Nwachukwu', N'Corner Shop 4, Karu Market Road, Karu, Abuja', N'08162233445', N'grace.nwachukwu@gmail.com', 1500.00);

SET IDENTITY_INSERT Customers OFF;
GO

-- 3. Insert Authentic AMAC Bills with explicit IDs
SET IDENTITY_INSERT Bills ON;

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

SET IDENTITY_INSERT Bills OFF;
GO

-- 4. Insert Payment Records with explicit IDs
SET IDENTITY_INSERT Payments ON;

INSERT INTO Payments (PaymentID, BillID, AmountPaid, PaymentDate, PaymentMethod) VALUES
(1, 1, 18500.00, '2026-06-12 10:14:22', 'Bank Transfer (GTBank)'),
(2, 3, 15000.00, '2026-05-28 14:30:05', 'POS Terminal'),
(3, 5, 35000.00, '2026-05-31 09:45:12', 'Remita Web Pay'),
(4, 6, 40000.00, '2026-07-01 16:20:00', 'Bank Transfer (Access Bank)'),
(5, 8, 16000.00, '2026-06-18 11:05:44', 'USSD (*737#)'),
(6, 12, 22000.00, '2026-06-08 15:10:30', 'Bank Transfer (Zenith Bank)');

SET IDENTITY_INSERT Payments OFF;
GO

PRINT 'Authentic AMAC seed data loaded successfully!';
GO
