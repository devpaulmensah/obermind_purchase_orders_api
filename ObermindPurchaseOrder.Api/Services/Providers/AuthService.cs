using System;
using System.Collections.Generic;
using System.IdentityModel.Tokens.Jwt;
using System.Net;
using System.Security.Claims;
using System.Text;
using System.Threading.Tasks;
using AutoMapper;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using Microsoft.IdentityModel.Tokens;
using Newtonsoft.Json;
using ObermindPurchaseOrder.Api.Configurations;
using ObermindPurchaseOrder.Api.Database;
using ObermindPurchaseOrder.Api.Helpers;
using ObermindPurchaseOrder.Api.Models.Requests;
using ObermindPurchaseOrder.Api.Models.Responses;
using ObermindPurchaseOrder.Api.Services.Interfaces;

namespace ObermindPurchaseOrder.Api.Services.Providers
{
    public class AuthService: IAuthService
    {
        private readonly ILogger<AuthService> _logger;
        private readonly ApplicationDbContext _dbContext;
        private readonly IMapper _mapper;
        private readonly BearerTokenConfig _bearerTokenConfig;

        public AuthService(ILogger<AuthService> logger,
            ApplicationDbContext dbContext, 
            IMapper mapper,
            IOptions<BearerTokenConfig> bearerTokenConfig)
        {
            _logger = logger;
            _dbContext = dbContext;
            _mapper = mapper;
            _bearerTokenConfig = bearerTokenConfig.Value;
        }

        /// <summary>
        /// Logs in a user account
        /// </summary>
        /// <param name="request"></param>
        /// <returns></returns>
        public async Task<BaseResponse<LoginResponse>> Login(LoginRequest request)
        {
            try
            {
                var user = await _dbContext.Users.AsNoTracking()
                    .FirstOrDefaultAsync(u => 
                        u.Username.Equals(request.Username));

                if (user == null)
                {
                    return new BaseResponse<LoginResponse>
                    {
                        Code = (int) HttpStatusCode.NotFound,
                        Message = "This account does not exist!"
                    };
                }

                var isPasswordValid = BCrypt.Net.BCrypt.Verify(request.Password, user.Password);
                if (!isPasswordValid)
                {
                    return new BaseResponse<LoginResponse>
                    {
                        Code = (int) HttpStatusCode.Unauthorized,
                        Message = "Incorrect username and password"
                    };
                }
                
                var userResponse = _mapper.Map<UserResponse>(user);
                
                var handler = new JwtSecurityTokenHandler();

                var mySecret = _bearerTokenConfig.Key;
                var mySecurityKey = new SymmetricSecurityKey(Encoding.ASCII.GetBytes(mySecret));
                var now = DateTime.UtcNow;
                
                var claims = new List<Claim>
                {
                    new Claim(ClaimTypes.Thumbprint, JsonConvert.SerializeObject(userResponse))
                };

                var token = new JwtSecurityToken
                (
                    _bearerTokenConfig.Issuer,
                    _bearerTokenConfig.Audience,
                    claims,
                    now.AddMilliseconds(-30),
                    now.AddDays(_bearerTokenConfig.ExpiryDays),
                    new SigningCredentials(mySecurityKey, SecurityAlgorithms.HmacSha256Signature)
                );
            
                var tokenStr = handler.WriteToken(token);
                
                return new BaseResponse<LoginResponse>
                {
                    Code = (int) HttpStatusCode.OK,
                    Message = "Login successful",
                    Data = new LoginResponse
                    {
                        User = userResponse,
                        Token = $"Bearer {tokenStr}",
                        Expiry = token.Payload.Exp
                    }
                };
            }
            catch (Exception e)
            {
                _logger.LogError(e, $"Error occured logging in user");
                
                return new BaseResponse<LoginResponse>
                {
                    Code = (int) HttpStatusCode.InternalServerError,
                    Message = CommonConstants.InternalServerErrorMessage
                };
            }
        }
    }
}