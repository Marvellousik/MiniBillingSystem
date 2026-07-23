import React from 'react';
import { LayoutDashboard, Users, FileText, CreditCard } from 'lucide-react';

export type NavigationTab = 'overview' | 'customers' | 'bills' | 'payments';

interface SidebarProps {
  activeTab: NavigationTab;
  onTabChange: (tab: NavigationTab) => void;
}

export const Sidebar: React.FC<SidebarProps> = ({ activeTab, onTabChange }) => {
  return (
    <aside className="sidebar">
      <div className="sidebar-header">
        <h1 className="sidebar-title">Water Utility Ops</h1>
        <p className="sidebar-subtitle">Municipal Billing System</p>
      </div>
      <nav className="sidebar-nav">
        <button
          className={`nav-item ${activeTab === 'overview' ? 'active' : ''}`}
          onClick={() => onTabChange('overview')}
        >
          <LayoutDashboard className="nav-icon" />
          <span>Overview</span>
        </button>
        
        <button
          className={`nav-item ${activeTab === 'customers' ? 'active' : ''}`}
          onClick={() => onTabChange('customers')}
        >
          <Users className="nav-icon" />
          <span>Customers</span>
        </button>
        
        <button
          className={`nav-item ${activeTab === 'bills' ? 'active' : ''}`}
          onClick={() => onTabChange('bills')}
        >
          <FileText className="nav-icon" />
          <span>Bills</span>
        </button>
        
        <button
          className={`nav-item ${activeTab === 'payments' ? 'active' : ''}`}
          onClick={() => onTabChange('payments')}
        >
          <CreditCard className="nav-icon" />
          <span>Payments</span>
        </button>
      </nav>
    </aside>
  );
};
