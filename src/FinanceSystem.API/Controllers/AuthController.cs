using FinanceSystem.API.Extensions;
using FinanceSystem.Application.DTOs.Login;
using FinanceSystem.Application.Interfaces;
using FinanceSystem.Resources;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Localization;
using System.Security.Claims;

namespace FinanceSystem.API.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class AuthController : ControllerBase
    {
        private readonly IUserService _userService;
        private readonly IStringLocalizer<ResourceFinanceApi> _localizer;

        public AuthController(
            IUserService userService,
            IStringLocalizer<ResourceFinanceApi> localizer)
        {
            _userService = userService;
            _localizer = localizer;
        }

        [HttpPost("login")]
        public async Task<ActionResult> Login([FromBody] LoginDto loginDto)
        {
            try
            {
                var response = await _userService.LoginAsync(loginDto);
                return this.ApiOk(response);
            }
            catch (Exception ex)
            {
                return this.ApiUnauthorized<LoginResponseDto>(ex.Message, _localizer);
            }
        }

        [HttpGet("verify-token")]
        public IActionResult VerifyToken()
        {

            var identity = User.Identity;

            if (identity is null || !identity.IsAuthenticated)
            {
                return this.ApiUnauthorized<object>(
                    ResourceFinanceApi.Auth_TokenInvalid,
                    _localizer);
            }

            var username = User.Identity?.Name;
            return this.ApiOk(new { username });

        }

        [HttpGet("user-permissions")]
        public IActionResult GetUserPermissions()
        {
            var identity = User.Identity;

            if (identity is null || !identity.IsAuthenticated)
            {
                return this.ApiUnauthorized<object>(
                    ResourceFinanceApi.Auth_UserNotAuthenticated,
                    _localizer);
            }

            var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);
            var roles = User.FindAll(ClaimTypes.Role).Select(c => c.Value).ToList();

            var permissions = new
            {
                userId,
                roles,
                isAdmin = User.IsInRole("Admin"),
                isModerator = User.IsInRole("Moderator")
            };

            return this.ApiOk(permissions);
        }
    }
}