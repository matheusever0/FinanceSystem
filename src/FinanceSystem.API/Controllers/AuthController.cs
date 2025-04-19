using FinanceSystem.API.Configuration;
using FinanceSystem.API.Extensions;
using FinanceSystem.API.Resources;
using FinanceSystem.Application.DTOs.Login;
using FinanceSystem.Application.Interfaces;
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
        private readonly ILogger<AuthController> _logger;
        private readonly IStringLocalizer<SharedResource> _localizer;

        public AuthController(
            IUserService userService,
            ILogger<AuthController> logger,
            IStringLocalizer<SharedResource> localizer)
        {
            _userService = userService;
            _logger = logger;
            _localizer = localizer;
        }

        [HttpPost("login")]
        public async Task<ActionResult> Login([FromBody] LoginDto loginDto)
        {
            _logger.LogOperation(LogMessageConstants.AuthLogin, _localizer, loginDto.Username);

            try
            {
                var response = await _userService.LoginAsync(loginDto);
                _logger.LogOperation(LogMessageConstants.AuthLoginSuccess, _localizer, loginDto.Username);
                return this.ApiOk(response);
            }
            catch (Exception ex)
            {
                _logger.LogWarningOperation(LogMessageConstants.AuthLoginFailed, _localizer, loginDto.Username);
                return this.ApiUnauthorized<LoginResponseDto>(ex.Message, _localizer);
            }
        }

        [HttpGet("verify-token")]
        public IActionResult VerifyToken()
        {
            try
            {
                var identity = User.Identity;
                if (identity.IsAuthenticated)
                {
                    var username = User.Identity.Name;
                    _logger.LogOperation(LogMessageConstants.AuthTokenValid, _localizer, username);

                    return this.ApiOk(new { username });
                }

                _logger.LogWarningOperation(LogMessageConstants.AuthTokenInvalid, _localizer);
                return this.ApiUnauthorized<object>("Auth.TokenInvalid", _localizer);
            }
            catch (Exception ex)
            {
                _logger.LogErrorOperation(ex, LogMessageConstants.AuthTokenVerifyError, _localizer);
                return this.ApiError<object>("Auth.TokenVerifyError", _localizer, StatusCodes.Status500InternalServerError);
            }
        }

        [HttpGet("user-permissions")]
        public IActionResult GetUserPermissions()
        {
            try
            {
                if (!User.Identity.IsAuthenticated)
                {
                    _logger.LogWarningOperation(LogMessageConstants.ErrorUnauthorized, _localizer, "auth permissions");
                    return this.ApiUnauthorized<object>("Auth.UserNotAuthenticated", _localizer);
                }

                var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);
                var roles = User.FindAll(ClaimTypes.Role).Select(c => c.Value).ToList();

                _logger.LogOperation(LogMessageConstants.AuthGetPermissions, _localizer, userId);

                var permissions = new
                {
                    userId,
                    roles,
                    isAdmin = User.IsInRole("Admin"),
                    isModerator = User.IsInRole("Moderator")
                };

                return this.ApiOk(permissions);
            }
            catch (Exception ex)
            {
                _logger.LogErrorOperation(ex, LogMessageConstants.ErrorGeneric, _localizer, "user permissions");
                return this.ApiError<object>("Error.GetUserPermissions", _localizer, StatusCodes.Status500InternalServerError);
            }
        }
    }
}