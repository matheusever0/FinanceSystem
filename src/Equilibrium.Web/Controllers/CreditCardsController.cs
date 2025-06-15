using Equilibrium.Resources.Web;
using Equilibrium.Resources.Web.Enums;
using Equilibrium.Resources.Web.Helpers;
using Equilibrium.Web.Filters;
using Equilibrium.Web.Interfaces;
using Equilibrium.Web.Models.CreditCard;
using Equilibrium.Web.Models.Filters;
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
        private readonly IPaymentService _paymentService;
        private const int CREDIT_CARD_PAYMENT_TYPE = 2;

        public CreditCardsController(
            ICreditCardService creditCardService, 
            IPaymentMethodService paymentMethodService, 
            IPaymentService paymentService)
        {
            _creditCardService = creditCardService;
            _paymentMethodService = paymentMethodService;
            _paymentService = paymentService;
        }

        public async Task<IActionResult> Index()
        {
            try
            {
                var token = GetToken();
                if (string.IsNullOrEmpty(token))
                {
                    return RedirectToAction("Login", "Account");
                }

                var creditCards = await _creditCardService.GetAllCreditCardsAsync(token);

                var updatedCreditCards = new List<CreditCardModel>();

                foreach (var card in creditCards)
                {
                    var availableLimit = await CalculateAvailableLimit(card.Id, token);

                    if (card.AvailableLimit != availableLimit)
                    {
                        try
                        {
                            await _creditCardService.UpdateLimitAsync(card.Id, availableLimit, token);

                            card.AvailableLimit = availableLimit;
                        }
                        catch (Exception ex)
                        {
                            Console.WriteLine($"Erro ao atualizar limite do cartão {card.Id}: {ex.Message}");
                        }
                    }

                    updatedCreditCards.Add(card);
                }

                return View(updatedCreditCards);
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
                ViewBag.PaymentsWithCard = await _paymentService.GetFilteredPaymentsAsync(new PaymentFilter { CreditCardId = id }, token);

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
                var creditCardPaymentMethods = await _paymentMethodService.GetByTypeAsync(CREDIT_CARD_PAYMENT_TYPE, GetToken());
                ViewBag.PaymentMethods = creditCardPaymentMethods;
                return View();
            }
            catch (Exception)
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
                var creditCard = await _creditCardService.CreateCreditCardAsync(model, GetToken());
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
                var creditCard = await _creditCardService.GetCreditCardByIdAsync(id, GetToken());

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
                await _creditCardService.UpdateCreditCardAsync(id, model, GetToken());
                TempData["SuccessMessage"] = MessageHelper.GetUpdateSuccessMessage(EntityNames.CreditCard);
                return RedirectToAction(nameof(Details), new { id });
            }
            catch (Exception ex)
            {
                ModelState.AddModelError(string.Empty, MessageHelper.GetUpdateErrorMessage(EntityNames.CreditCard, ex));
                return View(model);
            }
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        [RequirePermission("creditcards.delete")]
        public async Task<IActionResult> Delete(string id)
        {
            return await HandleGenericDelete(
                id,
                _creditCardService,
                async (service, itemId, token) => await service.DeleteCreditCardAsync(itemId, token),
                async (service, itemId, token) => await service.GetCreditCardByIdAsync(itemId, token),
                "cartão de crédito",
                null,
                async (item) =>
                {
                    if (item is CreditCardModel card)
                    {
                        var pendingPayments = await _paymentService.GetFilteredPaymentsAsync(new PaymentFilter { CreditCardId = card.Id }, GetToken());
                        if (pendingPayments?.Any() == true)
                        {
                            return (false, "Não é possivel excluir este cartão pois existe pagamentos associados.");
                        }
                    }
                    return (true, null);
                }
            );
        }

        private async Task LoadPaymentMethodsForView()
        {
            try
            {
                var creditCardPaymentMethods = await _paymentMethodService.GetByTypeAsync(CREDIT_CARD_PAYMENT_TYPE, GetToken());
                ViewBag.PaymentMethods = creditCardPaymentMethods;
            }
            catch
            {
                ViewBag.PaymentMethods = new List<object>();
            }
        }

        /// <summary>
        /// Calcula o limite disponível baseado no limite total menos os gastos dos payments
        /// </summary>
        /// <param name="creditCardId">ID do cartão de crédito</param>
        /// <param name="token">Token de autenticação</param>
        /// <returns>Valor do limite disponível</returns>
        private async Task<decimal> CalculateAvailableLimit(string creditCardId, string token)
        {
            try
            {
                var creditCard = await _creditCardService.GetCreditCardByIdAsync(creditCardId, token);

                var payments = await _paymentService.GetPaymentsByCreditCardAsync(creditCardId, token);

                var totalSpent = payments
                    .Where(p => p.Status == 1)
                    .Sum(p => p.Amount);

                var availableLimit = creditCard.Limit - totalSpent;

                return Math.Max(0, availableLimit);
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Erro ao calcular limite disponível do cartão {creditCardId}: {ex.Message}");
                return 0;
            }
        }
    }
}