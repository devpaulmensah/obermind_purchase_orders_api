using System.Threading.Tasks;
using ObermindPurchaseOrder.Api.Models.Requests;
using ObermindPurchaseOrder.Api.Models.Responses;

namespace ObermindPurchaseOrder.Api.Services.Interfaces
{
    public interface IUserService
    {
        Task<BaseResponse<UserResponse>> CreateUser(CreateUserRequest request);
        Task<BaseResponse<UserResponse>> GetUser(string username);
    }
}