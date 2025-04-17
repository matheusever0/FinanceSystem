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
        private readonly ILogger<AuthController> _logger;

        public AuthController(IUserService userService, ILogger<AuthController> logger)
        {
            _userService = userService;
            _logger = logger;
        }

        [HttpPost("login")]
        public async Task<ActionResult<LoginResponseDto>> Login([FromBody] LoginDto loginDto)
        {
            _logger.LogInformation("Tentativa de login para usuário: {Username}", loginDto.Username);
            try
            {
                var response = await _userService.LoginAsync(loginDto);
                _logger.LogInformation("Login bem-sucedido para {Username}", loginDto.Username);
                return Ok(response);
            }
            catch (Exception ex)
            {
                _logger.LogWarning(ex, "Falha no login para {Username}", loginDto.Username);
                return Unauthorized(new { message = ex.Message });
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
                    _logger.LogInformation("Token válido para usuário: {Username}", username);
                    return Ok(new { valid = true, username });
                }

                _logger.LogWarning("Token inválido ou expirado");
                return Unauthorized(new { valid = false, message = "Token inválido ou expirado" });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Erro ao verificar token");
                return StatusCode(500, new { valid = false, message = "Erro ao verificar token" });
            }
        }

        [HttpGet("user-permissions")]
        public IActionResult GetUserPermissions()
        {
            try
            {
                if (!User.Identity.IsAuthenticated)
                {
                    return Unauthorized(new { message = "Usuário não autenticado" });
                }

                var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);
                var roles = User.FindAll(ClaimTypes.Role).Select(c => c.Value).ToList();

                return Ok(new
                {
                    userId,
                    roles,
                    isAdmin = User.IsInRole("Admin"),
                    isModerator = User.IsInRole("Moderator")
                });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Erro ao obter informações do usuário autenticado");
                return StatusCode(500, new { message = "Erro ao obter informações do usuário" });
            }
        }
    }
}