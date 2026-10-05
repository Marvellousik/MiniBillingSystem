import { createRequire } from 'node:module';
const require = createRequire(import.meta.url);
const { getDb, query, get, run } = require('./_db.cjs');

export default async function handler(req, res) {
  const db = await getDb();

  if (req.method === 'GET') {
    try {
      let page = parseInt(req.query.page || '1', 10);
      let pageSize = parseInt(req.query.pageSize || '50', 10);
      const sortBy = (req.query.sortBy || 'recent_created').toLowerCase();

      if (isNaN(page) || page < 1) page = 1;
      if (isNaN(pageSize) || pageSize < 1 || pageSize > 100) pageSize = 50;
      const offset = (page - 1) * pageSize;

      let orderByClause = 'ORDER BY b.BillID DESC';
      if (sortBy === 'recent_activity') {
        orderByClause = 'ORDER BY COALESCE((SELECT MAX(p.PaymentDate) FROM Payments p WHERE p.BillID = b.BillID), b.DueDate) DESC, b.BillID DESC';
      } else if (sortBy === 'due_date') {
        orderByClause = 'ORDER BY b.DueDate ASC, b.BillID DESC';
      }

      const countRow = get(db, 'SELECT COUNT(*) as total FROM Bills');
      const totalCount = Number(countRow?.total || 0);

      const sql = `
        SELECT b.BillID, b.CustomerID, c.FullName, b.Amount, b.AmountDue, b.DueDate, b.IsPaid, b.Status 
        FROM Bills b 
        JOIN Customers c ON b.CustomerID = c.CustomerID
        ${orderByClause} LIMIT :pageSize OFFSET :offset
      `;

      const rows = query(db, sql, { ':pageSize': pageSize, ':offset': offset });
      const data = rows.map(r => ({
        billID: Number(r.BillID),
        customerID: Number(r.CustomerID),
        customerName: r.FullName || '',
        fullName: r.FullName || '',
        amount: Number(r.Amount),
        amountDue: Number(r.AmountDue),
        dueDate: r.DueDate || '',
        isPaid: Number(r.IsPaid) === 1,
        status: r.Status || 'Unpaid'
      }));

      return res.status(200).json({
        data,
        page,
        pageSize,
        totalCount,
        sortBy
      });
    } catch (err) {
      console.error('Error fetching bills:', err);
      return res.status(500).json({ error: err.message || "Oops! We couldn't load the bills list right now." });
    }
  }

  if (req.method === 'POST') {
    try {
      const body = req.body || {};
      const customerID = parseInt(body.customerID || body.CustomerID, 10);
      const amount = parseFloat(body.amount || body.Amount);
      const dueDate = (body.dueDate || body.DueDate || '').trim();

      if (isNaN(customerID) || customerID <= 0) {
        return res.status(400).json({ error: 'Please select a valid customer.' });
      }

      const customer = get(db, 'SELECT CustomerID FROM Customers WHERE CustomerID = :id', { ':id': customerID });
      if (!customer) {
        return res.status(404).json({ error: "Customer doesn't exist. Please select a valid customer." });
      }

      if (isNaN(amount) || amount <= 0) {
        return res.status(400).json({ error: 'Bill amount must be greater than zero.' });
      }

      if (!dueDate) {
        return res.status(400).json({ error: 'Due date is required.' });
      }

      const { lastInsertRowid } = run(db, `
        INSERT INTO Bills (CustomerID, Amount, AmountDue, DueDate, IsPaid, Status)
        VALUES (:customerID, :amount, :amountDue, :dueDate, 0, 'Unpaid')
      `, {
        ':customerID': customerID,
        ':amount': amount,
        ':amountDue': amount,
        ':dueDate': dueDate
      });

      return res.status(200).json({
        message: 'Bill generated successfully.',
        billID: lastInsertRowid
      });
    } catch (err) {
      console.error('Error creating bill:', err);
      return res.status(500).json({ error: err.message || 'An unexpected issue occurred while generating the bill.' });
    }
  }

  return res.status(405).json({ error: 'Method Not Allowed' });
}
