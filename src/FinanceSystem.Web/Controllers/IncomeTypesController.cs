using FinanceSystem.Web.Extensions;
using FinanceSystem.Web.Filters;
using FinanceSystem.Web.Models.IncomeType;
using FinanceSystem.Web.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace FinanceSystem.Web.Controllers
{
    [Authorize]
    [RequirePermission("incomes.view")]
    public class IncomeTypesController : Controller
    {
        private readonly IIncomeTypeService _incomeTypeService;
        private readonly ILogger<IncomeTypesController> _logger;

        public IncomeTypesController(
            IIncomeTypeService incomeTypeService,
            ILogger<IncomeTypesController> logger)
        {
            _incomeTypeService = incomeTypeService;
            _logger = logger;
        }

        public async Task<IActionResult> Index()
        {
            try
            {
                var token = HttpContext.GetJwtToken();
                var incomeTypes = await _incomeTypeService.GetAllIncomeTypesAsync(token);
                return View(incomeTypes);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Erro ao carregar tipos de receita");
                TempData["ErrorMessage"] = $"Erro ao carregar tipos de receita: {ex.Message}";
                return RedirectToAction("Index", "Home");
            }
        }

        public async Task<IActionResult> System()
        {
            try
            {
                var token = HttpContext.GetJwtToken();
                var incomeTypes = await _incomeTypeService.GetSystemIncomeTypesAsync(token);
                ViewBag.IsSystemView = true;
                ViewBag.Title = "Tipos de Receita do Sistema";
                return View("Index", incomeTypes);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Erro ao carregar tipos de receita do sistema");
                TempData["ErrorMessage"] = $"Erro ao carregar tipos de receita do sistema: {ex.Message}";
                return RedirectToAction(nameof(Index));
            }
        }

        public async Task<IActionResult> User()
        {
            try
            {
                var token = HttpContext.GetJwtToken();
                var incomeTypes = await _incomeTypeService.GetUserIncomeTypesAsync(token);
                ViewBag.IsUserView = true;
                ViewBag.Title = "Meus Tipos de Receita";
                return View("Index", incomeTypes);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Erro ao carregar tipos de receita do usuário");
                TempData["ErrorMessage"] = $"Erro ao carregar tipos de receita do usuário: {ex.Message}";
                return RedirectToAction(nameof(Index));
            }
        }

        public async Task<IActionResult> Details(string id)
        {
            try
            {
                var token = HttpContext.GetJwtToken();
                var incomeType = await _incomeTypeService.GetIncomeTypeByIdAsync(id, token);
                return View(incomeType);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Erro ao carregar detalhes do tipo de receita");
                TempData["ErrorMessage"] = $"Erro ao carregar detalhes do tipo de receita: {ex.Message}";
                return RedirectToAction(nameof(Index));
            }
        }

        [RequirePermission("incomes.create")]
        public IActionResult Create()
        {
            return View();
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        [RequirePermission("incomes.create")]
        public async Task<IActionResult> Create(CreateIncomeTypeModel model)
        {
            try
            {
                if (ModelState.IsValid)
                {
                    var token = HttpContext.GetJwtToken();
                    var incomeType = await _incomeTypeService.CreateIncomeTypeAsync(model, token);
                    TempData["SuccessMessage"] = "Tipo de receita criado com sucesso!";
                    return RedirectToAction(nameof(Details), new { id = incomeType.Id });
                }
                return View(model);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Erro ao criar tipo de receita");
                ModelState.AddModelError(string.Empty, $"Erro ao criar tipo de receita: {ex.Message}");
                return View(model);
            }
        }

        [RequirePermission("incomes.edit")]
        public async Task<IActionResult> Edit(string id)
        {
            try
            {
                var token = HttpContext.GetJwtToken();
                var incomeType = await _incomeTypeService.GetIncomeTypeByIdAsync(id, token);

                if (incomeType.IsSystem)
                {
                    TempData["ErrorMessage"] = "Não é possível editar tipos de receita do sistema";
                    return RedirectToAction(nameof(Details), new { id });
                }

                var model = new UpdateIncomeTypeModel
                {
                    Name = incomeType.Name,
                    Description = incomeType.Description
                };

                return View(model);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Erro ao carregar tipo de receita para edição");
                TempData["ErrorMessage"] = $"Erro ao carregar tipo de receita para edição: {ex.Message}";
                return RedirectToAction(nameof(Index));
            }
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        [RequirePermission("incomes.edit")]
        public async Task<IActionResult> Edit(string id, UpdateIncomeTypeModel model)
        {
            try
            {
                var token = HttpContext.GetJwtToken();
                var incomeType = await _incomeTypeService.GetIncomeTypeByIdAsync(id, token);

                if (incomeType.IsSystem)
                {
                    TempData["ErrorMessage"] = "Não é possível editar tipos de receita do sistema";
                    return RedirectToAction(nameof(Details), new { id });
                }

                if (ModelState.IsValid)
                {
                    await _incomeTypeService.UpdateIncomeTypeAsync(id, model, token);
                    TempData["SuccessMessage"] = "Tipo de receita atualizado com sucesso!";
                    return RedirectToAction(nameof(Details), new { id });
                }
                return View(model);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Erro ao atualizar tipo de receita");
                ModelState.AddModelError(string.Empty, $"Erro ao atualizar tipo de receita: {ex.Message}");
                return View(model);
            }
        }

        [RequirePermission("incomes.delete")]
        public async Task<IActionResult> Delete(string id)
        {
            try
            {
                var token = HttpContext.GetJwtToken();
                var incomeType = await _incomeTypeService.GetIncomeTypeByIdAsync(id, token);

                if (incomeType.IsSystem)
                {
                    TempData["ErrorMessage"] = "Não é possível excluir tipos de receita do sistema";
                    return RedirectToAction(nameof(Details), new { id });
                }

                return View(incomeType);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Erro ao carregar tipo de receita para exclusão");
                TempData["ErrorMessage"] = $"Erro ao carregar tipo de receita para exclusão: {ex.Message}";
                return RedirectToAction(nameof(Index));
            }
        }

        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        [RequirePermission("incomes.delete")]
        public async Task<IActionResult> DeleteConfirmed(string id)
        {
            try
            {
                var token = HttpContext.GetJwtToken();
                var incomeType = await _incomeTypeService.GetIncomeTypeByIdAsync(id, token);

                if (incomeType.IsSystem)
                {
                    TempData["ErrorMessage"] = "Não é possível excluir tipos de receita do sistema";
                    return RedirectToAction(nameof(Details), new { id });
                }

                await _incomeTypeService.DeleteIncomeTypeAsync(id, token);
                TempData["SuccessMessage"] = "Tipo de receita excluído com sucesso!";
                return RedirectToAction(nameof(Index));
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Erro ao excluir tipo de receita");
                TempData["ErrorMessage"] = $"Erro ao excluir tipo de receita: {ex.Message}";
                return RedirectToAction(nameof(Index));
            }
        }
    }
}