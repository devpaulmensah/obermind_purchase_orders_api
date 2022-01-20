using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Threading.Tasks;
using AutoMapper;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using Newtonsoft.Json;
using ObermindPurchaseOrder.Api.Configurations;
using ObermindPurchaseOrder.Api.Database;
using ObermindPurchaseOrder.Api.Database.Models;
using ObermindPurchaseOrder.Api.Helpers;
using ObermindPurchaseOrder.Api.Models;
using ObermindPurchaseOrder.Api.Models.Filters;
using ObermindPurchaseOrder.Api.Models.Requests;
using ObermindPurchaseOrder.Api.Models.Responses;
using ObermindPurchaseOrder.Api.Services.Interfaces;

namespace ObermindPurchaseOrder.Api.Services.Providers
{
    public class PurchaseOrderService : IPurchaseOrderService
    {
        private readonly ILogger<PurchaseOrderService> _logger;
        private readonly ApplicationDbContext _dbContext;
        private readonly IMapper _mapper;
        private readonly PurchaseOrderLimits _purchaseOrderLimits;

        public PurchaseOrderService(ILogger<PurchaseOrderService> logger,
            ApplicationDbContext dbContext,
            IMapper mapper,
            IOptions<PurchaseOrderLimits> purchaseOrderLimits)
        {
            _logger = logger;
            _dbContext = dbContext;
            _mapper = mapper;
            _purchaseOrderLimits = purchaseOrderLimits.Value;
        }
        
        public async Task<BaseResponse<PurchaseOrderResponse>> CreatePurchaseOrder(PurchaseOrderRequest request, UserResponse user)
        {
            try
            {
                if (request.HasNoLineItems())
                {
                    return new BaseResponse<PurchaseOrderResponse>
                    {
                        Code = (int) HttpStatusCode.BadRequest,
                        Message = "You must provide at least 1 line item"
                    };
                }
                
                if (request.HasExceededTotalNumberOfLineItemsAllowed(_purchaseOrderLimits.MaximumNumberOfLineItemsPerPurchaseOrder))
                {
                    return new BaseResponse<PurchaseOrderResponse>
                    {
                        Code = (int) HttpStatusCode.BadRequest,
                        Message = $"You cannot submit more than {_purchaseOrderLimits.MaximumNumberOfLineItemsPerPurchaseOrder} line items per purchase order!"
                    };
                }

                if ( request.HasExceededTotalLineItemsAmountLimit(_purchaseOrderLimits.TotalLineItemAmountAllowed))
                {
                    return new BaseResponse<PurchaseOrderResponse>
                    {
                        Code = (int) HttpStatusCode.BadRequest,
                        Message = $"Your total amount for line items must not exceed {_purchaseOrderLimits.TotalLineItemAmountAllowed}"
                    };
                }
                
                var userSubmittedPurchaseOrdersTodayList = await GetUserSubmittedPurchaseOrdersForToday(user.Username);

                var userHasReachedPurchaseOrderLimitPerDay =
                    userSubmittedPurchaseOrdersTodayList
                        .UserHasExceededPurchaseOrderPerDayLimit(_purchaseOrderLimits.LimitForSubmittedPurchaseOrderPerDay);

                if (CommonConstants.Submitted.Equals(request.Status) &&  userHasReachedPurchaseOrderLimitPerDay)
                {
                    return new BaseResponse<PurchaseOrderResponse>
                    {
                        Code = (int) HttpStatusCode.Conflict,
                        Message = $"You have reached your limit of {_purchaseOrderLimits.LimitForSubmittedPurchaseOrderPerDay} submitted purchase order per day!"
                    };
                }
                
                var newPurchaseOrder = _mapper.Map<PurchaseOrder>(request);
                newPurchaseOrder.TotalAmount = request.LineItems.Sum(li => li.Amount);
                newPurchaseOrder.Username = user.Username;

                await _dbContext.AddAsync(newPurchaseOrder);
                var saveResponse = await _dbContext.SaveChangesAsync();

                if (saveResponse < 1)
                {
                    _logger.LogError($"An error occured creating a purchase order" +
                                     $"\nRequest => {JsonConvert.SerializeObject(request)}");
                    
                    return new BaseResponse<PurchaseOrderResponse>
                    {
                        Code = (int) HttpStatusCode.FailedDependency,
                        Message = CommonConstants.FailedDependencyErrorMessage
                    };
                }
                
                var purchaseOrderResponse = _mapper.Map<PurchaseOrderResponse>(newPurchaseOrder);
                return new BaseResponse<PurchaseOrderResponse>
                {
                    Code = (int)HttpStatusCode.Created,
                    Message = "Purchase order created successfully",
                    Data = purchaseOrderResponse
                };
            }
            catch (Exception e)
            {
                _logger.LogError(e, $"An error occured creating a purchase order" +
                                    $"\nRequest => {request}");

                return new BaseResponse<PurchaseOrderResponse>
                {
                    Code = (int) HttpStatusCode.InternalServerError,
                    Message = CommonConstants.InternalServerErrorMessage
                };
            }
        }

        public async Task<BaseResponse<PurchaseOrderResponse>> GetPurchaseOrder(string purchaseOrderId)
        {
            try
            {
                var purchaseOrder = await _dbContext.PurchaseOrders.AsNoTracking()
                    .FirstOrDefaultAsync(po => po.Id.Equals(purchaseOrderId));

                if (purchaseOrder == null)
                {
                    return new BaseResponse<PurchaseOrderResponse>
                    {
                        Code = (int)HttpStatusCode.NotFound,
                        Message = "Purchase order not found"
                    };
                }

                var purchaseOrderResponse = _mapper.Map<PurchaseOrderResponse>(purchaseOrder);
                return new BaseResponse<PurchaseOrderResponse>
                {
                    Code = (int) HttpStatusCode.OK,
                    Message = "Retrieved successfully",
                    Data = purchaseOrderResponse
                };
            }
            catch (Exception e)
            {
                _logger.LogError(e, $"An error occured getting a purchase order with id => {purchaseOrderId}");

                return new BaseResponse<PurchaseOrderResponse>
                {
                    Code = (int) HttpStatusCode.InternalServerError,
                    Message = CommonConstants.InternalServerErrorMessage
                };
            }
        }

        public async Task<BaseResponse<PaginatedList<PurchaseOrderResponse>>> GetPurchaseOrders(
            PurchaseOrderFilter filter, UserResponse user)
        {
            try
            {
                filter.PageIndex = filter.PageIndex < 1 ? 1 : filter.PageIndex;
                filter.PageSize = filter.PageSize < 1 ? 10 : filter.PageSize;

                var filterQueryableResponse = await FilterPurchaseOrder(filter, user.Username);
                var filterResponse = await filterQueryableResponse.Paginate(filter.PageIndex, filter.PageSize);

                var purchaseOrderResponseList = filterResponse.Data.Select(po =>
                    _mapper.Map<PurchaseOrderResponse>(po)).ToList();
                
                return new BaseResponse<PaginatedList<PurchaseOrderResponse>>
                {
                    Code = (int)HttpStatusCode.OK,
                    Message = "Retrieved successfully",
                    Data = new PaginatedList<PurchaseOrderResponse>
                    {
                        Data  = purchaseOrderResponseList,
                        PageIndex = filterResponse.PageIndex,
                        PageSize = filterResponse.PageSize,
                        TotalPages = filterResponse.TotalPages,
                        TotalRecords = filterResponse.TotalRecords
                    }
                };
            }
            catch (Exception e)
            {
                _logger.LogError(e, $"An error occured getting purchase orders" +
                                    $"\nFilters => {JsonConvert.SerializeObject(filter)}");

                return new BaseResponse<PaginatedList<PurchaseOrderResponse>>
                {
                    Code = (int) HttpStatusCode.InternalServerError,
                    Message = CommonConstants.InternalServerErrorMessage
                };
            }
        }

        public async Task<BaseResponse<PurchaseOrderResponse>> UpdatePurchaseOrder(string purchaseOrderId, PurchaseOrderRequest request, UserResponse user)
        {
            try
            {
                if (request.HasNoLineItems())
                {
                    return new BaseResponse<PurchaseOrderResponse>
                    {
                        Code = (int) HttpStatusCode.BadRequest,
                        Message = "You must provide at least 1 line item"
                    };
                }
                
                if (request.HasExceededTotalNumberOfLineItemsAllowed(_purchaseOrderLimits.MaximumNumberOfLineItemsPerPurchaseOrder))
                {
                    return new BaseResponse<PurchaseOrderResponse>
                    {
                        Code = (int) HttpStatusCode.BadRequest,
                        Message = $"You cannot submit more than {_purchaseOrderLimits.MaximumNumberOfLineItemsPerPurchaseOrder} line items per purchase order!"
                    };
                }

                if ( request.HasExceededTotalLineItemsAmountLimit(_purchaseOrderLimits.TotalLineItemAmountAllowed))
                {
                    return new BaseResponse<PurchaseOrderResponse>
                    {
                        Code = (int) HttpStatusCode.BadRequest,
                        Message = $"Your total amount for line items must not exceed {_purchaseOrderLimits.TotalLineItemAmountAllowed}"
                    };
                }
                
                var purchaseOrder =
                    await _dbContext.PurchaseOrders.FirstOrDefaultAsync(po => 
                        po.Id.Equals(purchaseOrderId) && po.Username.Equals(user.Username));

                if (purchaseOrder == null)
                {
                    return new BaseResponse<PurchaseOrderResponse>
                    {
                        Code = (int) HttpStatusCode.NotFound,
                        Message = "Purchase order not found"
                    };
                }
                
                var userSubmittedPurchaseOrdersTodayList = await GetUserSubmittedPurchaseOrdersForToday(user.Username);

                var userHasReachedPurchaseOrderLimitPerDay =
                    userSubmittedPurchaseOrdersTodayList
                        .UserHasExceededPurchaseOrderPerDayLimit(_purchaseOrderLimits.LimitForSubmittedPurchaseOrderPerDay);
                
                if (CommonConstants.Submitted.Equals(request.Status) &&  userHasReachedPurchaseOrderLimitPerDay)
                {
                    return new BaseResponse<PurchaseOrderResponse>
                    {
                        Code = (int) HttpStatusCode.Conflict,
                        Message = $"You have reached your limit of {_purchaseOrderLimits.LimitForSubmittedPurchaseOrderPerDay} submitted purchase order per day!"
                    };
                }
                
                if (CommonConstants.Draft.Equals(purchaseOrder.Status))
                {
                    _mapper.Map(request, purchaseOrder);
                    purchaseOrder.TotalAmount = request.LineItems.Sum(li => li.Amount);
                }
                else
                {
                    purchaseOrder.Name = request.Name;
                    purchaseOrder.Status = request.Status;
                }

                purchaseOrder.UpdatedAt = DateTime.UtcNow;

                _dbContext.PurchaseOrders.Update(purchaseOrder);
                var updateResponse = await _dbContext.SaveChangesAsync();
                
                if (updateResponse < 1)
                {
                    _logger.LogError($"An error occured updating a purchase order" +
                                     $"\nRequest => {JsonConvert.SerializeObject(request)}");
                    
                    return new BaseResponse<PurchaseOrderResponse>
                    {
                        Code = (int) HttpStatusCode.FailedDependency,
                        Message = CommonConstants.FailedDependencyErrorMessage
                    };
                }
                
                var purchaseOrderResponse = _mapper.Map<PurchaseOrderResponse>(purchaseOrder);
                return new BaseResponse<PurchaseOrderResponse>
                {
                    Code = (int)HttpStatusCode.OK,
                    Message = "Purchase order updated successfully",
                    Data = purchaseOrderResponse
                };
            }
            catch (Exception e)
            {
                _logger.LogError(e, $"An error occured updating purchase orders" +
                                    $"\nRequest => {JsonConvert.SerializeObject(request)}");

                return new BaseResponse<PurchaseOrderResponse>
                {
                    Code = (int) HttpStatusCode.InternalServerError,
                    Message = CommonConstants.InternalServerErrorMessage
                };
            }
        }

        private async Task<IQueryable<PurchaseOrder>> FilterPurchaseOrder(PurchaseOrderFilter filter, string username)
        {
            await Task.Delay(0);
            var filterQuery = _dbContext.PurchaseOrders.AsNoTracking().AsQueryable();

            if (!string.IsNullOrEmpty(username))
            {
                filterQuery = filterQuery.Where(po => po.Username.Equals(username));
            }

            if (!string.IsNullOrEmpty(filter.Name))
            {
                filterQuery = filterQuery.Where(po => po.Name.ToLower().Contains(filter.Name.ToLower()));
            }

            if (!string.IsNullOrEmpty(filter.Status))
            {
                filterQuery = filterQuery.Where(po => po.Status.Equals(filter.Status));
            }
            
            if (filter.StartDate.HasValue)
            {
                filterQuery = filterQuery.Where(po => po.CreatedAt >= filter.StartDate.Value);
            }
            
            if (filter.EndDate.HasValue)
            {
                filterQuery = filterQuery.Where(po => po.CreatedAt <= filter.StartDate.Value);
            }

            filterQuery = !string.IsNullOrEmpty(filter.SortOrder) && filter.SortOrder == "desc"
                ? filterQuery.OrderByDescending(po => po.CreatedAt)
                : filterQuery.OrderBy(po => po.CreatedAt);

            return filterQuery;
        }

        private async Task<List<PurchaseOrder>> GetUserSubmittedPurchaseOrdersForToday(string username)
        {
            var userPurchaseOrderTodayList = await _dbContext.PurchaseOrders.AsNoTracking()
                .Where(po => po.Status.Equals(CommonConstants.Submitted)
                             && po.Username.Equals(username)
                             && po.CreatedAt.Date == DateTime.UtcNow.Date)
                .ToListAsync();

            return userPurchaseOrderTodayList;
        }
    }
}