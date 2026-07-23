import React, { useEffect, useState } from 'react';
import { api } from '../api';
import type { Summary, Bill } from '../types';
import { formatCurrency, formatDate } from '../utils';

interface OverviewPageProps {
  onSelectCustomer: (customerId: number) => void;
}

export const OverviewPage: React.FC<OverviewPageProps> = ({ onSelectCustomer }) => {
  const [summary, setSummary] = useState<Summary | null>(null);
  const [recentBills, setRecentBills] = useState<Bill[]>([]);
  const [loading, setLoading] = useState(true);
  const [error, setError] = useState<string | null>(null);

  useEffect(() => {
    async function loadOverview() {
      setLoading(true);
      setError(null);
      try {
        const [sumData, billsRes] = await Promise.all([
          api.getSummary(),
          api.getBills(1, 10),
        ]);
        setSummary(sumData);
        setRecentBills(billsRes.data);
      } catch (err: any) {
        setError(err.message || 'Failed to load overview data.');
      } finally {
        setLoading(false);
      }
    }
    loadOverview();
  }, []);

  if (loading) {
    return <div className="content-area"><p style={{ color: 'var(--text-secondary)' }}>Loading dashboard metrics...</p></div>;
  }

  if (error) {
    return (
      <div className="content-area">
        <div className="alert-box alert-error">{error}</div>
      </div>
    );
  }

  return (
    <div className="content-area">
      {/* 4 Flat Summary Cards */}
      <div className="card-grid">
        <div className="card">
          <div className="card-label">Total Customers</div>
          <div className="card-value tabular-nums">{summary?.totalCustomers.toLocaleString()}</div>
          <div className="card-subtitle">Active accounts registered</div>
        </div>

        <div className="card">
          <div className="card-label">Total Outstanding</div>
          <div className="card-value tabular-nums" style={{ color: 'var(--status-overdue-text)' }}>
            {formatCurrency(summary?.totalOutstanding || 0)}
          </div>
          <div className="card-subtitle">Unpaid bill balances</div>
        </div>

        <div className="card">
          <div className="card-label">Bills Due This Week</div>
          <div className="card-value tabular-nums" style={{ color: 'var(--status-unpaid-text)' }}>
            {summary?.billsDueThisWeek.toLocaleString()}
          </div>
          <div className="card-subtitle">Due within 7 days</div>
        </div>

        <div className="card">
          <div className="card-label">Total Collected</div>
          <div className="card-value tabular-nums" style={{ color: 'var(--status-paid-text)' }}>
            {formatCurrency(summary?.totalCollected || 0)}
          </div>
          <div className="card-subtitle">All payments processed</div>
        </div>
      </div>

      {/* 10 Most Recent Bills */}
      <div className="table-card">
        <div className="table-header">
          <h3 className="table-title">Recent Bills</h3>
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
            {recentBills.length === 0 ? (
              <tr>
                <td colSpan={5} style={{ textAlign: 'center', color: 'var(--text-secondary)' }}>
                  No recent bills found.
                </td>
              </tr>
            ) : (
              recentBills.map((bill) => (
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
      </div>
    </div>
  );
};
