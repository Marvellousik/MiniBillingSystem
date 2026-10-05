const { getDb, query } = require('./_db.js');

module.exports = async function handler(req, res) {
  try {
    const db = await getDb();
    const tables = query(db, "SELECT name FROM sqlite_master WHERE type='table' AND name NOT LIKE 'sqlite_%'");
    const tableNames = tables.map(t => t.name);

    return res.status(200).json({
      status: 'Healthy',
      engine: 'SQLite (Vercel Serverless)',
      tables: tableNames,
      timestamp: new Date().toISOString()
    });
  } catch (err) {
    return res.status(500).json({
      status: 'Unhealthy',
      error: err.message
    });
  }
};
