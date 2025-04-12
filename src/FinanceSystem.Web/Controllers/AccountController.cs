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
        public IActionResult Login(string returnUrl = null)
        {
            if (User.Identity.IsAuthenticated)
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
                return View(model);
            }

            try
            {
                _logger.LogInformation($"Tentando login para usuário: {model.Username}");

                // Tentar fazer login na API
                var result = await _userService.LoginAsync(model);

                if (result == null || string.IsNullOrEmpty(result.Token))
                {
                    ModelState.AddModelError(string.Empty, "Falha na autenticação: token inválido");
                    return View(model);
                }

                _logger.LogInformation($"Login bem-sucedido para {model.Username}. Token recebido.");

                // Armazenar o token na sessão
                HttpContext.Session.SetString("JWToken", result.Token);

                // Criar claims principal a partir do token
                var principal = await _apiService.GetClaimsPrincipalFromToken(result.Token);

                // Verificar se o principal foi criado corretamente
                if (principal == null || !principal.Identity.IsAuthenticated)
                {
                    ModelState.AddModelError(string.Empty, "Falha ao gerar identidade de usuário");
                    return View(model);
                }

                // Fazer login com cookies
                await HttpContext.SignInAsync(
                    CookieAuthenticationDefaults.AuthenticationScheme,
                    principal,
                    new AuthenticationProperties
                    {
                        IsPersistent = true,
                        ExpiresUtc = DateTimeOffset.UtcNow.AddHours(1)
                    });

                _logger.LogInformation("Cookie de autenticação criado com sucesso");

                TempData["SuccessMessage"] = "Login realizado com sucesso!";

                // Redirecionamento após o login
                if (!string.IsNullOrEmpty(returnUrl) && Url.IsLocalUrl(returnUrl))
                {
                    return Redirect(returnUrl);
                }

                return RedirectToAction("Index", "Home");
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Erro durante o processo de login");
                ModelState.AddModelError(string.Empty, $"Erro de autenticação: {ex.Message}");
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