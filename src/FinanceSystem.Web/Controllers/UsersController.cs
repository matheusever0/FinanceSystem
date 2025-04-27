using FinanceSystem.Resources.Web;
using FinanceSystem.Resources.Web.Enums;
using FinanceSystem.Resources.Web.Helpers;
using FinanceSystem.Web.Extensions;
using FinanceSystem.Web.Filters;
using FinanceSystem.Web.Helpers;
using FinanceSystem.Web.Models.User;
using FinanceSystem.Web.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System.Security.Claims;

namespace FinanceSystem.Web.Controllers
{
    [Authorize]
    [RequirePermission("users.view")]
    public class UsersController : Controller
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
                TempData["ErrorMessage"] = MessageHelper.GetLoadingErrorMessage(EntityNames.User, ex);
                return RedirectToAction("Index", "Home");
            }
        }

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
                var token = HttpContext.GetJwtToken();
                var roles = await _roleService.GetAllRolesAsync(token);
                ViewBag.Roles = roles;
                return View();
            }
            catch (Exception ex)
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

            if (model.Roles == null || !model.Roles.Any())
            {
                ModelState.AddModelError("Roles", ResourceFinanceWeb.Validation_SelectRole);
                await LoadRolesForView();
                return View(model);
            }

            try
            {
                var token = HttpContext.GetJwtToken();
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

        [RequirePermission("users.edit")]
        public async Task<IActionResult> Edit(string id)
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

                var currentUserId = HttpContext.User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
                var userPermissions = await _permissionService.GetPermissionsByUserIdAsync(currentUserId, token);

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
        [RequirePermission("users.edit")]
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

            if (model.Roles == null || !model.Roles.Any())
            {
                ModelState.AddModelError("Roles", ResourceFinanceWeb.Validation_SelectRole);
                await LoadRolesForView();
                return View(model);
            }

            try
            {
                var token = HttpContext.GetJwtToken();

                var currentUserId = HttpContext.User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
                var userPermissions = await _permissionService.GetPermissionsByUserIdAsync(currentUserId, token);

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

                var currentUserId = HttpContext.User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
                if (id == currentUserId)
                {
                    TempData["ErrorMessage"] = "Não é possível excluir seu próprio usuário.";
                    return RedirectToAction(nameof(Index));
                }

                return View(user);
            }
            catch (Exception ex)
            {
                TempData["ErrorMessage"] = MessageHelper.GetLoadingErrorMessage(EntityNames.User, ex);
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
                var token = HttpContext.GetJwtToken();

                var currentUserId = HttpContext.User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
                if (id == currentUserId)
                {
                    TempData["ErrorMessage"] = "Não é possível excluir seu próprio usuário.";
                    return RedirectToAction(nameof(Index));
                }

                await _userService.DeleteUserAsync(id, token);
                TempData["SuccessMessage"] = MessageHelper.GetDeletionSuccessMessage(EntityNames.User);
                return RedirectToAction(nameof(Index));
            }
            catch (Exception ex)
            {
                TempData["ErrorMessage"] = MessageHelper.GetDeletionErrorMessage(EntityNames.User, ex);
                return RedirectToAction(nameof(Index));
            }
        }

        private async Task LoadRolesForView()
        {
            try
            {
                var token = HttpContext.GetJwtToken();
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