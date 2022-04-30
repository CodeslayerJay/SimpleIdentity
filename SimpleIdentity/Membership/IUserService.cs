using Identity.Membership.Common;
using Identity.Membership.Models;
using Identity.Models;
using Microsoft.AspNetCore.Identity;
using System.Threading.Tasks;

namespace Identity.Membership
{
    public interface IUserService
    {
        Task<ServiceResult<IdentityUser>> CreateIdentityUserAsync(UpsertUserModel model);
        Task<IdentityUser> FindByEmailAsync(string email);
        Task<IdentityUser> FindByIdAsync(string email);
        Task<ServiceResult<TokenResult>> GetTokensAsync(TokenRequest request);
        Task<ServiceResult<TokenResult>> RefreshTokensAsync(TokenRequest request);
        Task<ServiceResult<IdentityUser>> UpdateIdentityUserAsync(UpsertUserModel model);
        Task<bool> ValidateUserAsync(IdentityUser user, string password);
    }
}