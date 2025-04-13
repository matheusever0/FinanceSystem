using FinanceSystem.Web.Interfaces;
using FinanceSystem.Web.Models;
using FinanceSystem.Web.Services;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.AspNetCore.Mvc;

namespace FinanceSystem.Web.Controllers
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
        public IActionResult Login(string returnUrl = null, bool expired = false)
        {
            if (User.Identity!.IsAuthenticated)
            {
                return RedirectToAction("Index", "Home");
            }

            ViewData["ReturnUrl"] = returnUrl;
            return View();
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Login(LoginModel model, string returnUrl = null)
        {
            ViewData["ReturnUrl"] = returnUrl;

            if (!ModelState.IsValid)
            {
                _logger.LogWarning($"Login inválido para usuário: {model.Username}");
                return View(model);
            }

            try
            {
                _logger.LogInformation($"Tentando login para usuário: {model.Username}");

                var result = await _userService.LoginAsync(model);

                if (result == null)
                {
                    ModelState.AddModelError(string.Empty, "Falha na autenticação");
                    _logger.LogWarning($"Login falhou para usuário: {model.Username} - Resultado nulo");
                    return View(model);
                }

                if (string.IsNullOrEmpty(result.Token))
                {
                    ModelState.AddModelError(string.Empty, "Token inválido");
                    _logger.LogWarning($"Login falhou para usuário: {model.Username} - Token vazio");
                    return View(model);
                }

                _logger.LogInformation($"Login bem-sucedido para {model.Username}");

                HttpContext.Session.SetString("JWToken", result.Token);

                var principal = await _apiService.GetClaimsPrincipalFromToken(result.Token);

                if (principal == null)
                {
                    ModelState.AddModelError(string.Empty, "Falha ao gerar identidade");
                    _logger.LogWarning($"Login falhou para usuário: {model.Username} - Principal nulo");
                    return View(model);
                }

                await HttpContext.SignInAsync(
                    CookieAuthenticationDefaults.AuthenticationScheme,
                    principal,
                    new AuthenticationProperties
                    {
                        IsPersistent = true,
                        ExpiresUtc = DateTimeOffset.UtcNow.AddHours(1)
                    });

                TempData["SuccessMessage"] = "Login realizado com sucesso!";

                return RedirectToLocal(returnUrl);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, $"Erro durante login para usuário {model.Username}");
                ModelState.AddModelError(string.Empty, "Erro de autenticação: " + ex.Message);
                return View(model);
            }
        }

        private IActionResult RedirectToLocal(string returnUrl)
        {
            if (Url.IsLocalUrl(returnUrl))
            {
                return Redirect(returnUrl);
            }
            return RedirectToAction("Index", "Home");
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Logout()
        {
            try
            {
                await HttpContext.SignOutAsync(CookieAuthenticationDefaults.AuthenticationScheme);
                HttpContext.Session.Clear();
                return RedirectToAction(nameof(Login));
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Erro durante o logout");
                TempData["ErrorMessage"] = $"Erro ao fazer logout: {ex.Message}";
                return RedirectToAction("Index", "Home");
            }
        }

        public IActionResult AccessDenied()
        {
            return View();
        }
    }
}