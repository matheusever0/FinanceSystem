using FinanceSystem.Web.Filters;
using FinanceSystem.Web.Models;
using FinanceSystem.Web.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace FinanceSystem.Web.Controllers
{
    [Authorize]
    public class RolesController : Controller
    {
        private readonly IRoleService _roleService;
        private readonly IPermissionService _permissionService;

        public RolesController(IRoleService roleService, IPermissionService permissionService)
        {
            _roleService = roleService;
            _permissionService = permissionService;
        }

        [RequirePermission("roles.view")]
        public async Task<IActionResult> Index()
        {
            try
            {
                var token = HttpContext.Session.GetString("JWToken");
                var roles = await _roleService.GetAllRolesAsync(token);
                return View(roles);
            }
            catch (Exception ex)
            {
                TempData["ErrorMessage"] = $"Erro ao carregar perfis: {ex.Message}";
                return RedirectToAction("Index", "Home");
            }
        }

        [RequirePermission("roles.view")]
        public async Task<IActionResult> Details(string id)
        {
            try
            {
                var token = HttpContext.Session.GetString("JWToken");
                var role = await _roleService.GetRoleByIdAsync(id, token);

                var permissions = await _permissionService.GetPermissionsByRoleIdAsync(id, token);
                ViewBag.Permissions = permissions;

                return View(role);
            }
            catch (Exception ex)
            {
                TempData["ErrorMessage"] = $"Erro ao carregar detalhes do perfil: {ex.Message}";
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
            try
            {
                if (ModelState.IsValid)
                {
                    var token = HttpContext.Session.GetString("JWToken");
                    await _roleService.CreateRoleAsync(model, token);
                    TempData["SuccessMessage"] = "Perfil criado com sucesso!";
                    return RedirectToAction(nameof(Index));
                }
                return View(model);
            }
            catch (Exception ex)
            {
                TempData["ErrorMessage"] = $"Erro ao criar perfil: {ex.Message}";
                return RedirectToAction(nameof(Index));
            }
        }

        [RequirePermission("roles.edit")]
        public async Task<IActionResult> Edit(string id)
        {
            try
            {
                var token = HttpContext.Session.GetString("JWToken");
                var role = await _roleService.GetRoleByIdAsync(id, token);

                var model = new UpdateRoleModel
                {
                    Name = role.Name,
                    Description = role.Description
                };

                return View(model);
            }
            catch (Exception ex)
            {
                TempData["ErrorMessage"] = $"Erro ao carregar perfil para edição: {ex.Message}";
                return RedirectToAction(nameof(Index));
            }
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        [RequirePermission("roles.edit")]
        public async Task<IActionResult> Edit(string id, UpdateRoleModel model)
        {
            try
            {
                if (ModelState.IsValid)
                {
                    var token = HttpContext.Session.GetString("JWToken");
                    await _roleService.UpdateRoleAsync(id, model, token);
                    TempData["SuccessMessage"] = "Perfil atualizado com sucesso!";
                    return RedirectToAction(nameof(Index));
                }
                return View(model);
            }
            catch (Exception ex)
            {
                TempData["ErrorMessage"] = $"Erro ao atualizar perfil: {ex.Message}";
                return RedirectToAction(nameof(Index));
            }
        }

        [RequirePermission("roles.delete")]
        public async Task<IActionResult> Delete(string id)
        {
            try
            {
                var token = HttpContext.Session.GetString("JWToken");
                var role = await _roleService.GetRoleByIdAsync(id, token);
                return View(role);
            }
            catch (Exception ex)
            {
                TempData["ErrorMessage"] = $"Erro ao carregar perfil para exclusão: {ex.Message}";
                return RedirectToAction(nameof(Index));
            }
        }

        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        [RequirePermission("roles.delete")]
        public async Task<IActionResult> DeleteConfirmed(string id)
        {
            try
            {
                var token = HttpContext.Session.GetString("JWToken");
                await _roleService.DeleteRoleAsync(id, token);
                TempData["SuccessMessage"] = "Perfil excluído com sucesso!";
                return RedirectToAction(nameof(Index));
            }
            catch (Exception ex)
            {
                TempData["ErrorMessage"] = $"Erro ao excluir perfil: {ex.Message}";
                return RedirectToAction(nameof(Index));
            }
        }

        [RequirePermission("permissions.manage")]
        public async Task<IActionResult> ManagePermissions(string id)
        {
            try
            {
                var token = HttpContext.Session.GetString("JWToken");
                var role = await _roleService.GetRoleByIdAsync(id, token);
                var allPermissions = await _permissionService.GetAllPermissionsAsync(token);
                var rolePermissions = await _permissionService.GetPermissionsByRoleIdAsync(id, token);

                ViewBag.Role = role;
                ViewBag.AllPermissions = allPermissions;
                ViewBag.RolePermissions = rolePermissions.Select(p => p.Id).ToList();

                return View();
            }
            catch (Exception ex)
            {
                TempData["ErrorMessage"] = $"Erro ao gerenciar permissões do perfil: {ex.Message}";
                return RedirectToAction(nameof(Details), new { id });
            }
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        [RequirePermission("permissions.manage")]
        public async Task<IActionResult> ManagePermissions(string id, List<string> selectedPermissions)
        {
            try
            {
                var token = HttpContext.Session.GetString("JWToken");

                var permissionList = selectedPermissions ?? new List<string>();

                await _roleService.UpdateRolePermissionsAsync(id, permissionList, token);

                TempData["SuccessMessage"] = "Permissões do perfil atualizadas com sucesso!";
                return RedirectToAction(nameof(Details), new { id });
            }
            catch (Exception ex)
            {
                TempData["ErrorMessage"] = $"Erro ao atualizar permissões do perfil: {ex.Message}";
                return RedirectToAction(nameof(Details), new { id });
            }
        }
    }
}