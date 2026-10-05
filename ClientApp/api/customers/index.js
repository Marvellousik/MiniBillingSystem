const { getDb, query, get, run } = require('../_db.js');

module.exports = async function handler(req, res) {
  const db = await getDb();

  if (req.method === 'GET') {
    const id = req.query.id;
    if (id) {
      return handleGetCustomerById(db, id, res);
    }

    try {
      let page = parseInt(req.query.page || '1', 10);
      let pageSize = parseInt(req.query.pageSize || '50', 10);
      const sortBy = (req.query.sortBy || 'recent_created').toLowerCase();

      if (isNaN(page) || page < 1) page = 1;
      if (isNaN(pageSize) || pageSize < 1 || pageSize > 100) pageSize = 50;
      const offset = (page - 1) * pageSize;

      let orderByClause = 'ORDER BY c.CustomerID DESC';
      if (sortBy === 'name') {
        orderByClause = 'ORDER BY c.FullName ASC, c.CustomerID DESC';
      } else if (sortBy === 'highest_balance') {
        orderByClause = 'ORDER BY c.AccountBalance DESC, c.CustomerID DESC';
      }

      const countRow = get(db, 'SELECT COUNT(*) as total FROM Customers');
      const totalCount = Number(countRow?.total || 0);

      const sql = `
        SELECT c.CustomerID, c.FullName, c.Address, c.PhoneNumber, c.Email,
               c.AccountBalance as Credit,
               COALESCE((SELECT SUM(b.AmountDue) FROM Bills b WHERE b.CustomerID = c.CustomerID AND b.IsPaid = 0), 0) as TotalOwed
        FROM Customers c
        ${orderByClause} LIMIT :pageSize OFFSET :offset
      `;

      const rows = query(db, sql, { ':pageSize': pageSize, ':offset': offset });
      const data = rows.map(r => ({
        customerID: Number(r.CustomerID),
        fullName: r.FullName || '',
        address: r.Address || '',
        phoneNumber: r.PhoneNumber || '',
        email: r.Email || '',
        accountBalance: Number(r.Credit || 0),
        totalOwed: Number(r.TotalOwed || 0)
      }));

      return res.status(200).json({
        data,
        page,
        pageSize,
        totalCount,
        sortBy
      });
    } catch (err) {
      console.error('Error fetching customers:', err);
      return res.status(500).json({ error: "Oops! We couldn't load the customer list right now." });
    }
  }

  if (req.method === 'POST') {
    try {
      const body = req.body || {};
      const fullName = (body.fullName || body.FullName || '').trim();
      const address = (body.address || body.Address || '').trim();
      const phoneNumber = (body.phoneNumber || body.PhoneNumber || '').trim();
      const email = (body.email || body.Email || '').trim();
      const balance = parseFloat(body.accountBalance || body.AccountBalance || body.initialBalance || 0) || 0;

      if (!fullName) return res.status(400).json({ error: 'Full Name cannot be left blank.' });
      if (!address) return res.status(400).json({ error: 'Address cannot be left blank.' });
      if (!phoneNumber) return res.status(400).json({ error: 'Phone Number cannot be left blank.' });
      if (!email) return res.status(400).json({ error: 'Email cannot be left blank.' });

      const existing = get(db, 'SELECT CustomerID FROM Customers WHERE LOWER(Email) = LOWER(:email)', { ':email': email });
      if (existing) {
        return res.status(400).json({ error: 'A customer with this email address is already registered.' });
      }

      const { lastInsertRowid } = run(db, `
        INSERT INTO Customers (FullName, Address, PhoneNumber, Email, AccountBalance)
        VALUES (:name, :address, :phone, :email, :balance)
      `, {
        ':name': fullName,
        ':address': address,
        ':phone': phoneNumber,
        ':email': email,
        ':balance': balance
      });

      return res.status(201).json({
        message: 'Customer registered successfully.',
        customerID: lastInsertRowid
      });
    } catch (err) {
      console.error('Error creating customer:', err);
      return res.status(500).json({ error: 'Failed to create customer.' });
    }
  }

  return res.status(405).json({ error: 'Method Not Allowed' });
};

function handleGetCustomerById(db, id, res) {
  try {
    const custId = parseInt(id, 10);
    const row = get(db, `
      SELECT c.CustomerID, c.FullName, c.Address, c.PhoneNumber, c.Email,
             c.AccountBalance,
             COALESCE((SELECT SUM(b.AmountDue) FROM Bills b WHERE b.CustomerID = c.CustomerID AND b.IsPaid = 0), 0) as TotalOwed
      FROM Customers c
      WHERE c.CustomerID = :id
    `, { ':id': custId });

    if (!row) {
      return res.status(404).json({ error: `We couldn't find a customer with ID '${id}'.` });
    }

    const billsRows = query(db, 'SELECT BillID, Amount, AmountDue, DueDate, IsPaid, Status FROM Bills WHERE CustomerID = :id ORDER BY BillID DESC', { ':id': custId });
    const bills = billsRows.map(b => ({
      billID: Number(b.BillID),
      customerID: custId,
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
    `, { ':id': custId });

    const payments = paymentsRows.map(p => ({
      paymentID: Number(p.PaymentID),
      billID: Number(p.BillID),
      customerID: custId,
      fullName: row.FullName,
      amountPaid: Number(p.AmountPaid),
      paymentDate: p.PaymentDate || '',
      paymentMethod: p.PaymentMethod || ''
    }));

    return res.status(200).json({
      customerID: custId,
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
