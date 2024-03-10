
using System.IdentityModel.Tokens.Jwt;
using Microsoft.AspNetCore.Authentication;
using VinhUni_Educator_API.Interfaces;
using VinhUni_Educator_API.Utils;

namespace VinhUni_Educator_API.Middlewares
{
    public class VerifyRevokedToken
    {
        private readonly RequestDelegate _next;
        public VerifyRevokedToken(RequestDelegate next)
        {
            _next = next;
        }
        public async Task InvokeAsync(HttpContext context, ICacheServices cacheServices, IJwtServices jwtServices)
        {
            var accessToken = await context.GetTokenAsync("access_token");
            if (accessToken is null)
            {
                await _next(context);
                return;
            }
            var tokenClaims = jwtServices.GetTokenClaims(accessToken);
            var tokenId = tokenClaims?.FirstOrDefault(x => x.Type == JwtRegisteredClaimNames.Jti)?.Value;
            var revokedToken = await cacheServices.GetDataAsync<string>($"RevokedID:{tokenId}");
            if (revokedToken != null && revokedToken == accessToken)
            {
                context.Response.StatusCode = 401;
                await context.Response.WriteAsJsonAsync(new ActionResponse
                {
                    StatusCode = 401,
                    IsSuccess = false,
                    Message = "You are not authorized, please login to get access"
                });
                return;
            }
            await _next(context);
        }
    }
}
