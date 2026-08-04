import React, { useEffect, useState } from 'react';
import { api } from '../api';
import type { Bill, BillResult } from '../types';
import { formatCurrency, formatDate } from '../utils';

interface BillsPageProps {
  onSelectCustomer: (customerId: number) => void;
  outcomeBanner: BillResult | null;
  onClearBanner: () => void;
}

export const BillsPage: React.FC<BillsPageProps> = ({
  onSelectCustomer,
  outcomeBanner,
  onClearBanner,
}) => {
  const [bills, setBills] = useState<Bill[]>([]);
  const [page, setPage] = useState(1);
  const [pageSize] = useState(50);
  const [sortBy, setSortBy] = useState('recent_created');
  const [totalCount, setTotalCount] = useState(0);
  const [loading, setLoading] = useState(true);
  const [error, setError] = useState<string | null>(null);

  const fetchBills = async (currentPage: number, currentSort: string) => {
    setLoading(true);
    setError(null);
    try {
      const res = await api.getBills(currentPage, pageSize, currentSort);
      setBills(res.data);
      setTotalCount(res.totalCount);
    } catch (err: any) {
      setError(err.message || "Oops! We couldn't load the bills right now.");
    } finally {
      setLoading(false);
    }
  };

  useEffect(() => {
    fetchBills(page, sortBy);
  }, [page, sortBy, outcomeBanner]);

  const totalPages = Math.ceil(totalCount / pageSize) || 1;

  return (
    <div className="content-area">
      {/* Outcome Banner from Bill Generation */}
      {outcomeBanner && (
        <div
          className={`alert-box ${
            outcomeBanner.outcome === 'FULLY_COVERED'
              ? 'alert-success'
              : outcomeBanner.outcome === 'PARTIALLY_COVERED'
              ? 'alert-warning'
              : 'alert-success'
          }`}
          style={{ display: 'flex', justifyContent: 'space-between', alignItems: 'center' }}
        >
          <div>
            <strong>Bill Generation Outcome ({outcomeBanner.outcome}):</strong>
            <div>{outcomeBanner.message}</div>
          </div>
          <button className="btn btn-secondary btn-sm" onClick={onClearBanner}>
            Dismiss
          </button>
        </div>
      )}

      {error && <div className="alert-box alert-error">{error}</div>}

      <div className="table-card">
        <div className="table-header" style={{ display: 'flex', justifyContent: 'space-between', alignItems: 'center' }}>
          <div>
            <h3 className="table-title">Billing Registry</h3>
            <span className="pagination-info">Total Bills: {totalCount}</span>
          </div>
          <div style={{ display: 'flex', alignItems: 'center', gap: '8px' }}>
            <label htmlFor="billSortSelect" style={{ fontSize: '0.85rem', color: 'var(--text-secondary)' }}>
              Sort By:
            </label>
            <select
              id="billSortSelect"
              className="form-control"
              style={{ width: 'auto', padding: '4px 10px', fontSize: '0.85rem' }}
              value={sortBy}
              onChange={(e) => {
                setSortBy(e.target.value);
                setPage(1);
              }}
            >
              <option value="recent_created">Most Recent Created</option>
              <option value="recent_activity">Most Recent Activity</option>
              <option value="due_date">Due Date (Earliest First)</option>
            </select>
          </div>
        </div>

        <table className="data-table">
          <thead>
            <tr>
              <th>Bill ID</th>
              <th>Customer</th>
              <th>Due Date</th>
              <th className="text-right">Amount Due</th>
              <th>Status</th>
            </tr>
          </thead>
          <tbody>
            {loading ? (
              <tr>
                <td colSpan={5} style={{ textAlign: 'center', color: 'var(--text-secondary)' }}>
                  Loading bill records...
                </td>
              </tr>
            ) : bills.length === 0 ? (
              <tr>
                <td colSpan={5} style={{ textAlign: 'center', color: 'var(--text-secondary)' }}>
                  No bills found.
                </td>
              </tr>
            ) : (
              bills.map((bill) => (
                <tr
                  key={bill.billID}
                  className="clickable-row"
                  onClick={() => onSelectCustomer(bill.customerID)}
                >
                  <td className="tabular-nums">#{bill.billID}</td>
                  <td>
                    <span style={{ fontWeight: 500 }}>{bill.fullName}</span>
                  </td>
                  <td>{formatDate(bill.dueDate)}</td>
                  <td className="text-right tabular-nums" style={{ fontWeight: 600 }}>
                    {formatCurrency(bill.amountDue)}
                  </td>
                  <td>
                    {bill.isPaid ? (
                      <span className="pill pill-paid">Paid</span>
                    ) : (
                      <span className="pill pill-unpaid">Unpaid</span>
                    )}
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
