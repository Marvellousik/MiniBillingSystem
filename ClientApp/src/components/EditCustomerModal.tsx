import React, { useState } from 'react';
import { api } from '../api';
import type { CustomerDetail } from '../types';
import { X } from 'lucide-react';

interface EditCustomerModalProps {
  customer: CustomerDetail;
  onClose: () => void;
  onSuccess: () => void;
}

export const EditCustomerModal: React.FC<EditCustomerModalProps> = ({
  customer,
  onClose,
  onSuccess,
}) => {
  const [fullName, setFullName] = useState(customer.fullName);
  const [address, setAddress] = useState(customer.address);
  const [phoneNumber, setPhoneNumber] = useState(customer.phoneNumber);
  const [email, setEmail] = useState(customer.email);
  const [submitting, setSubmitting] = useState(false);
  const [error, setError] = useState<string | null>(null);

  const handleSubmit = async (e: React.FormEvent) => {
    e.preventDefault();
    setError(null);
    setSubmitting(true);

    try {
      await api.updateCustomer(customer.customerID, {
        fullName,
        address,
        phoneNumber,
        email,
      });
      onSuccess();
      onClose();
    } catch (err: any) {
      setError(err.message || 'Failed to update customer details.');
    } finally {
      setSubmitting(false);
    }
  };

  return (
    <div className="modal-backdrop" onClick={onClose}>
      <div className="modal-content" onClick={(e) => e.stopPropagation()}>
        <div className="modal-header">
          <h3 className="modal-title">Edit Customer #{customer.customerID}</h3>
          <button className="btn-icon" onClick={onClose}>
            <X style={{ width: 18, height: 18 }} />
          </button>
        </div>

        {error && <div className="alert-box alert-error">{error}</div>}

        <form onSubmit={handleSubmit}>
          <div className="form-group">
            <label className="form-label" htmlFor="editFullName">
              Full Name *
            </label>
            <input
              id="editFullName"
              type="text"
              className="form-control"
              value={fullName}
              onChange={(e) => setFullName(e.target.value)}
              required
            />
          </div>

          <div className="form-group">
            <label className="form-label" htmlFor="editAddress">
              Address *
            </label>
            <input
              id="editAddress"
              type="text"
              className="form-control"
              value={address}
              onChange={(e) => setAddress(e.target.value)}
              required
            />
          </div>

          <div className="form-group">
            <label className="form-label" htmlFor="editPhoneNumber">
              Phone Number *
            </label>
            <input
              id="editPhoneNumber"
              type="text"
              className="form-control"
              value={phoneNumber}
              onChange={(e) => setPhoneNumber(e.target.value)}
              required
            />
          </div>

          <div className="form-group">
            <label className="form-label" htmlFor="editEmail">
              Email Address *
            </label>
            <input
              id="editEmail"
              type="email"
              className="form-control"
              value={email}
              onChange={(e) => setEmail(e.target.value)}
              required
            />
          </div>

          <div className="modal-footer">
            <button
              type="button"
              className="btn btn-secondary"
              onClick={onClose}
              disabled={submitting}
            >
              Cancel
            </button>
            <button type="submit" className="btn btn-primary" disabled={submitting}>
              {submitting ? 'Saving...' : 'Save Changes'}
            </button>
          </div>
        </form>
      </div>
    </div>
  );
};
