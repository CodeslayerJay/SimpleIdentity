using Identity.Models;
using System.Security.Claims;

namespace Identity.Services
{
    public interface ITokenService
    {
        JsonWebToken GenerateAccessToken(string identifier);
        RefreshToken GenerateRefreshToken(string userValue, string clientId);
        ClaimsPrincipal GetPrincipalFromExpiredToken(string token);
        TokenResult GetTokens(TokenRequest request);
    }
}