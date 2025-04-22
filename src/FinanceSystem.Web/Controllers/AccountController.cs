using FinanceSystem.Web.Extensions;
using FinanceSystem.Web.Interfaces;
using FinanceSystem.Web.Models.Login;
using FinanceSystem.Web.Services;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.AspNetCore.Mvc;
using System;
using System.Threading.Tasks;

namespace FinanceSystem.Web.Controllers
{
    public class AccountController : Controller
    {
        private readonly IUserService _userService;
        private readonly IApiService _apiService;

        private const string ERROR_AUTHENTICATION_FAILED = "Falha na autenticação";
        private const string ERROR_INVALID_TOKEN = "Token inválido";
        private const string ERROR_IDENTITY_GENERATION = "Falha ao gerar identidade";
        private const string ERROR_LOGOUT = "Erro ao fazer logout: {0}";

        private const string SUCCESS_LOGIN = "Login realizado com sucesso!";

        public AccountController(
            IUserService userService,
            IApiService apiService)
        {
            _userService = userService ?? throw new ArgumentNullException(nameof(userService));
            _apiService = apiService ?? throw new ArgumentNullException(nameof(apiService));
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
                    ModelState.AddModelError(string.Empty, ERROR_AUTHENTICATION_FAILED);
                    return View(model);
                }

                if (string.IsNullOrEmpty(result.Token))
                {
                    ModelState.AddModelError(string.Empty, ERROR_INVALID_TOKEN);
                    return View(model);
                }

                HttpContext.SetJwtToken(result.Token);

                var principal = await _apiService.GetClaimsPrincipalFromToken(result.Token);

                if (principal == null)
                {
                    ModelState.AddModelError(string.Empty, ERROR_IDENTITY_GENERATION);
                    return View(model);
                }

                await HttpContext.SignInAsync(
                    CookieAuthenticationDefaults.AuthenticationScheme,
                    principal,
                    new AuthenticationProperties
                    {
                        IsPersistent = true,
                        ExpiresUtc = DateTimeOffset.UtcNow.AddDays(1)
                    });

                TempData["SuccessMessage"] = SUCCESS_LOGIN;

                return RedirectToLocal(returnUrl);
            }
            catch (Exception ex)
            {
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
                return RedirectToAction(nameof(Login));
            }
            catch (Exception ex)
            {
                TempData["ErrorMessage"] = string.Format(ERROR_LOGOUT, ex.Message);
                return RedirectToAction("Index", "Home");
            }
        }

        public IActionResult AccessDenied()
        {
            return View();
        }

        private IActionResult RedirectToLocal(string returnUrl)
        {
            if (!string.IsNullOrEmpty(returnUrl) && Url.IsLocalUrl(returnUrl))
            {
                return Redirect(returnUrl);
            }

            return RedirectToAction("Index", "Home");
        }
    }
}