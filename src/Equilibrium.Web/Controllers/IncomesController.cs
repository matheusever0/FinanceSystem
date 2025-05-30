using Equilibrium.Resources.Web.Enums;
using Equilibrium.Web.Filters;
using Equilibrium.Web.Helpers;
using Equilibrium.Web.Interfaces;
using Equilibrium.Web.Models.Filters;
using Equilibrium.Web.Models.Income;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;

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
            _incomeService = incomeService;
            _incomeTypeService = incomeTypeService;
        }

        public async Task<IActionResult> Index()
        {
            try
            {
                var token = GetToken();

                var filter = FilterCacheHelper.GetIncomeFilter(HttpContext.Session);

                if (filter == null)
                    return View(new List<IncomeModel>());

                var hasActiveFilters = filter.HasFilters();

                await LoadFilterData(token);

                var incomes = await _incomeService.GetFilteredIncomesAsync(filter, token);

                ViewBag.CurrentFilter = filter;
                ViewBag.HasActiveFilters = hasActiveFilters;

                return View(incomes);
            }
            catch (Exception ex)
            {
                return HandleException(ex, EntityNames.Income, "loading");
            }
        }

        [HttpPost]
        public async Task<IActionResult> ApplyFilters(IncomeFilter filter)
        {
            try
            {
                // Salvar filtro no cache
                FilterCacheHelper.SaveIncomeFilter(HttpContext.Session, filter);

                return RedirectToAction("Index");
            }
            catch (Exception ex)
            {
                return HandleException(ex, EntityNames.Income, "filtering");
            }
        }

        public async Task<IActionResult> QuickFilter(string filterType)
        {
            try
            {
                var filter = filterType.ToLower() switch
                {
                    "thismonth" => new IncomeFilter { Month = DateTime.Now.Month, Year = DateTime.Now.Year, OrderBy = "dueDate", Ascending = true },
                    "thisweek" => CreateThisWeekIncomeFilter(),
                    "pending" => FilterHelper.QuickFilters.PendingIncomes(),
                    "received" => FilterHelper.QuickFilters.ReceivedThisMonth(),
                    _ => new IncomeFilter()
                };

                // Salvar filtro rápido no cache
                FilterCacheHelper.SaveIncomeFilter(HttpContext.Session, filter);

                return RedirectToAction("Index");
            }
            catch (Exception ex)
            {
                return HandleException(ex, EntityNames.Income, "filtering");
            }
        }

        public IActionResult ClearFilters()
        {
            try
            {
                // Limpar cache de filtros
                FilterCacheHelper.ClearIncomeFilter(HttpContext.Session);

                return RedirectToAction("Index");
            }
            catch (Exception ex)
            {
                return HandleException(ex, EntityNames.Income, "clearing filters");
            }
        }

        private IncomeFilter CreateThisWeekIncomeFilter()
        {
            var now = DateTime.Now;
            var startOfWeek = now.AddDays(-(int)now.DayOfWeek);
            var endOfWeek = startOfWeek.AddDays(6);

            return new IncomeFilter
            {
                StartDate = startOfWeek.Date,
                EndDate = endOfWeek.Date,
                OrderBy = "dueDate",
                Ascending = true
            };
        }

        private async Task LoadFilterData(string token)
        {
            try
            {
                // Carregar dados necessários para os filtros
                var incomeTypes = await _incomeTypeService.GetAllIncomeTypesAsync(token);

                // Status options para receitas
                var statusOptions = new List<SelectListItem>
            {
                new SelectListItem { Value = "Pending", Text = "Pendente" },
                new SelectListItem { Value = "Received", Text = "Recebido" },
                new SelectListItem { Value = "Cancelled", Text = "Cancelado" }
            };

                // Order by options para receitas
                var orderByOptions = new List<SelectListItem>
            {
                new SelectListItem { Value = "dueDate", Text = "Data de Vencimento" },
                new SelectListItem { Value = "description", Text = "Descrição" },
                new SelectListItem { Value = "amount", Text = "Valor" },
                new SelectListItem { Value = "receivedDate", Text = "Data de Recebimento" },
                new SelectListItem { Value = "status", Text = "Status" },
                new SelectListItem { Value = "createdAt", Text = "Data de Criação" }
            };

                ViewBag.IncomeTypes = incomeTypes;
                ViewBag.StatusOptions = statusOptions;
                ViewBag.OrderByOptions = orderByOptions;
            }
            catch (Exception)
            {
                // Em caso de erro, definir listas vazias
                ViewBag.IncomeTypes = new List<object>();
                ViewBag.StatusOptions = new List<SelectListItem>();
                ViewBag.OrderByOptions = new List<SelectListItem>();
            }
        }
    }
}