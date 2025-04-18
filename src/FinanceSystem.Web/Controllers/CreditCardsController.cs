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
        private readonly ILogger<CreditCardsController> _logger;

        public CreditCardsController(
            ICreditCardService creditCardService,
            IPaymentMethodService paymentMethodService,
            ILogger<CreditCardsController> logger)
        {
            _creditCardService = creditCardService;
            _paymentMethodService = paymentMethodService;
            _logger = logger;
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
                _logger.LogError(ex, "Erro ao carregar cartões de crédito");
                TempData["ErrorMessage"] = $"Erro ao carregar cartões de crédito: {ex.Message}";
                return RedirectToAction("Index", "Home");
            }
        }

        public async Task<IActionResult> Details(string id)
        {
            try
            {
                var token = HttpContext.GetJwtToken();
                var creditCard = await _creditCardService.GetCreditCardByIdAsync(id, token);
                return View(creditCard);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Erro ao carregar detalhes do cartão de crédito");
                TempData["ErrorMessage"] = $"Erro ao carregar detalhes do cartão de crédito: {ex.Message}";
                return RedirectToAction(nameof(Index));
            }
        }

        [RequirePermission("creditcards.create")]
        public async Task<IActionResult> Create()
        {
            try
            {
                var token = HttpContext.GetJwtToken();
                var creditCardPaymentMethods = await _paymentMethodService.GetByTypeAsync(2, token);
                ViewBag.PaymentMethods = creditCardPaymentMethods;
                return View();
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Erro ao preparar formulário de criação de cartão de crédito");
                TempData["ErrorMessage"] = $"Erro ao preparar formulário: {ex.Message}";
                return RedirectToAction(nameof(Index));
            }
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        [RequirePermission("creditcards.create")]
        public async Task<IActionResult> Create(CreateCreditCardModel model)
        {
            var token = HttpContext.GetJwtToken();

            try
            {
                if (ModelState.IsValid)
                {
                    var creditCard = await _creditCardService.CreateCreditCardAsync(model, token);
                    TempData["SuccessMessage"] = "Cartão de crédito criado com sucesso!";
                    return RedirectToAction(nameof(Details), new { id = creditCard.Id });
                }
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Erro ao criar cartão de crédito");
                ModelState.AddModelError(string.Empty, $"Erro ao criar cartão de crédito: {ex.Message}");
            }

            try
            {
                var creditCardPaymentMethods = await _paymentMethodService.GetByTypeAsync(2, token);
                ViewBag.PaymentMethods = creditCardPaymentMethods;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Erro ao recarregar métodos de pagamento para criação de cartão de crédito");
            }

            return View(model);
        }

        [RequirePermission("creditcards.edit")]
        public async Task<IActionResult> Edit(string id)
        {
            try
            {
                var token = HttpContext.GetJwtToken();
                var creditCard = await _creditCardService.GetCreditCardByIdAsync(id, token);

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
                _logger.LogError(ex, "Erro ao carregar cartão de crédito para edição");
                TempData["ErrorMessage"] = $"Erro ao carregar cartão de crédito para edição: {ex.Message}";
                return RedirectToAction(nameof(Index));
            }
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        [RequirePermission("creditcards.edit")]
        public async Task<IActionResult> Edit(string id, UpdateCreditCardModel model)
        {
            try
            {
                if (ModelState.IsValid)
                {
                    var token = HttpContext.GetJwtToken();
                    await _creditCardService.UpdateCreditCardAsync(id, model, token);
                    TempData["SuccessMessage"] = "Cartão de crédito atualizado com sucesso!";
                    return RedirectToAction(nameof(Details), new { id });
                }
                return View(model);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Erro ao atualizar cartão de crédito");
                ModelState.AddModelError(string.Empty, $"Erro ao atualizar cartão de crédito: {ex.Message}");
                return View(model);
            }
        }

        [RequirePermission("creditcards.delete")]
        public async Task<IActionResult> Delete(string id)
        {
            try
            {
                var token = HttpContext.GetJwtToken();
                var creditCard = await _creditCardService.GetCreditCardByIdAsync(id, token);
                return View(creditCard);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Erro ao carregar cartão de crédito para exclusão");
                TempData["ErrorMessage"] = $"Erro ao carregar cartão de crédito para exclusão: {ex.Message}";
                return RedirectToAction(nameof(Index));
            }
        }

        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        [RequirePermission("creditcards.delete")]
        public async Task<IActionResult> DeleteConfirmed(string id)
        {
            try
            {
                var token = HttpContext.GetJwtToken();
                await _creditCardService.DeleteCreditCardAsync(id, token);
                TempData["SuccessMessage"] = "Cartão de crédito excluído com sucesso!";
                return RedirectToAction(nameof(Index));
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Erro ao excluir cartão de crédito");
                TempData["ErrorMessage"] = $"Erro ao excluir cartão de crédito: {ex.Message}";
                return RedirectToAction(nameof(Index));
            }
        }
    }
}