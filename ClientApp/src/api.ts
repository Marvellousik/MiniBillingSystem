import type { 
  Customer, 
  CustomerDetail, 
  Bill, 
  Payment, 
  Summary, 
  CreateCustomerPayload, 
  CreateBillPayload, 
  BillResult, 
  RecordPaymentPayload, 
  PaymentResult,
  PaginatedResponse 
} from './types';

const API_BASE = '/api';

async function handleResponse<T>(response: Response): Promise<T> {
  const json = await response.json().catch(() => ({}));
  if (!response.ok) {
    const errorMsg = json.error || json.message || `Request failed with status ${response.status}`;
    throw new Error(errorMsg);
  }
  return json as T;
}

export const api = {
  getSummary: (): Promise<Summary> => 
    fetch(`${API_BASE}/summary`).then(handleResponse<Summary>),

  getCustomers: (page = 1, pageSize = 50): Promise<PaginatedResponse<Customer>> => 
    fetch(`${API_BASE}/customers?page=${page}&pageSize=${pageSize}`).then(handleResponse<PaginatedResponse<Customer>>),

  getCustomerById: (id: number): Promise<CustomerDetail> => 
    fetch(`${API_BASE}/customers/${id}`).then(handleResponse<CustomerDetail>),

  registerCustomer: (payload: CreateCustomerPayload): Promise<{ message: string; customerID: number }> => 
    fetch(`${API_BASE}/customers`, {
      method: 'POST',
      headers: { 'Content-Type': 'application/json' },
      body: JSON.stringify(payload)
    }).then(handleResponse<{ message: string; customerID: number }>),

  getBills: (page = 1, pageSize = 50): Promise<PaginatedResponse<Bill>> => 
    fetch(`${API_BASE}/bills?page=${page}&pageSize=${pageSize}`).then(handleResponse<PaginatedResponse<Bill>>),

  generateBill: (payload: CreateBillPayload): Promise<BillResult> => 
    fetch(`${API_BASE}/bills`, {
      method: 'POST',
      headers: { 'Content-Type': 'application/json' },
      body: JSON.stringify(payload)
    }).then(handleResponse<BillResult>),

  getPayments: (page = 1, pageSize = 50): Promise<PaginatedResponse<Payment>> => 
    fetch(`${API_BASE}/payments?page=${page}&pageSize=${pageSize}`).then(handleResponse<PaginatedResponse<Payment>>),

  recordPayment: (payload: RecordPaymentPayload): Promise<PaymentResult> => 
    fetch(`${API_BASE}/payments`, {
      method: 'POST',
      headers: { 'Content-Type': 'application/json' },
      body: JSON.stringify(payload)
    }).then(handleResponse<PaymentResult>)
};
