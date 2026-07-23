import React, { useState } from 'react';
import { api } from '../api';
import type { PaymentResult } from '../types';

interface RecordPaymentModalProps {
  isOpen: boolean;
  onClose: () => void;
  onSuccess: (result: PaymentResult) => void;
  defaultBillId?: number;
}

export const RecordPaymentModal: React.FC<RecordPaymentModalProps> = ({
  isOpen,
  onClose,
  onSuccess,
  defaultBillId,
}) => {
  const [billId, setBillId] = useState<string>(
    defaultBillId ? defaultBillId.toString() : ''
  );
  const [amountPaid, setAmountPaid] = useState<string>('');
  const [method, setMethod] = useState<string>('Cash');
  const [error, setError] = useState<string | null>(null);
  const [loading, setLoading] = useState(false);

  if (!isOpen) return null;

  const handleSubmit = async (e: React.FormEvent) => {
    e.preventDefault();
    setError(null);

    const parsedBillId = parseInt(billId.trim(), 10);
    if (isNaN(parsedBillId) || parsedBillId <= 0) {
      setError("We couldn't find that Bill ID. Please double-check it and try again.");
      return;
    }

    const parsedAmount = parseFloat(amountPaid.trim());
    if (isNaN(parsedAmount) || parsedAmount < 0) {
      setError("Please enter a valid positive number (e.g., 1500 or 1500.50). Letters and symbols are not allowed.");
      return;
    }

    if (!method.trim()) {
      setError("Payment method cannot be left blank. Please select or enter a method.");
      return;
    }

    setLoading(true);
    try {
      const result = await api.recordPayment({
        billID: parsedBillId,
        amountPaid: parsedAmount,
        paymentMethod: method,
      });
      onSuccess(result);
      onClose();
    } catch (err: any) {
      setError(err.message || "Sorry, the payment failed to process. No money was recorded. Please try again.");
    } finally {
      setLoading(false);
    }
  };

  return (
    <div className="modal-overlay">
      <div className="modal-card">
        <div className="modal-header">
          <h3 className="modal-title">Record a Payment</h3>
          <button className="btn btn-secondary btn-sm" onClick={onClose} disabled={loading}>
            Cancel
          </button>
        </div>
        <form onSubmit={handleSubmit}>
          <div className="modal-body">
            {error && <div className="alert-box alert-error">{error}</div>}

            <div className="form-group">
              <label className="form-label">Bill ID</label>
              <input
                type="number"
                className="form-control"
                placeholder="e.g. 50"
                value={billId}
                onChange={(e) => setBillId(e.target.value)}
                disabled={loading}
              />
            </div>

            <div className="form-group">
              <label className="form-label">Amount Paid ($)</label>
              <input
                type="number"
                step="0.01"
                min="0"
                className="form-control"
                placeholder="e.g. 150.50"
                value={amountPaid}
                onChange={(e) => setAmountPaid(e.target.value)}
                disabled={loading}
              />
            </div>

            <div className="form-group">
              <label className="form-label">Payment Method</label>
              <select
                className="form-control"
                value={method}
                onChange={(e) => setMethod(e.target.value)}
                disabled={loading}
              >
                <option value="Cash">Cash</option>
                <option value="Transfer">Transfer</option>
                <option value="Card">Card</option>
              </select>
            </div>
          </div>

          <div className="modal-footer">
            <button
              type="button"
              className="btn btn-secondary"
              onClick={onClose}
              disabled={loading}
            >
              Cancel
            </button>
            <button type="submit" className="btn btn-primary" disabled={loading}>
              {loading ? 'Processing...' : 'Record Payment'}
            </button>
          </div>
        </form>
      </div>
    </div>
  );
};
