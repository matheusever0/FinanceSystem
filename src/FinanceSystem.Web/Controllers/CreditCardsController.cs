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

        private const string ERROR_LOADING_CARDS = "Erro ao carregar cartões de crédito: {0}";
        private const string ERROR_LOADING_CARD_DETAILS = "Erro ao carregar detalhes do cartão de crédito: {0}";
        private const string ERROR_PREPARING_FORM = "Erro ao preparar formulário: {0}";
        private const string ERROR_CREATING_CARD = "Erro ao criar cartão de crédito: {0}";
        private const string ERROR_UPDATING_CARD = "Erro ao atualizar cartão de crédito: {0}";
        private const string ERROR_LOADING_CARD_EDIT = "Erro ao carregar cartão de crédito para edição: {0}";
        private const string ERROR_LOADING_CARD_DELETE = "Erro ao carregar cartão de crédito para exclusão: {0}";
        private const string ERROR_DELETING_CARD = "Erro ao excluir cartão de crédito: {0}";

        private const string SUCCESS_CREATE_CARD = "Cartão de crédito criado com sucesso!";
        private const string SUCCESS_UPDATE_CARD = "Cartão de crédito atualizado com sucesso!";
        private const string SUCCESS_DELETE_CARD = "Cartão de crédito excluído com sucesso!";

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
                TempData["ErrorMessage"] = string.Format(ERROR_LOADING_CARDS, ex.Message);
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
                TempData["ErrorMessage"] = string.Format(ERROR_LOADING_CARD_DETAILS, ex.Message);
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
                TempData["ErrorMessage"] = string.Format(ERROR_PREPARING_FORM, ex.Message);
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
                TempData["SuccessMessage"] = SUCCESS_CREATE_CARD;
                return RedirectToAction(nameof(Details), new { id = creditCard.Id });
            }
            catch (Exception ex)
            {
                ModelState.AddModelError(string.Empty, string.Format(ERROR_CREATING_CARD, ex.Message));
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
                TempData["ErrorMessage"] = string.Format(ERROR_LOADING_CARD_EDIT, ex.Message);
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
                TempData["SuccessMessage"] = SUCCESS_UPDATE_CARD;
                return RedirectToAction(nameof(Details), new { id });
            }
            catch (Exception ex)
            {
                ModelState.AddModelError(string.Empty, string.Format(ERROR_UPDATING_CARD, ex.Message));
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
                TempData["ErrorMessage"] = string.Format(ERROR_LOADING_CARD_DELETE, ex.Message);
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
                TempData["SuccessMessage"] = SUCCESS_DELETE_CARD;
                return RedirectToAction(nameof(Index));
            }
            catch (Exception ex)
            {
                TempData["ErrorMessage"] = string.Format(ERROR_DELETING_CARD, ex.Message);
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