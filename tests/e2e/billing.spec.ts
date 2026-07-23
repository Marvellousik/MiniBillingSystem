import { test, expect } from '@playwright/test';

const BACKEND_URL = process.env.BACKEND_URL || 'http://localhost:5000';
const FRONTEND_URL = process.env.BASE_URL || 'http://localhost:80';

test.describe('MiniBillingSystem End-to-End & Health Suite', () => {

  test('1. Verify Backend and Frontend Health Checks Pass', async ({ request }) => {
    // Backend Health Check
    const backendHealthRes = await request.get(`${BACKEND_URL}/health`);
    expect(backendHealthRes.status()).toBe(200);
    const backendHealth = await backendHealthRes.json();
    expect(backendHealth.status).toBe('Healthy');
    expect(backendHealth.checks.databaseConnectivity.status).toBe('Healthy');
    expect(backendHealth.checks.migrations.status).toBe('Healthy');
    expect(backendHealth.checks.configuration.status).toBe('Healthy');

    // Frontend Readiness Check
    const frontendHealthRes = await request.get(`${FRONTEND_URL}/healthz`);
    expect(frontendHealthRes.status()).toBe(200);
    const frontendHealth = await frontendHealthRes.json();
    expect(frontendHealth.status).toBe('Healthy');
  });

  test('2. Full Lifecycle: Create Customer API -> DB Verification -> Frontend UI Rendering -> Negative Test', async ({ request, page }) => {
    const timestamp = Date.now();
    const testCustomer = {
      fullName: `QA Test User ${timestamp}`,
      address: `100 Reliability Way ${timestamp}`,
      phoneNumber: `555-90${timestamp.toString().slice(-4)}`,
      email: `qa_${timestamp}@reliability.org`
    };

    // Step A: Create test customer via API
    const createCustomerRes = await request.post(`${BACKEND_URL}/api/customers`, {
      data: testCustomer
    });
    expect(createCustomerRes.status()).toBe(200);
    const customerData = await createCustomerRes.json();
    expect(customerData.customerID).toBeGreaterThan(0);
    const createdCustomerId = customerData.customerID;

    // Step B: Direct Database Verification via GET API
    const getCustomerRes = await request.get(`${BACKEND_URL}/api/customers/${createdCustomerId}`);
    expect(getCustomerRes.status()).toBe(200);
    const fetchedCustomer = await getCustomerRes.json();
    expect(fetchedCustomer.fullName).toBe(testCustomer.fullName);
    expect(fetchedCustomer.email).toBe(testCustomer.email);
    expect(fetchedCustomer.address).toBe(testCustomer.address);

    // Create a bill for this customer via API
    const billAmount = 199.99;
    const createBillRes = await request.post(`${BACKEND_URL}/api/bills`, {
      data: {
        customerID: createdCustomerId,
        amountDue: billAmount,
        dueDate: '2026-12-31'
      }
    });
    expect(createBillRes.status()).toBe(200);
    const billResult = await createBillRes.json();
    expect(billResult.outcome).toBe('NO_CREDIT_APPLIED');

    // Verify Bill in Database via customer detail endpoint
    const verifyDetailRes = await request.get(`${BACKEND_URL}/api/customers/${createdCustomerId}`);
    const detailWithBill = await verifyDetailRes.json();
    expect(detailWithBill.bills.length).toBeGreaterThan(0);
    expect(Number(detailWithBill.bills[0].amountDue)).toBe(billAmount);

    // Step C: Frontend UI Verification via Playwright Browser
    await page.goto(`${FRONTEND_URL}/`);
    await page.waitForLoadState('networkidle');

    // Navigate to Customers Page
    await page.click('button:has-text("Customers")');
    await expect(page.locator(`text=${testCustomer.fullName}`)).toBeVisible({ timeout: 10000 });

    // Navigate to Bills Page
    await page.click('button:has-text("Bills")');
    await expect(page.locator(`text=${testCustomer.fullName}`)).toBeVisible({ timeout: 10000 });

    // Step D: Failure / Negative Input Cases
    // 1. Missing Full Name
    const invalidCustomerRes = await request.post(`${BACKEND_URL}/api/customers`, {
      data: { fullName: '', address: 'Some place', phoneNumber: '123', email: 'test@test.com' }
    });
    expect(invalidCustomerRes.status()).toBe(400);

    // 2. Non-existent Customer ID for Bill Generation
    const nonExistentBillRes = await request.post(`${BACKEND_URL}/api/bills`, {
      data: { customerID: 9999999, amountDue: 100, dueDate: '2026-12-31' }
    });
    expect(nonExistentBillRes.status()).toBe(404);

    // 3. Negative Bill Amount
    const negativeBillRes = await request.post(`${BACKEND_URL}/api/bills`, {
      data: { customerID: createdCustomerId, amountDue: -50, dueDate: '2026-12-31' }
    });
    expect(negativeBillRes.status()).toBe(400);
  });

});
