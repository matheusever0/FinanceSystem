using Equilibrium.Resources.Web.Enums;
using Equilibrium.Web.Controllers;
using Equilibrium.Web.Filters;
using Equilibrium.Web.Interfaces;
using Equilibrium.Web.Models.Income;
using Equilibrium.Web.Models.Filters;
using Equilibrium.Web.Helpers;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace Equilibrium.Web.Controllers
{
    [Authorize]
    [RequirePermission("incomes.view")]
    public class IncomesController : BaseController
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

        public async Task<IActionResult> Index(IncomeFilter filter)
        {
            try
            {
                var token = GetToken();

                // Se não há filtros específicos, usa filtro padrão do mês atual
                if (!filter.HasFilters())
                {
                    var now = DateTime.Now;
                    filter = new IncomeFilter
                    {
                        Month = now.Month,
                        Year = now.Year,
                        OrderBy = "dueDate",
                        Ascending = true
                    };
                }

                var incomes = await _incomeService.GetFilteredIncomesAsync(filter, token);

                // Carregar dados para os dropdowns de filtro
                await LoadFilterDropdowns(token);

                ViewBag.CurrentFilter = filter;
                ViewBag.HasActiveFilters = filter.HasFilters();

                return View(incomes);
            }
            catch (Exception ex)
            {
                return HandleException(ex, EntityNames.Incomes, "loading");
            }
        }

        [HttpGet]
        public async Task<IActionResult> QuickFilter(string filterType)
        {
            try
            {
                var token = GetToken();
                IncomeFilter filter = filterType?.ToLower() switch
                {
                    "pending" => FilterHelper.QuickFilters.PendingIncomes(),
                    "received" => FilterHelper.QuickFilters.ReceivedThisMonth(),
                    "thisweek" => new IncomeFilter
                    {
                        StartDate = DateTime.Today.AddDays(-(int)DateTime.Today.DayOfWeek),
                        EndDate = DateTime.Today.AddDays(6 - (int)DateTime.Today.DayOfWeek),
                        OrderBy = "dueDate",
                        Ascending = true
                    },
                    "thismonth" => new IncomeFilter
                    {
                        Month = DateTime.Now.Month,
                        Year = DateTime.Now.Year,
                        OrderBy = "dueDate",
                        Ascending = true
                    },
                    _ => new IncomeFilter()
                };

                var incomes = await _incomeService.GetFilteredIncomesAsync(filter, token);
                await LoadFilterDropdowns(token);

                ViewBag.CurrentFilter = filter;
                ViewBag.HasActiveFilters = filter.HasFilters();

                return View("Index", incomes);
            }
            catch (Exception ex)
            {
                return HandleException(ex, EntityNames.Incomes, "loading");
            }
        }

        [HttpPost]
        public async Task<IActionResult> ApplyFilters(IncomeFilter filter)
        {
            try
            {
                var token = GetToken();
                var incomes = await _incomeService.GetFilteredIncomesAsync(filter, token);

                await LoadFilterDropdowns(token);

                ViewBag.CurrentFilter = filter;
                ViewBag.HasActiveFilters = filter.HasFilters();

                return View("Index", incomes);
            }
            catch (Exception ex)
            {
                return HandleException(ex, EntityNames.Incomes, "loading");
            }
        }

        [HttpGet]
        public async Task<IActionResult> ClearFilters()
        {
            try
            {
                var now = DateTime.Now;
                var filter = new IncomeFilter
                {
                    Month = now.Month,
                    Year = now.Year,
                    OrderBy = "dueDate",
                    Ascending = true
                };

                var token = GetToken();
                var incomes = await _incomeService.GetFilteredIncomesAsync(filter, token);

                await LoadFilterDropdowns(token);

                ViewBag.CurrentFilter = filter;
                ViewBag.HasActiveFilters = false;

                return View("Index", incomes);
            }
            catch (Exception ex)
            {
                return HandleException(ex, EntityNames.Incomes, "loading");
            }
        }

        public async Task<IActionResult> Details(string id)
        {
            if (string.IsNullOrEmpty(id))
            {
                return NotFound();
            }

            try
            {
                var token = GetToken();
                var income = await _incomeService.GetIncomeByIdAsync(id, token);

                if (income == null)
                {
                    return NotFound();
                }

                return View(income);
            }
            catch (Exception ex)
            {
                return HandleException(ex, EntityNames.Income, "loading");
            }
        }

        [HttpGet]
        [RequirePermission("incomes.create")]
        public async Task<IActionResult> Create()
        {
            try
            {
                var token = GetToken();
                await LoadCreateEditDropdowns(token);

                var model = new CreateIncomeModel
                {
                    DueDate = DateTime.Today,
                    NumberOfInstallments = 1,
                    Description = "",
                    Notes = "",
                    IncomeTypeId = ""
                };

                return View(model);
            }
            catch (Exception ex)
            {
                return HandleException(ex, EntityNames.Income, "preparing form");
            }
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        [RequirePermission("incomes.create")]
        public async Task<IActionResult> Create(CreateIncomeModel model)
        {
            if (!ModelState.IsValid)
            {
                try
                {
                    var token = GetToken();
                    await LoadCreateEditDropdowns(token);
                    return View(model);
                }
                catch (Exception ex)
                {
                    return HandleException(ex, EntityNames.Income, "preparing form");
                }
            }

            try
            {
                var token = GetToken();
                await _incomeService.CreateIncomeAsync(model, token);
                return RedirectToIndexWithSuccess("Incomes", EntityNames.Income, "create");
            }
            catch (Exception ex)
            {
                try
                {
                    var token = GetToken();
                    await LoadCreateEditDropdowns(token);
                    SetErrorMessage(EntityNames.Income, "create", ex);
                    return View(model);
                }
                catch
                {
                    return HandleException(ex, EntityNames.Income, "create");
                }
            }
        }

        [HttpGet]
        [RequirePermission("incomes.edit")]
        public async Task<IActionResult> Edit(string id)
        {
            if (string.IsNullOrEmpty(id))
            {
                return NotFound();
            }

            try
            {
                var token = GetToken();
                var income = await _incomeService.GetIncomeByIdAsync(id, token);

                if (income == null)
                {
                    return NotFound();
                }

                await LoadCreateEditDropdowns(token);

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
                return HandleException(ex, EntityNames.Income, "loading");
            }
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        [RequirePermission("incomes.edit")]
        public async Task<IActionResult> Edit(string id, UpdateIncomeModel model)
        {
            if (string.IsNullOrEmpty(id))
            {
                return NotFound();
            }

            if (!ModelState.IsValid)
            {
                try
                {
                    var token = GetToken();
                    await LoadCreateEditDropdowns(token);
                    return View(model);
                }
                catch (Exception ex)
                {
                    return HandleException(ex, EntityNames.Income, "preparing form");
                }
            }

            try
            {
                var token = GetToken();
                await _incomeService.UpdateIncomeAsync(id, model, token);
                return RedirectToDetailsWithSuccess("Incomes", id, EntityNames.Income, "update");
            }
            catch (Exception ex)
            {
                try
                {
                    var token = GetToken();
                    await LoadCreateEditDropdowns(token);
                    SetErrorMessage(EntityNames.Income, "update", ex);
                    return View(model);
                }
                catch
                {
                    return HandleException(ex, EntityNames.Income, "update");
                }
            }
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        [RequirePermission("incomes.delete")]
        public async Task<IActionResult> Delete(string id)
        {
            if (string.IsNullOrEmpty(id))
            {
                return NotFound();
            }

            try
            {
                var token = GetToken();
                await _incomeService.DeleteIncomeAsync(id, token);
                return RedirectToIndexWithSuccess("Incomes", EntityNames.Income, "delete");
            }
            catch (Exception ex)
            {
                return HandleException(ex, EntityNames.Income, "delete");
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
                var token = GetToken();
                await _incomeService.MarkAsReceivedAsync(id, receivedDate ?? DateTime.Now, token);
                SetStatusChangeSuccessMessage(EntityNames.Income, EntityStatus.Received);
                return RedirectToAction("Details", new { id });
            }
            catch (Exception ex)
            {
                SetStatusChangeErrorMessage(EntityNames.Income, EntityStatus.Received, ex);
                return RedirectToAction("Details", new { id });
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
                var token = GetToken();
                await _incomeService.CancelIncomeAsync(id, token);
                SetSuccessMessage(EntityNames.Income, "cancel");
                return RedirectToAction("Details", new { id });
            }
            catch (Exception ex)
            {
                SetErrorMessage(EntityNames.Income, "cancel", ex);
                return RedirectToAction("Details", new { id });
            }
        }

        // Endpoints para relatórios e análises
        [HttpGet]
        public async Task<IActionResult> Analysis(int? year)
        {
            try
            {
                year ??= DateTime.Now.Year;
                var token = GetToken();

                var monthlyData = await _incomeService.GetMonthlyIncomeAnalysisAsync(year.Value, token);
                var averageMonthly = await _incomeService.GetAverageMonthlyIncomeAsync(year.Value, token);
                var recentIncomes = await _incomeService.GetRecentIncomesAsync(5, token);

                ViewBag.Year = year;
                ViewBag.MonthlyData = monthlyData;
                ViewBag.AverageMonthly = averageMonthly;
                ViewBag.RecentIncomes = recentIncomes;

                return View();
            }
            catch (Exception ex)
            {
                return HandleException(ex, EntityNames.Incomes, "loading analysis");
            }
        }

        [HttpGet]
        public async Task<IActionResult> Upcoming(int days = 30)
        {
            try
            {
                var token = GetToken();
                var upcomingIncomes = await _incomeService.GetUpcomingIncomesAsync(days, token);

                ViewBag.Days = days;
                return View(upcomingIncomes);
            }
            catch (Exception ex)
            {
                return HandleException(ex, EntityNames.Incomes, "loading upcoming");
            }
        }

        // Métodos auxiliares privados
        private async Task LoadFilterDropdowns(string token)
        {
            try
            {
                var incomeTypes = await _incomeTypeService.GetAllIncomeTypesAsync(token);

                ViewBag.IncomeTypes = incomeTypes;

                // Opções de status
                ViewBag.StatusOptions = new List<dynamic>
                {
                    new { Value = "Pending", Text = "Pendente" },
                    new { Value = "Received", Text = "Recebido" },
                    new { Value = "Cancelled", Text = "Cancelado" }
                };

                // Opções de ordenação
                ViewBag.OrderByOptions = new List<dynamic>
                {
                    new { Value = "dueDate", Text = "Data de Vencimento" },
                    new { Value = "description", Text = "Descrição" },
                    new { Value = "amount", Text = "Valor" },
                    new { Value = "receivedDate", Text = "Data de Recebimento" },
                    new { Value = "status", Text = "Status" },
                    new { Value = "createdAt", Text = "Data de Criação" }
                };
            }
            catch (Exception)
            {
                // Log do erro, mas não falhar o carregamento da página
                SetCustomErrorMessage("Erro ao carregar opções de filtro. Algumas funcionalidades podem estar limitadas.");
            }
        }

        private async Task LoadCreateEditDropdowns(string token)
        {
            var incomeTypes = await _incomeTypeService.GetAllIncomeTypesAsync(token);
            ViewBag.IncomeTypes = incomeTypes;
        }

        // Endpoint para AJAX - Busca rápida
        [HttpGet]
        public async Task<IActionResult> SearchAsync(string term)
        {
            try
            {
                var token = GetToken();
                var incomes = await _incomeService.SearchIncomesAsync(term, token);

                return Json(incomes.Select(i => new
                {
                    id = i.Id,
                    description = i.Description,
                    amount = i.GetFormattedAmount(),
                    dueDate = i.GetFormattedDueDate(),
                    status = i.StatusDescription
                }));
            }
            catch (Exception ex)
            {
                return Json(new { error = ex.Message });
            }
        }

        // Endpoint para comparação de períodos (AJAX)
        [HttpGet]
        public async Task<IActionResult> ComparePeriods(DateTime currentStart, DateTime currentEnd)
        {
            try
            {
                var token = GetToken();
                var comparison = await _incomeService.ComparePeriodsAsync(currentStart, currentEnd, token);

                return Json(new
                {
                    currentPeriod = comparison.currentPeriod,
                    previousPeriod = comparison.previousPeriod,
                    percentageChange = comparison.percentageChange,
                    trend = comparison.percentageChange > 0 ? "up" : comparison.percentageChange < 0 ? "down" : "stable"
                });
            }
            catch (Exception ex)
            {
                return Json(new { error = ex.Message });
            }
        }
    }
}