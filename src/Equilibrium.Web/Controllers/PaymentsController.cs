using Equilibrium.Resources.Web.Enums;
using Equilibrium.Web.Filters;
using Equilibrium.Web.Helpers;
using Equilibrium.Web.Interfaces;
using Equilibrium.Web.Models.Filters;
using Equilibrium.Web.Models.Payment;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;

namespace Equilibrium.Web.Controllers
{
    [Authorize]
    [RequirePermission("payments.view")]
    public class PaymentsController : BaseController
    {
        private readonly IPaymentService _paymentService;
        private readonly IPaymentTypeService _paymentTypeService;
        private readonly IPaymentMethodService _paymentMethodService;
        private readonly ICreditCardService _creditCardService;

        public PaymentsController(
            IPaymentService paymentService,
            IPaymentTypeService paymentTypeService,
            IPaymentMethodService paymentMethodService,
            ICreditCardService creditCardService)
        {
            _paymentService = paymentService;
            _paymentTypeService = paymentTypeService;
            _paymentMethodService = paymentMethodService;
            _creditCardService = creditCardService;
        }

        public async Task<IActionResult> Index()
        {
            try
            {
                var token = GetToken();

                var filter = FilterCacheHelper.GetPaymentFilter(HttpContext.Session);

                if (filter == null)
                    return View(new List<PaymentModel>());


                var hasActiveFilters = filter.HasFilters();

                await LoadFilterData(token);

                var payments = await _paymentService.GetFilteredPaymentsAsync(filter, token);

                ViewBag.CurrentFilter = filter;
                ViewBag.HasActiveFilters = hasActiveFilters;

                return View(payments);

            }
            catch (Exception ex)
            {
                return HandleException(ex, EntityNames.Payment, "loading");
            }
        }

        [HttpPost]
        public async Task<IActionResult> ApplyFilters(PaymentFilter filter)
        {
            try
            {
                // Salvar filtro no cache
                FilterCacheHelper.SavePaymentFilter(HttpContext.Session, filter);

                return RedirectToAction("Index");
            }
            catch (Exception ex)
            {
                return HandleException(ex, EntityNames.Payment, "filtering");
            }
        }

        public async Task<IActionResult> QuickFilter(string filterType)
        {
            try
            {
                var filter = filterType.ToLower() switch
                {
                    "thismonth" => FilterHelper.QuickFilters.ThisMonth(),
                    "thisweek" => FilterHelper.QuickFilters.ThisWeek(),
                    "pending" => FilterHelper.QuickFilters.PendingPayments(),
                    "overdue" => FilterHelper.QuickFilters.OverduePayments(),
                    _ => new PaymentFilter()
                };

                // Salvar filtro rápido no cache
                FilterCacheHelper.SavePaymentFilter(HttpContext.Session, filter);

                return RedirectToAction("Index");
            }
            catch (Exception ex)
            {
                return HandleException(ex, EntityNames.Payment, "filtering");
            }
        }

        public IActionResult ClearFilters()
        {
            try
            {
                // Limpar cache de filtros
                FilterCacheHelper.ClearPaymentFilter(HttpContext.Session);

                return RedirectToAction("Index");
            }
            catch (Exception ex)
            {
                return HandleException(ex, EntityNames.Payment, "clearing filters");
            }
        }

        private async Task LoadFilterData(string token)
        {
            try
            {
                // Carregar dados necessários para os filtros
                var paymentTypes = await _paymentTypeService.GetAllPaymentTypesAsync(token);
                var paymentMethods = await _paymentMethodService.GetAllPaymentMethodsAsync(token);
                var creditCards = await _creditCardService.GetAllCreditCardsAsync(token);

                // Status options
                var statusOptions = new List<SelectListItem>
                {
                    new SelectListItem { Value = "Pending", Text = "Pendente" },
                    new SelectListItem { Value = "Paid", Text = "Pago" },
                    new SelectListItem { Value = "Overdue", Text = "Vencido" },
                    new SelectListItem { Value = "Cancelled", Text = "Cancelado" }
                };

                // Order by options
                var orderByOptions = new List<SelectListItem>
                {
                    new SelectListItem { Value = "dueDate", Text = "Data de Vencimento" },
                    new SelectListItem { Value = "description", Text = "Descrição" },
                    new SelectListItem { Value = "amount", Text = "Valor" },
                    new SelectListItem { Value = "paymentDate", Text = "Data de Pagamento" },
                    new SelectListItem { Value = "status", Text = "Status" },
                    new SelectListItem { Value = "createdAt", Text = "Data de Criação" }
                };

                ViewBag.PaymentTypes = paymentTypes;
                ViewBag.PaymentMethods = paymentMethods;
                ViewBag.CreditCards = creditCards;
                ViewBag.StatusOptions = statusOptions;
                ViewBag.OrderByOptions = orderByOptions;
            }
            catch (Exception)
            {
                // Em caso de erro, definir listas vazias
                ViewBag.PaymentTypes = new List<object>();
                ViewBag.PaymentMethods = new List<object>();
                ViewBag.CreditCards = new List<object>();
                ViewBag.StatusOptions = new List<SelectListItem>();
                ViewBag.OrderByOptions = new List<SelectListItem>();
            }
        }
    }
}