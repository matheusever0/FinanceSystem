using Equilibrium.Application.DTOs.Login;
using Equilibrium.Application.Interfaces;
using Equilibrium.Domain.Interfaces.Services;
using Equilibrium.Resources;
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

        [HttpGet("verify-token")]
        public IActionResult VerifyToken()
        {
            var identity = User.Identity;

            if (identity is null || !identity.IsAuthenticated)
            {
                Console.WriteLine("Token inválido ou usuário não autenticado");
                return Unauthorized(ResourceFinanceApi.Auth_TokenInvalid);
            }

            var expiryClaim = User.FindFirst("exp");
            if (expiryClaim != null)
            {
                var expiryUnix = long.Parse(expiryClaim.Value);
                var expiryDateTime = DateTimeOffset.FromUnixTimeSeconds(expiryUnix).DateTime;
                Console.WriteLine($"Token expira em: {expiryDateTime}");
                Console.WriteLine($"Hora atual: {DateTime.UtcNow}");
            }

            var username = User.Identity?.Name;
            return Ok(new { username, authenticated = true, expires = expiryClaim?.Value });
        }

        [HttpGet("debug-token")]
        public IActionResult DebugToken()
        {
            var claims = User.Claims.Select(c => new {
                c.Type,
                c.Value
            }).ToList();

            return Ok(new
            {
                Claims = claims,
                User.Identity.IsAuthenticated,
                ServerTime = DateTime.UtcNow
            });
        }
    }
}