using Equilibrium.Resources.Web.Enums;
using Equilibrium.Resources.Web.Helpers;
using Equilibrium.Web.Filters;
using Equilibrium.Web.Interfaces;
using Equilibrium.Web.Models.Role;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace Equilibrium.Web.Controllers
{
    [Authorize]
    [RequirePermission("roles.view")]
    public class RolesController : BaseController
    {
        private readonly IRoleService _roleService;
        private readonly IPermissionService _permissionService;

        public RolesController(
            IRoleService roleService,
            IPermissionService permissionService,
            IUserService userService)
        {
            _roleService = roleService;
            _permissionService = permissionService;
        }

        public async Task<IActionResult> Index()
        {
            try
            {
                var token = GetToken();
                var roles = await _roleService.GetAllRolesAsync(token);
                return View(roles);
            }
            catch (Exception ex)
            {
                TempData["ErrorMessage"] = MessageHelper.GetLoadingErrorMessage(EntityNames.Role, ex);
                return RedirectToAction("Index", "Home");
            }
        }

        public async Task<IActionResult> Details(string id)
        {
            if (string.IsNullOrEmpty(id))
            {
                return BadRequest("ID do perfil não fornecido");
            }

            try
            {
                var token = GetToken();
                var role = await _roleService.GetRoleByIdAsync(id, token);

                if (role == null)
                {
                    return NotFound("Perfil não encontrado");
                }

                var permissions = await _permissionService.GetPermissionsByRoleIdAsync(id, token);
                ViewBag.Permissions = permissions;

                return View(role);
            }
            catch (Exception ex)
            {
                TempData["ErrorMessage"] = MessageHelper.GetLoadingErrorMessage(EntityNames.Role, ex);
                return RedirectToAction(nameof(Index));
            }
        }

        [RequirePermission("roles.create")]
        public IActionResult Create()
        {
            return View();
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        [RequirePermission("roles.create")]
        public async Task<IActionResult> Create(CreateRoleModel model)
        {
            if (!ModelState.IsValid)
            {
                return View(model);
            }

            try
            {
                var token = GetToken();
                var role = await _roleService.CreateRoleAsync(model, token);
                TempData["SuccessMessage"] = MessageHelper.GetCreationSuccessMessage(EntityNames.Role);
                return RedirectToAction(nameof(Details), new { id = role.Id });
            }
            catch (Exception ex)
            {
                ModelState.AddModelError(string.Empty, MessageHelper.GetCreationErrorMessage(EntityNames.Role, ex));
                return View(model);
            }
        }

        [RequirePermission("roles.edit")]
        public async Task<IActionResult> Edit(string id)
        {
            if (string.IsNullOrEmpty(id))
            {
                return BadRequest("ID do perfil não fornecido");
            }

            try
            {
                var token = GetToken();
                var role = await _roleService.GetRoleByIdAsync(id, token);

                if (role == null)
                {
                    return NotFound("Perfil não encontrado");
                }

                var model = new UpdateRoleModel
                {
                    Name = role.Name,
                    Description = role.Description
                };

                return View(model);
            }
            catch (Exception ex)
            {
                TempData["ErrorMessage"] = MessageHelper.GetLoadingErrorMessage(EntityNames.Role, ex);
                return RedirectToAction(nameof(Index));
            }
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        [RequirePermission("roles.edit")]
        public async Task<IActionResult> Edit(string id, UpdateRoleModel model)
        {
            if (string.IsNullOrEmpty(id))
            {
                return BadRequest("ID do perfil não fornecido");
            }

            if (!ModelState.IsValid)
            {
                return View(model);
            }

            try
            {
                var token = GetToken();
                await _roleService.UpdateRoleAsync(id, model, token);
                TempData["SuccessMessage"] = MessageHelper.GetUpdateSuccessMessage(EntityNames.Role);
                return RedirectToAction(nameof(Details), new { id });
            }
            catch (Exception ex)
            {
                ModelState.AddModelError(string.Empty, MessageHelper.GetUpdateErrorMessage(EntityNames.Role, ex));
                return View(model);
            }
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        [RequirePermission("roles.delete")]
        public async Task<IActionResult> Delete(string id)
        {
            return await HandleGenericDelete(
                id,
                _roleService,
                async (service, itemId, token) => await service.DeleteRoleAsync(itemId, token),
                async (service, itemId, token) => await service.GetRoleByIdAsync(itemId, token),
                "perfil"
            );
        }

        [RequirePermission("permissions.manage")]
        public async Task<IActionResult> ManagePermissions(string id)
        {
            if (string.IsNullOrEmpty(id))
            {
                return BadRequest("ID do perfil não fornecido");
            }

            try
            {
                var token = GetToken();
                var role = await _roleService.GetRoleByIdAsync(id, token);

                if (role == null)
                {
                    return NotFound("Perfil não encontrado");
                }

                var allPermissions = await _permissionService.GetAllPermissionsAsync(token);
                var rolePermissions = await _permissionService.GetPermissionsByRoleIdAsync(id, token);

                ViewBag.Role = role;
                ViewBag.AllPermissions = allPermissions;
                ViewBag.RolePermissions = rolePermissions.Select(p => p.Id).ToList();

                return View();
            }
            catch (Exception ex)
            {
                TempData["ErrorMessage"] = MessageHelper.GetLoadingErrorMessage(EntityNames.Role, ex);
                return RedirectToAction(nameof(Details), new { id });
            }
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        [RequirePermission("permissions.manage")]
        public async Task<IActionResult> ManagePermissions(string id, List<string> selectedPermissions)
        {
            if (string.IsNullOrEmpty(id))
            {
                return BadRequest("ID do perfil não fornecido");
            }

            try
            {
                var token = GetToken();
                var role = await _roleService.GetRoleByIdAsync(id, token);

                if (role == null)
                {
                    return NotFound("Perfil não encontrado");
                }

                var permissionList = selectedPermissions ?? [];
                await _roleService.UpdateRolePermissionsAsync(id, permissionList, token);

                TempData["SuccessMessage"] = MessageHelper.GetUpdateSuccessMessage(EntityNames.Role);
                return RedirectToAction(nameof(Details), new { id });
            }
            catch (Exception ex)
            {
                TempData["ErrorMessage"] = MessageHelper.GetUpdateErrorMessage(EntityNames.Role, ex);
                return RedirectToAction(nameof(Details), new { id });
            }
        }
    }
}

