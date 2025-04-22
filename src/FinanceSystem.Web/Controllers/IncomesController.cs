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

        private const string ERROR_LOADING_INCOMES = "Erro ao carregar receitas: {0}";
        private const string ERROR_LOADING_PENDING_INCOMES = "Erro ao carregar receitas pendentes: {0}";
        private const string ERROR_LOADING_RECEIVED_INCOMES = "Erro ao carregar receitas recebidas: {0}";
        private const string ERROR_LOADING_MONTHLY_INCOMES = "Erro ao carregar receitas por mês: {0}";
        private const string ERROR_LOADING_INCOMES_BY_TYPE = "Erro ao carregar receitas por tipo: {0}";
        private const string ERROR_LOADING_INCOME_DETAILS = "Erro ao carregar detalhes da receita: {0}";
        private const string ERROR_PREPARING_FORM = "Erro ao preparar formulário: {0}";
        private const string ERROR_CREATING_INCOME = "Erro ao criar receita: {0}";
        private const string ERROR_LOADING_INCOME_EDIT = "Erro ao carregar receita para edição: {0}";
        private const string ERROR_UPDATING_INCOME = "Erro ao atualizar receita: {0}";
        private const string ERROR_LOADING_INCOME_DELETE = "Erro ao carregar receita para exclusão: {0}";
        private const string ERROR_DELETING_INCOME = "Erro ao excluir receita: {0}";
        private const string ERROR_MARK_RECEIVED = "Erro ao marcar receita como recebida: {0}";
        private const string ERROR_CANCEL_INCOME = "Erro ao cancelar receita: {0}";

        private const string SUCCESS_CREATE_INCOME = "Receita criada com sucesso!";
        private const string SUCCESS_UPDATE_INCOME = "Receita atualizada com sucesso!";
        private const string SUCCESS_DELETE_INCOME = "Receita excluída com sucesso!";
        private const string SUCCESS_MARK_RECEIVED = "Receita marcada como recebida com sucesso!";
        private const string SUCCESS_CANCEL_INCOME = "Receita cancelada com sucesso!";

        public IncomesController(
            IIncomeService incomeService,
            IIncomeTypeService incomeTypeService)
        {
            _incomeService = incomeService ?? throw new ArgumentNullException(nameof(incomeService));
            _incomeTypeService = incomeTypeService ?? throw new ArgumentNullException(nameof(incomeTypeService));
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
                TempData["ErrorMessage"] = string.Format(ERROR_LOADING_INCOMES, ex.Message);
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
                TempData["ErrorMessage"] = string.Format(ERROR_LOADING_PENDING_INCOMES, ex.Message);
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
                TempData["ErrorMessage"] = string.Format(ERROR_LOADING_RECEIVED_INCOMES, ex.Message);
                return RedirectToAction("Index", "Home");
            }
        }

        public async Task<IActionResult> ByMonth(int month, int year)
        {
            var currentDate = DateTime.Now;

            if (month <= 0 || month > 12)
            {
                month = currentDate.Month;
                year = currentDate.Year;
            }

            if (year <= 0)
            {
                year = currentDate.Year;
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
                TempData["ErrorMessage"] = string.Format(ERROR_LOADING_MONTHLY_INCOMES, ex.Message);
                return RedirectToAction("Index", "Home");
            }
        }

        public async Task<IActionResult> ByType(string id)
        {
            if (string.IsNullOrEmpty(id))
            {
                return BadRequest("ID do tipo de receita não fornecido");
            }

            try
            {
                var token = HttpContext.GetJwtToken();
                var incomes = await _incomeService.GetIncomesByTypeAsync(id, token);
                var incomeType = await _incomeTypeService.GetIncomeTypeByIdAsync(id, token);

                if (incomeType == null)
                {
                    return NotFound("Tipo de receita não encontrado");
                }

                ViewBag.Title = $"Receitas por Tipo: {incomeType.Name}";
                ViewBag.TypeId = id;

                return View("Index", incomes);
            }
            catch (Exception ex)
            {
                TempData["ErrorMessage"] = string.Format(ERROR_LOADING_INCOMES_BY_TYPE, ex.Message);
                return RedirectToAction("Index", "Home");
            }
        }

        public async Task<IActionResult> Details(string id)
        {
            if (string.IsNullOrEmpty(id))
            {
                return BadRequest("ID da receita não fornecido");
            }

            try
            {
                var token = HttpContext.GetJwtToken();
                var income = await _incomeService.GetIncomeByIdAsync(id, token);

                if (income == null)
                {
                    return NotFound("Receita não encontrada");
                }

                return View(income);
            }
            catch (Exception ex)
            {
                TempData["ErrorMessage"] = string.Format(ERROR_LOADING_INCOME_DETAILS, ex.Message);
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
                TempData["ErrorMessage"] = string.Format(ERROR_PREPARING_FORM, ex.Message);
                return RedirectToAction(nameof(Index));
            }
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        [RequirePermission("incomes.create")]
        public async Task<IActionResult> Create(CreateIncomeModel model)
        {
            if (!ModelState.IsValid)
            {
                await LoadIncomeTypesForView();
                return View(model);
            }

            try
            {
                var token = HttpContext.GetJwtToken();
                var income = await _incomeService.CreateIncomeAsync(model, token);
                TempData["SuccessMessage"] = SUCCESS_CREATE_INCOME;
                return RedirectToAction(nameof(Details), new { id = income.Id });
            }
            catch (Exception ex)
            {
                ModelState.AddModelError(string.Empty, string.Format(ERROR_CREATING_INCOME, ex.Message));
                await LoadIncomeTypesForView();
                return View(model);
            }
        }

        [RequirePermission("incomes.edit")]
        public async Task<IActionResult> Edit(string id)
        {
            if (string.IsNullOrEmpty(id))
            {
                return BadRequest("ID da receita não fornecido");
            }

            try
            {
                var token = HttpContext.GetJwtToken();
                var income = await _incomeService.GetIncomeByIdAsync(id, token);

                if (income == null)
                {
                    return NotFound("Receita não encontrada");
                }

                var incomeTypes = await _incomeTypeService.GetAllIncomeTypesAsync(token);
                ViewBag.IncomeTypes = incomeTypes;

                var model = new UpdateIncomeModel
                {
                    Id = income.Id,
                    Description = income.Description,
                    Amount = income.Amount,
                    DueDate = income.DueDate,
                    ReceivedDate = income.ReceivedDate,
                    Status = income.Status,
                    IsRecurring = income.IsRecurring,
                    Notes = income.Notes,
                    IncomeTypeId = income.IncomeTypeId
                };

                return View(model);
            }
            catch (Exception ex)
            {
                TempData["ErrorMessage"] = string.Format(ERROR_LOADING_INCOME_EDIT, ex.Message);
                return RedirectToAction(nameof(Index));
            }
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        [RequirePermission("incomes.edit")]
        public async Task<IActionResult> Edit(string id, UpdateIncomeModel model)
        {
            if (string.IsNullOrEmpty(id))
            {
                return BadRequest("ID da receita não fornecido");
            }

            if (!ModelState.IsValid)
            {
                await LoadIncomeTypesForView();
                return View(model);
            }

            try
            {
                var token = HttpContext.GetJwtToken();
                await _incomeService.UpdateIncomeAsync(id, model, token);
                TempData["SuccessMessage"] = SUCCESS_UPDATE_INCOME;
                return RedirectToAction(nameof(Details), new { id });
            }
            catch (Exception ex)
            {
                ModelState.AddModelError(string.Empty, string.Format(ERROR_UPDATING_INCOME, ex.Message));
                await LoadIncomeTypesForView();
                return View(model);
            }
        }

        [RequirePermission("incomes.delete")]
        public async Task<IActionResult> Delete(string id)
        {
            if (string.IsNullOrEmpty(id))
            {
                return BadRequest("ID da receita não fornecido");
            }

            try
            {
                var token = HttpContext.GetJwtToken();
                var income = await _incomeService.GetIncomeByIdAsync(id, token);

                if (income == null)
                {
                    return NotFound("Receita não encontrada");
                }

                return View(income);
            }
            catch (Exception ex)
            {
                TempData["ErrorMessage"] = string.Format(ERROR_LOADING_INCOME_DELETE, ex.Message);
                return RedirectToAction(nameof(Index));
            }
        }

        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        [RequirePermission("incomes.delete")]
        public async Task<IActionResult> DeleteConfirmed(string id)
        {
            if (string.IsNullOrEmpty(id))
            {
                return BadRequest("ID da receita não fornecido");
            }

            try
            {
                var token = HttpContext.GetJwtToken();
                await _incomeService.DeleteIncomeAsync(id, token);
                TempData["SuccessMessage"] = SUCCESS_DELETE_INCOME;
                return RedirectToAction(nameof(Index));
            }
            catch (Exception ex)
            {
                TempData["ErrorMessage"] = string.Format(ERROR_DELETING_INCOME, ex.Message);
                return RedirectToAction(nameof(Index));
            }
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        [RequirePermission("incomes.edit")]
        public async Task<IActionResult> MarkAsReceived(string id, DateTime? receivedDate = null)
        {
            if (string.IsNullOrEmpty(id))
            {
                return BadRequest("ID da receita não fornecido");
            }

            try
            {
                var token = HttpContext.GetJwtToken();
                await _incomeService.MarkAsReceivedAsync(id, receivedDate ?? DateTime.Now, token);
                TempData["SuccessMessage"] = SUCCESS_MARK_RECEIVED;
                return RedirectToAction(nameof(Details), new { id });
            }
            catch (Exception ex)
            {
                TempData["ErrorMessage"] = string.Format(ERROR_MARK_RECEIVED, ex.Message);
                return RedirectToAction(nameof(Details), new { id });
            }
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        [RequirePermission("incomes.edit")]
        public async Task<IActionResult> Cancel(string id)
        {
            if (string.IsNullOrEmpty(id))
            {
                return BadRequest("ID da receita não fornecido");
            }

            try
            {
                var token = HttpContext.GetJwtToken();
                await _incomeService.CancelIncomeAsync(id, token);
                TempData["SuccessMessage"] = SUCCESS_CANCEL_INCOME;
                return RedirectToAction(nameof(Details), new { id });
            }
            catch (Exception ex)
            {
                TempData["ErrorMessage"] = string.Format(ERROR_CANCEL_INCOME, ex.Message);
                return RedirectToAction(nameof(Details), new { id });
            }
        }

        private async Task LoadIncomeTypesForView()
        {
            try
            {
                var token = HttpContext.GetJwtToken();
                var incomeTypes = await _incomeTypeService.GetAllIncomeTypesAsync(token);
                ViewBag.IncomeTypes = incomeTypes;
            }
            catch
            {
                ViewBag.IncomeTypes = new List<object>();
            }
        }
    }
}