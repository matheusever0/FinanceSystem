using FinanceSystem.Web.Extensions;
using FinanceSystem.Web.Filters;
using FinanceSystem.Web.Models.Income;
using FinanceSystem.Web.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace FinanceSystem.Web.Controllers
{
    [Authorize]
    [RequirePermission("incomes.view")]
    public class IncomesController : Controller
    {
        private readonly IIncomeService _incomeService;
        private readonly IIncomeTypeService _incomeTypeService;
        private readonly ILogger<IncomesController> _logger;

        public IncomesController(
            IIncomeService incomeService,
            IIncomeTypeService incomeTypeService,
            ILogger<IncomesController> logger)
        {
            _incomeService = incomeService;
            _incomeTypeService = incomeTypeService;
            _logger = logger;
        }

        public async Task<IActionResult> Index()
        {
            try
            {
                var token = HttpContext.GetJwtToken();
                var incomes = await _incomeService.GetAllIncomesAsync(token);
                return View(incomes);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Erro ao carregar receitas");
                TempData["ErrorMessage"] = $"Erro ao carregar receitas: {ex.Message}";
                return RedirectToAction("Index", "Home");
            }
        }

        public async Task<IActionResult> Pending()
        {
            try
            {
                var token = HttpContext.GetJwtToken();
                var incomes = await _incomeService.GetPendingIncomesAsync(token);
                ViewBag.Title = "Receitas Pendentes";
                return View("Index", incomes);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Erro ao carregar receitas pendentes");
                TempData["ErrorMessage"] = $"Erro ao carregar receitas pendentes: {ex.Message}";
                return RedirectToAction("Index", "Home");
            }
        }

        public async Task<IActionResult> Received()
        {
            try
            {
                var token = HttpContext.GetJwtToken();
                var incomes = await _incomeService.GetReceivedIncomesAsync(token);
                ViewBag.Title = "Receitas Recebidas";
                return View("Index", incomes);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Erro ao carregar receitas recebidas");
                TempData["ErrorMessage"] = $"Erro ao carregar receitas recebidas: {ex.Message}";
                return RedirectToAction("Index", "Home");
            }
        }

        public async Task<IActionResult> ByMonth(int month, int year)
        {
            if (month <= 0 || month > 12)
            {
                var currentDate = DateTime.Now;
                month = currentDate.Month;
                year = currentDate.Year;
            }

            if (year <= 0)
            {
                year = DateTime.Now.Year;
            }

            try
            {
                var token = HttpContext.GetJwtToken();
                var incomes = await _incomeService.GetIncomesByMonthAsync(month, year, token);

                ViewBag.Month = month;
                ViewBag.Year = year;
                ViewBag.Title = $"Receitas de {new DateTime(year, month, 1).ToString("MMMM/yyyy")}";

                return View("Index", incomes);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Erro ao carregar receitas por mês");
                TempData["ErrorMessage"] = $"Erro ao carregar receitas por mês: {ex.Message}";
                return RedirectToAction("Index", "Home");
            }
        }

        public async Task<IActionResult> ByType(string id)
        {
            try
            {
                var token = HttpContext.GetJwtToken();
                var incomes = await _incomeService.GetIncomesByTypeAsync(id, token);
                var incomeType = await _incomeTypeService.GetIncomeTypeByIdAsync(id, token);

                ViewBag.Title = $"Receitas por Tipo: {incomeType.Name}";
                ViewBag.TypeId = id;

                return View("Index", incomes);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Erro ao carregar receitas por tipo");
                TempData["ErrorMessage"] = $"Erro ao carregar receitas por tipo: {ex.Message}";
                return RedirectToAction("Index", "Home");
            }
        }

        public async Task<IActionResult> Details(string id)
        {
            try
            {
                var token = HttpContext.GetJwtToken();
                var income = await _incomeService.GetIncomeByIdAsync(id, token);
                return View(income);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Erro ao carregar detalhes da receita");
                TempData["ErrorMessage"] = $"Erro ao carregar detalhes da receita: {ex.Message}";
                return RedirectToAction(nameof(Index));
            }
        }

        [RequirePermission("incomes.create")]
        public async Task<IActionResult> Create()
        {
            try
            {
                var token = HttpContext.GetJwtToken();
                var incomeTypes = await _incomeTypeService.GetAllIncomeTypesAsync(token);

                ViewBag.IncomeTypes = incomeTypes;

                return View();
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Erro ao preparar formulário de criação de receita");
                TempData["ErrorMessage"] = $"Erro ao preparar formulário: {ex.Message}";
                return RedirectToAction(nameof(Index));
            }
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        [RequirePermission("incomes.create")]
        public async Task<IActionResult> Create(CreateIncomeModel model)
        {
            var token = HttpContext.GetJwtToken();

            try
            {
                if (ModelState.IsValid)
                {
                    _logger.LogInformation("Usuário {UserName} criando nova receita", User.Identity.Name);
                    var income = await _incomeService.CreateIncomeAsync(model, token);
                    TempData["SuccessMessage"] = "Receita criada com sucesso!";
                    return RedirectToAction(nameof(Details), new { id = income.Id });
                }
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Erro ao criar receita");
                ModelState.AddModelError(string.Empty, $"Erro ao criar receita: {ex.Message}");
            }

            try
            {
                var incomeTypes = await _incomeTypeService.GetAllIncomeTypesAsync(token);
                ViewBag.IncomeTypes = incomeTypes;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Erro ao recarregar dados de referência para criação de receita");
            }

            return View(model);
        }

        [RequirePermission("incomes.edit")]
        public async Task<IActionResult> Edit(string id)
        {
            try
            {
                var token = HttpContext.GetJwtToken();
                var income = await _incomeService.GetIncomeByIdAsync(id, token);
                var incomeTypes = await _incomeTypeService.GetAllIncomeTypesAsync(token);

                ViewBag.IncomeTypes = incomeTypes;

                var model = new UpdateIncomeModel
                {
                    Description = income.Description,
                    Amount = income.Amount,
                    DueDate = income.DueDate,
                    IsRecurring = income.IsRecurring,
                    Notes = income.Notes,
                    IncomeTypeId = income.IncomeTypeId
                };

                return View(model);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Erro ao carregar receita para edição");
                TempData["ErrorMessage"] = $"Erro ao carregar receita para edição: {ex.Message}";
                return RedirectToAction(nameof(Index));
            }
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        [RequirePermission("incomes.edit")]
        public async Task<IActionResult> Edit(string id, UpdateIncomeModel model)
        {
            var token = HttpContext.GetJwtToken();

            try
            {
                if (ModelState.IsValid)
                {
                    await _incomeService.UpdateIncomeAsync(id, model, token);
                    TempData["SuccessMessage"] = "Receita atualizada com sucesso!";
                    return RedirectToAction(nameof(Details), new { id });
                }
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Erro ao atualizar receita");
                ModelState.AddModelError(string.Empty, $"Erro ao atualizar receita: {ex.Message}");
            }

            try
            {
                var incomeTypes = await _incomeTypeService.GetAllIncomeTypesAsync(token);
                ViewBag.IncomeTypes = incomeTypes;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Erro ao recarregar dados de referência para edição de receita");
            }

            return View(model);
        }

        [RequirePermission("incomes.delete")]
        public async Task<IActionResult> Delete(string id)
        {
            try
            {
                var token = HttpContext.GetJwtToken();
                var income = await _incomeService.GetIncomeByIdAsync(id, token);
                return View(income);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Erro ao carregar receita para exclusão");
                TempData["ErrorMessage"] = $"Erro ao carregar receita para exclusão: {ex.Message}";
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
                await _incomeService.DeleteIncomeAsync(id, token);
                TempData["SuccessMessage"] = "Receita excluída com sucesso!";
                return RedirectToAction(nameof(Index));
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Erro ao excluir receita");
                TempData["ErrorMessage"] = $"Erro ao excluir receita: {ex.Message}";
                return RedirectToAction(nameof(Index));
            }
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        [RequirePermission("incomes.edit")]
        public async Task<IActionResult> MarkAsReceived(string id, DateTime? receivedDate = null)
        {
            try
            {
                var token = HttpContext.GetJwtToken();
                await _incomeService.MarkAsReceivedAsync(id, receivedDate ?? DateTime.Now, token);
                TempData["SuccessMessage"] = "Receita marcada como recebida com sucesso!";
                return RedirectToAction(nameof(Details), new { id });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Erro ao marcar receita como recebida");
                TempData["ErrorMessage"] = $"Erro ao marcar receita como recebida: {ex.Message}";
                return RedirectToAction(nameof(Details), new { id });
            }
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        [RequirePermission("incomes.edit")]
        public async Task<IActionResult> Cancel(string id)
        {
            try
            {
                var token = HttpContext.GetJwtToken();
                await _incomeService.CancelIncomeAsync(id, token);
                TempData["SuccessMessage"] = "Receita cancelada com sucesso!";
                return RedirectToAction(nameof(Details), new { id });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Erro ao cancelar receita");
                TempData["ErrorMessage"] = $"Erro ao cancelar receita: {ex.Message}";
                return RedirectToAction(nameof(Details), new { id });
            }
        }
    }
}