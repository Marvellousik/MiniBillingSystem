using System.Net;
using System.Net.Http.Json;
using Microsoft.AspNetCore.Mvc.Testing;
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
        public async Task HealthEndpoint_Returns_Response()
        {
            // Act
            var response = await _client.GetAsync("/health");

            // Assert - Health endpoint should return structured JSON payload
            Assert.True(response.StatusCode == HttpStatusCode.OK || response.StatusCode == HttpStatusCode.ServiceUnavailable);
            var content = await response.Content.ReadAsStringAsync();
            Assert.Contains("checks", content);
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
    }
}
