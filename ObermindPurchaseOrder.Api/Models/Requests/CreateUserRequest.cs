using System.ComponentModel.DataAnnotations;

namespace ObermindPurchaseOrder.Api.Models.Requests
{
    public class CreateUserRequest
    {
        [Required(AllowEmptyStrings = false)]
        public string Name { get; set; }
        [MinLength(4)]
        [Required(AllowEmptyStrings = false)]
        public string Username { get; set; }
        [MinLength(6)]
        [Required(AllowEmptyStrings = false)]
        [DataType(DataType.Password)]
        public string Password { get; set; }
        [MinLength(6)]
        [Required(AllowEmptyStrings = false)]
        [DataType(DataType.Password)]
        [Compare("Password", ErrorMessage = "Passwords do not match")]
        public string ConfirmPassword { get; set; }
    }
}