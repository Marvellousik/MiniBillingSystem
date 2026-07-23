import React, { useEffect, useState } from 'react';
import { api } from '../api';
import type { Customer } from '../types';
import { formatCurrency } from '../utils';

interface CustomersPageProps {
  onSelectCustomer: (customerId: number) => void;
}

export const CustomersPage: React.FC<CustomersPageProps> = ({ onSelectCustomer }) => {
  const [customers, setCustomers] = useState<Customer[]>([]);
  const [page, setPage] = useState(1);
  const [pageSize] = useState(50);
  const [totalCount, setTotalCount] = useState(0);
  const [loading, setLoading] = useState(true);
  const [error, setError] = useState<string | null>(null);

  const fetchCustomers = async (currentPage: number) => {
    setLoading(true);
    setError(null);
    try {
      const res = await api.getCustomers(currentPage, pageSize);
      setCustomers(res.data);
      setTotalCount(res.totalCount);
    } catch (err: any) {
      setError(err.message || "Oops! We couldn't load the customer list right now.");
    } finally {
      setLoading(false);
    }
  };

  useEffect(() => {
    fetchCustomers(page);
  }, [page]);

  const totalPages = Math.ceil(totalCount / pageSize) || 1;

  return (
    <div className="content-area">
      {error && <div className="alert-box alert-error">{error}</div>}

      <div className="table-card">
        <div className="table-header">
          <h3 className="table-title">Customer Registry</h3>
          <span className="pagination-info">Total Accounts: {totalCount}</span>
        </div>
        
        <table className="data-table">
          <thead>
            <tr>
              <th>ID</th>
              <th>Full Name</th>
              <th className="text-right">Credit (+)</th>
              <th className="text-right">Total Owed (-)</th>
            </tr>
          </thead>
          <tbody>
            {loading ? (
              <tr>
                <td colSpan={4} style={{ textAlign: 'center', color: 'var(--text-secondary)' }}>
                  Loading customer records...
                </td>
              </tr>
            ) : customers.length === 0 ? (
              <tr>
                <td colSpan={4} style={{ textAlign: 'center', color: 'var(--text-secondary)' }}>
                  No customers found.
                </td>
              </tr>
            ) : (
              customers.map((cust) => (
                <tr
                  key={cust.customerID}
                  className="clickable-row"
                  onClick={() => onSelectCustomer(cust.customerID)}
                >
                  <td className="tabular-nums">#{cust.customerID}</td>
                  <td>
                    <span style={{ fontWeight: 500 }}>{cust.fullName}</span>
                  </td>
                  <td className="text-right tabular-nums" style={{ color: cust.accountBalance > 0 ? 'var(--status-paid-text)' : 'inherit' }}>
                    {formatCurrency(cust.accountBalance)}
                  </td>
                  <td className="text-right tabular-nums" style={{ color: cust.totalOwed > 0 ? 'var(--status-overdue-text)' : 'inherit', fontWeight: cust.totalOwed > 0 ? 600 : 400 }}>
                    {formatCurrency(cust.totalOwed)}
                  </td>
                </tr>
              ))
            )}
          </tbody>
        </table>

        {/* Pagination footer */}
        {totalPages > 1 && (
          <div className="pagination">
            <span className="pagination-info">
              Page {page} of {totalPages}
            </span>
            <div className="pagination-controls">
              <button
                className="btn btn-secondary btn-sm"
                disabled={page <= 1 || loading}
                onClick={() => setPage((p) => p - 1)}
              >
                Previous
              </button>
              <button
                className="btn btn-secondary btn-sm"
                disabled={page >= totalPages || loading}
                onClick={() => setPage((p) => p + 1)}
              >
                Next
              </button>
            </div>
          </div>
        )}
      </div>
    </div>
  );
};
