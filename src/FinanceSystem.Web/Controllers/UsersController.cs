using FinanceSystem.Web.Extensions;
using FinanceSystem.Web.Filters;
using FinanceSystem.Web.Helpers;
using FinanceSystem.Web.Models.User;
using FinanceSystem.Web.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace FinanceSystem.Web.Controllers
{
    [Authorize]
    public class UsersController : Controller
    {
        private readonly IUserService _userService;
        private readonly IRoleService _roleService;
        private readonly IPermissionService _permissionService;

        private const string ERROR_LOADING_USERS = "Erro ao carregar usuários: {0}";
        private const string ERROR_LOADING_USER_DETAILS = "Erro ao carregar detalhes do usuário: {0}";
        private const string ERROR_PREPARING_FORM = "Erro ao preparar formulário: {0}";
        private const string ERROR_CREATING_USER = "Erro ao criar usuário: {0}";
        private const string ERROR_EDITING_USER = "Erro ao carregar usuário para edição: {0}";
        private const string ERROR_UPDATING_USER = "Erro ao atualizar usuário: {0}";
        private const string ERROR_DELETING_USER = "Erro ao excluir usuário: {0}";
        private const string ERROR_LOADING_USER_DELETE = "Erro ao carregar usuário para exclusão: {0}";
        private const string ERROR_PERMISSION_DENIED = "Você não tem permissão para editar outros usuários, apenas o seu.";

        private const string SUCCESS_USER_CREATED = "Usuário criado com sucesso!";
        private const string SUCCESS_USER_UPDATED = "Usuário atualizado com sucesso!";
        private const string SUCCESS_USER_DELETED = "Usuário excluído com sucesso!";

        private const string VALIDATION_SELECT_ROLE = "É necessário selecionar pelo menos um perfil.";
        private const string VALIDATION_EMAIL_REQUIRED = "É necessário ter um email cadastrado.";

        public UsersController(
            IUserService userService,
            IRoleService roleService,
            IPermissionService permissionService)
        {
            _userService = userService ?? throw new ArgumentNullException(nameof(userService));
            _roleService = roleService ?? throw new ArgumentNullException(nameof(roleService));
            _permissionService = permissionService ?? throw new ArgumentNullException(nameof(permissionService));
        }

        [RequirePermission("users.view")]
        public async Task<IActionResult> Index()
        {
            try
            {
                var token = HttpContext.GetJwtToken();
                var users = await _userService.GetAllUsersAsync(token);
                return View(users);
            }
            catch (Exception ex)
            {
                TempData["ErrorMessage"] = string.Format(ERROR_LOADING_USERS, ex.Message);
                return RedirectToAction("Index", "Home");
            }
        }

        [RequirePermission("users.view")]
        public async Task<IActionResult> Details(string id)
        {
            if (string.IsNullOrEmpty(id))
            {
                return BadRequest("ID do usuário não fornecido");
            }

            try
            {
                var token = HttpContext.GetJwtToken();
                var user = await _userService.GetUserByIdAsync(id, token);

                return user == null ? NotFound("Usuário não encontrado") : View(user);
            }
            catch (Exception ex)
            {
                TempData["ErrorMessage"] = string.Format(ERROR_LOADING_USER_DETAILS, ex.Message);
                return RedirectToAction(nameof(Index));
            }
        }

        [RequirePermission("users.create")]
        public async Task<IActionResult> Create()
        {
            try
            {
                var token = HttpContext.GetJwtToken();
                var roles = await _roleService.GetAllRolesAsync(token);
                ViewBag.Roles = roles;
                return View();
            }
            catch (Exception ex)
            {
                TempData["ErrorMessage"] = string.Format(ERROR_PREPARING_FORM, ex.Message);
                return RedirectToAction(nameof(Index));
            }
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        [RequirePermission("users.create")]
        public async Task<IActionResult> Create(CreateUserModel model, List<string> selectedRoles)
        {
            var token = HttpContext.GetJwtToken();

            try
            {
                if (selectedRoles == null || selectedRoles.Count == 0)
                {
                    ModelState.AddModelError("Roles", VALIDATION_SELECT_ROLE);
                    var roles = await _roleService.GetAllRolesAsync(token);
                    ViewBag.Roles = roles;
                    return View(model);
                }

                model.Roles = selectedRoles;

                if (!ModelState.IsValid)
                {
                    var roles = await _roleService.GetAllRolesAsync(token);
                    ViewBag.Roles = roles;
                    return View(model);
                }

                await _userService.CreateUserAsync(model, token);
                TempData["SuccessMessage"] = SUCCESS_USER_CREATED;
                return RedirectToAction(nameof(Index));
            }
            catch (Exception ex)
            {
                TempData["ErrorMessage"] = string.Format(ERROR_CREATING_USER, ex.Message);

                try
                {
                    var roles = await _roleService.GetAllRolesAsync(token);
                    ViewBag.Roles = roles;
                    return View(model);
                }
                catch
                {
                    return RedirectToAction(nameof(Index));
                }
            }
        }

        [RequirePermission("users.edit, users.edit.unique")]
        public async Task<IActionResult> Edit(string id)
        {
            if (string.IsNullOrEmpty(id))
            {
                return BadRequest("ID do usuário não fornecido");
            }

            try
            {
                var token = HttpContext.GetJwtToken();
                var currentUserId = HttpContext.GetCurrentUserId()!;
                var user = await _userService.GetUserByIdAsync(id, token);

                if (user == null)
                {
                    return NotFound("Usuário não encontrado");
                }

                var permissions = await _permissionService.GetPermissionsByUserIdAsync(currentUserId, token);
                var canEditOnlyOwnUser = PermissionHelper.PodeEditarSomenteProprioUsuario(permissions);

                if (canEditOnlyOwnUser && currentUserId != user.Id)
                {
                    TempData["ErrorMessage"] = ERROR_PERMISSION_DENIED;
                    return RedirectToAction(nameof(Index));
                }

                var roles = await _roleService.GetAllRolesAsync(token);
                if (canEditOnlyOwnUser)
                {
                    roles = [.. roles.Where(r => r.Name != "Admin")];
                }
                ViewBag.Roles = roles;

                var model = new UpdateUserModel
                {
                    Username = user.Username,
                    Email = user.Email,
                    IsActive = user.IsActive,
                    Roles = user.Roles ?? []
                };

                return View(model);
            }
            catch (Exception ex)
            {
                TempData["ErrorMessage"] = string.Format(ERROR_EDITING_USER, ex.Message);
                return RedirectToAction(nameof(Index));
            }
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        [RequirePermission("users.edit, users.edit.unique")]
        public async Task<IActionResult> Edit(string id, UpdateUserModel model, List<string> selectedRoles)
        {
            if (string.IsNullOrEmpty(id))
            {
                return BadRequest("ID do usuário não fornecido");
            }

            var token = HttpContext.GetJwtToken();

            try
            {
                var currentUserId = HttpContext.GetCurrentUserId()!;
                var currentUser = await _userService.GetUserByIdAsync(id, token);

                if (currentUser == null)
                {
                    return NotFound("Usuário não encontrado");
                }

                var permissions = await _permissionService.GetPermissionsByUserIdAsync(currentUserId, token);
                var canEditOnlyOwnUser = PermissionHelper.PodeEditarSomenteProprioUsuario(permissions);

                if (canEditOnlyOwnUser && currentUserId != currentUser.Id)
                {
                    TempData["ErrorMessage"] = ERROR_PERMISSION_DENIED;
                    return RedirectToAction(nameof(Index));
                }

                if (string.IsNullOrEmpty(model.Email))
                {
                    ModelState.AddModelError("Email", VALIDATION_EMAIL_REQUIRED);
                }

                if (selectedRoles == null || selectedRoles.Count == 0)
                {
                    ModelState.AddModelError("Roles", VALIDATION_SELECT_ROLE);

                    var rolesList = await _roleService.GetAllRolesAsync(token);
                    if (canEditOnlyOwnUser)
                    {
                        rolesList = [.. rolesList.Where(r => r.Name != "Admin")];
                    }
                    ViewBag.Roles = rolesList;

                    return View(model);
                }

                model.Roles = selectedRoles;

                if (string.IsNullOrWhiteSpace(model.Password))
                {
                    ModelState.Remove("Password");
                }

                if (!ModelState.IsValid)
                {
                    var roles = await _roleService.GetAllRolesAsync(token);
                    if (canEditOnlyOwnUser)
                    {
                        roles = [.. roles.Where(r => r.Name != "Admin")];
                    }
                    ViewBag.Roles = roles;

                    return View(model);
                }

                await _userService.UpdateUserAsync(id, model, token);
                TempData["SuccessMessage"] = SUCCESS_USER_UPDATED;
                return RedirectToAction(nameof(Index));
            }
            catch (Exception ex)
            {
                TempData["ErrorMessage"] = string.Format(ERROR_UPDATING_USER, ex.Message);

                try
                {
                    var roles = await _roleService.GetAllRolesAsync(token);
                    var canEditOnlyOwnUser = await CheckCanEditOnlyOwnUserAsync(token);

                    if (canEditOnlyOwnUser)
                    {
                        roles = [.. roles.Where(r => r.Name != "Admin")];
                    }

                    ViewBag.Roles = roles;
                    return View(model);
                }
                catch
                {
                    return RedirectToAction(nameof(Index));
                }
            }
        }

        [RequirePermission("users.delete")]
        public async Task<IActionResult> Delete(string id)
        {
            if (string.IsNullOrEmpty(id))
            {
                return BadRequest("ID do usuário não fornecido");
            }

            try
            {
                var token = HttpContext.GetJwtToken();
                var user = await _userService.GetUserByIdAsync(id, token);

                if (user == null)
                {
                    return NotFound("Usuário não encontrado");
                }

                if (id == HttpContext.GetCurrentUserId())
                {
                    TempData["ErrorMessage"] = "Não é possível excluir o próprio usuário";
                    return RedirectToAction(nameof(Index));
                }

                return View(user);
            }
            catch (Exception ex)
            {
                TempData["ErrorMessage"] = string.Format(ERROR_LOADING_USER_DELETE, ex.Message);
                return RedirectToAction(nameof(Index));
            }
        }

        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        [RequirePermission("users.delete")]
        public async Task<IActionResult> DeleteConfirmed(string id)
        {
            if (string.IsNullOrEmpty(id))
            {
                return BadRequest("ID do usuário não fornecido");
            }

            try
            {
                if (id == HttpContext.GetCurrentUserId())
                {
                    TempData["ErrorMessage"] = "Não é possível excluir o próprio usuário";
                    return RedirectToAction(nameof(Index));
                }

                var token = HttpContext.GetJwtToken();
                await _userService.DeleteUserAsync(id, token);
                TempData["SuccessMessage"] = SUCCESS_USER_DELETED;
                return RedirectToAction(nameof(Index));
            }
            catch (Exception ex)
            {
                TempData["ErrorMessage"] = string.Format(ERROR_DELETING_USER, ex.Message);
                return RedirectToAction(nameof(Index));
            }
        }

        private async Task<bool> CheckCanEditOnlyOwnUserAsync(string token)
        {
            var currentUserId = HttpContext.GetCurrentUserId()!;
            var permissions = await _permissionService.GetPermissionsByUserIdAsync(currentUserId, token);
            return PermissionHelper.PodeEditarSomenteProprioUsuario(permissions);
        }
    }
}