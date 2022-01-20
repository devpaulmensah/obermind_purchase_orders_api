using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;

namespace ObermindPurchaseOrder.Api.Models.Requests
{
    public class PurchaseOrderRequest
    {
        [Required(AllowEmptyStrings = false)]
        public string Name { get; set; }
        [Required(AllowEmptyStrings = false)]
        [RegularExpression("Draft|Submitted")]
        public string Status { get; set; }
        [MinLength(1)]
        public List<LineItem> LineItems { get; set; } = new List<LineItem>();
    }
}