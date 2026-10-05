const fs = require('fs');
const path = require('path');
const initSqlJs = require('sql.js');
const wasmBinary = require('./_wasm.cjs');

let dbInstance = null;
let SQL_MODULE = null;
const DB_PATH = process.env.SQLITE_DB_PATH || path.join('/tmp', 'billing.db');

const SEED_SQL = `
PRAGMA foreign_keys = ON;

CREATE TABLE IF NOT EXISTS Customers (
    CustomerID INTEGER PRIMARY KEY AUTOINCREMENT,
    FullName TEXT NOT NULL,
    Address TEXT NOT NULL,
    PhoneNumber TEXT NOT NULL,
    Email TEXT NOT NULL UNIQUE,
    AccountBalance NUMERIC NOT NULL DEFAULT 0.00
);

CREATE TABLE IF NOT EXISTS Bills (
    BillID INTEGER PRIMARY KEY AUTOINCREMENT,
    CustomerID INTEGER NOT NULL,
    Amount NUMERIC NOT NULL CHECK (Amount >= 0),
    AmountDue NUMERIC NOT NULL CHECK (AmountDue >= 0),
    DueDate TEXT NOT NULL,
    IsPaid INTEGER NOT NULL DEFAULT 0,
    Status TEXT NOT NULL DEFAULT 'Unpaid' CHECK (Status IN ('Unpaid','PartiallyPaid','Paid')),
    FOREIGN KEY (CustomerID) REFERENCES Customers(CustomerID)
);

CREATE TABLE IF NOT EXISTS Payments (
    PaymentID INTEGER PRIMARY KEY AUTOINCREMENT,
    BillID INTEGER NOT NULL,
    AmountPaid NUMERIC NOT NULL CHECK (AmountPaid > 0),
    PaymentDate TEXT NOT NULL DEFAULT (datetime('now')),
    PaymentMethod TEXT NOT NULL DEFAULT 'Cash',
    FOREIGN KEY (BillID) REFERENCES Bills(BillID)
);

INSERT OR IGNORE INTO Customers (CustomerID, FullName, Address, PhoneNumber, Email, AccountBalance) VALUES
(1, 'Emeka Okafor', 'Plot 142, Aminu Kano Crescent, Wuse II, Abuja', '08033124590', 'emeka.okafor@gmail.com', 5000.00),
(2, 'Hauwa Mohammed Bello', 'No. 8, Crescent 4, Kado Estate, Abuja', '08124567891', 'hmbello@yahoo.com', 0.00),
(3, 'Adebayo Chukwuma Adeleke', 'Block 12, Flat 3, FHA Estate, Gwarinpa, Abuja', '07061234567', 'bayo.adeleke@outlook.com', 12500.50),
(4, 'Fatima Garba Aliyu', 'Shop 15, Area 1 Shopping Complex, Garki, Abuja', '08059876543', 'fatima.aliyu@garki-biz.ng', 0.00),
(5, 'Chidiebere Nnamdi Okonkwo', 'No. 24, Obafemi Awolowo Way, Jabi, Abuja', '08187654321', 'c.okonkwo@gmail.com', 2500.00),
(6, 'Aisha Ibrahim Danjuma', 'Suite B12, Banex Plaza, Wuse 2, Abuja', '08023456789', 'aisha.danjuma@hotmail.com', 0.00),
(7, 'Babatunde Olumide Johnson', 'House 7, 3rd Avenue, Model City, Lugbe, Abuja', '08139988776', 'babs.johnson@gmail.com', 0.00),
(8, 'Nkechi Blessing Ezekwesili', 'Plot 402, Cadastral Zone B06, Mabushi, Abuja', '07031122334', 'nkechi.blessing@yahoo.com', 7500.00),
(9, 'Umar Farouq Yakubu', 'No. 5, Shehu Shagari Way, Maitama, Abuja', '08094455667', 'uf.yakubu@amaccouncil.gov.ng', 0.00),
(10, 'Grace Ifeoma Nwachukwu', 'Corner Shop 4, Karu Market Road, Karu, Abuja', '08162233445', 'grace.nwachukwu@gmail.com', 1500.00);

INSERT OR IGNORE INTO Bills (BillID, CustomerID, AmountDue, DueDate, IsPaid, Amount, Status) VALUES
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

INSERT OR IGNORE INTO Payments (PaymentID, BillID, AmountPaid, PaymentDate, PaymentMethod) VALUES
(1, 1, 18500.00, '2026-06-12 10:14:22', 'Bank Transfer (GTBank)'),
(2, 3, 15000.00, '2026-05-28 14:30:05', 'POS Terminal'),
(3, 5, 35000.00, '2026-05-31 09:45:12', 'Remita Web Pay'),
(4, 6, 40000.00, '2026-07-01 16:20:00', 'Bank Transfer (Access Bank)'),
(5, 8, 16000.00, '2026-06-18 11:05:44', 'USSD (*737#)'),
(6, 12, 22000.00, '2026-06-08 15:10:30', 'Bank Transfer (Zenith Bank)');
`;

async function getDb() {
  if (!SQL_MODULE) {
    SQL_MODULE = await initSqlJs({
      wasmBinary
    });
  }

  if (dbInstance) {
    return dbInstance;
  }

  let buffer = null;
  if (fs.existsSync(DB_PATH)) {
    try {
      buffer = fs.readFileSync(DB_PATH);
    } catch {
      buffer = null;
    }
  }

  if (!buffer) {
    const projectDbPath = path.join(process.cwd(), 'billing.db');
    if (fs.existsSync(projectDbPath)) {
      try {
        buffer = fs.readFileSync(projectDbPath);
      } catch {
        buffer = null;
      }
    }
  }

  if (buffer && buffer.length > 0) {
    try {
      dbInstance = new SQL_MODULE.Database(buffer);
    } catch {
      dbInstance = new SQL_MODULE.Database();
    }
  } else {
    dbInstance = new SQL_MODULE.Database();
  }

  // Ensure tables and seed data exist
  dbInstance.run(SEED_SQL);
  saveDb(dbInstance);

  return dbInstance;
}

function saveDb(db) {
  try {
    const data = db.export();
    const buffer = Buffer.from(data);
    const dir = path.dirname(DB_PATH);
    if (!fs.existsSync(dir)) {
      fs.mkdirSync(dir, { recursive: true });
    }
    fs.writeFileSync(DB_PATH, buffer);
  } catch (err) {
    console.error('Failed to save SQLite state:', err.message);
  }
}

function query(db, sql, params = {}) {
  const stmt = db.prepare(sql);
  if (params && Object.keys(params).length > 0) {
    stmt.bind(params);
  }
  const rows = [];
  while (stmt.step()) {
    rows.push(stmt.getAsObject());
  }
  stmt.free();
  return rows;
}

function get(db, sql, params = {}) {
  const rows = query(db, sql, params);
  return rows.length > 0 ? rows[0] : null;
}

function run(db, sql, params = {}) {
  const stmt = db.prepare(sql);
  if (params && Object.keys(params).length > 0) {
    stmt.bind(params);
  }
  stmt.step();
  stmt.free();

  let lastId = 0;
  try {
    const res = db.exec('SELECT last_insert_rowid() as id');
    if (res && res.length > 0 && res[0].values && res[0].values.length > 0) {
      lastId = Number(res[0].values[0][0]);
    }
  } catch {}

  saveDb(db);
  return { lastInsertRowid: lastId };
}

module.exports = {
  getDb,
  saveDb,
  query,
  get,
  run
};
