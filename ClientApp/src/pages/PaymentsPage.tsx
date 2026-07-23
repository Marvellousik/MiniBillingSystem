import React, { useEffect, useState } from 'react';
import { api } from '../api';
import type { Payment, PaymentResult } from '../types';
import { formatCurrency, formatDate } from '../utils';

interface PaymentsPageProps {
  onSelectCustomer: (customerId: number) => void;
  outcomeBanner: PaymentResult | null;
  onClearBanner: () => void;
}

export const PaymentsPage: React.FC<PaymentsPageProps> = ({
  onSelectCustomer,
  outcomeBanner,
  onClearBanner,
}) => {
  const [payments, setPayments] = useState<Payment[]>([]);
  const [page, setPage] = useState(1);
  const [pageSize] = useState(50);
  const [totalCount, setTotalCount] = useState(0);
  const [loading, setLoading] = useState(true);
  const [error, setError] = useState<string | null>(null);

  const fetchPayments = async (currentPage: number) => {
    setLoading(true);
    setError(null);
    try {
      const res = await api.getPayments(currentPage, pageSize);
      setPayments(res.data);
      setTotalCount(res.totalCount);
    } catch (err: any) {
      setError(err.message || "Oops! We couldn't load the payment history right now.");
    } finally {
      setLoading(false);
    }
  };

  useEffect(() => {
    fetchPayments(page);
  }, [page, outcomeBanner]);

  const totalPages = Math.ceil(totalCount / pageSize) || 1;

  return (
    <div className="content-area">
      {/* Outcome Banner from Payment Recording */}
      {outcomeBanner && (
        <div
          className={`alert-box ${
            outcomeBanner.outcome === 'OVERPAYMENT'
              ? 'alert-success'
              : outcomeBanner.outcome === 'FULLY_PAID'
              ? 'alert-success'
              : 'alert-warning'
          }`}
          style={{ display: 'flex', justifyContent: 'space-between', alignItems: 'center' }}
        >
          <div>
            <strong>Payment Outcome ({outcomeBanner.outcome}):</strong>
            <div>{outcomeBanner.message}</div>
          </div>
          <button className="btn btn-secondary btn-sm" onClick={onClearBanner}>
            Dismiss
          </button>
        </div>
      )}

      {error && <div className="alert-box alert-error">{error}</div>}

      <div className="table-card">
        <div className="table-header">
          <h3 className="table-title">Payment History</h3>
          <span className="pagination-info">Total Transactions: {totalCount}</span>
        </div>

        <table className="data-table">
          <thead>
            <tr>
              <th>Pay ID</th>
              <th>Customer</th>
              <th>Bill ID</th>
              <th className="text-right">Amount Paid</th>
              <th>Method</th>
              <th>Date</th>
            </tr>
          </thead>
          <tbody>
            {loading ? (
              <tr>
                <td colSpan={6} style={{ textAlign: 'center', color: 'var(--text-secondary)' }}>
                  Loading payment records...
                </td>
              </tr>
            ) : payments.length === 0 ? (
              <tr>
                <td colSpan={6} style={{ textAlign: 'center', color: 'var(--text-secondary)' }}>
                  No payment records found.
                </td>
              </tr>
            ) : (
              payments.map((pay) => (
                <tr
                  key={pay.paymentID}
                  className="clickable-row"
                  onClick={() => onSelectCustomer(pay.customerID)}
                >
                  <td className="tabular-nums">#{pay.paymentID}</td>
                  <td>
                    <span style={{ fontWeight: 500 }}>{pay.fullName}</span>
                  </td>
                  <td className="tabular-nums">#{pay.billID}</td>
                  <td className="text-right tabular-nums" style={{ color: 'var(--status-paid-text)', fontWeight: 600 }}>
                    {formatCurrency(pay.amountPaid)}
                  </td>
                  <td>
                    <span className="pill pill-neutral">{pay.paymentMethod}</span>
                  </td>
                  <td>{formatDate(pay.paymentDate)}</td>
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
