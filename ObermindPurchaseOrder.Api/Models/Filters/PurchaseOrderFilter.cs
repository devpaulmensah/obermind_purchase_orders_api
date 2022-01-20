using System;

namespace ObermindPurchaseOrder.Api.Models.Filters
{
    public class PurchaseOrderFilter
    {
        public string Name { get; set; }
        public string Status { get; set; }
        public DateTime? StartDate { get; set; }
        public DateTime? EndDate { get; set; }
        public int PageIndex { get; set; } = 1;
        public int PageSize { get; set; } = 10;
        public string SortOrder { get; set; } = "desc";
    }
}