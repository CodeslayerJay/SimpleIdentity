using Identity.Membership;
using Identity.Membership.Models;
using Identity.Models;
using Identity.Utils;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace Identity.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class UserController : ControllerBase
    {
        private readonly IUserService _userService;

        public UserController(IUserService userService)
        {
            _userService = userService;
        }

        [HttpGet("GetById")]
        [Authorize]
        public async Task<IActionResult> GetById(string id)
        {
            try
            {
                return Ok(MapToUserDetails(await _userService.FindByIdAsync(id)));
            }
            catch(Exception ex)
            {
                return BadRequest(AppConstants.GenericErrorMsg);
            }
        }

        [HttpGet("GetByEmail")]
        [Authorize]
        public async Task<IActionResult> GetByEmail(string email)
        {
            try
            {
                return Ok(MapToUserDetails(await _userService.FindByEmailAsync(email)));
            }
            catch (Exception ex)
            {
                return BadRequest(AppConstants.GenericErrorMsg);
            }
        }

        [HttpPut("Update")]
        [Authorize]
        public async Task<IActionResult> Update(UpdateUserApiModel model)
        {
            try
            {

                var result = await _userService.UpdateIdentityUserAsync(new UpsertUserModel
                {
                    Email = model.Email,
                    Password = model.Password,
                });

                if (result.IsValid == false)
                    return BadRequest(result.Errors);

                return Ok();
            }
            catch(Exception ex)
            {
                return BadRequest(AppConstants.GenericErrorMsg);
            }
        }
        

        private UserDetails MapToUserDetails(IdentityUser identityUser)
        {
            var ud = new UserDetails();

            if (identityUser == null) return ud;

            ud.Id = identityUser.Id;
            ud.Email = identityUser.Email;
            ud.Username = identityUser.UserName;

            return ud;
        }
    }
}
