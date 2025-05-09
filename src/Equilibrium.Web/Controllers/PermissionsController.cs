using Equilibrium.Resources.Web.Enums;
using Equilibrium.Resources.Web.Helpers;
using Equilibrium.Web.Extensions;
using Equilibrium.Web.Filters;
using Equilibrium.Web.Models.Filters;
using Equilibrium.Web.Models.Permission;
using Equilibrium.Web.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace Equilibrium.Web.Controllers
{
    [Authorize]
    [RequirePermission("permissions.manage")]
    public class PermissionsController : Controller
    {
        private readonly IPermissionService _permissionService;
        private readonly IRoleService _roleService;

        public PermissionsController(
            IPermissionService permissionService,
            IRoleService roleService)
        {
            _permissionService = permissionService ?? throw new ArgumentNullException(nameof(permissionService));
            _roleService = roleService ?? throw new ArgumentNullException(nameof(roleService));
        }

        public async Task<IActionResult> Index()
        {
            try
            {
                var token = HttpContext.GetJwtToken();
                var permissions = await _permissionService.GetAllPermissionsAsync(token);
                return View(permissions);
            }
            catch (Exception ex)
            {
                TempData["ErrorMessage"] = MessageHelper.GetLoadingErrorMessage(EntityNames.Permission, ex);
                return RedirectToAction("Index", "Home");
            }
        }

        public async Task<IActionResult> Details(string id)
        {
            if (string.IsNullOrEmpty(id))
            {
                return BadRequest("ID da permissão não fornecido");
            }

            try
            {
                var token = HttpContext.GetJwtToken();
                var permission = await _permissionService.GetPermissionByIdAsync(id, token);

                return permission == null ? NotFound("Permissão não encontrada") : View(permission);
            }
            catch (Exception ex)
            {
                TempData["ErrorMessage"] = MessageHelper.GetLoadingErrorMessage(EntityNames.Permission, ex);
                return RedirectToAction(nameof(Index));
            }
        }

        public IActionResult Create()
        {
            return View();
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create(CreatePermissionModel model)
        {
            if (!ModelState.IsValid)
            {
                return View(model);
            }

            try
            {
                var token = HttpContext.GetJwtToken();
                var permission = await _permissionService.CreatePermissionAsync(model, token);
                TempData["SuccessMessage"] = MessageHelper.GetCreationSuccessMessage(EntityNames.Permission);
                return RedirectToAction(nameof(Details), new { id = permission.Id });
            }
            catch (Exception ex)
            {
                ModelState.AddModelError(string.Empty, MessageHelper.GetCreationErrorMessage(EntityNames.Permission, ex));
                return View(model);
            }
        }

        public async Task<IActionResult> Edit(string id)
        {
            if (string.IsNullOrEmpty(id))
            {
                return BadRequest("ID da permissão não fornecido");
            }

            try
            {
                var token = HttpContext.GetJwtToken();
                var permission = await _permissionService.GetPermissionByIdAsync(id, token);

                if (permission == null)
                {
                    return NotFound("Permissão não encontrada");
                }

                var model = new UpdatePermissionModel
                {
                    Name = permission.Name,
                    Description = permission.Description
                };

                return View(model);
            }
            catch (Exception ex)
            {
                TempData["ErrorMessage"] = MessageHelper.GetLoadingErrorMessage(EntityNames.Permission, ex);
                return RedirectToAction(nameof(Index));
            }
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(string id, UpdatePermissionModel model)
        {
            if (string.IsNullOrEmpty(id))
            {
                return BadRequest("ID da permissão não fornecido");
            }

            if (!ModelState.IsValid)
            {
                return View(model);
            }

            try
            {
                var token = HttpContext.GetJwtToken();
                await _permissionService.UpdatePermissionAsync(id, model, token);
                TempData["SuccessMessage"] = MessageHelper.GetUpdateSuccessMessage(EntityNames.Permission);
                return RedirectToAction(nameof(Details), new { id });
            }
            catch (Exception ex)
            {
                ModelState.AddModelError(string.Empty, MessageHelper.GetUpdateErrorMessage(EntityNames.Permission, ex));
                return View(model);
            }
        }

        public async Task<IActionResult> Delete(string id)
        {
            if (string.IsNullOrEmpty(id))
            {
                return BadRequest("ID da permissão não fornecido");
            }

            try
            {
                var token = HttpContext.GetJwtToken();
                var permission = await _permissionService.GetPermissionByIdAsync(id, token);

                return permission == null ? NotFound("Permissão não encontrada") : View(permission);
            }
            catch (Exception ex)
            {
                TempData["ErrorMessage"] = MessageHelper.GetLoadingErrorMessage(EntityNames.Permission, ex);
                return RedirectToAction(nameof(Index));
            }
        }

        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DeleteConfirmed(string id)
        {
            if (string.IsNullOrEmpty(id))
            {
                return BadRequest("ID da permissão não fornecido");
            }

            try
            {
                var token = HttpContext.GetJwtToken();
                await _permissionService.DeletePermissionAsync(id, token);
                TempData["SuccessMessage"] = MessageHelper.GetDeletionSuccessMessage(EntityNames.Permission);
                return RedirectToAction(nameof(Index));
            }
            catch (Exception ex)
            {
                TempData["ErrorMessage"] = MessageHelper.GetDeletionErrorMessage(EntityNames.Permission, ex);
                return RedirectToAction(nameof(Index));
            }
        }

        public async Task<IActionResult> RolePermissions(string id)
        {
            if (string.IsNullOrEmpty(id))
            {
                return BadRequest("ID do perfil não fornecido");
            }

            try
            {
                var token = HttpContext.GetJwtToken();
                var role = await _roleService.GetRoleByIdAsync(id, token);

                if (role == null)
                {
                    return NotFound("Perfil não encontrado");
                }

                var permissions = await _permissionService.GetAllPermissionsAsync(token);
                var rolePermissions = await _permissionService.GetPermissionsByRoleIdAsync(id, token);

                ViewBag.Role = role;
                ViewBag.AllPermissions = permissions;
                ViewBag.RolePermissions = rolePermissions.Select(p => p.Id).ToList();

                return View();
            }
            catch (Exception ex)
            {
                TempData["ErrorMessage"] = MessageHelper.GetLoadingErrorMessage(EntityNames.Permission, ex);
                return RedirectToAction("Index", "Roles");
            }
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> UpdateRolePermissions(string id, List<string> permissions)
        {
            if (string.IsNullOrEmpty(id))
            {
                return BadRequest("ID do perfil não fornecido");
            }

            try
            {
                var token = HttpContext.GetJwtToken();
                permissions ??= new List<string>();

                await _roleService.UpdateRolePermissionsAsync(id, permissions, token);
                TempData["SuccessMessage"] = MessageHelper.GetUpdateSuccessMessage(EntityNames.Permission);
                return RedirectToAction("Details", "Roles", new { id });
            }
            catch (Exception ex)
            {
                TempData["ErrorMessage"] = "Erro ao atualizar permissões do perfil: " + ex.Message;
                return RedirectToAction("RolePermissions", new { id });
            }
        }

        [HttpGet("filter")]
        [RequirePermission("permissions.manage")]
        public async Task<IActionResult> Filter(PermissionFilter filter = null)
        {
            if (filter == null)
                filter = new PermissionFilter();

            try
            {
                var token = HttpContext.GetJwtToken();
                var result = await _permissionService.GetFilteredAsync(filter, token);

                // Add pagination headers
                Response.Headers.Add("X-Pagination-Total", result.TotalCount.ToString());
                Response.Headers.Add("X-Pagination-Pages", result.TotalPages.ToString());
                Response.Headers.Add("X-Pagination-Page", result.PageNumber.ToString());
                Response.Headers.Add("X-Pagination-Size", result.PageSize.ToString());

                ViewBag.Filter = filter;
                ViewBag.TotalCount = result.TotalCount;
                ViewBag.TotalPages = result.TotalPages;
                ViewBag.CurrentPage = result.PageNumber;
                ViewBag.PageSize = result.PageSize;

                return View("Index", result.Items);
            }
            catch (Exception ex)
            {
                TempData["ErrorMessage"] = MessageHelper.GetLoadingErrorMessage(EntityNames.Permission, ex);
                return RedirectToAction("Index", "Home");
            }
        }

        [HttpGet("api/filter")]
        [RequirePermission("permissions.manage")]
        public async Task<IActionResult> FilterJson([FromQuery] PermissionFilter filter)
        {
            if (filter == null)
                filter = new PermissionFilter();

            try
            {
                var token = HttpContext.GetJwtToken();
                var result = await _permissionService.GetFilteredAsync(filter, token);

                return Json(new
                {
                    items = result.Items,
                    totalCount = result.TotalCount,
                    pageNumber = result.PageNumber,
                    pageSize = result.PageSize,
                    totalPages = result.TotalPages,
                    hasPreviousPage = result.HasPreviousPage,
                    hasNextPage = result.HasNextPage
                });
            }
            catch (Exception ex)
            {
                return BadRequest(new { error = ex.Message });
            }
        }
    }
}
