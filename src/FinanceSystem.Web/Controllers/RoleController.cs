using FinanceSystem.Web.Models;
using FinanceSystem.Web.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace FinanceSystem.Web.Controllers
{
    [Authorize(Roles = "Admin")]
    public class RolesController : Controller
    {
        private readonly IRoleService _roleService;

        public RolesController(IRoleService roleService)
        {
            _roleService = roleService;
        }

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

        public async Task<IActionResult> Details(string id)
        {
            try
            {
                var token = HttpContext.Session.GetString("JWToken");
                var role = await _roleService.GetRoleByIdAsync(id, token);
                return View(role);
            }
            catch (Exception ex)
            {
                TempData["ErrorMessage"] = $"Erro ao carregar detalhes do perfil: {ex.Message}";
                return RedirectToAction(nameof(Index));
            }
        }

        public IActionResult Create()
        {
            return View();
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
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
    }
}