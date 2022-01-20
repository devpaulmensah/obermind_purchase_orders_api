namespace ObermindPurchaseOrder.Api.Configurations
{
    public class PurchaseOrderLimits
    {
        public int MaximumNumberOfLineItemsPerPurchaseOrder { get; set; }
        public int LimitForSubmittedPurchaseOrderPerDay { get; set; }
        public decimal TotalLineItemAmountAllowed { get; set; }
    }
}