import React, { useState } from 'react';
import { api } from '../api';
import type { BillResult } from '../types';

interface GenerateBillModalProps {
  isOpen: boolean;
  onClose: () => void;
  onSuccess: (result: BillResult) => void;
  defaultCustomerId?: number;
}

export const GenerateBillModal: React.FC<GenerateBillModalProps> = ({
  isOpen,
  onClose,
  onSuccess,
  defaultCustomerId,
}) => {
  const [customerId, setCustomerId] = useState<string>(
    defaultCustomerId ? defaultCustomerId.toString() : ''
  );
  const [amount, setAmount] = useState<string>('');
  const [dueDate, setDueDate] = useState<string>(
    new Date(Date.now() + 14 * 86400000).toISOString().split('T')[0]
  );
  const [error, setError] = useState<string | null>(null);
  const [loading, setLoading] = useState(false);

  if (!isOpen) return null;

  const handleSubmit = async (e: React.FormEvent) => {
    e.preventDefault();
    setError(null);

    const parsedId = parseInt(customerId.trim(), 10);
    if (isNaN(parsedId) || parsedId <= 0) {
      setError("Please double-check the ID and try again.");
      return;
    }

    const parsedAmount = parseFloat(amount.trim());
    if (isNaN(parsedAmount) || parsedAmount < 0) {
      setError("Please enter a valid positive number (e.g., 1500 or 1500.50). Letters and symbols are not allowed.");
      return;
    }

    if (!dueDate) {
      setError("That date format wasn't recognized. Please use YYYY-MM-DD (e.g., 2026-08-01).");
      return;
    }

    setLoading(true);
    try {
      const result = await api.generateBill({
        customerID: parsedId,
        amountDue: parsedAmount,
        dueDate: dueDate,
      });
      onSuccess(result);
      onClose();
    } catch (err: any) {
      setError(err.message || "Sorry, we ran into an issue generating that bill. Please try again.");
    } finally {
      setLoading(false);
    }
  };

  return (
    <div className="modal-overlay">
      <div className="modal-card">
        <div className="modal-header">
          <h3 className="modal-title">Generate Custom Bill</h3>
          <button className="btn btn-secondary btn-sm" onClick={onClose} disabled={loading}>
            Cancel
          </button>
        </div>
        <form onSubmit={handleSubmit}>
          <div className="modal-body">
            {error && <div className="alert-box alert-error">{error}</div>}

            <div className="form-group">
              <label className="form-label">Customer ID</label>
              <input
                type="number"
                className="form-control"
                placeholder="e.g. 1007"
                value={customerId}
                onChange={(e) => setCustomerId(e.target.value)}
                disabled={loading}
              />
            </div>

            <div className="form-group">
              <label className="form-label">Amount Due ($)</label>
              <input
                type="number"
                step="0.01"
                min="0"
                className="form-control"
                placeholder="e.g. 4250.75"
                value={amount}
                onChange={(e) => setAmount(e.target.value)}
                disabled={loading}
              />
            </div>

            <div className="form-group">
              <label className="form-label">Due Date (YYYY-MM-DD)</label>
              <input
                type="date"
                className="form-control"
                value={dueDate}
                onChange={(e) => setDueDate(e.target.value)}
                disabled={loading}
              />
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
              {loading ? 'Generating...' : 'Generate Bill'}
            </button>
          </div>
        </form>
      </div>
    </div>
  );
};
