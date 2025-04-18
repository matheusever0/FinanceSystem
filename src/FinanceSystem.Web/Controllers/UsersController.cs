using FinanceSystem.Web.Extensions;
using FinanceSystem.Web.Filters;
using FinanceSystem.Web.Models.User;
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
                var token = HttpContext.GetJwtToken();
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
                var token = HttpContext.GetJwtToken();
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
                var token = HttpContext.GetJwtToken();
                var roles = await _roleService.GetAllRolesAsync(token);

                if (!User.IsInRole("Admin"))
                {
                    roles = roles.Where(r => r.Name != "Admin").ToList();
                }

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
                var token = HttpContext.GetJwtToken();

                if (!User.IsInRole("Admin") && selectedRoles != null)
                {
                    selectedRoles = selectedRoles.Where(r => r != "Admin").ToList();
                }

                if (selectedRoles != null && selectedRoles.Any())
                {
                    model.Roles = selectedRoles;
                }
                else
                {
                    ModelState.AddModelError("Roles", "É necessário selecionar pelo menos um perfil.");
                    var roles = await _roleService.GetAllRolesAsync(token);
                    if (!User.IsInRole("Admin"))
                    {
                        roles = roles.Where(r => r.Name != "Admin").ToList();
                    }
                    ViewBag.Roles = roles;
                    return View(model);
                }

                if (ModelState.IsValid)
                {
                    _logger.LogInformation("Usuário {UserName} criando novo usuário {NewUserName}", User.Identity.Name, model.Username);
                    await _userService.CreateUserAsync(model, token);
                    TempData["SuccessMessage"] = "Usuário criado com sucesso!";
                    return RedirectToAction(nameof(Index));
                }

                var rolesList = await _roleService.GetAllRolesAsync(token);

                if (!User.IsInRole("Admin"))
                {
                    rolesList = rolesList.Where(r => r.Name != "Admin").ToList();
                }

                ViewBag.Roles = rolesList;
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
                var token = HttpContext.GetJwtToken();
                var user = await _userService.GetUserByIdAsync(id, token);

                bool isTargetUserAdmin = user.Roles.Contains("Admin");

                if (!User.IsInRole("Admin") && isTargetUserAdmin)
                {
                    _logger.LogWarning("Usuário {UserName} tentou editar um administrador {AdminId}", User.Identity.Name, id);
                    TempData["ErrorMessage"] = "Você não tem permissão para editar administradores.";
                    return RedirectToAction(nameof(Index));
                }

                _logger.LogInformation("Usuário {UserName} editando usuário {UserId}", User.Identity.Name, id);
                var roles = await _roleService.GetAllRolesAsync(token);

                if (!User.IsInRole("Admin"))
                {
                    roles = roles.Where(r => r.Name != "Admin").ToList();
                }

                ViewBag.Roles = roles;

                var model = new UpdateUserModel
                {
                    Username = user.Username,
                    Email = user.Email,
                    IsActive = user.IsActive,
                    Roles = user.Roles ?? new List<string>()
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
                var token = HttpContext.GetJwtToken();

                var currentUser = await _userService.GetUserByIdAsync(id, token);
                bool isTargetUserAdmin = currentUser.Roles.Contains("Admin");

                if (!User.IsInRole("Admin") && isTargetUserAdmin)
                {
                    _logger.LogWarning("Usuário {UserName} tentou editar um administrador {AdminId}", User.Identity.Name, id);
                    TempData["ErrorMessage"] = "Você não tem permissão para editar administradores.";
                    return RedirectToAction(nameof(Index));
                }

                if (!User.IsInRole("Admin") && selectedRoles != null)
                {
                    selectedRoles = selectedRoles.Where(r => r != "Admin").ToList();

                    if (isTargetUserAdmin)
                    {
                        selectedRoles.Add("Admin");
                    }
                }

                if (selectedRoles != null && selectedRoles.Any())
                {
                    model.Roles = selectedRoles;
                }
                else
                {
                    ModelState.AddModelError("Roles", "É necessário selecionar pelo menos um perfil.");
                    var rolesList = await _roleService.GetAllRolesAsync(token);

                    if (!User.IsInRole("Admin"))
                    {
                        rolesList = rolesList.Where(r => r.Name != "Admin").ToList();
                    }

                    ViewBag.Roles = rolesList;
                    return View(model);
                }

                if (ModelState.IsValid)
                {
                    _logger.LogInformation("Usuário {UserName} atualizando usuário {UserId} - Status Ativo: {IsActive}",
                        User.Identity.Name, id, model.IsActive);

                    await _userService.UpdateUserAsync(id, model, token);
                    TempData["SuccessMessage"] = "Usuário atualizado com sucesso!";
                    return RedirectToAction(nameof(Index));
                }

                var roles = await _roleService.GetAllRolesAsync(token);

                if (!User.IsInRole("Admin"))
                {
                    roles = roles.Where(r => r.Name != "Admin").ToList();
                }

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
                var token = HttpContext.GetJwtToken();
                var user = await _userService.GetUserByIdAsync(id, token);

                bool isTargetUserAdmin = user.Roles.Contains("Admin");

                if (!User.IsInRole("Admin") && isTargetUserAdmin)
                {
                    _logger.LogWarning("Usuário {UserName} tentou excluir um administrador {AdminId}", User.Identity.Name, id);
                    TempData["ErrorMessage"] = "Você não tem permissão para excluir administradores.";
                    return RedirectToAction(nameof(Index));
                }

                _logger.LogInformation("Usuário {UserName} acessando página de exclusão do usuário {UserId}", User.Identity.Name, id);
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
                var token = HttpContext.GetJwtToken();

                var user = await _userService.GetUserByIdAsync(id, token);
                bool isTargetUserAdmin = user.Roles.Contains("Admin");

                if (!User.IsInRole("Admin") && isTargetUserAdmin)
                {
                    _logger.LogWarning("Usuário {UserName} tentou excluir um administrador {AdminId}", User.Identity.Name, id);
                    TempData["ErrorMessage"] = "Você não tem permissão para excluir administradores.";
                    return RedirectToAction(nameof(Index));
                }

                _logger.LogInformation("Usuário {UserName} excluindo usuário {UserId}", User.Identity.Name, id);
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