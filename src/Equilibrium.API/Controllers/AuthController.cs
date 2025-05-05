using Equilibrium.Application.DTOs.Login;
using Equilibrium.Application.Interfaces;
using Equilibrium.Domain.Interfaces.Services;
using Equilibrium.Resources;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace Equilibrium.API.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class AuthController(IUnitOfWork unitOfWork, 
        IUserService userService) : BaseController(unitOfWork)
    {
        [HttpPost("login")]
        public async Task<ActionResult> Login([FromBody] LoginDto loginDto)
        {
            try
            {
                var response = await userService.LoginAsync(loginDto);

                return Ok(response);
            }
            catch (Exception ex)
            {
                return Unauthorized(ex.Message);
            }
        }

        [HttpGet("test-auth")]
        [Authorize]
        public IActionResult TestAuth()
        {
            return Ok(new { message = "Você está autenticado!" });
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