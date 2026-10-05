const { getDb, get } = require('./_db.js');

module.exports = async function handler(req, res) {
  if (req.method !== 'GET') {
    return res.status(405).json({ error: 'Method Not Allowed' });
  }

  try {
    const db = await getDb();

    const custRow = get(db, 'SELECT COUNT(*) as total FROM Customers');
    const outRow = get(db, 'SELECT COALESCE(SUM(AmountDue), 0) as total FROM Bills WHERE IsPaid = 0');
    const dueRow = get(db, `
      SELECT COUNT(*) as total FROM Bills 
      WHERE IsPaid = 0 AND date(DueDate) >= date('now') AND date(DueDate) <= date('now', '+7 days')
    `);
    const collRow = get(db, 'SELECT COALESCE(SUM(AmountPaid), 0) as total FROM Payments');

    return res.status(200).json({
      totalCustomers: Number(custRow?.total || 0),
      totalOutstanding: Number(outRow?.total || 0),
      billsDueThisWeek: Number(dueRow?.total || 0),
      totalCollected: Number(collRow?.total || 0)
    });
  } catch (err) {
    console.error('Error fetching summary:', err);
    return res.status(500).json({ error: 'Unable to load summary metrics right now.' });
  }
};
