using FinanceSystem.Web.Filters;
using FinanceSystem.Web.Models;
using FinanceSystem.Web.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace FinanceSystem.Web.Controllers
{
    [Authorize(Roles = "Admin")]
    public class PermissionsController : Controller
    {
        private readonly IPermissionService _permissionService;
        private readonly IRoleService _roleService;

        public PermissionsController(IPermissionService permissionService, IRoleService roleService)
        {
            _permissionService = permissionService;
            _roleService = roleService;
        }

        public async Task<IActionResult> Index()
        {
            try
            {
                var token = HttpContext.Session.GetString("JWToken");
                var permissions = await _permissionService.GetAllPermissionsAsync(token);
                return View(permissions);
            }
            catch (Exception ex)
            {
                TempData["ErrorMessage"] = $"Erro ao carregar permissões: {ex.Message}";
                return RedirectToAction("Index", "Home");
            }
        }

        public async Task<IActionResult> Details(string id)
        {
            try
            {
                var token = HttpContext.Session.GetString("JWToken");
                var permission = await _permissionService.GetPermissionByIdAsync(id, token);
                return View(permission);
            }
            catch (Exception ex)
            {
                TempData["ErrorMessage"] = $"Erro ao carregar detalhes da permissão: {ex.Message}";
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
            try
            {
                if (ModelState.IsValid)
                {
                    var token = HttpContext.Session.GetString("JWToken");
                    await _permissionService.CreatePermissionAsync(model, token);
                    TempData["SuccessMessage"] = "Permissão criada com sucesso!";
                    return RedirectToAction(nameof(Index));
                }
                return View(model);
            }
            catch (Exception ex)
            {
                TempData["ErrorMessage"] = $"Erro ao criar permissão: {ex.Message}";
                return View(model);
            }
        }

        public async Task<IActionResult> Edit(string id)
        {
            try
            {
                var token = HttpContext.Session.GetString("JWToken");
                var permission = await _permissionService.GetPermissionByIdAsync(id, token);

                var model = new UpdatePermissionModel
                {
                    Name = permission.Name,
                    Description = permission.Description
                };

                return View(model);
            }
            catch (Exception ex)
            {
                TempData["ErrorMessage"] = $"Erro ao carregar permissão para edição: {ex.Message}";
                return RedirectToAction(nameof(Index));
            }
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(string id, UpdatePermissionModel model)
        {
            try
            {
                if (ModelState.IsValid)
                {
                    var token = HttpContext.Session.GetString("JWToken");
                    await _permissionService.UpdatePermissionAsync(id, model, token);
                    TempData["SuccessMessage"] = "Permissão atualizada com sucesso!";
                    return RedirectToAction(nameof(Index));
                }
                return View(model);
            }
            catch (Exception ex)
            {
                TempData["ErrorMessage"] = $"Erro ao atualizar permissão: {ex.Message}";
                return View(model);
            }
        }

        public async Task<IActionResult> Delete(string id)
        {
            try
            {
                var token = HttpContext.Session.GetString("JWToken");
                var permission = await _permissionService.GetPermissionByIdAsync(id, token);
                return View(permission);
            }
            catch (Exception ex)
            {
                TempData["ErrorMessage"] = $"Erro ao carregar permissão para exclusão: {ex.Message}";
                return RedirectToAction(nameof(Index));
            }
        }

        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DeleteConfirmed(string id)
        {
            try
            {
                var token = HttpContext.Session.GetString("JWToken");
                await _permissionService.DeletePermissionAsync(id, token);
                TempData["SuccessMessage"] = "Permissão excluída com sucesso!";
                return RedirectToAction(nameof(Index));
            }
            catch (Exception ex)
            {
                TempData["ErrorMessage"] = $"Erro ao excluir permissão: {ex.Message}";
                return RedirectToAction(nameof(Index));
            }
        }

        public async Task<IActionResult> RolePermissions(string roleId)
        {
            try
            {
                var token = HttpContext.Session.GetString("JWToken");
                var role = await _roleService.GetRoleByIdAsync(roleId, token);
                var allPermissions = await _permissionService.GetAllPermissionsAsync(token);
                var rolePermissions = await _permissionService.GetPermissionsByRoleIdAsync(roleId, token);

                ViewBag.Role = role;
                ViewBag.AllPermissions = allPermissions;
                ViewBag.RolePermissions = rolePermissions.Select(p => p.Id).ToList();

                return View();
            }
            catch (Exception ex)
            {
                TempData["ErrorMessage"] = $"Erro ao carregar permissões do perfil: {ex.Message}";
                return RedirectToAction("Index", "Roles");
            }
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> UpdateRolePermissions(string roleId, List<string> selectedPermissions)
        {
            try
            {
                var token = HttpContext.Session.GetString("JWToken");
                var role = await _roleService.GetRoleByIdAsync(roleId, token);
                var allPermissions = await _permissionService.GetAllPermissionsAsync(token);
                var rolePermissions = await _permissionService.GetPermissionsByRoleIdAsync(roleId, token);

                // Converter IDs de string para Guid
                var selectedPermissionIds = selectedPermissions != null
                    ? selectedPermissions.Select(p => Guid.Parse(p)).ToList()
                    : new List<Guid>();

                // Atualizar permissões do perfil via Roles API
                await _roleService.UpdateRolePermissionsAsync(roleId, selectedPermissionIds.Select(p => p.ToString()).ToList(), token);

                TempData["SuccessMessage"] = "Permissões do perfil atualizadas com sucesso!";
                return RedirectToAction("Details", "Roles", new { id = roleId });
            }
            catch (Exception ex)
            {
                TempData["ErrorMessage"] = $"Erro ao atualizar permissões do perfil: {ex.Message}";
                return RedirectToAction("Details", "Roles", new { id = roleId });
            }
        }
    }
}