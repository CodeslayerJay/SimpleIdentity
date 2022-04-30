using Identity.Models;
using Microsoft.Extensions.Configuration;
using Microsoft.IdentityModel.Tokens;
using System;
using System.Collections.Generic;
using System.IdentityModel.Tokens.Jwt;
using System.Linq;
using System.Security.Claims;
using System.Security.Cryptography;
using System.Text;
using System.Threading.Tasks;

namespace Identity.Services
{
    public class TokenService : ITokenService
    {
        private readonly IConfiguration _config;
        private readonly string _key;
        private readonly string _validIssuer;
        private readonly string _validAudience;
        private readonly DateTime? _expiresAt = null;
        private readonly int _defaultTokenTimeOut = 20;


        public TokenService(IConfiguration configuration)
        {

            _config = configuration;

            _key = _config.GetSection("TokenSettings:SecurityKey").Value;
            _validIssuer = _config.GetSection("TokenSettings:ValidIssuer").Value;
            _validAudience = _config.GetSection("TokenSettings:ValidAudience").Value;
            _defaultTokenTimeOut = int.Parse(_config.GetSection("TokenSettings:DefaultTimeout").Value);
            _expiresAt = DateTime.Now.AddMinutes(_defaultTokenTimeOut);
        }

        public TokenResult GetTokens(TokenRequest request)
        {
            var result = new TokenResult();
            result.AccessToken = GenerateAccessToken(request.Identifier);
            result.RefreshToken = GenerateRefreshToken(request.UserValue, request.ClientId);

            return result;
        }

        public JsonWebToken GenerateAccessToken(string identifier)
        {
            //Add Claims
            var claims = new[]
            {
                new Claim(JwtRegisteredClaimNames.UniqueName,identifier),
                new Claim(JwtRegisteredClaimNames.Sub, identifier),
                new Claim(JwtRegisteredClaimNames.Jti, Guid.NewGuid().ToString()),
            };

            var creds = new SigningCredentials(GetKey(), SecurityAlgorithms.HmacSha256);

            var token = new JwtSecurityToken(
                _validIssuer,
                _validAudience,
                claims,
                expires: _expiresAt,
                signingCredentials: creds);

            var accessToken = new JsonWebToken()
            {
                Token = new JwtSecurityTokenHandler().WriteToken(token),
                Expires = _defaultTokenTimeOut,
                ExpiresAt = _expiresAt.Value,
                Type = "bearer"
            };

            return accessToken;
        }

        public RefreshToken GenerateRefreshToken(string userValue, string clientId)
        {
            if (string.IsNullOrEmpty(userValue))
                throw new ArgumentNullException(nameof(userValue));

            if (string.IsNullOrEmpty(clientId))
                throw new ArgumentNullException(nameof(clientId));

            return new RefreshToken
            {
                ClientId = clientId,
                UserValue = userValue,
                IssuedAt = DateTime.Now,
                ExpiresAt = DateTime.Now.AddDays(1),
                RefreshKey = GenerateRefreshTokenKey()
            };
        }

        private string GenerateRefreshTokenKey(int size = 32)
        {
            var randomNumber = new byte[size];
            using (var rng = RandomNumberGenerator.Create())
            {
                rng.GetBytes(randomNumber);
                return Convert.ToBase64String(randomNumber);
            }
        }

        private SymmetricSecurityKey GetKey()
        {
            var key = new SymmetricSecurityKey(Encoding.ASCII.GetBytes(_key));
            return key;
        }

        public static SymmetricSecurityKey GetKey(string key)
        {
            if (string.IsNullOrEmpty(key))
                throw new ArgumentNullException(nameof(key));

            return new SymmetricSecurityKey(Encoding.ASCII.GetBytes(key));
        }

        public ClaimsPrincipal GetPrincipalFromExpiredToken(string token)
        {
            var tokenValidationParameters = new TokenValidationParameters
            {
                ValidateAudience = false, //you might want to validate the audience and issuer depending on your use case
                ValidateIssuer = false,
                ValidateIssuerSigningKey = true,
                IssuerSigningKey = GetKey(),
                ValidateLifetime = false //here we are saying that we don't care about the token's expiration date
            };

            var tokenHandler = new JwtSecurityTokenHandler();
            SecurityToken securityToken;
            var principal = tokenHandler.ValidateToken(token, tokenValidationParameters, out securityToken);
            var jwtSecurityToken = securityToken as JwtSecurityToken;
            if (jwtSecurityToken == null || !jwtSecurityToken.Header.Alg.Equals(SecurityAlgorithms.HmacSha256, StringComparison.InvariantCultureIgnoreCase))
                throw new SecurityTokenException("Invalid token");

            return principal;
        }

    }


    public class JsonWebToken
    {
        public string Token { get; set; }
        public string Type { get; set; } = "bearer";
        public int Expires { get; set; }
        public DateTime ExpiresAt { get; set; }
    }
}
