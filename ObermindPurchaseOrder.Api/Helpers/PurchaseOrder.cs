using System.Linq;
using ObermindPurchaseOrder.Api.Models.Requests;

namespace ObermindPurchaseOrder.Api.Helpers
{
    public static class LineItemsHelper
    {
        public static bool HasNoLineItems(this PurchaseOrderRequest purchaseOrder)
        {
            return purchaseOrder.LineItems.Count < 1;
        }

        public static bool HasExceededTotalLineItemsAmountLimit(this PurchaseOrderRequest purchaseOrder, decimal amountLimit)
        {
            var totalLineItemsAmount = purchaseOrder.LineItems.Sum(li => li.Amount);
            return totalLineItemsAmount > amountLimit;
        }

        public static bool HasExceededTotalNumberOfLineItemsAllowed(this PurchaseOrderRequest purchaseOrder, int maximumNumberOfLineItemsAllowed)
        {
            return purchaseOrder.LineItems.Count > maximumNumberOfLineItemsAllowed;
        }
    }
}