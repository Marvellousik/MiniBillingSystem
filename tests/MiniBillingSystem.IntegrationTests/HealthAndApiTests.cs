using System.Net;
using System.Net.Http.Json;
using System.Text.Json;
using Microsoft.AspNetCore.Mvc.Testing;
using MiniBillingSystem.Models;
using Xunit;

namespace MiniBillingSystem.IntegrationTests
{
    public class HealthAndApiTests : IClassFixture<WebApplicationFactory<Program>>
    {
        private readonly HttpClient _client;

        public HealthAndApiTests(WebApplicationFactory<Program> factory)
        {
            _client = factory.CreateClient();
        }

        [Fact]
        public async Task HealthEndpoint_Returns_Healthy_Status()
        {
            // Act
            var response = await _client.GetAsync("/health");

            // Assert
            Assert.Equal(HttpStatusCode.OK, response.StatusCode);
            var content = await response.Content.ReadAsStringAsync();
            using var doc = JsonDocument.Parse(content);
            Assert.Equal("Healthy", doc.RootElement.GetProperty("status").GetString());
            Assert.Equal("Healthy", doc.RootElement.GetProperty("checks").GetProperty("databaseConnectivity").GetProperty("status").GetString());
            Assert.Equal("Healthy", doc.RootElement.GetProperty("checks").GetProperty("migrations").GetProperty("status").GetString());
        }

        [Fact]
        public async Task RegisterCustomer_WithEmptyName_ReturnsBadRequest()
        {
            // Act
            var response = await _client.PostAsJsonAsync("/api/customers", new
            {
                FullName = "",
                Address = "123 Main St",
                PhoneNumber = "555-1234",
                Email = "test@example.com"
            });

            // Assert
            Assert.Equal(HttpStatusCode.BadRequest, response.StatusCode);
        }

        [Fact]
        public async Task GenerateBill_WithNegativeAmount_ReturnsBadRequest()
        {
            // Act
            var response = await _client.PostAsJsonAsync("/api/bills", new
            {
                CustomerID = 1,
                AmountDue = -100.00m,
                DueDate = "2026-12-31"
            });

            // Assert
            Assert.Equal(HttpStatusCode.BadRequest, response.StatusCode);
        }

        [Fact]
        public async Task UpdateCustomer_WithEmptyName_ReturnsBadRequest()
        {
            // Act
            var response = await _client.PutAsJsonAsync("/api/customers/1", new
            {
                FullName = "",
                Address = "Updated Address",
                PhoneNumber = "08012345678",
                Email = "updated@example.com"
            });

            // Assert
            Assert.Equal(HttpStatusCode.BadRequest, response.StatusCode);
        }

        [Fact]
        public async Task DeleteCustomer_NonExistentCustomer_ReturnsNotFound()
        {
            // Act
            var response = await _client.DeleteAsync("/api/customers/999999");

            // Assert
            Assert.Equal(HttpStatusCode.NotFound, response.StatusCode);
        }

        [Fact]
        public async Task FullCustomerAndBillingLifecycle_Succeeds()
        {
            var uniqueEmail = $"user_{Guid.NewGuid():N}@test.com";

            // 1. Create customer
            var createRes = await _client.PostAsJsonAsync("/api/customers", new CreateCustomerRequest
            {
                FullName = "Integration Test User",
                Address = "Plot 505 Test Crescent",
                PhoneNumber = "08099887766",
                Email = uniqueEmail
            });
            Assert.Equal(HttpStatusCode.OK, createRes.StatusCode);
            var createJson = await createRes.Content.ReadFromJsonAsync<JsonElement>();
            int customerId = createJson.GetProperty("customerID").GetInt32();
            Assert.True(customerId > 0);

            // 2. Fetch customer history
            var getRes = await _client.GetAsync($"/api/customers/{customerId}");
            Assert.Equal(HttpStatusCode.OK, getRes.StatusCode);
            var customer = await getRes.Content.ReadFromJsonAsync<CustomerDetailDto>();
            Assert.NotNull(customer);
            Assert.Equal("Integration Test User", customer.FullName);
            Assert.Equal(uniqueEmail, customer.Email);

            // 3. Create a bill
            var billRes = await _client.PostAsJsonAsync("/api/bills", new CreateBillRequest
            {
                CustomerID = customerId,
                AmountDue = 5000.00m,
                DueDate = "2026-12-31"
            });
            Assert.Equal(HttpStatusCode.OK, billRes.StatusCode);
            var billResult = await billRes.Content.ReadFromJsonAsync<BillCreationResultDto>();
            Assert.NotNull(billResult);
            Assert.Equal("NO_CREDIT_APPLIED", billResult.Outcome);
            Assert.Equal(5000.00m, billResult.FinalAmountDue);

            // 4. Verify customer detail has bill
            getRes = await _client.GetAsync($"/api/customers/{customerId}");
            customer = await getRes.Content.ReadFromJsonAsync<CustomerDetailDto>();
            Assert.NotNull(customer);
            Assert.NotEmpty(customer.Bills);
            int billId = customer.Bills[0].BillID;

            // 5. Record partial payment
            var payRes = await _client.PostAsJsonAsync("/api/payments", new RecordPaymentRequest
            {
                BillID = billId,
                AmountPaid = 2000.00m,
                PaymentMethod = "Debit Card"
            });
            Assert.Equal(HttpStatusCode.OK, payRes.StatusCode);
            var payResult = await payRes.Content.ReadFromJsonAsync<PaymentResultDto>();
            Assert.NotNull(payResult);
            Assert.Equal("PARTIAL_PAYMENT", payResult.Outcome);
            Assert.Equal(3000.00m, payResult.RemainingAmountDue);

            // 6. Record remaining payment with overpayment
            payRes = await _client.PostAsJsonAsync("/api/payments", new RecordPaymentRequest
            {
                BillID = billId,
                AmountPaid = 4000.00m,
                PaymentMethod = "Bank Transfer"
            });
            Assert.Equal(HttpStatusCode.OK, payRes.StatusCode);
            payResult = await payRes.Content.ReadFromJsonAsync<PaymentResultDto>();
            Assert.NotNull(payResult);
            Assert.Equal("OVERPAYMENT", payResult.Outcome);
            Assert.Equal(0.00m, payResult.RemainingAmountDue);

            // 7. Check Summary Metrics
            var summaryRes = await _client.GetAsync("/api/summary");
            Assert.Equal(HttpStatusCode.OK, summaryRes.StatusCode);
            var summary = await summaryRes.Content.ReadFromJsonAsync<SummaryDto>();
            Assert.NotNull(summary);
            Assert.True(summary.TotalCustomers > 0);
            Assert.True(summary.TotalCollected > 0);
        }
    }
}
