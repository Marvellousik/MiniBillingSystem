const { getDb, query, get, run } = require('../_db.js');

module.exports = async function handler(req, res) {
  const db = await getDb();
  const id = parseInt(req.query.id, 10);

  if (isNaN(id) || id <= 0) {
    return res.status(400).json({ error: 'Invalid Customer ID.' });
  }

  if (req.method === 'GET') {
    try {
      const row = get(db, `
        SELECT c.CustomerID, c.FullName, c.Address, c.PhoneNumber, c.Email,
               c.AccountBalance,
               COALESCE((SELECT SUM(b.AmountDue) FROM Bills b WHERE b.CustomerID = c.CustomerID AND b.IsPaid = 0), 0) as TotalOwed
        FROM Customers c
        WHERE c.CustomerID = :id
      `, { ':id': id });

      if (!row) {
        return res.status(404).json({ error: `We couldn't find a customer with ID '${id}'.` });
      }

      const billsRows = query(db, 'SELECT BillID, Amount, AmountDue, DueDate, IsPaid, Status FROM Bills WHERE CustomerID = :id ORDER BY BillID DESC', { ':id': id });
      const bills = billsRows.map(b => ({
        billID: Number(b.BillID),
        customerID: id,
        fullName: row.FullName,
        amount: Number(b.Amount || 0),
        amountDue: Number(b.AmountDue || 0),
        dueDate: b.DueDate || '',
        isPaid: Number(b.IsPaid) === 1,
        status: b.Status || 'Unpaid'
      }));

      const paymentsRows = query(db, `
        SELECT p.PaymentID, p.BillID, p.AmountPaid, p.PaymentDate, p.PaymentMethod 
        FROM Payments p 
        JOIN Bills b ON p.BillID = b.BillID 
        WHERE b.CustomerID = :id 
        ORDER BY p.PaymentID DESC
      `, { ':id': id });

      const payments = paymentsRows.map(p => ({
        paymentID: Number(p.PaymentID),
        billID: Number(p.BillID),
        customerID: id,
        fullName: row.FullName,
        amountPaid: Number(p.AmountPaid),
        paymentDate: p.PaymentDate || '',
        paymentMethod: p.PaymentMethod || ''
      }));

      return res.status(200).json({
        customerID: id,
        fullName: row.FullName,
        address: row.Address,
        phoneNumber: row.PhoneNumber,
        email: row.Email,
        accountBalance: Number(row.AccountBalance),
        totalOwed: Number(row.TotalOwed),
        bills,
        payments
      });
    } catch (err) {
      console.error('Error fetching customer by id:', err);
      return res.status(500).json({ error: "Sorry, we ran into an issue pulling up this customer's dashboard." });
    }
  }

  if (req.method === 'PUT') {
    try {
      const body = req.body || {};
      const fullName = (body.fullName || body.FullName || '').trim();
      const address = (body.address || body.Address || '').trim();
      const phoneNumber = (body.phoneNumber || body.PhoneNumber || '').trim();
      const email = (body.email || body.Email || '').trim();

      if (!fullName) return res.status(400).json({ error: 'Full Name cannot be left blank.' });
      if (!address) return res.status(400).json({ error: 'Address cannot be left blank.' });
      if (!phoneNumber) return res.status(400).json({ error: 'Phone Number cannot be left blank.' });
      if (!email) return res.status(400).json({ error: 'Email cannot be left blank.' });

      const existing = get(db, 'SELECT CustomerID FROM Customers WHERE CustomerID = :id', { ':id': id });
      if (!existing) {
        return res.status(404).json({ error: `We couldn't find a customer with ID '${id}'.` });
      }

      const dupEmail = get(db, 'SELECT CustomerID FROM Customers WHERE LOWER(Email) = LOWER(:email) AND CustomerID != :id', { ':email': email, ':id': id });
      if (dupEmail) {
        return res.status(400).json({ error: 'Another customer is already using this email address.' });
      }

      run(db, `
        UPDATE Customers
        SET FullName = :name, Address = :address, PhoneNumber = :phone, Email = :email
        WHERE CustomerID = :id
      `, {
        ':name': fullName,
        ':address': address,
        ':phone': phoneNumber,
        ':email': email,
        ':id': id
      });

      return res.status(200).json({ message: 'Customer updated successfully.' });
    } catch (err) {
      console.error('Error updating customer:', err);
      return res.status(500).json({ error: 'Failed to update customer.' });
    }
  }

  if (req.method === 'DELETE') {
    try {
      const existing = get(db, 'SELECT CustomerID FROM Customers WHERE CustomerID = :id', { ':id': id });
      if (!existing) {
        return res.status(404).json({ error: `Customer not found.` });
      }

      const unpaidBills = get(db, 'SELECT COUNT(*) as total FROM Bills WHERE CustomerID = :id AND IsPaid = 0', { ':id': id });
      if (Number(unpaidBills?.total || 0) > 0) {
        return res.status(400).json({ error: 'Cannot delete customer with active unpaid bills.' });
      }

      // Delete payments for customer's bills, then bills, then customer
      run(db, `
        DELETE FROM Payments WHERE BillID IN (SELECT BillID FROM Bills WHERE CustomerID = :id)
      `, { ':id': id });
      run(db, 'DELETE FROM Bills WHERE CustomerID = :id', { ':id': id });
      run(db, 'DELETE FROM Customers WHERE CustomerID = :id', { ':id': id });

      return res.status(200).json({ message: 'Customer deleted successfully.' });
    } catch (err) {
      console.error('Error deleting customer:', err);
      return res.status(500).json({ error: 'Failed to delete customer.' });
    }
  }

  return res.status(405).json({ error: 'Method Not Allowed' });
};
