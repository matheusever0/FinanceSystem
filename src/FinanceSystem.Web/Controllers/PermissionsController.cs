using FinanceSystem.Web.Extensions;
using FinanceSystem.Web.Filters;
using FinanceSystem.Web.Models.Permission;
using FinanceSystem.Web.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace FinanceSystem.Web.Controllers
{
    [Authorize]
    [RequirePermission("permissions.manage")]
    public class PermissionsController : Controller
    {
        private readonly IPermissionService _permissionService;
        private readonly IRoleService _roleService;

        private const string ERROR_LOADING_PERMISSIONS = "Erro ao carregar permissões: {0}";
        private const string ERROR_LOADING_PERMISSION_DETAILS = "Erro ao carregar detalhes da permissão: {0}";
        private const string ERROR_CREATING_PERMISSION = "Erro ao criar permissão: {0}";
        private const string ERROR_LOADING_PERMISSION_EDIT = "Erro ao carregar permissão para edição: {0}";
        private const string ERROR_UPDATING_PERMISSION = "Erro ao atualizar permissão: {0}";
        private const string ERROR_LOADING_PERMISSION_DELETE = "Erro ao carregar permissão para exclusão: {0}";
        private const string ERROR_DELETING_PERMISSION = "Erro ao excluir permissão: {0}";
        private const string ERROR_LOADING_ROLE_PERMISSIONS = "Erro ao carregar permissões do perfil: {0}";
        private const string ERROR_UPDATING_ROLE_PERMISSIONS = "Erro ao atualizar permissões do perfil: {0}";

        private const string SUCCESS_CREATE_PERMISSION = "Permissão criada com sucesso!";
        private const string SUCCESS_UPDATE_PERMISSION = "Permissão atualizada com sucesso!";
        private const string SUCCESS_DELETE_PERMISSION = "Permissão excluída com sucesso!";
        private const string SUCCESS_UPDATE_ROLE_PERMISSIONS = "Permissões do perfil atualizadas com sucesso!";

        public PermissionsController(IPermissionService permissionService, IRoleService roleService)
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
                TempData["ErrorMessage"] = string.Format(ERROR_LOADING_PERMISSIONS, ex.Message);
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
                TempData["ErrorMessage"] = string.Format(ERROR_LOADING_PERMISSION_DETAILS, ex.Message);
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
                await _permissionService.CreatePermissionAsync(model, token);
                TempData["SuccessMessage"] = SUCCESS_CREATE_PERMISSION;
                return RedirectToAction(nameof(Index));
            }
            catch (Exception ex)
            {
                TempData["ErrorMessage"] = string.Format(ERROR_CREATING_PERMISSION, ex.Message);
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
                TempData["ErrorMessage"] = string.Format(ERROR_LOADING_PERMISSION_EDIT, ex.Message);
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
                TempData["SuccessMessage"] = SUCCESS_UPDATE_PERMISSION;
                return RedirectToAction(nameof(Index));
            }
            catch (Exception ex)
            {
                TempData["ErrorMessage"] = string.Format(ERROR_UPDATING_PERMISSION, ex.Message);
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
                TempData["ErrorMessage"] = string.Format(ERROR_LOADING_PERMISSION_DELETE, ex.Message);
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
                TempData["SuccessMessage"] = SUCCESS_DELETE_PERMISSION;
                return RedirectToAction(nameof(Index));
            }
            catch (Exception ex)
            {
                TempData["ErrorMessage"] = string.Format(ERROR_DELETING_PERMISSION, ex.Message);
                return RedirectToAction(nameof(Index));
            }
        }

        public async Task<IActionResult> RolePermissions(string roleId)
        {
            if (string.IsNullOrEmpty(roleId))
            {
                return BadRequest("ID do perfil não fornecido");
            }

            try
            {
                var token = HttpContext.GetJwtToken();
                var role = await _roleService.GetRoleByIdAsync(roleId, token);

                if (role == null)
                {
                    return NotFound("Perfil não encontrado");
                }

                var allPermissions = await _permissionService.GetAllPermissionsAsync(token);
                var rolePermissions = await _permissionService.GetPermissionsByRoleIdAsync(roleId, token);

                ViewBag.Role = role;
                ViewBag.AllPermissions = allPermissions;
                ViewBag.RolePermissions = rolePermissions.Select(p => p.Id).ToList();

                return View();
            }
            catch (Exception ex)
            {
                TempData["ErrorMessage"] = string.Format(ERROR_LOADING_ROLE_PERMISSIONS, ex.Message);
                return RedirectToAction("Index", "Roles");
            }
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> UpdateRolePermissions(string roleId, List<string> selectedPermissions)
        {
            if (string.IsNullOrEmpty(roleId))
            {
                return BadRequest("ID do perfil não fornecido");
            }

            try
            {
                var token = HttpContext.GetJwtToken();
                var role = await _roleService.GetRoleByIdAsync(roleId, token);

                if (role == null)
                {
                    return NotFound("Perfil não encontrado");
                }

                var selectedPermissionIds = selectedPermissions != null
                    ? selectedPermissions.Select(p => Guid.Parse(p)).ToList()
                    : [];

                await _roleService.UpdateRolePermissionsAsync(roleId, [.. selectedPermissionIds.Select(p => p.ToString())], token);

                TempData["SuccessMessage"] = SUCCESS_UPDATE_ROLE_PERMISSIONS;
                return RedirectToAction("Details", "Roles", new { id = roleId });
            }
            catch (Exception ex)
            {
                TempData["ErrorMessage"] = string.Format(ERROR_UPDATING_ROLE_PERMISSIONS, ex.Message);
                return RedirectToAction("Details", "Roles", new { id = roleId });
            }
        }
    }
}