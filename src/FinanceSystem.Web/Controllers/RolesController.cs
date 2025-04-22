using FinanceSystem.Web.Extensions;
using FinanceSystem.Web.Filters;
using FinanceSystem.Web.Models.Role;
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

        private const string ERROR_LOADING_ROLES = "Erro ao carregar perfis: {0}";
        private const string ERROR_LOADING_ROLE_DETAILS = "Erro ao carregar detalhes do perfil: {0}";
        private const string ERROR_CREATING_ROLE = "Erro ao criar perfil: {0}";
        private const string ERROR_LOADING_ROLE_EDIT = "Erro ao carregar perfil para edição: {0}";
        private const string ERROR_UPDATING_ROLE = "Erro ao atualizar perfil: {0}";
        private const string ERROR_LOADING_ROLE_DELETE = "Erro ao carregar perfil para exclusão: {0}";
        private const string ERROR_DELETING_ROLE = "Erro ao excluir perfil: {0}";
        private const string ERROR_MANAGING_ROLE_PERMISSIONS = "Erro ao gerenciar permissões do perfil: {0}";
        private const string ERROR_UPDATING_ROLE_PERMISSIONS = "Erro ao atualizar permissões do perfil: {0}";

        private const string SUCCESS_CREATE_ROLE = "Perfil criado com sucesso!";
        private const string SUCCESS_UPDATE_ROLE = "Perfil atualizado com sucesso!";
        private const string SUCCESS_DELETE_ROLE = "Perfil excluído com sucesso!";
        private const string SUCCESS_UPDATE_ROLE_PERMISSIONS = "Permissões do perfil atualizadas com sucesso!";

        public RolesController(IRoleService roleService, IPermissionService permissionService)
        {
            _roleService = roleService ?? throw new ArgumentNullException(nameof(roleService));
            _permissionService = permissionService ?? throw new ArgumentNullException(nameof(permissionService));
        }

        [RequirePermission("roles.view")]
        public async Task<IActionResult> Index()
        {
            try
            {
                var token = HttpContext.GetJwtToken();
                var roles = await _roleService.GetAllRolesAsync(token);
                return View(roles);
            }
            catch (Exception ex)
            {
                TempData["ErrorMessage"] = string.Format(ERROR_LOADING_ROLES, ex.Message);
                return RedirectToAction("Index", "Home");
            }
        }

        [RequirePermission("roles.view")]
        public async Task<IActionResult> Details(string id)
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

                var permissions = await _permissionService.GetPermissionsByRoleIdAsync(id, token);
                ViewBag.Permissions = permissions;

                return View(role);
            }
            catch (Exception ex)
            {
                TempData["ErrorMessage"] = string.Format(ERROR_LOADING_ROLE_DETAILS, ex.Message);
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
                var token = HttpContext.GetJwtToken();
                await _roleService.CreateRoleAsync(model, token);
                TempData["SuccessMessage"] = SUCCESS_CREATE_ROLE;
                return RedirectToAction(nameof(Index));
            }
            catch (Exception ex)
            {
                TempData["ErrorMessage"] = string.Format(ERROR_CREATING_ROLE, ex.Message);
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
                var token = HttpContext.GetJwtToken();
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
                TempData["ErrorMessage"] = string.Format(ERROR_LOADING_ROLE_EDIT, ex.Message);
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
                var token = HttpContext.GetJwtToken();
                await _roleService.UpdateRoleAsync(id, model, token);
                TempData["SuccessMessage"] = SUCCESS_UPDATE_ROLE;
                return RedirectToAction(nameof(Index));
            }
            catch (Exception ex)
            {
                TempData["ErrorMessage"] = string.Format(ERROR_UPDATING_ROLE, ex.Message);
                return View(model);
            }
        }

        [RequirePermission("roles.delete")]
        public async Task<IActionResult> Delete(string id)
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

                return View(role);
            }
            catch (Exception ex)
            {
                TempData["ErrorMessage"] = string.Format(ERROR_LOADING_ROLE_DELETE, ex.Message);
                return RedirectToAction(nameof(Index));
            }
        }

        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        [RequirePermission("roles.delete")]
        public async Task<IActionResult> DeleteConfirmed(string id)
        {
            if (string.IsNullOrEmpty(id))
            {
                return BadRequest("ID do perfil não fornecido");
            }

            try
            {
                var token = HttpContext.GetJwtToken();
                await _roleService.DeleteRoleAsync(id, token);
                TempData["SuccessMessage"] = SUCCESS_DELETE_ROLE;
                return RedirectToAction(nameof(Index));
            }
            catch (Exception ex)
            {
                TempData["ErrorMessage"] = string.Format(ERROR_DELETING_ROLE, ex.Message);
                return RedirectToAction(nameof(Index));
            }
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
                var token = HttpContext.GetJwtToken();
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
                TempData["ErrorMessage"] = string.Format(ERROR_MANAGING_ROLE_PERMISSIONS, ex.Message);
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
                var token = HttpContext.GetJwtToken();
                var role = await _roleService.GetRoleByIdAsync(id, token);

                if (role == null)
                {
                    return NotFound("Perfil não encontrado");
                }

                var permissionList = selectedPermissions ?? new List<string>();
                await _roleService.UpdateRolePermissionsAsync(id, permissionList, token);

                TempData["SuccessMessage"] = SUCCESS_UPDATE_ROLE_PERMISSIONS;
                return RedirectToAction(nameof(Details), new { id });
            }
            catch (Exception ex)
            {
                TempData["ErrorMessage"] = string.Format(ERROR_UPDATING_ROLE_PERMISSIONS, ex.Message);
                return RedirectToAction(nameof(Details), new { id });
            }
        }
    }
}