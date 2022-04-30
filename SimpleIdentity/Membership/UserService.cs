using Identity.Services;
using Microsoft.AspNetCore.Identity;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Identity.Membership.Common;
using Identity.Membership.Models;
using Identity.Models;

namespace Identity.Membership
{
    public class UserService : IUserService
    {
        private readonly UserManager<IdentityUser> _userManger;
        private SignInManager<IdentityUser> _signinManager;
        private readonly ITokenService _tokenService;
        private IdentityContext _idContext;

        public UserService(UserManager<IdentityUser> userManager,
            SignInManager<IdentityUser> signInManager,
            ITokenService tokenService,
            IdentityContext idContext)
        {
            _userManger = userManager;
            _signinManager = signInManager;
            _tokenService = tokenService;
            _idContext = idContext;
        }

        public async Task<ServiceResult<IdentityUser>> CreateIdentityUserAsync(UpsertUserModel model)
        {

            var serviceResult = new ServiceResult<IdentityUser>();

            var identityUser = new IdentityUser();
            identityUser.Email = model.Email;
            identityUser.UserName = model.Username ?? model.Email;

            var result = await _userManger.CreateAsync(identityUser, model.Password);

            if (result.Succeeded == false)
            {
                foreach (var err in result.Errors)
                {
                    serviceResult.AddError(err.Description);
                }
            }
            else
            {
                serviceResult.ResultObj = identityUser;
            }

            return serviceResult;
        }

        public async Task<ServiceResult<IdentityUser>> UpdateIdentityUserAsync(UpsertUserModel model)
        {
            var serviceResult = new ServiceResult<IdentityUser>();

            var identityUser = await _userManger.FindByEmailAsync(model.Email);

            if (identityUser == null)
            {
                serviceResult.AddError("No identity user found.");
            }
            else
            {
                identityUser.Email = model.Email;
                identityUser.UserName = model.Username ?? model.Email;

                var updateResult = await _userManger.UpdateAsync(identityUser);

                if (updateResult.Succeeded == false)
                {
                    foreach (var err in updateResult.Errors)
                    {
                        serviceResult.AddError(err.Description);
                    }
                }
                else
                {
                    serviceResult.ResultObj = identityUser;
                }
            }

            return serviceResult;
        }

        public async Task<ServiceResult<TokenResult>> GetTokensAsync(TokenRequest request)
        {

            var result = new ServiceResult<TokenResult>();
            result.ResultObj = new TokenResult();

            var appUser = await _userManger.FindByIdAsync(request.Identifier);

            if (appUser == null)
            {
                result.AddError($"User {request.UserValue ?? request.Identifier} not found.");
                return result;
            }

            result.ResultObj.AccessToken = _tokenService.GenerateAccessToken(appUser.Id);

            var refreshToken = _tokenService.GenerateRefreshToken(appUser.Email, request.ClientId);
            await SaveRefreshTokenToDbAsync(refreshToken);

            result.ResultObj.RefreshToken = refreshToken;

            return result;

        }

        public async Task<ServiceResult<TokenResult>> RefreshTokensAsync(TokenRequest request)
        {
            var result = new ServiceResult<TokenResult>();

            var principal = _tokenService.GetPrincipalFromExpiredToken(request.AccessToken);
            var identifier = principal.Identity.Name;

            var user = await _userManger.FindByIdAsync(identifier);

            if (user == null)
            {
                result.AddError("User / Access Token Mismatch.");
                return result;
            }

            var refreshToken = _idContext.RefreshTokens.Where(q => q.ClientId == request.ClientId
                                                                    && q.RefreshKey == request.RefreshToken
                                                                    && q.UserValue == user.Email
                                                                    && q.ExpiresAt >= DateTime.Now).FirstOrDefault();

            if (refreshToken == null)
            {
                result.AddError("Refresh Token not found. It has expired or been revoked.");
                return result;
            }

            await RemoveRefreshTokenAsync(refreshToken);

            var tokensResult = await GetTokensAsync(new TokenRequest
            {
                ClientId = refreshToken.ClientId,
                Identifier = user.Id,
                UserValue = user.Email
            });

            result.ResultObj = tokensResult.ResultObj;

            return result;

        }

        private async Task<bool> RemoveRefreshTokenAsync(RefreshToken refreshToken)
        {
            if (refreshToken != null)
            {
                _idContext.RefreshTokens.Remove(refreshToken);
                await _idContext.SaveChangesAsync();
                return true;
            }

            return false;
        }

        private async Task<int> SaveRefreshTokenToDbAsync(RefreshToken refreshToken)
        {
            if (refreshToken != null)
            {
                var refreshEntry = _idContext.RefreshTokens.Where(q => q.ClientId == refreshToken.ClientId && q.UserValue == refreshToken.UserValue).FirstOrDefault();

                if (refreshEntry != null)
                {
                    refreshEntry.RefreshKey = refreshToken.RefreshKey;
                    refreshEntry.IssuedAt = refreshToken.IssuedAt;
                    refreshEntry.ExpiresAt = refreshToken.ExpiresAt;
                }
                else
                {
                    _idContext.RefreshTokens.Add(refreshToken);
                }

                return await _idContext.SaveChangesAsync();
            }

            return 0;
        }

        public async Task<IdentityUser> FindByEmailAsync(string email)
        {
            return await _userManger.FindByEmailAsync(email);
        }

        public async Task<IdentityUser> FindByIdAsync(string id)
        {
            return await _userManger.FindByIdAsync(id);
        }

        public async Task<bool> ValidateUserAsync(IdentityUser user, string password)
        {
            var result = await _signinManager.CheckPasswordSignInAsync(user, password, false);

            return result.Succeeded;
        }
    }
}
