using FinanceSystem.Resources.Web;
using FinanceSystem.Resources.Web.Enums;
using FinanceSystem.Resources.Web.Helpers;
using FinanceSystem.Web.Extensions;
using FinanceSystem.Web.Filters;
using FinanceSystem.Web.Models.CreditCard;
using FinanceSystem.Web.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace FinanceSystem.Web.Controllers
{
    [Authorize]
    [RequirePermission("creditcards.view")]
    public class CreditCardsController : Controller
    {
        private readonly ICreditCardService _creditCardService;
        private readonly IPaymentMethodService _paymentMethodService;

        private const int CREDIT_CARD_PAYMENT_TYPE = 2;

        public CreditCardsController(
            ICreditCardService creditCardService,
            IPaymentMethodService paymentMethodService)
        {
            _creditCardService = creditCardService ?? throw new ArgumentNullException(nameof(creditCardService));
            _paymentMethodService = paymentMethodService ?? throw new ArgumentNullException(nameof(paymentMethodService));
        }

        public async Task<IActionResult> Index()
        {
            try
            {
                var token = HttpContext.GetJwtToken();
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
                var token = HttpContext.GetJwtToken();
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
                var token = HttpContext.GetJwtToken();
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
                var token = HttpContext.GetJwtToken();
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
                var token = HttpContext.GetJwtToken();
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
                var token = HttpContext.GetJwtToken();
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
                var token = HttpContext.GetJwtToken();
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
                var token = HttpContext.GetJwtToken();
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
                var token = HttpContext.GetJwtToken();
                var creditCardPaymentMethods = await _paymentMethodService.GetByTypeAsync(CREDIT_CARD_PAYMENT_TYPE, token);
                ViewBag.PaymentMethods = creditCardPaymentMethods;
            }
            catch
            {
                ViewBag.PaymentMethods = new List<object>();
            }
        }
    }
}