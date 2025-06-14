using Equilibrium.Resources.Web;
using Equilibrium.Resources.Web.Enums;
using Equilibrium.Resources.Web.Helpers;
using Equilibrium.Web.Filters;
using Equilibrium.Web.Helpers;
using Equilibrium.Web.Interfaces;
using Equilibrium.Web.Models.User;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System.Net.Http;
using System.Security.Claims;

namespace Equilibrium.Web.Controllers
{
    [Authorize]
    public class UsersController : BaseController
    {
        private readonly IUserService _userService;
        private readonly IRoleService _roleService;
        private readonly IPermissionService _permissionService;

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
                var token = GetToken();
                var users = await _userService.GetAllUsersAsync(token);
                return View(users);
            }
            catch (Exception ex)
            {
                TempData["ErrorMessage"] = MessageHelper.GetLoadingErrorMessage(EntityNames.User, ex);
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
                var token = GetToken();
                var user = await _userService.GetUserByIdAsync(id, token);

                if (user == null)
                {
                    return NotFound("Usuário não encontrado");
                }

                return View(user);
            }
            catch (Exception ex)
            {
                TempData["ErrorMessage"] = MessageHelper.GetLoadingErrorMessage(EntityNames.User, ex);
                return RedirectToAction(nameof(Index));
            }
        }

        [RequirePermission("users.create")]
        public async Task<IActionResult> Create()
        {
            try
            {
                var token = GetToken();
                var roles = await _roleService.GetAllRolesAsync(token);
                ViewBag.Roles = roles;
                return View();
            }
            catch (Exception)
            {
                TempData["ErrorMessage"] = ResourceFinanceWeb.Error_PreparingForm;
                return RedirectToAction(nameof(Index));
            }
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        [RequirePermission("users.create")]
        public async Task<IActionResult> Create(CreateUserModel model)
        {
            if (!ModelState.IsValid)
            {
                await LoadRolesForView();
                return View(model);
            }

            if (model.Roles == null || model.Roles.Count == 0)
            {
                ModelState.AddModelError("Roles", ResourceFinanceWeb.Validation_SelectRole);
                await LoadRolesForView();
                return View(model);
            }

            try
            {
                var token = GetToken();
                var user = await _userService.CreateUserAsync(model, token);
                TempData["SuccessMessage"] = MessageHelper.GetCreationSuccessMessage(EntityNames.User);
                return RedirectToAction(nameof(Details), new { id = user.Id });
            }
            catch (Exception ex)
            {
                ModelState.AddModelError(string.Empty, MessageHelper.GetCreationErrorMessage(EntityNames.User, ex));
                await LoadRolesForView();
                return View(model);
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
                var token = GetToken();
                var user = await _userService.GetUserByIdAsync(id, token);

                if (user == null)
                {
                    return NotFound("Usuário não encontrado");
                }

                var currentUserId = HttpContext.User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
                var userPermissions = await _permissionService.GetPermissionsByUserIdAsync(currentUserId!, token);

                if (userPermissions.PodeEditarSomenteProprioUsuario() && id != currentUserId)
                {
                    TempData["ErrorMessage"] = ResourceFinanceWeb.Error_PermissionDenied;
                    return RedirectToAction(nameof(Index));
                }

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
                TempData["ErrorMessage"] = MessageHelper.GetLoadingErrorMessage(EntityNames.User, ex);
                return RedirectToAction(nameof(Index));
            }
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        [RequirePermission("users.edit, users.edit.unique")]
        public async Task<IActionResult> Edit(string id, UpdateUserModel model)
        {
            if (string.IsNullOrEmpty(id))
            {
                return BadRequest("ID do usuário não fornecido");
            }

            if (!ModelState.IsValid)
            {
                await LoadRolesForView();
                return View(model);
            }

            if (model.Roles == null || model.Roles.Count == 0)
            {
                ModelState.AddModelError("Roles", ResourceFinanceWeb.Validation_SelectRole);
                await LoadRolesForView();
                return View(model);
            }

            try
            {
                var token = GetToken();

                var currentUserId = HttpContext.User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
                var userPermissions = await _permissionService.GetPermissionsByUserIdAsync(currentUserId!, token);

                if (userPermissions.PodeEditarSomenteProprioUsuario() && id != currentUserId)
                {
                    TempData["ErrorMessage"] = ResourceFinanceWeb.Error_PermissionDenied;
                    return RedirectToAction(nameof(Index));
                }

                await _userService.UpdateUserAsync(id, model, token);
                TempData["SuccessMessage"] = MessageHelper.GetUpdateSuccessMessage(EntityNames.User);
                return RedirectToAction(nameof(Details), new { id });
            }
            catch (Exception ex)
            {
                ModelState.AddModelError(string.Empty, MessageHelper.GetUpdateErrorMessage(EntityNames.User, ex));
                await LoadRolesForView();
                return View(model);
            }
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        [RequirePermission("users.delete")]
        public async Task<IActionResult> Delete(string id)
        {
            return await HandleGenericDelete(
                id,
                _userService,
                async (service, itemId, token) => await service.DeleteUserAsync(itemId, token),
                async (service, itemId, token) => await service.GetUserByIdAsync(itemId, token),
                "usuário",
                null,
                async (item) =>
                {
                    if (item is UserModel user && user.Id == GetCurrentUserId())
                    {
                        return (false, "Você não pode excluir sua própria conta.");
                    }
                    return (true, null);
                }
            );
        }

        private async Task LoadRolesForView()
        {
            try
            {
                var token = GetToken();
                var roles = await _roleService.GetAllRolesAsync(token);
                ViewBag.Roles = roles;
            }
            catch
            {
                ViewBag.Roles = new List<object>();
            }
        }
    }
}
