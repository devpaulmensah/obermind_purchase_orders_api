using System;
using System.Collections.Generic;
using ObermindPurchaseOrder.Api.Database.Models;
using ObermindPurchaseOrder.Api.Helpers;
using ObermindPurchaseOrder.Api.Models.Responses;
using Xunit;

namespace ObermindPurchaseOrder.Api.Tests.UnitTests
{
    public class UserHelperShould
    {
        [Theory]
        [InlineData(5, 5, false)]
        [InlineData(10, 5, true)]
        [InlineData(4, 5, false)]
        public void Check_If_UserHasExceededPurchaseOrderPerDayLimit(int numberOfSubmittedPurchaseOrdersToday, 
            int purchaseOrderLimitPerDay, 
            bool expectedResult)
        {
            // Arrange
            List<PurchaseOrder> purchaseOrderList = new List<PurchaseOrder>();
            for (int i = 0; i < numberOfSubmittedPurchaseOrdersToday; i++)
            {
                purchaseOrderList.Add(new PurchaseOrder());
            }
            
            // Act
            var result = purchaseOrderList.UserHasExceededPurchaseOrderPerDayLimit(purchaseOrderLimitPerDay);

            // Assert
            Assert.Equal(expectedResult, result);
        }
        
        [Fact]
        public void Check_If_GenerateToken_DoesNotReturnNull()
        {
            // Arrange
            var user = new UserResponse
            {
                Id = Guid.NewGuid().ToString("N"),
                Name = "Paul Mensah",
                Username = "paul",
                CreatedAt = DateTime.UtcNow
            };
            
            // Act
            var token = new TokenGenerator().GenerateToken(user);
            
            // Assert
            Assert.NotNull(token);
        }
    }
}