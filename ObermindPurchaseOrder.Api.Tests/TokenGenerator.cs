using System;
using System.Collections.Generic;
using System.IdentityModel.Tokens.Jwt;
using System.IO;
using System.Security.Claims;
using System.Text;
using Microsoft.Extensions.Configuration;
using Microsoft.IdentityModel.Tokens;
using Newtonsoft.Json;
using ObermindPurchaseOrder.Api.Models.Responses;

namespace ObermindPurchaseOrder.Api.Tests
{
    public class TokenGenerator
    {
        private readonly IConfigurationSection _configuration;

        public TokenGenerator()
        {
            _configuration = new ConfigurationBuilder()
                .SetBasePath(Directory.GetCurrentDirectory())
                .AddJsonFile("appsettings.json")
                .Build()
                .GetSection("BearerTokenConfig");
        }

        public string GenerateToken(UserResponse user)
        {
            var key = _configuration["Key"];
            var issuer = _configuration["Issuer"];
            var audience = _configuration["Audience"];

            var tokenHandler = new JwtSecurityTokenHandler();
            var securityKey = new SymmetricSecurityKey(Encoding.ASCII.GetBytes(key));
            var now = DateTime.UtcNow;

            var claims = new List<Claim>
            {
                new Claim(ClaimTypes.Thumbprint, JsonConvert.SerializeObject(user))
            };

            var token = new JwtSecurityToken(
                issuer,
                audience,
                claims,
                now.AddMilliseconds(-30),
                now.AddHours(12),
                new SigningCredentials(securityKey, SecurityAlgorithms.HmacSha256Signature));

            var tokenString = tokenHandler.WriteToken(token);

            return tokenString;
        }
    }
}