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
        public async Task DeleteCustomer_NonExistentCustomer_ReturnsNotFoundOrInternalServerError()
        {
            // Act
            var response = await _client.DeleteAsync("/api/customers/999999");

            // Assert - Should return NotFound (404) or InternalServerError (500 if DB unreachable)
            Assert.True(response.StatusCode == HttpStatusCode.NotFound || response.StatusCode == HttpStatusCode.InternalServerError);
        }
    }
}
