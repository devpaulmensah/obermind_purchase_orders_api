using System;
using System.Collections.Generic;
using System.Net;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Text;
using System.Threading.Tasks;
using Bogus;
using Microsoft.AspNetCore.Mvc.Testing;
using Newtonsoft.Json;
using ObermindPurchaseOrder.Api.Helpers;
using ObermindPurchaseOrder.Api.Models;
using ObermindPurchaseOrder.Api.Models.Requests;
using ObermindPurchaseOrder.Api.Models.Responses;
using Xunit;

namespace ObermindPurchaseOrder.Api.Tests.IntegrationTests
{
    public class PurchasOrderControllerShould : IClassFixture<WebApplicationFactory<ObermindPurchaseOrder.Api.Startup>>
    {
        private readonly HttpClient httpClient;
        private readonly UserResponse _user;
        private readonly Faker _faker;

        public PurchasOrderControllerShould(WebApplicationFactory<ObermindPurchaseOrder.Api.Startup> factory)
        {
            httpClient = factory.CreateClient();
            
            _user = new UserResponse
            {
                Id = Guid.NewGuid().ToString("N"),
                Name = "Paul Mensah",
                Username = "paul",
                CreatedAt = DateTime.UtcNow
            };

            _faker = new Faker();
        }

        [Fact]
        public async Task Return_Unauthorized_When_GetPurchaseOrderEndpoint_Is_Called_Without_BearerToken()
        {
            // Arrange
            var id = Guid.NewGuid().ToString("N");
            
            // Act
            var response = await httpClient.GetAsync($"api/PurchaseOrder/{id}");
            var statusCode = response.StatusCode;

            // Assert
            Assert.Equal(HttpStatusCode.Unauthorized, statusCode);
        }
        
        [Fact]
        public async Task Return_Unauthorized_When_GetPurchaseOrdersEndpoint_Is_Called_Without_BearerToken()
        {
            // Act
            var response = await httpClient.GetAsync($"api/PurchaseOrder");
            var statusCode = response.StatusCode;

            // Assert
            Assert.Equal(HttpStatusCode.Unauthorized, statusCode);
        }

        [Theory]
        [InlineData("{ Name = '', Status = 'Draft', LineItems = [{ Name = 'Item 1', Amount: 200.50 }]  }")]
        [InlineData("{ Name = 'Purchase For Cameras', Status = '', LineItems = [{ Name = 'Item 1', Amount: 200.50 }]  }")]
        public async Task Return_BadRequest_When_Creating_PurchaseOrders_Without_Required_Fields(string request)
        {
            // Arrange
            var purchaseOrderRequest = new StringContent(request, Encoding.UTF8, "application/json");
            
            // Act
            var token = new TokenGenerator().GenerateToken(_user);
            httpClient.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", token);
            
            var response = await httpClient.PostAsync("api/PurchaseOrder",purchaseOrderRequest);

            // Assert
            Assert.Equal(HttpStatusCode.BadRequest, response.StatusCode);
        }

        [Theory]
        [InlineData("{ Name = 'Purchase For Cameras', Status = 'New', LineItems = [{ Name = 'Item 1', Amount: 200.50 }]  }")]
        public async Task Return_BadRequest_When_Creating_PurchaseOrders_With_Invalid_Status(string request)
        {
            // Arrange
            var purchaseOrderRequest = new StringContent(request, Encoding.UTF8, "application/json");
            
            // Act
            var token = new TokenGenerator().GenerateToken(_user);
            httpClient.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", token);
            
            var response = await httpClient.PostAsync("api/PurchaseOrder",purchaseOrderRequest);

            // Assert
            Assert.Equal(HttpStatusCode.BadRequest, response.StatusCode);
        }
        
        [Theory]
        [InlineData("{ Name = 'Purchase For Cameras', Status = 'Submitted', LineItems = [{ Name = 'Item 1', Amount: 200.50 }]  }")]
        [InlineData("{ Name = 'Purchase For Cameras', Status = 'Rejected', LineItems = [{ Name = 'Item 1', Amount: 200.50 }]  }")]
        public async Task Return_BadRequest_When_Creating_PurchaseOrders_With_Status_Other_Than_Draft(string request)
        {
            // Arrange
            var purchaseOrderRequest = new StringContent(request, Encoding.UTF8, "application/json");
            
            // Act
            var token = new TokenGenerator().GenerateToken(_user);
            httpClient.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", token);
            
            var response = await httpClient.PostAsync("api/PurchaseOrder",purchaseOrderRequest);

            // Assert
            Assert.Equal(HttpStatusCode.BadRequest, response.StatusCode);
        }
        
        [Theory]
        [InlineData("{ Name = 'Purchase For Cameras', Status = 'Draft', LineItems = []  }")]
        public async Task Return_BadRequest_When_Creating_PurchaseOrder_With_No_LineItem(string request)
        {
            // Arrange
            var purchaseOrderRequest = new StringContent(request, Encoding.UTF8, "application/json");
            
            // Act
            var token = new TokenGenerator().GenerateToken(_user);
            httpClient.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", token);
            
            var response = await httpClient.PostAsync("api/PurchaseOrder",purchaseOrderRequest);

            // Assert
            Assert.Equal(HttpStatusCode.BadRequest, response.StatusCode);
        }
        
        [Fact]
        public async Task Return_Unauthorized_When_UpdatePurchaseOrderEndpoint_Is_Called_Without_BearerToken()
        {
            var purchaseOrderRequest = new PurchaseOrderRequest
            {
                Name = _faker.Random.Words(3),
                Status = CommonConstants.Draft,
                LineItems = new List<LineItem>
                {
                    new LineItem { Name = _faker.Commerce.ProductName(), Amount = 10000 }
                }
            };
            
            // Act
            var content = new StringContent(JsonConvert.SerializeObject(purchaseOrderRequest), Encoding.UTF8, "application/json");
            var response = await httpClient.PutAsync($"api/PurchaseOrder/{Guid.NewGuid().ToString("N")}", content );
            var statusCode = response.StatusCode;

            // Assert
            Assert.Equal(HttpStatusCode.Unauthorized, statusCode);
        }
        
        [Fact]
        public async Task Not_Return_Unauthorized_When_UpdatePurchaseOrderEndpoint_Is_Called_With_BearerToken()
        {
            var purchaseOrderRequest = new PurchaseOrderRequest
            {
                Name = _faker.Random.Words(3),
                Status = CommonConstants.Draft,
                LineItems = new List<LineItem>
                {
                    new LineItem { Name = _faker.Commerce.ProductName(), Amount = 10000 }
                }
            };
            
            // Act
            var token = new TokenGenerator().GenerateToken(_user);
            httpClient.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", token);

            var content = new StringContent(JsonConvert.SerializeObject(purchaseOrderRequest), Encoding.UTF8, "application/json");
            var response = await httpClient.PutAsync($"api/PurchaseOrder/{Guid.NewGuid().ToString("N")}", content );
            var statusCode = response.StatusCode;

            // Assert
            Assert.NotEqual(HttpStatusCode.Unauthorized, statusCode);
        }
    }
}