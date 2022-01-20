using Bogus;
using ObermindPurchaseOrder.Api.Helpers;
using ObermindPurchaseOrder.Api.Models;
using ObermindPurchaseOrder.Api.Models.Requests;
using Xunit;

namespace ObermindPurchaseOrder.Api.Tests.UnitTests
{
    public class PurchaseOrderHelperShould
    {
        private readonly Faker _faker = new Faker();
        private readonly PurchaseOrderRequest _purchaseOrderRequest;

        public PurchaseOrderHelperShould()
        {
            _purchaseOrderRequest = new PurchaseOrderRequest
            {
                Name = _faker.Random.Words(3),
                Status = CommonConstants.Draft,
            };
        }
        
        [Theory]
        [InlineData(0, true)]
        [InlineData(2, false)]
        public void Check_If_PurchaseOrderHasNoLineItems(int lineItemsCount, bool expectedResult)
        {
            // Arrange
            for (int i = 0; i < lineItemsCount; i++)
            {
                _purchaseOrderRequest.LineItems.Add(new LineItem
                {
                    Name = _faker.Commerce.ProductName(),
                    Amount = 10
                });
            }

            // Act
            var hasNoLineItems = _purchaseOrderRequest.HasNoLineItems();

            // Assert
            Assert.Equal(expectedResult, hasNoLineItems);
        }

        [Theory]
        [InlineData(5, 10, false)]
        [InlineData(10, 10, false)]
        [InlineData(25, 10, true)]
        public void Check_If_PurchaseOrderLineItems_Exceeds_TotalNumberOfLineItemsAllowed(int lineItemsCount, 
            int lineItemsLimit, 
            bool expectedResult)
        {
            // Arrange
            for (int i = 0; i < lineItemsCount; i++)
            {
                _purchaseOrderRequest.LineItems.Add(new LineItem
                {
                    Name = _faker.Commerce.ProductName(),
                    Amount = 10
                });
            }
            
            // Act
            var result = _purchaseOrderRequest.HasExceededTotalNumberOfLineItemsAllowed(lineItemsLimit);

            // Assert
            Assert.Equal(expectedResult, result);
        }

        [Theory]
        [InlineData(1500, 1000, true)]
        [InlineData(1000, 1000, false)]
        [InlineData(1000.01, 1000, true)]
        public void Check_If_TotalAmount_Of_PurchaseOrder_LineItems_Exceeds_TotalLineItemsAmountLimit(decimal totalAmount,
            decimal purchaseOrderTotalAmount, 
            bool expectedResult)
        {
            // Arrange
            _purchaseOrderRequest.LineItems.Add(new LineItem
            {
                Name = _faker.Commerce.ProductName(),
                Amount = totalAmount
            });
            
            // Act
            var result = _purchaseOrderRequest.HasExceededTotalLineItemsAmountLimit(purchaseOrderTotalAmount);

            // Assert
            Assert.Equal(expectedResult, result);
        }
    }
}