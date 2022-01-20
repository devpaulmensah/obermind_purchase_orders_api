namespace ObermindPurchaseOrder.Api.Models.Responses
{
    public class LoginResponse
    {
        public string Token { get; set; }
        public int? Expiry { get; set; }
        public UserResponse User { get; set; }
    }
}