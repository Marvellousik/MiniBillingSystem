export interface Customer {
  customerID: number;
  fullName: string;
  accountBalance: number;
  totalOwed: number;
}

export interface CustomerDetail {
  customerID: number;
  fullName: string;
  address: string;
  phoneNumber: string;
  email: string;
  accountBalance: number;
  totalOwed: number;
  bills: Bill[];
  payments: Payment[];
}

export interface Bill {
  billID: number;
  customerID: number;
  fullName: string;
  amountDue: number;
  dueDate: string;
  isPaid: boolean;
}

export interface Payment {
  paymentID: number;
  billID: number;
  customerID: number;
  fullName: string;
  amountPaid: number;
  paymentMethod: string;
  paymentDate: string;
}

export interface Summary {
  totalCustomers: number;
  totalOutstanding: number;
  billsDueThisWeek: number;
  totalCollected: number;
}

export interface CreateCustomerPayload {
  fullName: string;
  address: string;
  phoneNumber: string;
  email: string;
}

export interface CreateBillPayload {
  customerID: number;
  amountDue: number;
  dueDate: string;
}

export interface BillResult {
  message: string;
  outcome: string;
  finalAmountDue: number;
  isPaid: boolean;
}

export interface RecordPaymentPayload {
  billID: number;
  amountPaid: number;
  paymentMethod: string;
}

export interface PaymentResult {
  message: string;
  outcome: string;
  remainingAmountDue: number;
}

export interface PaginatedResponse<T> {
  data: T[];
  page: number;
  pageSize: number;
  totalCount: number;
}
