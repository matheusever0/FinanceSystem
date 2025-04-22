using FinanceSystem.Application.DTOs.Login;
using FinanceSystem.Application.Interfaces;
using FinanceSystem.Resources;
using Microsoft.AspNetCore.Mvc;

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
                return Ok(response);
            }
            catch (Exception ex)
            {
                return Unauthorized(ex.Message);
            }
        }

        [HttpGet("verify-token")]
        public IActionResult VerifyToken()
        {
            var identity = User.Identity;

            if (identity is null || !identity.IsAuthenticated)
            {
                return Unauthorized(ResourceFinanceApi.Auth_TokenInvalid);
            }

            var username = User.Identity?.Name;
            return Ok(new { username });
        }
    }
}