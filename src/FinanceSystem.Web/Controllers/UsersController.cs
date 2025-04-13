using FinanceSystem.Web.Filters;
using FinanceSystem.Web.Models;
using FinanceSystem.Web.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace FinanceSystem.Web.Controllers
{
    [Authorize]
    public class UsersController : Controller
    {
        private readonly IUserService _userService;
        private readonly IRoleService _roleService;
        private readonly ILogger<UsersController> _logger;

        public UsersController(
            IUserService userService,
            IRoleService roleService,
            ILogger<UsersController> logger)
        {
            _userService = userService;
            _roleService = roleService;
            _logger = logger;
        }

        [RequirePermission("users.view")]
        public async Task<IActionResult> Index()
        {
            try
            {
                _logger.LogInformation("Usuário {UserName} acessando listagem de usuários", User.Identity.Name);
                var token = HttpContext.Session.GetString("JWToken");
                var users = await _userService.GetAllUsersAsync(token);
                return View(users);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Erro ao carregar listagem de usuários");
                TempData["ErrorMessage"] = $"Erro ao carregar usuários: {ex.Message}";
                return RedirectToAction("Index", "Home");
            }
        }

        [RequirePermission("users.view")]
        public async Task<IActionResult> Details(string id)
        {
            try
            {
                _logger.LogInformation("Usuário {UserName} visualizando detalhes do usuário {UserId}", User.Identity.Name, id);
                var token = HttpContext.Session.GetString("JWToken");
                var user = await _userService.GetUserByIdAsync(id, token);
                return View(user);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Erro ao carregar detalhes do usuário {UserId}", id);
                TempData["ErrorMessage"] = $"Erro ao carregar detalhes do usuário: {ex.Message}";
                return RedirectToAction(nameof(Index));
            }
        }

        [RequirePermission("users.create")]
        public async Task<IActionResult> Create()
        {
            try
            {
                _logger.LogInformation("Usuário {UserName} acessando página de criação de usuário", User.Identity.Name);
                var token = HttpContext.Session.GetString("JWToken");
                var roles = await _roleService.GetAllRolesAsync(token);
                ViewBag.Roles = roles;
                return View();
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Erro ao preparar formulário de criação de usuário");
                TempData["ErrorMessage"] = $"Erro ao preparar formulário: {ex.Message}";
                return RedirectToAction(nameof(Index));
            }
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        [RequirePermission("users.create")]
        public async Task<IActionResult> Create(CreateUserModel model, List<string> selectedRoles)
        {
            try
            {
                var token = HttpContext.Session.GetString("JWToken");

                if (selectedRoles != null && selectedRoles.Any())
                {
                    model.Roles = selectedRoles;
                }

                if (ModelState.IsValid)
                {
                    _logger.LogInformation("Usuário {UserName} criando novo usuário {NewUserName}", User.Identity.Name, model.Username);
                    await _userService.CreateUserAsync(model, token);
                    TempData["SuccessMessage"] = "Usuário criado com sucesso!";
                    return RedirectToAction(nameof(Index));
                }

                var roles = await _roleService.GetAllRolesAsync(token);
                ViewBag.Roles = roles;
                return View(model);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Erro ao criar usuário {Username}", model.Username);
                TempData["ErrorMessage"] = $"Erro ao criar usuário: {ex.Message}";
                return RedirectToAction(nameof(Index));
            }
        }

        [RequirePermission("users.edit")]
        public async Task<IActionResult> Edit(string id)
        {
            try
            {
                _logger.LogInformation("Usuário {UserName} editando usuário {UserId}", User.Identity.Name, id);
                var token = HttpContext.Session.GetString("JWToken");
                var user = await _userService.GetUserByIdAsync(id, token);
                var roles = await _roleService.GetAllRolesAsync(token);

                ViewBag.Roles = roles;

                var model = new UpdateUserModel
                {
                    Username = user.Username,
                    Email = user.Email,
                    IsActive = user.IsActive,
                    Roles = user.Roles
                };

                return View(model);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Erro ao carregar usuário {UserId} para edição", id);
                TempData["ErrorMessage"] = $"Erro ao carregar usuário para edição: {ex.Message}";
                return RedirectToAction(nameof(Index));
            }
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        [RequirePermission("users.edit")]
        public async Task<IActionResult> Edit(string id, UpdateUserModel model, List<string> selectedRoles)
        {
            try
            {
                var token = HttpContext.Session.GetString("JWToken");

                if (selectedRoles != null && selectedRoles.Any())
                {
                    model.Roles = selectedRoles;
                }

                if (ModelState.IsValid)
                {
                    _logger.LogInformation("Usuário {UserName} atualizando usuário {UserId}", User.Identity.Name, id);
                    await _userService.UpdateUserAsync(id, model, token);
                    TempData["SuccessMessage"] = "Usuário atualizado com sucesso!";
                    return RedirectToAction(nameof(Index));
                }

                var roles = await _roleService.GetAllRolesAsync(token);
                ViewBag.Roles = roles;
                return View(model);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Erro ao atualizar usuário {UserId}", id);
                TempData["ErrorMessage"] = $"Erro ao atualizar usuário: {ex.Message}";
                return RedirectToAction(nameof(Index));
            }
        }

        [RequirePermission("users.delete")]
        public async Task<IActionResult> Delete(string id)
        {
            try
            {
                _logger.LogInformation("Usuário {UserName} acessando página de exclusão do usuário {UserId}", User.Identity.Name, id);
                var token = HttpContext.Session.GetString("JWToken");
                var user = await _userService.GetUserByIdAsync(id, token);
                return View(user);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Erro ao carregar usuário {UserId} para exclusão", id);
                TempData["ErrorMessage"] = $"Erro ao carregar usuário para exclusão: {ex.Message}";
                return RedirectToAction(nameof(Index));
            }
        }

        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        [RequirePermission("users.delete")]
        public async Task<IActionResult> DeleteConfirmed(string id)
        {
            try
            {
                _logger.LogInformation("Usuário {UserName} excluindo usuário {UserId}", User.Identity.Name, id);
                var token = HttpContext.Session.GetString("JWToken");
                await _userService.DeleteUserAsync(id, token);
                TempData["SuccessMessage"] = "Usuário excluído com sucesso!";
                return RedirectToAction(nameof(Index));
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Erro ao excluir usuário {UserId}", id);
                TempData["ErrorMessage"] = $"Erro ao excluir usuário: {ex.Message}";
                return RedirectToAction(nameof(Index));
            }
        }
    }
}