using FinanceSystem.Web.Extensions;
using FinanceSystem.Web.Filters;
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

        private const string ERROR_LOADING_PAYMENTS = "Erro ao carregar pagamentos: {0}";
        private const string ERROR_LOADING_PENDING_PAYMENTS = "Erro ao carregar pagamentos pendentes: {0}";
        private const string ERROR_LOADING_OVERDUE_PAYMENTS = "Erro ao carregar pagamentos vencidos: {0}";
        private const string ERROR_LOADING_MONTHLY_PAYMENTS = "Erro ao carregar pagamentos por mês: {0}";
        private const string ERROR_LOADING_PAYMENTS_BY_TYPE = "Erro ao carregar pagamentos por tipo: {0}";
        private const string ERROR_LOADING_PAYMENTS_BY_METHOD = "Erro ao carregar pagamentos por método: {0}";
        private const string ERROR_LOADING_PAYMENT_DETAILS = "Erro ao carregar detalhes do pagamento: {0}";
        private const string ERROR_PREPARING_FORM = "Erro ao preparar formulário: {0}";
        private const string ERROR_CREATING_PAYMENT = "Erro ao criar pagamento: {0}";
        private const string ERROR_LOADING_PAYMENT_EDIT = "Erro ao carregar pagamento para edição: {0}";
        private const string ERROR_UPDATING_PAYMENT = "Erro ao atualizar pagamento: {0}";
        private const string ERROR_LOADING_PAYMENT_DELETE = "Erro ao carregar pagamento para exclusão: {0}";
        private const string ERROR_DELETING_PAYMENT = "Erro ao excluir pagamento: {0}";
        private const string ERROR_MARK_PAID = "Erro ao marcar pagamento como pago: {0}";
        private const string ERROR_MARK_OVERDUE = "Erro ao marcar pagamento como vencido: {0}";
        private const string ERROR_CANCEL_PAYMENT = "Erro ao cancelar pagamento: {0}";
        private const string ERROR_CREDIT_CARD_REQUIRED = "Cartão de crédito é obrigatório para este método de pagamento.";

        private const string SUCCESS_CREATE_PAYMENT = "Pagamento criado com sucesso!";
        private const string SUCCESS_UPDATE_PAYMENT = "Pagamento atualizado com sucesso!";
        private const string SUCCESS_DELETE_PAYMENT = "Pagamento excluído com sucesso!";
        private const string SUCCESS_MARK_PAID = "Pagamento marcado como pago com sucesso!";
        private const string SUCCESS_MARK_OVERDUE = "Pagamento marcado como vencido com sucesso!";
        private const string SUCCESS_CANCEL_PAYMENT = "Pagamento cancelado com sucesso!";

        private const int CREDIT_CARD_PAYMENT_TYPE = 2;

        public PaymentsController(
            IPaymentService paymentService,
            IPaymentTypeService paymentTypeService,
            IPaymentMethodService paymentMethodService,
            ICreditCardService creditCardService)
        {
            _paymentService = paymentService ?? throw new ArgumentNullException(nameof(paymentService));
            _paymentTypeService = paymentTypeService ?? throw new ArgumentNullException(nameof(paymentTypeService));
            _paymentMethodService = paymentMethodService ?? throw new ArgumentNullException(nameof(paymentMethodService));
            _creditCardService = creditCardService ?? throw new ArgumentNullException(nameof(creditCardService));
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
                TempData["ErrorMessage"] = string.Format(ERROR_LOADING_PAYMENTS, ex.Message);
                return RedirectToAction("Index", "Home");
            }
        }

        public async Task<IActionResult> Pending()
        {
            try
            {
                var token = HttpContext.GetJwtToken();
                var payments = await _paymentService.GetPendingPaymentsAsync(token);
                ViewBag.Title = "Pagamentos Pendentes";
                return View("Index", payments);
            }
            catch (Exception ex)
            {
                TempData["ErrorMessage"] = string.Format(ERROR_LOADING_PENDING_PAYMENTS, ex.Message);
                return RedirectToAction("Index", "Home");
            }
        }

        public async Task<IActionResult> Overdue()
        {
            try
            {
                var token = HttpContext.GetJwtToken();
                var payments = await _paymentService.GetOverduePaymentsAsync(token);
                ViewBag.Title = "Pagamentos Vencidos";
                return View("Index", payments);
            }
            catch (Exception ex)
            {
                TempData["ErrorMessage"] = string.Format(ERROR_LOADING_OVERDUE_PAYMENTS, ex.Message);
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
                var payments = await _paymentService.GetPaymentsByMonthAsync(month, year, token);

                ViewBag.Month = month;
                ViewBag.Year = year;
                ViewBag.Title = $"Pagamentos de {new DateTime(year, month, 1).ToString("MMMM/yyyy")}";

                return View("Index", payments);
            }
            catch (Exception ex)
            {
                TempData["ErrorMessage"] = string.Format(ERROR_LOADING_MONTHLY_PAYMENTS, ex.Message);
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
                var paymentType = await _paymentTypeService.GetPaymentTypeByIdAsync(id, token);

                if (paymentType == null)
                {
                    return NotFound("Tipo de pagamento não encontrado");
                }

                var payments = await _paymentService.GetPaymentsByTypeAsync(id, token);

                ViewBag.Title = $"Pagamentos por Tipo: {paymentType.Name}";
                ViewBag.TypeId = id;

                return View("Index", payments);
            }
            catch (Exception ex)
            {
                TempData["ErrorMessage"] = string.Format(ERROR_LOADING_PAYMENTS_BY_TYPE, ex.Message);
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
                var paymentMethod = await _paymentMethodService.GetPaymentMethodByIdAsync(id, token);

                if (paymentMethod == null)
                {
                    return NotFound("Método de pagamento não encontrado");
                }

                var payments = await _paymentService.GetPaymentsByMethodAsync(id, token);

                ViewBag.Title = $"Pagamentos por Método: {paymentMethod.Name}";
                ViewBag.MethodId = id;

                return View("Index", payments);
            }
            catch (Exception ex)
            {
                TempData["ErrorMessage"] = string.Format(ERROR_LOADING_PAYMENTS_BY_METHOD, ex.Message);
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

                if (payment == null)
                {
                    return NotFound("Pagamento não encontrado");
                }

                return View(payment);
            }
            catch (Exception ex)
            {
                TempData["ErrorMessage"] = string.Format(ERROR_LOADING_PAYMENT_DETAILS, ex.Message);
                return RedirectToAction(nameof(Index));
            }
        }

        [RequirePermission("payments.create")]
        public async Task<IActionResult> Create()
        {
            try
            {
                var token = HttpContext.GetJwtToken();
                await LoadReferenceDataForView(token);
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
        [RequirePermission("payments.create")]
        public async Task<IActionResult> Create(CreatePaymentModel model, List<string> selectedRoles)
        {
            var token = HttpContext.GetJwtToken();

            try
            {
                if (string.IsNullOrEmpty(model.CreditCardId))
                {
                    var paymentMethod = await _paymentMethodService.GetPaymentMethodByIdAsync(model.PaymentMethodId, token);

                    if (paymentMethod != null && paymentMethod.Type == CREDIT_CARD_PAYMENT_TYPE)
                    {
                        ModelState.AddModelError("CreditCardId", ERROR_CREDIT_CARD_REQUIRED);
                    }
                    else
                    {
                        ModelState.Remove("CreditCardId");
                    }
                }

                if (!ModelState.IsValid)
                {
                    await LoadReferenceDataForView(token);
                    return View(model);
                }

                var payment = await _paymentService.CreatePaymentAsync(model, token);
                TempData["SuccessMessage"] = SUCCESS_CREATE_PAYMENT;
                return RedirectToAction(nameof(Details), new { id = payment.Id });
            }
            catch (Exception ex)
            {
                ModelState.AddModelError(string.Empty, string.Format(ERROR_CREATING_PAYMENT, ex.Message));
                await LoadReferenceDataForView(token);
                return View(model);
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

                ViewBag.PaymentTypes = paymentTypes;
                ViewBag.PaymentMethods = paymentMethods;

                var model = new UpdatePaymentModel
                {
                    Description = payment.Description,
                    Amount = payment.Amount,
                    DueDate = payment.DueDate,
                    IsRecurring = payment.IsRecurring,
                    Notes = payment.Notes,
                    PaymentTypeId = payment.PaymentTypeId,
                    PaymentMethodId = payment.PaymentMethodId
                };

                return View(model);
            }
            catch (Exception ex)
            {
                TempData["ErrorMessage"] = string.Format(ERROR_LOADING_PAYMENT_EDIT, ex.Message);
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
                var token = HttpContext.GetJwtToken();
                var paymentTypes = await _paymentTypeService.GetAllPaymentTypesAsync(token);
                var paymentMethods = await _paymentMethodService.GetAllPaymentMethodsAsync(token);

                ViewBag.PaymentTypes = paymentTypes;
                ViewBag.PaymentMethods = paymentMethods;

                return View(model);
            }

            try
            {
                var token = HttpContext.GetJwtToken();
                await _paymentService.UpdatePaymentAsync(id, model, token);
                TempData["SuccessMessage"] = SUCCESS_UPDATE_PAYMENT;
                return RedirectToAction(nameof(Details), new { id });
            }
            catch (Exception ex)
            {
                ModelState.AddModelError(string.Empty, string.Format(ERROR_UPDATING_PAYMENT, ex.Message));

                try
                {
                    var token = HttpContext.GetJwtToken();
                    var paymentTypes = await _paymentTypeService.GetAllPaymentTypesAsync(token);
                    var paymentMethods = await _paymentMethodService.GetAllPaymentMethodsAsync(token);

                    ViewBag.PaymentTypes = paymentTypes;
                    ViewBag.PaymentMethods = paymentMethods;
                }
                catch
                {
                    // Se falhar ao carregar dados de referência, continua sem exibir as listas
                }

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

                if (payment == null)
                {
                    return NotFound("Pagamento não encontrado");
                }

                return View(payment);
            }
            catch (Exception ex)
            {
                TempData["ErrorMessage"] = string.Format(ERROR_LOADING_PAYMENT_DELETE, ex.Message);
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
                TempData["SuccessMessage"] = SUCCESS_DELETE_PAYMENT;
                return RedirectToAction(nameof(Index));
            }
            catch (Exception ex)
            {
                TempData["ErrorMessage"] = string.Format(ERROR_DELETING_PAYMENT, ex.Message);
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
                TempData["SuccessMessage"] = SUCCESS_MARK_PAID;
                return RedirectToAction(nameof(Details), new { id });
            }
            catch (Exception ex)
            {
                TempData["ErrorMessage"] = string.Format(ERROR_MARK_PAID, ex.Message);
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
                TempData["SuccessMessage"] = SUCCESS_MARK_OVERDUE;
                return RedirectToAction(nameof(Details), new { id });
            }
            catch (Exception ex)
            {
                TempData["ErrorMessage"] = string.Format(ERROR_MARK_OVERDUE, ex.Message);
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
                TempData["SuccessMessage"] = SUCCESS_CANCEL_PAYMENT;
                return RedirectToAction(nameof(Details), new { id });
            }
            catch (Exception ex)
            {
                TempData["ErrorMessage"] = string.Format(ERROR_CANCEL_PAYMENT, ex.Message);
                return RedirectToAction(nameof(Details), new { id });
            }
        }

        private async Task LoadReferenceDataForView(string token)
        {
            try
            {
                var paymentTypes = await _paymentTypeService.GetAllPaymentTypesAsync(token);
                var paymentMethods = await _paymentMethodService.GetAllPaymentMethodsAsync(token);
                var creditCards = await _creditCardService.GetAllCreditCardsAsync(token);

                ViewBag.PaymentTypes = paymentTypes;
                ViewBag.PaymentMethods = paymentMethods;
                ViewBag.CreditCards = creditCards;
            }
            catch
            {
                ViewBag.PaymentTypes = new List<object>();
                ViewBag.PaymentMethods = new List<object>();
                ViewBag.CreditCards = new List<object>();
            }
        }
    }
}