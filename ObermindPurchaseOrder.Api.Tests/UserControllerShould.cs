using System;
using System.Net;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc.Testing;
using ObermindPurchaseOrder.Api.Models.Responses;
using Xunit;

namespace ObermindPurchaseOrder.Api.Tests
{
    public class UserControllerShould : IClassFixture<WebApplicationFactory<ObermindPurchaseOrder.Api.Startup>>
    {
        private readonly HttpClient httpClient;
        private readonly UserResponse _user;
        
        public UserControllerShould(WebApplicationFactory<ObermindPurchaseOrder.Api.Startup> factory)
        {
            httpClient = factory.CreateClient();
            
            _user = new UserResponse
            {
                Id = Guid.NewGuid().ToString("N"),
                Name = "Paul Mensah",
                Username = "paul",
                CreatedAt = DateTime.UtcNow
            };
        }

        [Fact]
        public async Task Return_Unauthorised_When_GettingUserProfile_Without_Token()
        {
            // Act
            var response = await httpClient.GetAsync("api/User/profile");
            var statusCode = response.StatusCode;

            // Assert
            Assert.Equal(HttpStatusCode.Unauthorized, statusCode);
        }

        [Fact]
        public async Task Not_Return_Unauthorised_When_GettingUserProfile_With_Token()
        {
            // Act
            var token = new TokenGenerator().GenerateToken(_user);
            httpClient.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", token);

            var response = await httpClient.GetAsync("api/User/profile");
            var statusCode = response.StatusCode;
            
            // Assert
            Assert.NotEqual(HttpStatusCode.Unauthorized, statusCode);
        }
    }
}