using System.ComponentModel.DataAnnotations;

namespace ObermindPurchaseOrder.Api.Models.Requests
{
    public class LoginRequest
    {
        [Required(AllowEmptyStrings = false)]
        public string Username { get; set; }
        [Required(AllowEmptyStrings = false)]
        [DataType(DataType.Password)]
        public string Password { get; set; }
    }
}