using Equilibrium.Resources.Web;
using Equilibrium.Web.Extensions;
using Equilibrium.Web.Interfaces;
using Equilibrium.Web.Models.Login;
using Equilibrium.Web.Services;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.AspNetCore.Mvc;

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
            _userService = userService ?? throw new ArgumentNullException(nameof(userService));
            _apiService = apiService ?? throw new ArgumentNullException(nameof(apiService));
            _logger = logger ?? throw new ArgumentNullException(nameof(logger));
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

                HttpContext.SetJwtTokenCookie(result.Token);
                await HttpContext.SignInAsync(
                    CookieAuthenticationDefaults.AuthenticationScheme,
                    principal,
                    new AuthenticationProperties
                    {
                        IsPersistent = true,
                        ExpiresUtc = DateTimeOffset.UtcNow.AddDays(1)
                    });

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
                await HttpContext.SignOutAsync(CookieAuthenticationDefaults.AuthenticationScheme);
                HttpContext.Session.Clear();
                HttpContext.Response.Cookies.Delete("JWToken");
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
        public async Task<IActionResult> VerifyToken()
        {
            try
            {
                if (!HttpContext.IsUserAuthenticated())
                {
                    return Unauthorized();
                }

                var token = HttpContext.GetJwtToken();
                if (string.IsNullOrEmpty(token))
                {
                    return Unauthorized();
                }

                var authResult = await HttpContext.AuthenticateAsync(CookieAuthenticationDefaults.AuthenticationScheme);
                if (!authResult.Succeeded || authResult.Properties?.ExpiresUtc == null)
                {
                    return Unauthorized();
                }

                return Ok(new
                {
                    authenticated = true,
                    expiresAt = authResult.Properties.ExpiresUtc.Value,
                    user = HttpContext.GetUserName()
                });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Erro ao verificar token");
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

                // Renovar o cookie de autenticação
                var authResult = await HttpContext.AuthenticateAsync(CookieAuthenticationDefaults.AuthenticationScheme);
                if (authResult.Succeeded)
                {
                    var newProperties = new AuthenticationProperties
                    {
                        IsPersistent = true,
                        ExpiresUtc = DateTimeOffset.UtcNow.AddDays(1)
                    };

                    await HttpContext.SignInAsync(
                        CookieAuthenticationDefaults.AuthenticationScheme,
                        authResult.Principal,
                        newProperties);

                    // Renovar o cookie JWT
                    HttpContext.SetJwtTokenCookie(token);

                    _logger.LogInformation("Token renovado com sucesso para usuário: {User}", HttpContext.GetUserName());
                    return Ok(new { success = true, message = "Token renovado com sucesso" });
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
            return !string.IsNullOrEmpty(returnUrl) && Url.IsLocalUrl(returnUrl) ? Redirect(returnUrl) : RedirectToAction("Index", "Home");
        }
    }
}