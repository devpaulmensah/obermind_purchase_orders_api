using System.Net.Mime;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using ObermindPurchaseOrder.Api.Helpers;
using ObermindPurchaseOrder.Api.Models;
using ObermindPurchaseOrder.Api.Models.Filters;
using ObermindPurchaseOrder.Api.Models.Requests;
using ObermindPurchaseOrder.Api.Models.Responses;
using ObermindPurchaseOrder.Api.Services.Interfaces;
using Swashbuckle.AspNetCore.Annotations;

namespace ObermindPurchaseOrder.Api.Controllers
{
    [ApiController]
    [Authorize(AuthenticationSchemes = "Bearer")]
    [Route("api/[controller]")]
    [ProducesResponseType(StatusCodes.Status500InternalServerError, Type = typeof(BaseResponse<EmptyResponse>))]
    public class PurchaseOrderController : ControllerBase
    {
        private readonly IPurchaseOrderService _purchaseOrderService;

        public PurchaseOrderController(IPurchaseOrderService purchaseOrderService)
        {
            _purchaseOrderService = purchaseOrderService;
        }

        /// <summary>
        /// Create a purchase order
        /// </summary>
        /// <param name="request"></param>
        /// <returns></returns>
        [HttpPost]
        [Consumes(MediaTypeNames.Application.Json)]
        [Produces(MediaTypeNames.Application.Json)]
        [ProducesResponseType(StatusCodes.Status201Created, Type = typeof(BaseResponse<PurchaseOrderResponse>))]
        [ProducesResponseType(StatusCodes.Status400BadRequest, Type = typeof(BaseResponse<EmptyResponse>))]
        [ProducesResponseType(StatusCodes.Status409Conflict, Type = typeof(BaseResponse<EmptyResponse>))]
        [ProducesResponseType(StatusCodes.Status424FailedDependency, Type = typeof(BaseResponse<EmptyResponse>))]
        [SwaggerOperation("Create a purchase order", OperationId = nameof(CreatePurchaseOrder))]
        public async Task<IActionResult> CreatePurchaseOrder([FromBody] PurchaseOrderRequest request)
        {
            var response = await _purchaseOrderService.CreatePurchaseOrder(request, User.GetUserData());

            if (!201.Equals(response.Code))
            {
                return StatusCode(response.Code, response);
            }

            var contextRequest = HttpContext.Request;
            var url = $"{contextRequest.Scheme}://{contextRequest.Host}{contextRequest.Path}/{response.Data.Id}";

            return Created(url, response);
        }

        /// <summary>
        /// Get purchase order details
        /// </summary>
        /// <param name="purchaseOrderId"></param>
        /// <returns></returns>
        [HttpGet("{purchaseOrderId}")]
        [Produces(MediaTypeNames.Application.Json)]
        [ProducesResponseType(StatusCodes.Status200OK, Type = typeof(BaseResponse<PurchaseOrderResponse>))]
        [ProducesResponseType(StatusCodes.Status404NotFound, Type = typeof(BaseResponse<EmptyResponse>))]
        [SwaggerOperation("Get purchase order details", OperationId = nameof(GetPurchaseOrder))]
        public async Task<IActionResult> GetPurchaseOrder([FromRoute] string purchaseOrderId)
        {
            var response = await _purchaseOrderService.GetPurchaseOrder(purchaseOrderId);

            return !200.Equals(response.Code)
                ? StatusCode(response.Code, response)
                : Ok(response);
        }
        
        /// <summary>
        /// Get paginated list of purchase orders
        /// </summary>
        /// <param name="filter"></param>
        /// <returns></returns>
        [HttpGet]
        [Produces(MediaTypeNames.Application.Json)]
        [ProducesResponseType(StatusCodes.Status200OK, Type = typeof(BaseResponse<PaginatedList<PurchaseOrderResponse>>))]
        [SwaggerOperation("Get paginated list of purchase orders", OperationId = nameof(GetPurchaseOrders))]
        public async Task<IActionResult> GetPurchaseOrders([FromQuery] PurchaseOrderFilter filter)
        {
            var response = await _purchaseOrderService.GetPurchaseOrders(filter, User.GetUserData());

            return !200.Equals(response.Code)
                ? StatusCode(response.Code, response)
                : Ok(response);
        }

        /// <summary>
        /// Update a purchase order
        /// </summary>
        /// <param name="purchaseOrderId"></param>
        /// <param name="request"></param>
        /// <returns></returns>
        [HttpPut("{purchaseOrderId}")]
        [Consumes(MediaTypeNames.Application.Json)]
        [Produces(MediaTypeNames.Application.Json)]
        [ProducesResponseType(StatusCodes.Status201Created, Type = typeof(BaseResponse<PurchaseOrderResponse>))]
        [ProducesResponseType(StatusCodes.Status400BadRequest, Type = typeof(BaseResponse<EmptyResponse>))]
        [ProducesResponseType(StatusCodes.Status409Conflict, Type = typeof(BaseResponse<EmptyResponse>))]
        [ProducesResponseType(StatusCodes.Status424FailedDependency, Type = typeof(BaseResponse<EmptyResponse>))]
        [SwaggerOperation("Update a purchase order", OperationId = nameof(UpdatePurchaseOrder))]
        public async Task<IActionResult> UpdatePurchaseOrder([FromRoute] string purchaseOrderId, [FromBody] PurchaseOrderRequest request)
        {
            var response = await _purchaseOrderService.UpdatePurchaseOrder(purchaseOrderId, request, User.GetUserData());

            return !200.Equals(response.Code)
                ? StatusCode(response.Code, response)
                : Ok(response);
        }
    }
}