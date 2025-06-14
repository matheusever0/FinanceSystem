using Equilibrium.Resources.Web;
using Equilibrium.Web.Extensions;
using Equilibrium.Web.Interfaces;
using Equilibrium.Web.Models.Login;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.AspNetCore.Mvc;
using System.Text.Json;

namespace Equilibrium.Web.Controllers
{
    public class AccountController : Controller
    {
        private readonly IUserService _userService;
        private readonly IApiService _apiService;
        private readonly ILogger<AccountController> _logger;

        public AccountController(
            IUserService userService,
            IApiService apiService,
            ILogger<AccountController> logger)
        {
            _userService = userService;
            _apiService = apiService;
            _logger = logger;
        }

        [HttpGet]
        public IActionResult Login(string returnUrl)
        {
            if (HttpContext.IsUserAuthenticated())
            {
                return RedirectToAction("Index", "Home");
            }

            ViewData["ReturnUrl"] = returnUrl;
            return View();
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Login(LoginModel model, string returnUrl)
        {
            ViewData["ReturnUrl"] = returnUrl;

            ModelState.Remove("ReturnUrl");

            if (!ModelState.IsValid)
            {
                return View(model);
            }

            try
            {
                var result = await _userService.LoginAsync(model);

                if (result == null)
                {
                    ModelState.AddModelError(string.Empty, ResourceFinanceWeb.Error_AuthenticationFailed);
                    return View(model);
                }

                if (string.IsNullOrEmpty(result.Token))
                {
                    ModelState.AddModelError(string.Empty, ResourceFinanceWeb.Error_InvalidToken);
                    return View(model);
                }

                var principal = await _apiService.GetClaimsPrincipalFromToken(result.Token);

                if (principal == null)
                {
                    ModelState.AddModelError(string.Empty, ResourceFinanceWeb.Error_InvalidToken);
                    return View(model);
                }

                var authProperties = new AuthenticationProperties
                {
                    IsPersistent = true,
                    ExpiresUtc = DateTimeOffset.UtcNow.AddDays(1),
                    AllowRefresh = true,
                    IssuedUtc = DateTimeOffset.UtcNow
                };

                HttpContext.SetJwtTokenCookie(result.Token);

                await HttpContext.SignInAsync(
                    CookieAuthenticationDefaults.AuthenticationScheme,
                    principal,
                    authProperties);

                _logger.LogInformation("Login realizado com sucesso para usuário: {Username}. Token expira em: {Expiration}",
                    model.Username, authProperties.ExpiresUtc);

                return RedirectToLocal(returnUrl);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Erro durante o login para usuário: {Username}", model.Username);
                ModelState.AddModelError("CustomError", ex.Message);
                return View(model);
            }
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Logout()
        {
            try
            {
                var username = HttpContext.GetUserName();

                await HttpContext.SignOutAsync(CookieAuthenticationDefaults.AuthenticationScheme);

                HttpContext.Session.Clear();

                HttpContext.ClearJwtToken();

                _logger.LogInformation("Logout realizado com sucesso para usuário: {Username}", username);

                return RedirectToAction(nameof(Login));
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Erro durante logout");
                TempData["ErrorMessage"] = string.Format(ResourceFinanceWeb.Error_Logout, ex.Message);
                return RedirectToAction("Index", "Home");
            }
        }

        [HttpGet]
        public async Task<IActionResult> GetVersion()
        {
            var content = await _userService.GetVersion();

            var versionObj = JsonSerializer.Deserialize<JsonElement>(content.ToString());

            var response = new
            {
                Version = versionObj.GetProperty("version").GetString(),
                BuildDate = versionObj.GetProperty("buildDate").GetString(),
                Environment = versionObj.GetProperty("environment").GetString()
            };

            return Json(response);

        }

        [HttpGet]
        public async Task<IActionResult> VerifyToken()
        {
            try
            {
                if (!HttpContext.IsUserAuthenticated())
                {
                    _logger.LogWarning("Usuário não autenticado tentando verificar token");
                    return Unauthorized();
                }

                var token = HttpContext.GetJwtToken();
                if (string.IsNullOrEmpty(token))
                {
                    _logger.LogWarning("Token JWT não encontrado para usuário autenticado");
                    return Unauthorized();
                }

                var isValid = await _apiService.VerifyTokenAsync(token);
                if (!isValid)
                {
                    _logger.LogWarning("Token JWT inválido para usuário: {User}", HttpContext.GetUserName());
                    return Unauthorized();
                }

                var authResult = await HttpContext.AuthenticateAsync(CookieAuthenticationDefaults.AuthenticationScheme);
                if (!authResult.Succeeded || authResult.Properties?.ExpiresUtc == null)
                {
                    _logger.LogWarning("Cookie de autenticação inválido para usuário: {User}", HttpContext.GetUserName());
                    return Unauthorized();
                }

                if (authResult.Properties.ExpiresUtc <= DateTimeOffset.UtcNow)
                {
                    _logger.LogWarning("Cookie de autenticação expirado para usuário: {User}", HttpContext.GetUserName());
                    return Unauthorized();
                }

                return Ok(new
                {
                    authenticated = true,
                    expiresAt = authResult.Properties.ExpiresUtc.Value,
                    user = HttpContext.GetUserName(),
                    timeRemaining = (authResult.Properties.ExpiresUtc.Value - DateTimeOffset.UtcNow).TotalMinutes
                });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Erro ao verificar token para usuário: {User}", HttpContext.GetUserName());
                return Unauthorized();
            }
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> RenewToken()
        {
            try
            {
                if (!HttpContext.IsUserAuthenticated())
                {
                    return Unauthorized(new { success = false, message = "Usuário não autenticado" });
                }

                var token = HttpContext.GetJwtToken();
                if (string.IsNullOrEmpty(token))
                {
                    return Unauthorized(new { success = false, message = "Token não encontrado" });
                }

                var isValid = await _apiService.VerifyTokenAsync(token);
                if (!isValid)
                {
                    return Unauthorized(new { success = false, message = "Token inválido" });
                }

                var authResult = await HttpContext.AuthenticateAsync(CookieAuthenticationDefaults.AuthenticationScheme);
                if (authResult.Succeeded)
                {
                    var newProperties = new AuthenticationProperties
                    {
                        IsPersistent = true,
                        ExpiresUtc = DateTimeOffset.UtcNow.AddDays(1),
                        AllowRefresh = true,
                        IssuedUtc = DateTimeOffset.UtcNow
                    };

                    await HttpContext.SignInAsync(
                        CookieAuthenticationDefaults.AuthenticationScheme,
                        authResult.Principal,
                        newProperties);

                    HttpContext.SetJwtTokenCookie(token);

                    _logger.LogInformation("Token renovado com sucesso para usuário: {User}. Nova expiração: {Expiration}",
                        HttpContext.GetUserName(), newProperties.ExpiresUtc);

                    return Ok(new
                    {
                        success = true,
                        message = "Token renovado com sucesso",
                        expiresAt = newProperties.ExpiresUtc
                    });
                }

                return Unauthorized(new { success = false, message = "Falha na renovação" });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Erro ao renovar token para usuário: {User}", HttpContext.GetUserName());
                return StatusCode(500, new { success = false, message = "Erro interno do servidor" });
            }
        }

        public IActionResult AccessDenied()
        {
            return View();
        }

        private IActionResult RedirectToLocal(string returnUrl)
        {
            return !string.IsNullOrEmpty(returnUrl) && Url.IsLocalUrl(returnUrl) ?
                Redirect(returnUrl) :
                RedirectToAction("Index", "Home");
        }
    }
}