using System.Threading.Tasks;
using ObermindPurchaseOrder.Api.Models;
using ObermindPurchaseOrder.Api.Models.Filters;
using ObermindPurchaseOrder.Api.Models.Requests;
using ObermindPurchaseOrder.Api.Models.Responses;

namespace ObermindPurchaseOrder.Api.Services.Interfaces
{
    public interface IPurchaseOrderService
    {
        Task<BaseResponse<PurchaseOrderResponse>> CreatePurchaseOrder(PurchaseOrderRequest request, UserResponse user);
        Task<BaseResponse<PurchaseOrderResponse>> GetPurchaseOrder(string purchaseOrderId);
        Task<BaseResponse<PaginatedList<PurchaseOrderResponse>>> GetPurchaseOrders(PurchaseOrderFilter filter, UserResponse user);
        Task<BaseResponse<PurchaseOrderResponse>> UpdatePurchaseOrder(string purchaseOrderId, PurchaseOrderRequest request, UserResponse user);
    }
}