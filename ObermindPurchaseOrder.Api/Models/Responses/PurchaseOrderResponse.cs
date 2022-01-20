using System;
using System.Collections.Generic;

namespace ObermindPurchaseOrder.Api.Models.Responses
{
    public class PurchaseOrderResponse
    {
        public string Id { get; set; }
        public string Username { get; set; }
        public string Name { get; set; }
        public string Status { get; set; }
        public List<LineItem> LineItems { get; set; } = new List<LineItem>();
        public decimal TotalAmount { get; set; }
        public DateTime CreatedAt { get; set; }
        public DateTime? UpdatedAt { get; set; }
        
    }
}