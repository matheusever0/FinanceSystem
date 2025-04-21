using FinanceSystem.API.Extensions;
using FinanceSystem.Application.DTOs.Login;
using FinanceSystem.Application.Interfaces;
using Microsoft.AspNetCore.Mvc;
using System.Security.Claims;

namespace FinanceSystem.API.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class AuthController : ControllerBase
    {
        private readonly IUserService _userService;

        public AuthController(IUserService userService)
        {
            _userService = userService;
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
                return this.ApiUnauthorized<LoginResponseDto>(ex.Message);
            }
        }

        [HttpGet("verify-token")]
        public IActionResult VerifyToken()
        {
            var identity = User.Identity;

            if (identity is null || !identity.IsAuthenticated)
            {
                return this.ApiUnauthorized<object>("Auth.TokenInvalid");
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
                return this.ApiUnauthorized<object>("Auth.UserNotAuthenticated");
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