using FinanceSystem.Resources.Web;
using FinanceSystem.Resources.Web.Enums;
using FinanceSystem.Resources.Web.Helpers;
using FinanceSystem.Web.Extensions;
using FinanceSystem.Web.Filters;
using FinanceSystem.Web.Interfaces;
using FinanceSystem.Web.Models.Payment;
using FinanceSystem.Web.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace FinanceSystem.Web.Controllers
{
    [Authorize]
    [RequirePermission("payments.view")]
    public class PaymentsController : Controller
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

        public async Task<IActionResult> Index()
        {
            try
            {
                var token = HttpContext.GetJwtToken();
                var payments = await _paymentService.GetAllPaymentsAsync(token);
                return View(payments);
            }
            catch (Exception ex)
            {
                TempData["ErrorMessage"] = MessageHelper.GetLoadingErrorMessage(EntityNames.Payments, ex);
                return RedirectToAction("Index", "Home");
            }
        }

        public async Task<IActionResult> Pending()
        {
            try
            {
                var token = HttpContext.GetJwtToken();
                var pendingPayments = await _paymentService.GetPendingPaymentsAsync(token);
                ViewBag.Title = "Pagamentos Pendentes";
                return View("Index", pendingPayments);
            }
            catch (Exception ex)
            {
                TempData["ErrorMessage"] = MessageHelper.GetLoadingErrorMessage(EntityNames.Payments, ex);
                return RedirectToAction("Index", "Home");
            }
        }

        public async Task<IActionResult> Overdue()
        {
            try
            {
                var token = HttpContext.GetJwtToken();
                var overduePayments = await _paymentService.GetOverduePaymentsAsync(token);
                ViewBag.Title = "Pagamentos Vencidos";
                return View("Index", overduePayments);
            }
            catch (Exception ex)
            {
                TempData["ErrorMessage"] = MessageHelper.GetLoadingErrorMessage(EntityNames.Payments, ex);
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
                var monthlyPayments = await _paymentService.GetPaymentsByMonthAsync(month, year, token);

                ViewBag.Month = month;
                ViewBag.Year = year;
                ViewBag.Title = $"Pagamentos de {new DateTime(year, month, 1).ToString("MMMM/yyyy")}";

                return View("Index", monthlyPayments);
            }
            catch (Exception ex)
            {
                TempData["ErrorMessage"] = MessageHelper.GetLoadingErrorMessage(EntityNames.Payments, ex);
                return RedirectToAction("Index", "Home");
            }
        }

        public async Task<IActionResult> ByType(string id)
        {
            if (string.IsNullOrEmpty(id))
            {
                return BadRequest("ID do tipo de pagamento não fornecido");
            }

            try
            {
                var token = HttpContext.GetJwtToken();
                var payments = await _paymentService.GetPaymentsByTypeAsync(id, token);
                var paymentType = await _paymentTypeService.GetPaymentTypeByIdAsync(id, token);

                if (paymentType == null)
                {
                    return NotFound("Tipo de pagamento não encontrado");
                }

                ViewBag.Title = $"Pagamentos do Tipo: {paymentType.Name}";
                ViewBag.TypeId = id;

                return View("Index", payments);
            }
            catch (Exception ex)
            {
                TempData["ErrorMessage"] = MessageHelper.GetLoadingErrorMessage(EntityNames.Payments, ex);
                return RedirectToAction("Index", "Home");
            }
        }

        public async Task<IActionResult> ByMethod(string id)
        {
            if (string.IsNullOrEmpty(id))
            {
                return BadRequest("ID do método de pagamento não fornecido");
            }

            try
            {
                var token = HttpContext.GetJwtToken();
                var payments = await _paymentService.GetPaymentsByMethodAsync(id, token);
                var paymentMethod = await _paymentMethodService.GetPaymentMethodByIdAsync(id, token);

                if (paymentMethod == null)
                {
                    return NotFound("Método de pagamento não encontrado");
                }

                ViewBag.Title = $"Pagamentos por Método: {paymentMethod.Name}";
                ViewBag.MethodId = id;

                return View("Index", payments);
            }
            catch (Exception ex)
            {
                TempData["ErrorMessage"] = MessageHelper.GetLoadingErrorMessage(EntityNames.Payments, ex);
                return RedirectToAction("Index", "Home");
            }
        }

        public async Task<IActionResult> Details(string id)
        {
            if (string.IsNullOrEmpty(id))
            {
                return BadRequest("ID do pagamento não fornecido");
            }

            try
            {
                var token = HttpContext.GetJwtToken();
                var payment = await _paymentService.GetPaymentByIdAsync(id, token);

                return payment == null ? NotFound("Pagamento não encontrado") : View(payment);
            }
            catch (Exception ex)
            {
                TempData["ErrorMessage"] = MessageHelper.GetLoadingErrorMessage(EntityNames.Payment, ex);
                return RedirectToAction(nameof(Index));
            }
        }

        [RequirePermission("payments.create")]
        public async Task<IActionResult> Create()
        {
            try
            {
                var token = HttpContext.GetJwtToken();
                var paymentTypes = await _paymentTypeService.GetAllPaymentTypesAsync(token);
                var paymentMethods = await _paymentMethodService.GetAllPaymentMethodsAsync(token);
                var creditCards = await _creditCardService.GetAllCreditCardsAsync(token);
                var financings = await _financingService.GetActiveFinancingsAsync(token);

                ViewBag.PaymentTypes = paymentTypes;
                ViewBag.PaymentMethods = paymentMethods;
                ViewBag.CreditCards = creditCards;
                ViewBag.Financings = financings;

                return View();
            }
            catch (Exception ex)
            {
                TempData["ErrorMessage"] = "Erro ao preparar o formulário.";
                return RedirectToAction(nameof(Index));
            }
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        [RequirePermission("payments.create")]
        public async Task<IActionResult> Create(CreatePaymentModel model)
        {
            if (!ModelState.IsValid)
            {
                await LoadFormDependencies();
                return View(model);
            }

            try
            {
                var token = HttpContext.GetJwtToken();

                // Verificar se o tipo de pagamento é de financiamento
                var paymentType = await _paymentTypeService.GetPaymentTypeByIdAsync(model.PaymentTypeId, token);
                if (paymentType != null && paymentType.IsFinancingType && string.IsNullOrEmpty(model.FinancingId))
                {
                    ModelState.AddModelError("FinancingId", "Um financiamento deve ser selecionado para este tipo de pagamento.");
                    await LoadFormDependencies();
                    return View(model);
                }

                // Verificar se o método de pagamento é cartão de crédito e se foi selecionado um cartão
                var paymentMethod = await _paymentMethodService.GetPaymentMethodByIdAsync(model.PaymentMethodId, token);
                if (paymentMethod != null && paymentMethod.Type == 2 && string.IsNullOrEmpty(model.CreditCardId))
                {
                    ModelState.AddModelError("CreditCardId", ResourceFinanceWeb.Error_CreditCardRequired);
                    await LoadFormDependencies();
                    return View(model);
                }

                var payment = await _paymentService.CreatePaymentAsync(model, token);
                TempData["SuccessMessage"] = MessageHelper.GetCreationSuccessMessage(EntityNames.Payment);
                return RedirectToAction(nameof(Details), new { id = payment.Id });
            }
            catch (Exception ex)
            {
                ModelState.AddModelError(string.Empty, MessageHelper.GetCreationErrorMessage(EntityNames.Payment, ex));
                await LoadFormDependencies();
                return View(model);
            }
        }

        [RequirePermission("payments.create")]
        public async Task<IActionResult> CreateWithFinancing()
        {
            try
            {
                var token = HttpContext.GetJwtToken();
                var paymentTypes = await _paymentTypeService.GetAllPaymentTypesAsync(token);

                // Filtrar tipos de pagamento que são marcados como tipo de financiamento
                var financingPaymentTypes = paymentTypes.Where(pt => pt.IsFinancingType).ToList();

                if (!financingPaymentTypes.Any())
                {
                    TempData["WarningMessage"] = "Não há tipos de pagamento configurados para financiamento.";
                    return RedirectToAction(nameof(Create));
                }

                var paymentMethods = await _paymentMethodService.GetAllPaymentMethodsAsync(token);
                var financings = await _financingService.GetActiveFinancingsAsync(token);

                ViewBag.PaymentTypes = financingPaymentTypes;
                ViewBag.PaymentMethods = paymentMethods;
                ViewBag.Financings = financings;
                ViewBag.IsFinancingPayment = true;

                return View("Create");
            }
            catch (Exception ex)
            {
                TempData["ErrorMessage"] = MessageHelper.GetLoadingErrorMessage(EntityNames.Payment, ex);
                return RedirectToAction(nameof(Index));
            }
        }

        [RequirePermission("payments.edit")]
        public async Task<IActionResult> Edit(string id)
        {
            if (string.IsNullOrEmpty(id))
            {
                return BadRequest("ID do pagamento não fornecido");
            }

            try
            {
                var token = HttpContext.GetJwtToken();
                var payment = await _paymentService.GetPaymentByIdAsync(id, token);

                if (payment == null)
                {
                    return NotFound("Pagamento não encontrado");
                }

                var paymentTypes = await _paymentTypeService.GetAllPaymentTypesAsync(token);
                var paymentMethods = await _paymentMethodService.GetAllPaymentMethodsAsync(token);
                var creditCards = await _creditCardService.GetAllCreditCardsAsync(token);

                ViewBag.PaymentTypes = paymentTypes;
                ViewBag.PaymentMethods = paymentMethods;
                ViewBag.CreditCards = creditCards;
                ViewBag.CreditCardPaymentMethod = paymentMethods.FirstOrDefault(pm => pm.Type == 2)?.Id;

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
                TempData["ErrorMessage"] = MessageHelper.GetLoadingErrorMessage(EntityNames.Payment, ex);
                return RedirectToAction(nameof(Index));
            }
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        [RequirePermission("payments.edit")]
        public async Task<IActionResult> Edit(string id, UpdatePaymentModel model)
        {
            if (string.IsNullOrEmpty(id))
            {
                return BadRequest("ID do pagamento não fornecido");
            }

            if (!ModelState.IsValid)
            {
                await LoadFormDependencies();
                return View(model);
            }

            try
            {
                var token = HttpContext.GetJwtToken();
                await _paymentService.UpdatePaymentAsync(id, model, token);
                TempData["SuccessMessage"] = MessageHelper.GetUpdateSuccessMessage(EntityNames.Payment);
                return RedirectToAction(nameof(Details), new { id });
            }
            catch (Exception ex)
            {
                ModelState.AddModelError(string.Empty, MessageHelper.GetUpdateErrorMessage(EntityNames.Payment, ex));
                await LoadFormDependencies();
                return View(model);
            }
        }

        [RequirePermission("payments.delete")]
        public async Task<IActionResult> Delete(string id)
        {
            if (string.IsNullOrEmpty(id))
            {
                return BadRequest("ID do pagamento não fornecido");
            }

            try
            {
                var token = HttpContext.GetJwtToken();
                var payment = await _paymentService.GetPaymentByIdAsync(id, token);

                return payment == null ? NotFound("Pagamento não encontrado") : View(payment);
            }
            catch (Exception ex)
            {
                TempData["ErrorMessage"] = MessageHelper.GetLoadingErrorMessage(EntityNames.Payment, ex);
                return RedirectToAction(nameof(Index));
            }
        }

        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        [RequirePermission("payments.delete")]
        public async Task<IActionResult> DeleteConfirmed(string id)
        {
            if (string.IsNullOrEmpty(id))
            {
                return BadRequest("ID do pagamento não fornecido");
            }

            try
            {
                var token = HttpContext.GetJwtToken();
                await _paymentService.DeletePaymentAsync(id, token);
                TempData["SuccessMessage"] = MessageHelper.GetDeletionSuccessMessage(EntityNames.Payment);
                return RedirectToAction(nameof(Index));
            }
            catch (Exception ex)
            {
                TempData["ErrorMessage"] = MessageHelper.GetDeletionErrorMessage(EntityNames.Payment, ex);
                return RedirectToAction(nameof(Index));
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
                var token = HttpContext.GetJwtToken();
                await _paymentService.MarkAsPaidAsync(id, paymentDate ?? DateTime.Now, token);
                TempData["SuccessMessage"] = MessageHelper.GetStatusChangeSuccessMessage(EntityNames.Payment, EntityStatus.Paid);
                return RedirectToAction(nameof(Details), new { id });
            }
            catch (Exception ex)
            {
                TempData["ErrorMessage"] = MessageHelper.GetStatusChangeErrorMessage(EntityNames.Payment, EntityStatus.Paid, ex);
                return RedirectToAction(nameof(Details), new { id });
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
                var token = HttpContext.GetJwtToken();
                await _paymentService.MarkAsOverdueAsync(id, token);
                TempData["SuccessMessage"] = MessageHelper.GetStatusChangeSuccessMessage(EntityNames.Payment, EntityStatus.Overdue);
                return RedirectToAction(nameof(Details), new { id });
            }
            catch (Exception ex)
            {
                TempData["ErrorMessage"] = MessageHelper.GetStatusChangeErrorMessage(EntityNames.Payment, EntityStatus.Overdue, ex);
                return RedirectToAction(nameof(Details), new { id });
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
                var token = HttpContext.GetJwtToken();
                await _paymentService.CancelPaymentAsync(id, token);
                TempData["SuccessMessage"] = MessageHelper.GetCancelSuccessMessage(EntityNames.Payment);
                return RedirectToAction(nameof(Details), new { id });
            }
            catch (Exception ex)
            {
                TempData["ErrorMessage"] = MessageHelper.GetCancelErrorMessage(EntityNames.Payment, ex);
                return RedirectToAction(nameof(Details), new { id });
            }
        }

        private async Task LoadFormDependencies(bool includeFinancings = true)
        {
            try
            {
                var token = HttpContext.GetJwtToken();
                var paymentTypes = await _paymentTypeService.GetAllPaymentTypesAsync(token);
                var paymentMethods = await _paymentMethodService.GetAllPaymentMethodsAsync(token);
                var creditCards = await _creditCardService.GetAllCreditCardsAsync(token);
                var financings = await _financingService.GetActiveFinancingsAsync(token);

                ViewBag.PaymentTypes = paymentTypes;
                ViewBag.PaymentMethods = paymentMethods;
                ViewBag.CreditCards = creditCards;
                ViewBag.Financings = financings;
                ViewBag.CreditCardPaymentMethod = paymentMethods.FirstOrDefault(pm => pm.Type == 2)?.Id;
            }
            catch
            {
                ViewBag.PaymentTypes = new List<object>();
                ViewBag.PaymentMethods = new List<object>();
                ViewBag.CreditCards = new List<object>();
                ViewBag.Financings = new List<object>();
            }
        }
    }
}