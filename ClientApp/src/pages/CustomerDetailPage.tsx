import React, { useEffect, useState } from 'react';
import { api } from '../api';
import type { CustomerDetail } from '../types';
import { formatCurrency, formatDate } from '../utils';
import { ArrowLeft } from 'lucide-react';

interface CustomerDetailPageProps {
  customerId: number;
  onBack: () => void;
}

export const CustomerDetailPage: React.FC<CustomerDetailPageProps> = ({
  customerId,
  onBack,
}) => {
  const [detail, setDetail] = useState<CustomerDetail | null>(null);
  const [loading, setLoading] = useState(true);
  const [error, setError] = useState<string | null>(null);

  const fetchDetail = async () => {
    setLoading(true);
    setError(null);
    try {
      const data = await api.getCustomerById(customerId);
      setDetail(data);
    } catch (err: any) {
      setError(err.message || "Sorry, we ran into an issue pulling up this customer's dashboard.");
    } finally {
      setLoading(false);
    }
  };

  useEffect(() => {
    fetchDetail();
  }, [customerId]);

  if (loading) {
    return (
      <div className="content-area">
        <button className="btn btn-secondary btn-sm" onClick={onBack} style={{ marginBottom: 20 }}>
          <ArrowLeft style={{ width: 16, height: 16 }} /> Back to Customers
        </button>
        <p style={{ color: 'var(--text-secondary)' }}>Loading customer account details...</p>
      </div>
    );
  }

  if (error || !detail) {
    return (
      <div className="content-area">
        <button className="btn btn-secondary btn-sm" onClick={onBack} style={{ marginBottom: 20 }}>
          <ArrowLeft style={{ width: 16, height: 16 }} /> Back to Customers
        </button>
        <div className="alert-box alert-error">{error || 'Customer not found.'}</div>
      </div>
    );
  }

  return (
    <div className="content-area">
      <button className="btn btn-secondary btn-sm" onClick={onBack} style={{ marginBottom: 20 }}>
        <ArrowLeft style={{ width: 16, height: 16 }} /> Back to Customers
      </button>

      {/* Profile Card */}
      <div className="card" style={{ marginBottom: 24 }}>
        <div style={{ display: 'flex', justifyContent: 'space-between', alignItems: 'flex-start' }}>
          <div>
            <h2 style={{ fontSize: 20, fontWeight: 600, color: 'var(--text-primary)' }}>
              {detail.fullName}
            </h2>
            <p style={{ color: 'var(--text-secondary)', fontSize: 13, marginTop: 2 }}>
              Customer ID: #{detail.customerID}
            </p>
          </div>
          <div style={{ display: 'flex', gap: 16 }}>
            <div style={{ textAlign: 'right' }}>
              <div className="card-label">Account Credit</div>
              <div className="card-value tabular-nums" style={{ color: 'var(--status-paid-text)', fontSize: 20 }}>
                {formatCurrency(detail.accountBalance)}
              </div>
            </div>
            <div style={{ textAlign: 'right' }}>
              <div className="card-label">Total Owed</div>
              <div className="card-value tabular-nums" style={{ color: detail.totalOwed > 0 ? 'var(--status-overdue-text)' : 'var(--text-primary)', fontSize: 20 }}>
                {formatCurrency(detail.totalOwed)}
              </div>
            </div>
          </div>
        </div>

        <div style={{ display: 'grid', gridTemplateColumns: 'repeat(auto-fit, minmax(200px, 1fr))', gap: 16, marginTop: 20, paddingTop: 16, borderTop: '1px solid var(--border-color)' }}>
          <div>
            <div className="card-label">Address</div>
            <div style={{ fontSize: 14, color: 'var(--text-primary)' }}>{detail.address || '—'}</div>
          </div>
          <div>
            <div className="card-label">Phone Number</div>
            <div style={{ fontSize: 14, color: 'var(--text-primary)' }}>{detail.phoneNumber || '—'}</div>
          </div>
          <div>
            <div className="card-label">Email Address</div>
            <div style={{ fontSize: 14, color: 'var(--text-primary)' }}>{detail.email || '—'}</div>
          </div>
        </div>
      </div>

      {/* Stacked Tables */}
      {/* 1. Billing History */}
      <div className="table-card" style={{ marginBottom: 24 }}>
        <div className="table-header">
          <h3 className="table-title">Billing History ({detail.bills.length})</h3>
        </div>
        <table className="data-table">
          <thead>
            <tr>
              <th>Bill ID</th>
              <th>Due Date</th>
              <th className="text-right">Amount Due</th>
              <th>Status</th>
            </tr>
          </thead>
          <tbody>
            {detail.bills.length === 0 ? (
              <tr>
                <td colSpan={4} style={{ textAlign: 'center', color: 'var(--text-secondary)' }}>
                  No bills found for this customer.
                </td>
              </tr>
            ) : (
              detail.bills.map((bill) => (
                <tr key={bill.billID}>
                  <td className="tabular-nums">#{bill.billID}</td>
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

      {/* 2. Payment History */}
      <div className="table-card">
        <div className="table-header">
          <h3 className="table-title">Payment History ({detail.payments.length})</h3>
        </div>
        <table className="data-table">
          <thead>
            <tr>
              <th>Pay ID</th>
              <th>For Bill</th>
              <th className="text-right">Amount Paid</th>
              <th>Method</th>
              <th>Payment Date</th>
            </tr>
          </thead>
          <tbody>
            {detail.payments.length === 0 ? (
              <tr>
                <td colSpan={5} style={{ textAlign: 'center', color: 'var(--text-secondary)' }}>
                  No payment records found for this customer.
                </td>
              </tr>
            ) : (
              detail.payments.map((pay) => (
                <tr key={pay.paymentID}>
                  <td className="tabular-nums">#{pay.paymentID}</td>
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
      </div>
    </div>
  );
};
