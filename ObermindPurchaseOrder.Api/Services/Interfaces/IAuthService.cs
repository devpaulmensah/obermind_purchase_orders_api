using System.Threading.Tasks;
using ObermindPurchaseOrder.Api.Models.Requests;
using ObermindPurchaseOrder.Api.Models.Responses;

namespace ObermindPurchaseOrder.Api.Services.Interfaces
{
    public interface IAuthService
    {
        Task<BaseResponse<LoginResponse>> Login(LoginRequest request);
    }
}