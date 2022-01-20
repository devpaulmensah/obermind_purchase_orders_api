using System.Net.Mime;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using ObermindPurchaseOrder.Api.Helpers;
using ObermindPurchaseOrder.Api.Models.Responses;
using ObermindPurchaseOrder.Api.Services.Interfaces;
using Swashbuckle.AspNetCore.Annotations;

namespace ObermindPurchaseOrder.Api.Controllers
{
    [ApiController]
    [Authorize(AuthenticationSchemes = "Bearer")]
    [Route("api/[controller]")]
    [ProducesResponseType(StatusCodes.Status500InternalServerError, Type = typeof(BaseResponse<EmptyResponse>))]
    public class UserController : ControllerBase
    {
        private readonly IUserService _userService;

        public UserController(IUserService userService)
        {
            _userService = userService;
        }

        /// <summary>
        /// Get signed in user's profile
        /// </summary>
        /// <returns></returns>
        [HttpGet("profile")]
        [Produces(MediaTypeNames.Application.Json)]
        [ProducesResponseType(StatusCodes.Status200OK, Type = typeof(BaseResponse<LoginResponse>))]
        [ProducesResponseType(StatusCodes.Status404NotFound, Type = typeof(BaseResponse<EmptyResponse>))]
        [ProducesResponseType(StatusCodes.Status401Unauthorized, Type = typeof(BaseResponse<EmptyResponse>))]
        [SwaggerOperation("Get signed in user's profile", OperationId = nameof(GetProfile))]
        public async Task<IActionResult> GetProfile()
        {
            var response = await _userService.GetUser(User.GetUserData().Username);

            return !200.Equals(response.Code)
                ? StatusCode(response.Code, response)
                : Ok(response);
        }
    }
}