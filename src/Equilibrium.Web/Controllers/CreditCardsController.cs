using Equilibrium.Resources.Web;
using Equilibrium.Resources.Web.Enums;
using Equilibrium.Resources.Web.Helpers;
using Equilibrium.Web.Filters;
using Equilibrium.Web.Interfaces;
using Equilibrium.Web.Models.CreditCard;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace Equilibrium.Web.Controllers
{
    [Authorize]
    [RequirePermission("creditcards.view")]
    public class CreditCardsController : BaseController
    {
        private readonly ICreditCardService _creditCardService;
        private readonly IPaymentMethodService _paymentMethodService;
        private readonly ICreditCardInvoiceService _invoiceService;

        private const int CREDIT_CARD_PAYMENT_TYPE = 2;

        public CreditCardsController(
            ICreditCardService creditCardService,
            IPaymentMethodService paymentMethodService,
            ICreditCardInvoiceService invoiceService)
        {
            _creditCardService = creditCardService ?? throw new ArgumentNullException(nameof(creditCardService));
            _paymentMethodService = paymentMethodService ?? throw new ArgumentNullException(nameof(paymentMethodService));
            _invoiceService = invoiceService;
        }

        public async Task<IActionResult> Index()
        {
            try
            {
                var token = GetToken();
                var creditCards = await _creditCardService.GetAllCreditCardsAsync(token);
                return View(creditCards);
            }
            catch (Exception ex)
            {
                TempData["ErrorMessage"] = MessageHelper.GetLoadingErrorMessage(EntityNames.CreditCard, ex);
                return RedirectToAction("Index", "Home");
            }
        }

        public async Task<IActionResult> Details(string id)
        {
            if (string.IsNullOrEmpty(id))
            {
                return BadRequest("ID do cartão de crédito não fornecido");
            }

            try
            {
                var token = GetToken();
                var creditCard = await _creditCardService.GetCreditCardByIdAsync(id, token);

                return creditCard == null ? NotFound("Cartão de crédito não encontrado") : View(creditCard);
            }
            catch (Exception ex)
            {
                TempData["ErrorMessage"] = MessageHelper.GetLoadingErrorMessage(EntityNames.CreditCard, ex);
                return RedirectToAction(nameof(Index));
            }
        }

        [RequirePermission("creditcards.create")]
        public async Task<IActionResult> Create()
        {
            try
            {
                var token = GetToken();
                var creditCardPaymentMethods = await _paymentMethodService.GetByTypeAsync(CREDIT_CARD_PAYMENT_TYPE, token);
                ViewBag.PaymentMethods = creditCardPaymentMethods;
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
        [RequirePermission("creditcards.create")]
        public async Task<IActionResult> Create(CreateCreditCardModel model)
        {
            if (!ModelState.IsValid)
            {
                await LoadPaymentMethodsForView();
                return View(model);
            }

            try
            {
                var token = GetToken();
                var creditCard = await _creditCardService.CreateCreditCardAsync(model, token);
                TempData["SuccessMessage"] = MessageHelper.GetCreationSuccessMessage(EntityNames.CreditCard);
                return RedirectToAction(nameof(Details), new { id = creditCard.Id });
            }
            catch (Exception ex)
            {
                ModelState.AddModelError(string.Empty, MessageHelper.GetCreationErrorMessage(EntityNames.CreditCard, ex));
                await LoadPaymentMethodsForView();
                return View(model);
            }
        }

        [RequirePermission("creditcards.edit")]
        public async Task<IActionResult> Edit(string id)
        {
            if (string.IsNullOrEmpty(id))
            {
                return BadRequest("ID do cartão de crédito não fornecido");
            }

            try
            {
                var token = GetToken();
                var creditCard = await _creditCardService.GetCreditCardByIdAsync(id, token);

                if (creditCard == null)
                {
                    return NotFound("Cartão de crédito não encontrado");
                }

                var model = new UpdateCreditCardModel
                {
                    Name = creditCard.Name,
                    ClosingDay = creditCard.ClosingDay,
                    DueDay = creditCard.DueDay,
                    Limit = creditCard.Limit
                };

                return View(model);
            }
            catch (Exception ex)
            {
                TempData["ErrorMessage"] = MessageHelper.GetLoadingErrorMessage(EntityNames.CreditCard, ex);
                return RedirectToAction(nameof(Index));
            }
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        [RequirePermission("creditcards.edit")]
        public async Task<IActionResult> Edit(string id, UpdateCreditCardModel model)
        {
            if (string.IsNullOrEmpty(id))
            {
                return BadRequest("ID do cartão de crédito não fornecido");
            }

            if (!ModelState.IsValid)
            {
                return View(model);
            }

            try
            {
                var token = GetToken();
                await _creditCardService.UpdateCreditCardAsync(id, model, token);
                TempData["SuccessMessage"] = MessageHelper.GetUpdateSuccessMessage(EntityNames.CreditCard);
                return RedirectToAction(nameof(Details), new { id });
            }
            catch (Exception ex)
            {
                ModelState.AddModelError(string.Empty, MessageHelper.GetUpdateErrorMessage(EntityNames.CreditCard, ex));
                return View(model);
            }
        }

        [RequirePermission("creditcards.delete")]
        public async Task<IActionResult> Delete(string id)
        {
            if (string.IsNullOrEmpty(id))
            {
                return BadRequest("ID do cartão de crédito não fornecido");
            }

            try
            {
                var token = GetToken();
                var creditCard = await _creditCardService.GetCreditCardByIdAsync(id, token);

                return creditCard == null ? NotFound("Cartão de crédito não encontrado") : View(creditCard);
            }
            catch (Exception ex)
            {
                TempData["ErrorMessage"] = MessageHelper.GetLoadingErrorMessage(EntityNames.CreditCard, ex);
                return RedirectToAction(nameof(Index));
            }
        }

        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        [RequirePermission("creditcards.delete")]
        public async Task<IActionResult> DeleteConfirmed(string id)
        {
            if (string.IsNullOrEmpty(id))
            {
                return BadRequest("ID do cartão de crédito não fornecido");
            }

            try
            {
                var token = GetToken();
                await _creditCardService.DeleteCreditCardAsync(id, token);
                TempData["SuccessMessage"] = MessageHelper.GetDeletionSuccessMessage(EntityNames.CreditCard);
                return RedirectToAction(nameof(Index));
            }
            catch (Exception ex)
            {
                TempData["ErrorMessage"] = MessageHelper.GetDeletionErrorMessage(EntityNames.CreditCard, ex);
                return RedirectToAction(nameof(Index));
            }
        }

        private async Task LoadPaymentMethodsForView()
        {
            try
            {
                var token = GetToken();
                var creditCardPaymentMethods = await _paymentMethodService.GetByTypeAsync(CREDIT_CARD_PAYMENT_TYPE, token);
                ViewBag.PaymentMethods = creditCardPaymentMethods;
            }
            catch
            {
                ViewBag.PaymentMethods = new List<object>();
            }
        }

        /// <summary>
        /// Exibe a fatura atual do cartão
        /// </summary>
        [RequirePermission("creditcards.view")]
        public async Task<IActionResult> CurrentInvoice(string id)
        {
            if (string.IsNullOrEmpty(id))
            {
                return BadRequest("ID do cartão não fornecido");
            }

            try
            {
                var token = GetToken();
                var card = await _creditCardService.GetCreditCardByIdAsync(id, token);

                if (card == null)
                {
                    return NotFound("Cartão não encontrado");
                }

                var invoice = await _invoiceService.GetCurrentInvoiceAsync(id, token);

                ViewBag.CreditCard = card;
                return View(invoice);
            }
            catch (Exception ex)
            {
                return HandleException(ex, EntityNames.CreditCard, "loading invoice");
            }
        }

        /// <summary>
        /// Exibe histórico de faturas
        /// </summary>
        [RequirePermission("creditcards.view")]
        public async Task<IActionResult> InvoiceHistory(string id, int months = 12)
        {
            if (string.IsNullOrEmpty(id))
            {
                return BadRequest("ID do cartão não fornecido");
            }

            try
            {
                var token = GetToken();
                var card = await _creditCardService.GetCreditCardByIdAsync(id, token);

                if (card == null)
                {
                    return NotFound("Cartão não encontrado");
                }

                var invoices = await _invoiceService.GetInvoiceHistoryAsync(id, months, token);

                ViewBag.CreditCard = card;
                ViewBag.Months = months;
                return View(invoices);
            }
            catch (Exception ex)
            {
                return HandleException(ex, EntityNames.CreditCard, "loading invoice history");
            }
        }

        /// <summary>
        /// Exibe detalhes de uma fatura específica
        /// </summary>
        [RequirePermission("creditcards.view")]
        public async Task<IActionResult> InvoiceDetails(string id, DateTime? referenceDate = null)
        {
            if (string.IsNullOrEmpty(id))
            {
                return BadRequest("ID do cartão não fornecido");
            }

            try
            {
                var token = GetToken();
                var card = await _creditCardService.GetCreditCardByIdAsync(id, token);

                if (card == null)
                {
                    return NotFound("Cartão não encontrado");
                }

                var invoiceDetails = await _invoiceService.GetInvoiceDetailsAsync(id, referenceDate, token);

                ViewBag.CreditCard = card;
                return View(invoiceDetails);
            }
            catch (Exception ex)
            {
                return HandleException(ex, EntityNames.CreditCard, "loading invoice details");
            }
        }

        /// <summary>
        /// Processa pagamento de fatura
        /// </summary>
        [HttpPost]
        [ValidateAntiForgeryToken]
        [RequirePermission("creditcards.edit")]
        public async Task<IActionResult> PayInvoice(string id, PayInvoiceModel model)
        {
            if (string.IsNullOrEmpty(id))
            {
                return BadRequest("ID do cartão não fornecido");
            }

            if (!ModelState.IsValid)
            {
                TempData["ErrorMessage"] = "Dados inválidos para pagamento";
                return RedirectToAction(nameof(CurrentInvoice), new { id });
            }

            try
            {
                var token = GetToken();
                var success = await _invoiceService.PayInvoiceAsync(id, model, token);

                if (success)
                {
                    SetCustomSuccessMessage($"Pagamento de {model.GetFormattedAmount()} processado com sucesso!");
                    return RedirectToAction(nameof(CurrentInvoice), new { id });
                }
                else
                {
                    SetCustomErrorMessage("Erro ao processar pagamento da fatura");
                    return RedirectToAction(nameof(CurrentInvoice), new { id });
                }
            }
            catch (Exception ex)
            {
                return HandleException(ex, EntityNames.CreditCard, "processing payment",
                    redirectAction: nameof(CurrentInvoice), routeValues: new { id });
            }
        }

        /// <summary>
        /// Modal para pagamento de fatura
        /// </summary>
        [RequirePermission("creditcards.edit")]
        public async Task<IActionResult> PayInvoice(string id)
        {
            if (string.IsNullOrEmpty(id))
            {
                return BadRequest("ID do cartão não fornecido");
            }

            try
            {
                var token = GetToken();
                var card = await _creditCardService.GetCreditCardByIdAsync(id, token);
                var invoice = await _invoiceService.GetCurrentInvoiceAsync(id, token);

                var model = new PayInvoiceModel
                {
                    Amount = invoice.RemainingAmount,
                    PaymentDate = DateTime.Now,
                    PayFullAmount = true
                };

                ViewBag.CreditCard = card;
                ViewBag.Invoice = invoice;
                return PartialView("_PayInvoiceModal", model);
            }
            catch (Exception ex)
            {
                return HandleException(ex, EntityNames.CreditCard, "loading payment form");
            }
        }

        /// <summary>
        /// API endpoint para dados de fatura (AJAX)
        /// </summary>
        [HttpGet]
        [RequirePermission("creditcards.view")]
        public async Task<IActionResult> GetInvoiceData(string id)
        {
            try
            {
                var token = GetToken();
                var invoice = await _invoiceService.GetCurrentInvoiceAsync(id, token);

                return Json(new
                {
                    success = true,
                    data = new
                    {
                        totalAmount = invoice.TotalAmount,
                        paidAmount = invoice.PaidAmount,
                        remainingAmount = invoice.RemainingAmount,
                        dueDate = invoice.GetFormattedDueDate(),
                        isPaid = invoice.IsPaid,
                        isOverdue = invoice.IsOverdue,
                        usagePercentage = invoice.UsagePercentage,
                        transactionCount = invoice.TransactionCount
                    }
                });
            }
            catch (Exception ex)
            {
                return Json(new { success = false, message = ex.Message });
            }
        }
    }
}
