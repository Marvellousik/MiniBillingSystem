import { createRequire } from 'node:module';
const require = createRequire(import.meta.url);
const { getDb, query, get, run } = require('./_db.cjs');

export default async function handler(req, res) {
  const db = await getDb();

  if (req.method === 'GET') {
    try {
      let page = parseInt(req.query.page || '1', 10);
      let pageSize = parseInt(req.query.pageSize || '50', 10);

      if (isNaN(page) || page < 1) page = 1;
      if (isNaN(pageSize) || pageSize < 1 || pageSize > 100) pageSize = 50;
      const offset = (page - 1) * pageSize;

      const countRow = get(db, 'SELECT COUNT(*) as total FROM Payments');
      const totalCount = Number(countRow?.total || 0);

      const sql = `
        SELECT p.PaymentID, c.FullName, b.BillID, b.CustomerID, p.AmountPaid, p.PaymentDate, p.PaymentMethod 
        FROM Payments p 
        JOIN Bills b ON p.BillID = b.BillID 
        JOIN Customers c ON b.CustomerID = c.CustomerID
        ORDER BY p.PaymentID DESC LIMIT :pageSize OFFSET :offset
      `;

      const rows = query(db, sql, { ':pageSize': pageSize, ':offset': offset });
      const data = rows.map(r => ({
        paymentID: Number(r.PaymentID),
        fullName: r.FullName || '',
        billID: Number(r.BillID),
        customerID: Number(r.CustomerID),
        amountPaid: Number(r.AmountPaid),
        paymentDate: r.PaymentDate || '',
        paymentMethod: r.PaymentMethod || 'Cash'
      }));

      return res.status(200).json({
        data,
        page,
        pageSize,
        totalCount
      });
    } catch (err) {
      console.error('Error fetching payments:', err);
      return res.status(500).json({ error: err.message || "Oops! We couldn't load the payment history right now." });
    }
  }

  if (req.method === 'POST') {
    try {
      const body = req.body || {};
      const billID = parseInt(body.billID || body.BillID, 10);
      const amountPaid = parseFloat(body.amountPaid || body.AmountPaid);
      const paymentMethod = (body.paymentMethod || body.PaymentMethod || 'Cash').trim();

      if (isNaN(billID) || billID <= 0) {
        return res.status(400).json({ error: 'Please specify a valid bill.' });
      }

      if (isNaN(amountPaid) || amountPaid <= 0) {
        return res.status(400).json({ error: 'Payment amount must be greater than zero.' });
      }

      const bill = get(db, 'SELECT BillID, CustomerID, AmountDue, IsPaid FROM Bills WHERE BillID = :id', { ':id': billID });
      if (!bill) {
        return res.status(404).json({ error: "Bill not found." });
      }

      const currentDue = Number(bill.AmountDue);
      if (Number(bill.IsPaid) === 1 || currentDue <= 0) {
        return res.status(400).json({ error: 'This bill has already been settled in full.' });
      }

      if (amountPaid > currentDue) {
        return res.status(400).json({ error: `Payment amount (${amountPaid}) cannot exceed remaining balance (${currentDue}).` });
      }

      const { lastInsertRowid } = run(db, `
        INSERT INTO Payments (BillID, AmountPaid, PaymentDate, PaymentMethod)
        VALUES (:billID, :amountPaid, datetime('now'), :paymentMethod)
      `, {
        ':billID': billID,
        ':amountPaid': amountPaid,
        ':paymentMethod': paymentMethod || 'Cash'
      });

      const paymentID = lastInsertRowid;

      const newDue = Math.max(0, currentDue - amountPaid);
      const isPaid = newDue === 0 ? 1 : 0;
      const status = newDue === 0 ? 'Paid' : 'PartiallyPaid';

      run(db, `
        UPDATE Bills 
        SET AmountDue = :newDue, IsPaid = :isPaid, Status = :status
        WHERE BillID = :billID
      `, {
        ':newDue': newDue,
        ':isPaid': isPaid,
        ':status': status,
        ':billID': billID
      });

      return res.status(200).json({
        message: 'Payment processed successfully.',
        paymentID
      });
    } catch (err) {
      console.error('Error recording payment:', err);
      return res.status(500).json({ error: err.message || 'An issue occurred while recording the payment.' });
    }
  }

  return res.status(405).json({ error: 'Method Not Allowed' });
}
