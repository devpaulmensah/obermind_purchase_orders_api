using System.Collections.Generic;
using System.Linq;
using System.Security.Claims;
using Newtonsoft.Json;
using ObermindPurchaseOrder.Api.Database.Models;
using ObermindPurchaseOrder.Api.Models.Responses;

namespace ObermindPurchaseOrder.Api.Helpers
{
    public static class UserHelper
    {
        public static bool UserHasExceededPurchaseOrderPerDayLimit(this List<PurchaseOrder> submittedPurchaseOrderList,
            int limitPerDay)
        {
            return submittedPurchaseOrderList.Count > limitPerDay;
        }
        
        public static UserResponse GetUserData(this ClaimsPrincipal claims)
        {
            var claimsIdentity = claims.Identities.FirstOrDefault(i => i.AuthenticationType == "obermind_user");
            var userData = claimsIdentity?.FindFirst(ClaimTypes.Thumbprint);

            if (userData == null)
            {
                return new UserResponse();
            }

            var user = JsonConvert.DeserializeObject<UserResponse>(userData.Value);
            return user;
        }
    }
}