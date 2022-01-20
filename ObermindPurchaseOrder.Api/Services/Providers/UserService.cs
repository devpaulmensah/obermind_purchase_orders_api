using System;
using System.Net;
using System.Threading.Tasks;
using AutoMapper;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using Newtonsoft.Json;
using ObermindPurchaseOrder.Api.Database;
using ObermindPurchaseOrder.Api.Database.Models;
using ObermindPurchaseOrder.Api.Helpers;
using ObermindPurchaseOrder.Api.Models.Requests;
using ObermindPurchaseOrder.Api.Models.Responses;
using ObermindPurchaseOrder.Api.Services.Interfaces;

namespace ObermindPurchaseOrder.Api.Services.Providers
{
    public class UserService : IUserService
    {
        private readonly ILogger<UserService> _logger;
        private readonly ApplicationDbContext _dbContext;
        private readonly IMapper _mapper;

        public UserService(ILogger<UserService> logger,
            ApplicationDbContext dbContext,
            IMapper mapper)
        {
            _logger = logger;
            _dbContext = dbContext;
            _mapper = mapper;
        }
        
        public async Task<BaseResponse<UserResponse>> CreateUser(CreateUserRequest request)
        {
            try
            {
                var userExists = await _dbContext.Users.AsNoTracking()
                    .AnyAsync(u => u.Username.ToLower().Equals(request.Username.ToLower()));

                if (userExists)
                {
                    return new BaseResponse<UserResponse>
                    {
                        Code = (int) HttpStatusCode.Conflict,
                        Message = "Username has already been chosen"
                    };
                }

                if (!request.Password.Equals(request.ConfirmPassword))
                {
                    return new BaseResponse<UserResponse>
                    {
                        Code =  (int) HttpStatusCode.BadRequest,
                        Message = "Passwords don\'t match"
                    };
                }

                var newUser = _mapper.Map<User>(request);
                newUser.Password = BCrypt.Net.BCrypt.HashPassword(request.Password);

                await _dbContext.AddAsync(newUser);
                var saveResponse = await _dbContext.SaveChangesAsync();

                if (saveResponse < 1)
                {
                    _logger.LogError($"An error occured creating a new user" +
                                     $"\nUserDetails => {JsonConvert.SerializeObject(newUser)}");
                    
                    return new BaseResponse<UserResponse>
                    {
                        Code = (int) HttpStatusCode.FailedDependency,
                        Message = CommonConstants.FailedDependencyErrorMessage
                    };
                }

                var userResponse = _mapper.Map<UserResponse>(newUser);
                return new BaseResponse<UserResponse>
                {
                    Code = (int) HttpStatusCode.Created,
                    Message = "Account created successfully",
                    Data = userResponse
                };
            }
            catch (Exception e)
            {
                _logger.LogError(e, $"An error occured creating a user" +
                                    $"\nRequest => {JsonConvert.SerializeObject(request)}");

                return new BaseResponse<UserResponse>
                {
                    Code = (int) HttpStatusCode.InternalServerError,
                    Message = CommonConstants.InternalServerErrorMessage
                };
            }
        }

        public async Task<BaseResponse<UserResponse>> GetUser(string username)
        {
            try
            {
                var user = await _dbContext.Users.AsNoTracking()
                    .FirstOrDefaultAsync(u => u.Username.Equals(username));

                if (user == null)
                {
                    return new BaseResponse<UserResponse>
                    {
                        Code = (int) HttpStatusCode.NotFound,
                        Message = "User not found"
                    };
                }

                var userResponse = _mapper.Map<UserResponse>(user);
                return new BaseResponse<UserResponse>
                {
                    Code = (int) HttpStatusCode.OK,
                    Message = "Retrieved successfully",
                    Data = userResponse
                };
            }
            catch (Exception e)
            {
                _logger.LogError(e, $"An error occured getting user ({username}) details");
                return new BaseResponse<UserResponse>
                {
                    Code = (int)HttpStatusCode.InternalServerError,
                    Message = CommonConstants.InternalServerErrorMessage
                };
            }
        }
    }
}