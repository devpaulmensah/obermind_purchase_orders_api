using AutoMapper;
using ObermindPurchaseOrder.Api.Database.Models;
using ObermindPurchaseOrder.Api.Models.Requests;
using ObermindPurchaseOrder.Api.Models.Responses;

namespace ObermindPurchaseOrder.Api.Mappings
{
    public class AutoMapper : Profile
    {
        public AutoMapper()
        {
            CreateMap<CreateUserRequest, User>().ReverseMap();
            CreateMap<UserResponse, User>().ReverseMap();
            CreateMap<PurchaseOrder, PurchaseOrderRequest>().ReverseMap();
            CreateMap<PurchaseOrder, PurchaseOrderResponse>().ReverseMap();
        }
    }
}