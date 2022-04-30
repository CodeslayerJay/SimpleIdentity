using Identity.Membership;
using Identity.Membership.Models;
using Identity.Models;
using Identity.Utils;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.Claims;
using System.Threading.Tasks;

namespace Identity.Controllers
{
    public class AuthController : ApiBaseController
    {
        private readonly IUserService _userService;

        public AuthController(IUserService userService)
        {
            _userService = userService;

        }

        [HttpPost("Refresh")]
        [AllowAnonymous]
        public async Task<IActionResult> Refresh(TokenRequestApiModel tokenRequest)
        {
            try
            {
                if (!ModelState.IsValid) return BadRequest("Token failed to generate");

                var result = await _userService.RefreshTokensAsync(new TokenRequest
                {
                    RefreshToken = tokenRequest.RefreshKey,
                    AccessToken = tokenRequest.AccessKey,
                    ClientId = HttpContext.Connection.LocalIpAddress.ToString()
                });

                if (result.IsValid == false)
                    return Unauthorized();

                return Ok(result.ResultObj);
            }
            catch (Exception ex)
            {
                return BadRequest(AppConstants.GenericErrorMsg);
            }

        }

        [AllowAnonymous]
        [HttpPost("Register")]
        public async Task<IActionResult> Register(CreateUserApiModel model)
        {
            try
            {
                if (ModelState.IsValid == false)
                    return ValidationProblem();

                if(string.IsNullOrEmpty(model.Password) == false && string.IsNullOrEmpty(model.PasswordConfirm) == false && model.Password != model.PasswordConfirm)
                {
                    return BadRequest("Passwords do not match.");
                }

                var userResult = await _userService.CreateIdentityUserAsync(new UpsertUserModel
                {
                    Email = model.Email,
                    Password = model.Password,
                    Username = model.Username
                });

                if (userResult.IsValid == false)
                    return BadRequest(userResult.Errors);

                var tokenResult = await _userService.GetTokensAsync(new TokenRequest
                {
                    UserValue = userResult.ResultObj.UserName,
                    Identifier = userResult.ResultObj.Id,
                    ClientId = HttpContext.Connection.LocalIpAddress.ToString()
                });

                return Ok(new { tokens = tokenResult.ResultObj, userId = userResult.ResultObj.Id });
            }
            catch (Exception ex)
            {
                return BadRequest(AppConstants.GenericErrorMsg);
            }
        }

        [AllowAnonymous]
        [HttpPost("Validate")]
        public async Task<IActionResult> Validate(ValidateUserApiModel model)
        {
            try
            {

                if (ModelState.IsValid == false)
                    return ValidationProblem();

                var user = await _userService.FindByEmailAsync(model.Email);

                 if (user == null)
                    return Unauthorized();

                var result = await _userService.ValidateUserAsync(user, model.Password);

                if (result == false)
                    return Unauthorized();

                var tokenResult = await _userService.GetTokensAsync(new TokenRequest
                {
                    UserValue = user.Email,
                    Identifier = user.Id,
                    ClientId = HttpContext.Connection.LocalIpAddress.ToString()
                });


                return Ok(tokenResult);
            }
            catch (Exception ex)
            {
                return BadRequest(AppConstants.GenericErrorMsg);
            }
        }

        [Authorize]
        [HttpGet("Test")]
        public async Task<IActionResult> Test(string message =  null)
        {
            try
            {

                var identity = HttpContext.User.Identity as ClaimsIdentity;
                if (identity != null)
                {

                    //identity.FindFirst("ClaimName").Value;
                    var claim = identity.FindFirst(q => q.Issuer == "IdentityApp");
                }

                return Ok(message ?? $"Hello, from { nameof(Test) }. =D");
            }
            catch(Exception ex)
            {
                return BadRequest(AppConstants.SomethingBadMsg);
            }
        }
    }
}
