import React, { useState } from 'react';
import { Sidebar } from './components/Sidebar';
import type { NavigationTab } from './components/Sidebar';
import { Header } from './components/Header';
import { OverviewPage } from './pages/OverviewPage';
import { CustomersPage } from './pages/CustomersPage';
import { BillsPage } from './pages/BillsPage';
import { PaymentsPage } from './pages/PaymentsPage';
import { CustomerDetailPage } from './pages/CustomerDetailPage';

import { RegisterCustomerModal } from './components/RegisterCustomerModal';
import { GenerateBillModal } from './components/GenerateBillModal';
import { RecordPaymentModal } from './components/RecordPaymentModal';
import type { BillResult, PaymentResult } from './types';

export const App: React.FC = () => {
  const [activeTab, setActiveTab] = useState<NavigationTab>('overview');
  const [selectedCustomerId, setSelectedCustomerId] = useState<number | null>(null);

  // Modals
  const [registerModalOpen, setRegisterModalOpen] = useState(false);
  const [generateBillModalOpen, setGenerateBillModalOpen] = useState(false);
  const [recordPaymentModalOpen, setRecordPaymentModalOpen] = useState(false);

  // Banners / Alerts
  const [billOutcome, setBillOutcome] = useState<BillResult | null>(null);
  const [paymentOutcome, setPaymentOutcome] = useState<PaymentResult | null>(null);

  const handleTabChange = (tab: NavigationTab) => {
    setActiveTab(tab);
    setSelectedCustomerId(null);
  };

  const handleSelectCustomer = (customerId: number) => {
    setSelectedCustomerId(customerId);
  };

  // Determine page header title & action button
  let title = 'Overview';
  let actionButton: { label: string; onClick: () => void } | undefined = undefined;

  if (selectedCustomerId !== null) {
    title = 'Customer Detail';
  } else {
    switch (activeTab) {
      case 'overview':
        title = 'Operations Overview';
        break;
      case 'customers':
        title = 'Customers';
        actionButton = {
          label: 'Register Customer',
          onClick: () => setRegisterModalOpen(true),
        };
        break;
      case 'bills':
        title = 'Bills';
        actionButton = {
          label: 'Generate Bill',
          onClick: () => setGenerateBillModalOpen(true),
        };
        break;
      case 'payments':
        title = 'Payments';
        actionButton = {
          label: 'Record Payment',
          onClick: () => setRecordPaymentModalOpen(true),
        };
        break;
    }
  }

  return (
    <div className="app-container">
      {/* Sidebar Navigation */}
      <Sidebar activeTab={activeTab} onTabChange={handleTabChange} />

      {/* Main Area */}
      <div className="main-wrapper">
        <Header title={title} actionButton={actionButton} />

        {/* Page Content */}
        {selectedCustomerId !== null ? (
          <CustomerDetailPage
            customerId={selectedCustomerId}
            onBack={() => setSelectedCustomerId(null)}
          />
        ) : (
          <>
            {activeTab === 'overview' && (
              <OverviewPage onSelectCustomer={handleSelectCustomer} />
            )}
            {activeTab === 'customers' && (
              <CustomersPage onSelectCustomer={handleSelectCustomer} />
            )}
            {activeTab === 'bills' && (
              <BillsPage
                onSelectCustomer={handleSelectCustomer}
                outcomeBanner={billOutcome}
                onClearBanner={() => setBillOutcome(null)}
              />
            )}
            {activeTab === 'payments' && (
              <PaymentsPage
                onSelectCustomer={handleSelectCustomer}
                outcomeBanner={paymentOutcome}
                onClearBanner={() => setPaymentOutcome(null)}
              />
            )}
          </>
        )}
      </div>

      {/* Modals */}
      <RegisterCustomerModal
        isOpen={registerModalOpen}
        onClose={() => setRegisterModalOpen(false)}
        onSuccess={() => {
          setRegisterModalOpen(false);
          setActiveTab('customers');
        }}
      />

      <GenerateBillModal
        isOpen={generateBillModalOpen}
        onClose={() => setGenerateBillModalOpen(false)}
        onSuccess={(result) => {
          setBillOutcome(result);
          setActiveTab('bills');
        }}
      />

      <RecordPaymentModal
        isOpen={recordPaymentModalOpen}
        onClose={() => setRecordPaymentModalOpen(false)}
        onSuccess={(result) => {
          setPaymentOutcome(result);
          setActiveTab('payments');
        }}
      />
    </div>
  );
};

export default App;
