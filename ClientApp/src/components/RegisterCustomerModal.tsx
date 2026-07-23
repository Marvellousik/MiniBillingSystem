import React, { useState } from 'react';
import { api } from '../api';

interface RegisterCustomerModalProps {
  isOpen: boolean;
  onClose: () => void;
  onSuccess: () => void;
}

export const RegisterCustomerModal: React.FC<RegisterCustomerModalProps> = ({
  isOpen,
  onClose,
  onSuccess,
}) => {
  const [fullName, setFullName] = useState('');
  const [address, setAddress] = useState('');
  const [phoneNumber, setPhoneNumber] = useState('');
  const [email, setEmail] = useState('');
  const [error, setError] = useState<string | null>(null);
  const [loading, setLoading] = useState(false);

  if (!isOpen) return null;

  const handleSubmit = async (e: React.FormEvent) => {
    e.preventDefault();
    setError(null);

    const trimmedName = fullName.trim();
    const trimmedAddress = address.trim();
    const trimmedPhone = phoneNumber.trim();
    const trimmedEmail = email.trim();

    if (!trimmedName) {
      setError('This field cannot be left blank. Please enter a value.');
      return;
    }
    if (trimmedName.length > 100) {
      setError("That's a bit too long! Please shorten it to 100 characters or less.");
      return;
    }

    if (!trimmedAddress) {
      setError('This field cannot be left blank. Please enter a value.');
      return;
    }
    if (trimmedAddress.length > 255) {
      setError("That's a bit too long! Please shorten it to 255 characters or less.");
      return;
    }

    if (!trimmedPhone) {
      setError('This field cannot be left blank. Please enter a value.');
      return;
    }
    if (trimmedPhone.length > 20) {
      setError("That's a bit too long! Please shorten it to 20 characters or less.");
      return;
    }

    if (!trimmedEmail) {
      setError('This field cannot be left blank. Please enter a value.');
      return;
    }
    if (trimmedEmail.length > 100) {
      setError("That's a bit too long! Please shorten it to 100 characters or less.");
      return;
    }

    setLoading(true);
    try {
      await api.registerCustomer({
        fullName: trimmedName,
        address: trimmedAddress,
        phoneNumber: trimmedPhone,
        email: trimmedEmail,
      });
      setFullName('');
      setAddress('');
      setPhoneNumber('');
      setEmail('');
      onSuccess();
      onClose();
    } catch (err: any) {
      setError(err.message || 'Failed to register customer.');
    } finally {
      setLoading(false);
    }
  };

  return (
    <div className="modal-overlay">
      <div className="modal-card">
        <div className="modal-header">
          <h3 className="modal-title">Register New Customer</h3>
          <button className="btn btn-secondary btn-sm" onClick={onClose} disabled={loading}>
            Cancel
          </button>
        </div>
        <form onSubmit={handleSubmit}>
          <div className="modal-body">
            {error && <div className="alert-box alert-error">{error}</div>}

            <div className="form-group">
              <label className="form-label">Full Name</label>
              <input
                type="text"
                className="form-control"
                placeholder="e.g. Amaka Obi"
                value={fullName}
                onChange={(e) => setFullName(e.target.value)}
                maxLength={100}
                disabled={loading}
              />
            </div>

            <div className="form-group">
              <label className="form-label">Address</label>
              <input
                type="text"
                className="form-control"
                placeholder="e.g. 12 Waterline Avenue, Marina"
                value={address}
                onChange={(e) => setAddress(e.target.value)}
                maxLength={255}
                disabled={loading}
              />
            </div>

            <div className="form-group">
              <label className="form-label">Phone Number</label>
              <input
                type="text"
                className="form-control"
                placeholder="e.g. 08012345678"
                value={phoneNumber}
                onChange={(e) => setPhoneNumber(e.target.value)}
                maxLength={20}
                disabled={loading}
              />
            </div>

            <div className="form-group">
              <label className="form-label">Email Address</label>
              <input
                type="email"
                className="form-control"
                placeholder="e.g. amaka.obi@example.com"
                value={email}
                onChange={(e) => setEmail(e.target.value)}
                maxLength={100}
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
              {loading ? 'Registering...' : 'Register Customer'}
            </button>
          </div>
        </form>
      </div>
    </div>
  );
};
