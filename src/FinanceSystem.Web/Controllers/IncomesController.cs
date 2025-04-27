using FinanceSystem.Resources.Web;
using FinanceSystem.Resources.Web.Enums;
using FinanceSystem.Resources.Web.Helpers;
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
                TempData["ErrorMessage"] = MessageHelper.GetLoadingErrorMessage(EntityNames.Incomes, ex);
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
                TempData["ErrorMessage"] = MessageHelper.GetLoadingErrorMessage(EntityNames.Incomes, ex);
                return RedirectToAction("Index", "Home");
            }
        }

        public async Task<IActionResult> Overdue()
        {
            try
            {
                var token = HttpContext.GetJwtToken();
                var incomes = await _incomeService.GetOverdueIncomesAsync(token);
                ViewBag.Title = "Receitas Vencidas";
                return View("Index", incomes);
            }
            catch (Exception ex)
            {
                TempData["ErrorMessage"] = MessageHelper.GetLoadingErrorMessage(EntityNames.Incomes, ex);
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
                TempData["ErrorMessage"] = MessageHelper.GetLoadingErrorMessage(EntityNames.Incomes, ex);
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
                TempData["ErrorMessage"] = MessageHelper.GetLoadingErrorMessage(EntityNames.Incomes, ex);
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
                TempData["ErrorMessage"] = MessageHelper.GetLoadingErrorMessage(EntityNames.Incomes, ex);
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

                return income == null ? NotFound("Receita não encontrada") : View(income);
            }
            catch (Exception ex)
            {
                TempData["ErrorMessage"] = MessageHelper.GetLoadingErrorMessage(EntityNames.Income, ex);
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
                TempData["ErrorMessage"] = ResourceFinanceWeb.Error_PreparingForm;
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
                TempData["SuccessMessage"] = MessageHelper.GetCreationSuccessMessage(EntityNames.Income);
                return RedirectToAction(nameof(Details), new { id = income.Id });
            }
            catch (Exception ex)
            {
                ModelState.AddModelError(string.Empty, MessageHelper.GetCreationErrorMessage(EntityNames.Income, ex));
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
                TempData["ErrorMessage"] = MessageHelper.GetLoadingErrorMessage(EntityNames.Income, ex);
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
                TempData["SuccessMessage"] = MessageHelper.GetUpdateSuccessMessage(EntityNames.Income);
                return RedirectToAction(nameof(Details), new { id });
            }
            catch (Exception ex)
            {
                ModelState.AddModelError(string.Empty, MessageHelper.GetUpdateErrorMessage(EntityNames.Income, ex));
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

                return income == null ? NotFound("Receita não encontrada") : View(income);
            }
            catch (Exception ex)
            {
                TempData["ErrorMessage"] = MessageHelper.GetLoadingErrorMessage(EntityNames.Income, ex);
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
                TempData["SuccessMessage"] = MessageHelper.GetDeletionSuccessMessage(EntityNames.Income);
                return RedirectToAction(nameof(Index));
            }
            catch (Exception ex)
            {
                TempData["ErrorMessage"] = MessageHelper.GetDeletionErrorMessage(EntityNames.Income, ex);
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
                TempData["SuccessMessage"] = MessageHelper.GetStatusChangeSuccessMessage(EntityNames.Income, EntityStatus.Received);
                return RedirectToAction(nameof(Details), new { id });
            }
            catch (Exception ex)
            {
                TempData["ErrorMessage"] = MessageHelper.GetStatusChangeErrorMessage(EntityNames.Income, EntityStatus.Received, ex);
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
                TempData["SuccessMessage"] = MessageHelper.GetCancelSuccessMessage(EntityNames.Income);
                return RedirectToAction(nameof(Details), new { id });
            }
            catch (Exception ex)
            {
                TempData["ErrorMessage"] = MessageHelper.GetCancelErrorMessage(EntityNames.Income, ex);
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