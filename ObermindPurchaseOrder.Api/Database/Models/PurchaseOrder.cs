using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using ObermindPurchaseOrder.Api.Models;

namespace ObermindPurchaseOrder.Api.Database.Models
{
    public class PurchaseOrder
    {
        [Key, DatabaseGenerated(DatabaseGeneratedOption.Identity)]
        public string Id { get; set; } = Guid.NewGuid().ToString("N");
        public string Username { get; set; }
        public string Name { get; set; }
        public string Status { get; set; }
        [Column(TypeName = "jsonb")] 
        public List<LineItem> LineItems { get; set; } = new List<LineItem>();
        public decimal TotalAmount { get; set; }
        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
        public DateTime? UpdatedAt { get; set; }
    }
}