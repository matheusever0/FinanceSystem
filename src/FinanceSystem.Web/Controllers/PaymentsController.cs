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
        private readonly ILogger<PaymentsController> _logger;

        public PaymentsController(
            IPaymentService paymentService,
            IPaymentTypeService paymentTypeService,
            IPaymentMethodService paymentMethodService,
            ICreditCardService creditCardService,
            ILogger<PaymentsController> logger)
        {
            _paymentService = paymentService;
            _paymentTypeService = paymentTypeService;
            _paymentMethodService = paymentMethodService;
            _creditCardService = creditCardService;
            _logger = logger;
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
                _logger.LogError(ex, "Erro ao carregar pagamentos");
                TempData["ErrorMessage"] = $"Erro ao carregar pagamentos: {ex.Message}";
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
                _logger.LogError(ex, "Erro ao carregar pagamentos pendentes");
                TempData["ErrorMessage"] = $"Erro ao carregar pagamentos pendentes: {ex.Message}";
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
                _logger.LogError(ex, "Erro ao carregar pagamentos vencidos");
                TempData["ErrorMessage"] = $"Erro ao carregar pagamentos vencidos: {ex.Message}";
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
                var payments = await _paymentService.GetPaymentsByMonthAsync(month, year, token);

                ViewBag.Month = month;
                ViewBag.Year = year;
                ViewBag.Title = $"Pagamentos de {new DateTime(year, month, 1).ToString("MMMM/yyyy")}";

                return View("Index", payments);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Erro ao carregar pagamentos por mês");
                TempData["ErrorMessage"] = $"Erro ao carregar pagamentos por mês: {ex.Message}";
                return RedirectToAction("Index", "Home");
            }
        }

        public async Task<IActionResult> ByType(string id)
        {
            try
            {
                var token = HttpContext.GetJwtToken();
                var payments = await _paymentService.GetPaymentsByTypeAsync(id, token);
                var paymentType = await _paymentTypeService.GetPaymentTypeByIdAsync(id, token);

                ViewBag.Title = $"Pagamentos por Tipo: {paymentType.Name}";
                ViewBag.TypeId = id;

                return View("Index", payments);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Erro ao carregar pagamentos por tipo");
                TempData["ErrorMessage"] = $"Erro ao carregar pagamentos por tipo: {ex.Message}";
                return RedirectToAction("Index", "Home");
            }
        }

        public async Task<IActionResult> ByMethod(string id)
        {
            try
            {
                var token = HttpContext.GetJwtToken();
                var payments = await _paymentService.GetPaymentsByMethodAsync(id, token);
                var paymentMethod = await _paymentMethodService.GetPaymentMethodByIdAsync(id, token);

                ViewBag.Title = $"Pagamentos por Método: {paymentMethod.Name}";
                ViewBag.MethodId = id;

                return View("Index", payments);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Erro ao carregar pagamentos por método");
                TempData["ErrorMessage"] = $"Erro ao carregar pagamentos por método: {ex.Message}";
                return RedirectToAction("Index", "Home");
            }
        }

        public async Task<IActionResult> Details(string id)
        {
            try
            {
                var token = HttpContext.GetJwtToken();
                var payment = await _paymentService.GetPaymentByIdAsync(id, token);
                return View(payment);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Erro ao carregar detalhes do pagamento");
                TempData["ErrorMessage"] = $"Erro ao carregar detalhes do pagamento: {ex.Message}";
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

                ViewBag.PaymentTypes = paymentTypes;
                ViewBag.PaymentMethods = paymentMethods;
                ViewBag.CreditCards = creditCards;

                return View();
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Erro ao preparar formulário de criação de pagamento");
                TempData["ErrorMessage"] = $"Erro ao preparar formulário: {ex.Message}";
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

                    if (paymentMethod != null && paymentMethod.Type == 2)
                    {
                        ModelState.AddModelError("CreditCardId", "Cartão de crédito é obrigatório para este método de pagamento.");
                    }
                    else
                    {
                        ModelState.Remove("CreditCardId");
                    }
                }

                if (ModelState.IsValid)
                {
                    _logger.LogInformation("Usuário {UserName} criando novo pagamento", User.Identity.Name);
                    var payment = await _paymentService.CreatePaymentAsync(model, token);
                    TempData["SuccessMessage"] = "Pagamento criado com sucesso!";
                    return RedirectToAction(nameof(Details), new { id = payment.Id });
                }
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Erro ao criar pagamento");
                ModelState.AddModelError(string.Empty, $"Erro ao criar pagamento: {ex.Message}");
            }

            try
            {
                var paymentTypes = await _paymentTypeService.GetAllPaymentTypesAsync(token);
                var paymentMethods = await _paymentMethodService.GetAllPaymentMethodsAsync(token);
                var creditCards = await _creditCardService.GetAllCreditCardsAsync(token);

                ViewBag.PaymentTypes = paymentTypes;
                ViewBag.PaymentMethods = paymentMethods;
                ViewBag.CreditCards = creditCards;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Erro ao recarregar dados de referência para criação de pagamento");
            }

            return View(model);
        }

        [RequirePermission("payments.edit")]
        public async Task<IActionResult> Edit(string id)
        {
            try
            {
                var token = HttpContext.GetJwtToken();
                var payment = await _paymentService.GetPaymentByIdAsync(id, token);
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
                _logger.LogError(ex, "Erro ao carregar pagamento para edição");
                TempData["ErrorMessage"] = $"Erro ao carregar pagamento para edição: {ex.Message}";
                return RedirectToAction(nameof(Index));
            }
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        [RequirePermission("payments.edit")]
        public async Task<IActionResult> Edit(string id, UpdatePaymentModel model)
        {
            var token = HttpContext.GetJwtToken();

            try
            {
                if (ModelState.IsValid)
                {
                    await _paymentService.UpdatePaymentAsync(id, model, token);
                    TempData["SuccessMessage"] = "Pagamento atualizado com sucesso!";
                    return RedirectToAction(nameof(Details), new { id });
                }
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Erro ao atualizar pagamento");
                ModelState.AddModelError(string.Empty, $"Erro ao atualizar pagamento: {ex.Message}");
            }

                        try
            {
                var paymentTypes = await _paymentTypeService.GetAllPaymentTypesAsync(token);
                var paymentMethods = await _paymentMethodService.GetAllPaymentMethodsAsync(token);

                ViewBag.PaymentTypes = paymentTypes;
                ViewBag.PaymentMethods = paymentMethods;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Erro ao recarregar dados de referência para edição de pagamento");
            }

            return View(model);
        }

        [RequirePermission("payments.delete")]
        public async Task<IActionResult> Delete(string id)
        {
            try
            {
                var token = HttpContext.GetJwtToken();
                var payment = await _paymentService.GetPaymentByIdAsync(id, token);
                return View(payment);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Erro ao carregar pagamento para exclusão");
                TempData["ErrorMessage"] = $"Erro ao carregar pagamento para exclusão: {ex.Message}";
                return RedirectToAction(nameof(Index));
            }
        }

        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        [RequirePermission("payments.delete")]
        public async Task<IActionResult> DeleteConfirmed(string id)
        {
            try
            {
                var token = HttpContext.GetJwtToken();
                await _paymentService.DeletePaymentAsync(id, token);
                TempData["SuccessMessage"] = "Pagamento excluído com sucesso!";
                return RedirectToAction(nameof(Index));
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Erro ao excluir pagamento");
                TempData["ErrorMessage"] = $"Erro ao excluir pagamento: {ex.Message}";
                return RedirectToAction(nameof(Index));
            }
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        [RequirePermission("payments.edit")]
        public async Task<IActionResult> MarkAsPaid(string id, DateTime? paymentDate = null)
        {
            try
            {
                var token = HttpContext.GetJwtToken();
                await _paymentService.MarkAsPaidAsync(id, paymentDate ?? DateTime.Now, token);
                TempData["SuccessMessage"] = "Pagamento marcado como pago com sucesso!";
                return RedirectToAction(nameof(Details), new { id });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Erro ao marcar pagamento como pago");
                TempData["ErrorMessage"] = $"Erro ao marcar pagamento como pago: {ex.Message}";
                return RedirectToAction(nameof(Details), new { id });
            }
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        [RequirePermission("payments.edit")]
        public async Task<IActionResult> MarkAsOverdue(string id)
        {
            try
            {
                var token = HttpContext.GetJwtToken();
                await _paymentService.MarkAsOverdueAsync(id, token);
                TempData["SuccessMessage"] = "Pagamento marcado como vencido com sucesso!";
                return RedirectToAction(nameof(Details), new { id });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Erro ao marcar pagamento como vencido");
                TempData["ErrorMessage"] = $"Erro ao marcar pagamento como vencido: {ex.Message}";
                return RedirectToAction(nameof(Details), new { id });
            }
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        [RequirePermission("payments.edit")]
        public async Task<IActionResult> Cancel(string id)
        {
            try
            {
                var token = HttpContext.GetJwtToken();
                await _paymentService.CancelPaymentAsync(id, token);
                TempData["SuccessMessage"] = "Pagamento cancelado com sucesso!";
                return RedirectToAction(nameof(Details), new { id });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Erro ao cancelar pagamento");
                TempData["ErrorMessage"] = $"Erro ao cancelar pagamento: {ex.Message}";
                return RedirectToAction(nameof(Details), new { id });
            }
        }
    }
}