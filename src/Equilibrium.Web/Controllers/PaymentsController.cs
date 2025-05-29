using Equilibrium.Resources.Web.Enums;
using Equilibrium.Web.Filters;
using Equilibrium.Web.Helpers;
using Equilibrium.Web.Interfaces;
using Equilibrium.Web.Models.Filters;
using Equilibrium.Web.Models.Payment;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

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
        private readonly IFinancingService _financingService;

        public PaymentsController(
            IPaymentService paymentService,
            IPaymentTypeService paymentTypeService,
            IPaymentMethodService paymentMethodService,
            ICreditCardService creditCardService,
            IFinancingService financingService)
        {
            _paymentService = paymentService ?? throw new ArgumentNullException(nameof(paymentService));
            _paymentTypeService = paymentTypeService ?? throw new ArgumentNullException(nameof(paymentTypeService));
            _paymentMethodService = paymentMethodService ?? throw new ArgumentNullException(nameof(paymentMethodService));
            _creditCardService = creditCardService ?? throw new ArgumentNullException(nameof(creditCardService));
            _financingService = financingService ?? throw new ArgumentNullException(nameof(financingService));
        }

        public async Task<IActionResult> Index(PaymentFilter filter)
        {
            try
            {
                var token = GetToken();

                // Se não há filtros específicos, usa filtro padrão do mês atual
                if (!filter.HasFilters())
                {
                    filter = FilterHelper.QuickFilters.ThisMonth();
                }

                var payments = await _paymentService.GetFilteredPaymentsAsync(filter, token);

                // Carregar dados para os dropdowns de filtro
                await LoadFilterDropdowns(token);

                ViewBag.CurrentFilter = filter;
                ViewBag.HasActiveFilters = filter.HasFilters();

                return View(payments);
            }
            catch (Exception ex)
            {
                return HandleException(ex, EntityNames.Payments, "loading");
            }
        }

        [HttpGet]
        public async Task<IActionResult> QuickFilter(string filterType)
        {
            try
            {
                var token = GetToken();
                PaymentFilter filter = filterType?.ToLower() switch
                {
                    "pending" => FilterHelper.QuickFilters.PendingPayments(),
                    "overdue" => FilterHelper.QuickFilters.OverduePayments(),
                    "thisweek" => FilterHelper.QuickFilters.ThisWeek(),
                    "thismonth" => FilterHelper.QuickFilters.ThisMonth(),
                    _ => new PaymentFilter()
                };

                var payments = await _paymentService.GetFilteredPaymentsAsync(filter, token);
                await LoadFilterDropdowns(token);

                ViewBag.CurrentFilter = filter;
                ViewBag.HasActiveFilters = filter.HasFilters();

                return View("Index", payments);
            }
            catch (Exception ex)
            {
                return HandleException(ex, EntityNames.Payments, "loading");
            }
        }

        [HttpPost]
        public async Task<IActionResult> ApplyFilters(PaymentFilter filter)
        {
            try
            {
                var token = GetToken();
                var payments = await _paymentService.GetFilteredPaymentsAsync(filter, token);

                await LoadFilterDropdowns(token);

                ViewBag.CurrentFilter = filter;
                ViewBag.HasActiveFilters = filter.HasFilters();

                return View("Index", payments);
            }
            catch (Exception ex)
            {
                return HandleException(ex, EntityNames.Payments, "loading");
            }
        }

        [HttpGet]
        public async Task<IActionResult> ClearFilters()
        {
            try
            {
                var filter = FilterHelper.QuickFilters.ThisMonth();
                var token = GetToken();
                var payments = await _paymentService.GetFilteredPaymentsAsync(filter, token);

                await LoadFilterDropdowns(token);

                ViewBag.CurrentFilter = filter;
                ViewBag.HasActiveFilters = false;

                return View("Index", payments);
            }
            catch (Exception ex)
            {
                return HandleException(ex, EntityNames.Payments, "loading");
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
                var payment = await _paymentService.GetPaymentByIdAsync(id, token);

                if (payment == null)
                {
                    return NotFound();
                }

                return View(payment);
            }
            catch (Exception ex)
            {
                return HandleException(ex, EntityNames.Payment, "loading");
            }
        }

        [HttpGet]
        [RequirePermission("payments.create")]
        public async Task<IActionResult> Create()
        {
            try
            {
                var token = GetToken();
                await LoadCreateEditDropdowns(token);

                var model = new CreatePaymentModel
                {
                    DueDate = DateTime.Today,
                    NumberOfInstallments = 1
                };

                return View(model);
            }
            catch (Exception ex)
            {
                return HandleException(ex, EntityNames.Payment, "preparing form");
            }
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        [RequirePermission("payments.create")]
        public async Task<IActionResult> Create(CreatePaymentModel model)
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
                    return HandleException(ex, EntityNames.Payment, "preparing form");
                }
            }

            try
            {
                var token = GetToken();
                await _paymentService.CreatePaymentAsync(model, token);
                return RedirectToIndexWithSuccess("Payments", EntityNames.Payment, "create");
            }
            catch (Exception ex)
            {
                try
                {
                    var token = GetToken();
                    await LoadCreateEditDropdowns(token);
                    SetErrorMessage(EntityNames.Payment, "create", ex);
                    return View(model);
                }
                catch
                {
                    return HandleException(ex, EntityNames.Payment, "create");
                }
            }
        }

        [HttpGet]
        [RequirePermission("payments.edit")]
        public async Task<IActionResult> Edit(string id)
        {
            if (string.IsNullOrEmpty(id))
            {
                return NotFound();
            }

            try
            {
                var token = GetToken();
                var payment = await _paymentService.GetPaymentByIdAsync(id, token);

                if (payment == null)
                {
                    return NotFound();
                }

                await LoadCreateEditDropdowns(token);

                var model = new UpdatePaymentModel
                {
                    Description = payment.Description,
                    Amount = payment.Amount,
                    DueDate = payment.DueDate,
                    PaymentDate = payment.PaymentDate,
                    Status = payment.Status,
                    IsRecurring = payment.IsRecurring,
                    Notes = payment.Notes,
                    PaymentTypeId = payment.PaymentTypeId,
                    PaymentMethodId = payment.PaymentMethodId
                };

                return View(model);
            }
            catch (Exception ex)
            {
                return HandleException(ex, EntityNames.Payment, "loading");
            }
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        [RequirePermission("payments.edit")]
        public async Task<IActionResult> Edit(string id, UpdatePaymentModel model)
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
                    return HandleException(ex, EntityNames.Payment, "preparing form");
                }
            }

            try
            {
                var token = GetToken();
                await _paymentService.UpdatePaymentAsync(id, model, token);
                return RedirectToDetailsWithSuccess("Payments", id, EntityNames.Payment, "update");
            }
            catch (Exception ex)
            {
                try
                {
                    var token = GetToken();
                    await LoadCreateEditDropdowns(token);
                    SetErrorMessage(EntityNames.Payment, "update", ex);
                    return View(model);
                }
                catch
                {
                    return HandleException(ex, EntityNames.Payment, "update");
                }
            }
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        [RequirePermission("payments.delete")]
        public async Task<IActionResult> Delete(string id)
        {
            if (string.IsNullOrEmpty(id))
            {
                return NotFound();
            }

            try
            {
                var token = GetToken();
                await _paymentService.DeletePaymentAsync(id, token);
                return RedirectToIndexWithSuccess("Payments", EntityNames.Payment, "delete");
            }
            catch (Exception ex)
            {
                return HandleException(ex, EntityNames.Payment, "delete");
            }
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        [RequirePermission("payments.edit")]
        public async Task<IActionResult> MarkAsPaid(string id, DateTime? paymentDate = null)
        {
            if (string.IsNullOrEmpty(id))
            {
                return BadRequest("ID do pagamento não fornecido");
            }

            try
            {
                var token = GetToken();
                await _paymentService.MarkAsPaidAsync(id, paymentDate ?? DateTime.Now, token);
                SetStatusChangeSuccessMessage(EntityNames.Payment, EntityStatus.Paid);
                return RedirectToAction("Details", new { id });
            }
            catch (Exception ex)
            {
                SetStatusChangeErrorMessage(EntityNames.Payment, EntityStatus.Paid, ex);
                return RedirectToAction("Details", new { id });
            }
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        [RequirePermission("payments.edit")]
        public async Task<IActionResult> MarkAsOverdue(string id)
        {
            if (string.IsNullOrEmpty(id))
            {
                return BadRequest("ID do pagamento não fornecido");
            }

            try
            {
                var token = GetToken();
                await _paymentService.MarkAsOverdueAsync(id, token);
                SetStatusChangeSuccessMessage(EntityNames.Payment, EntityStatus.Overdue);
                return RedirectToAction("Details", new { id });
            }
            catch (Exception ex)
            {
                SetStatusChangeErrorMessage(EntityNames.Payment, EntityStatus.Overdue, ex);
                return RedirectToAction("Details", new { id });
            }
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        [RequirePermission("payments.edit")]
        public async Task<IActionResult> Cancel(string id)
        {
            if (string.IsNullOrEmpty(id))
            {
                return BadRequest("ID do pagamento não fornecido");
            }

            try
            {
                var token = GetToken();
                await _paymentService.CancelPaymentAsync(id, token);
                SetSuccessMessage(EntityNames.Payment, "cancel");
                return RedirectToAction("Details", new { id });
            }
            catch (Exception ex)
            {
                SetErrorMessage(EntityNames.Payment, "cancel", ex);
                return RedirectToAction("Details", new { id });
            }
        }

        // Métodos auxiliares privados
        private async Task LoadFilterDropdowns(string token)
        {
            try
            {
                var paymentTypes = await _paymentTypeService.GetAllPaymentTypesAsync(token);
                var paymentMethods = await _paymentMethodService.GetAllPaymentMethodsAsync(token);
                var creditCards = await _creditCardService.GetAllCreditCardsAsync(token);

                ViewBag.PaymentTypes = paymentTypes;
                ViewBag.PaymentMethods = paymentMethods;
                ViewBag.CreditCards = creditCards;

                // Opções de status
                ViewBag.StatusOptions = new List<dynamic>
                {
                    new { Value = "Pending", Text = "Pendente" },
                    new { Value = "Paid", Text = "Pago" },
                    new { Value = "Overdue", Text = "Vencido" },
                    new { Value = "Cancelled", Text = "Cancelado" }
                };

                // Opções de ordenação
                ViewBag.OrderByOptions = new List<dynamic>
                {
                    new { Value = "dueDate", Text = "Data de Vencimento" },
                    new { Value = "description", Text = "Descrição" },
                    new { Value = "amount", Text = "Valor" },
                    new { Value = "paymentDate", Text = "Data de Pagamento" },
                    new { Value = "status", Text = "Status" },
                    new { Value = "createdAt", Text = "Data de Criação" }
                };
            }
            catch (Exception ex)
            {
                // Log do erro, mas não falhar o carregamento da página
                SetCustomErrorMessage("Erro ao carregar opções de filtro. Algumas funcionalidades podem estar limitadas.");
            }
        }

        private async Task LoadCreateEditDropdowns(string token)
        {
            var paymentTypes = await _paymentTypeService.GetAllPaymentTypesAsync(token);
            var paymentMethods = await _paymentMethodService.GetAllPaymentMethodsAsync(token);
            var creditCards = await _creditCardService.GetAllCreditCardsAsync(token);
            var financings = await _financingService.GetActiveFinancingsAsync(token);

            ViewBag.PaymentTypes = paymentTypes;
            ViewBag.PaymentMethods = paymentMethods;
            ViewBag.CreditCards = creditCards;
            ViewBag.Financings = financings;
        }

        // Endpoint para AJAX - Busca rápida
        [HttpGet]
        public async Task<IActionResult> SearchAsync(string term)
        {
            try
            {
                var token = GetToken();
                var payments = await _paymentService.SearchPaymentsAsync(term, token);

                return Json(payments.Select(p => new
                {
                    id = p.Id,
                    description = p.Description,
                    amount = p.GetFormattedAmount(),
                    dueDate = p.GetFormattedDueDate(),
                    status = p.StatusDescription
                }));
            }
            catch (Exception ex)
            {
                return Json(new { error = ex.Message });
            }
        }
    }
}