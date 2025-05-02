using Equilibrium.Resources.Web;
using Equilibrium.Resources.Web.Enums;
using Equilibrium.Resources.Web.Helpers;
using Equilibrium.Web.Extensions;
using Equilibrium.Web.Filters;
using Equilibrium.Web.Interfaces;
using Equilibrium.Web.Models.Investment;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace Equilibrium.Web.Controllers
{
    [Authorize]
    [RequirePermission("investments.view")]
    public class InvestmentTransactionsController : Controller
    {
        private readonly IInvestmentService _investmentService;
        private readonly IInvestmentTransactionService _transactionService;
        private readonly ILogger<InvestmentTransactionsController> _logger;

        public InvestmentTransactionsController(
            IInvestmentService investmentService,
            IInvestmentTransactionService transactionService,
            ILogger<InvestmentTransactionsController> logger)
        {
            _investmentService = investmentService ?? throw new ArgumentNullException(nameof(investmentService));
            _transactionService = transactionService ?? throw new ArgumentNullException(nameof(transactionService));
            _logger = logger ?? throw new ArgumentNullException(nameof(logger));
        }

        [RequirePermission("investments.transactions.create")]
        public async Task<IActionResult> Create(string investmentId)
        {
            if (string.IsNullOrEmpty(investmentId))
            {
                return BadRequest("ID do investimento não fornecido");
            }

            try
            {
                var token = HttpContext.GetJwtToken();
                var investment = await _investmentService.GetInvestmentByIdAsync(investmentId, token);

                if (investment == null)
                {
                    return NotFound("Investimento não encontrado");
                }

                ViewBag.Investment = investment;

                var model = new CreateInvestmentTransactionModel
                {
                    Date = DateTime.Now,
                    Type = investment.Type
                };

                return View(model);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Erro ao preparar formulário de criação de transação para investimento: {InvestmentId}", investmentId);
                TempData["ErrorMessage"] = ResourceFinanceWeb.Error_PreparingForm;
                return RedirectToAction("Details", "Investments", new { id = investmentId });
            }
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        [RequirePermission("investments.transactions.create")]
        public async Task<IActionResult> Create(string investmentId, CreateInvestmentTransactionModel model)
        {
            if (string.IsNullOrEmpty(investmentId))
            {
                return BadRequest("ID do investimento não fornecido");
            }

            if (!ModelState.IsValid)
            {
                try
                {
                    var token = HttpContext.GetJwtToken();
                    var investment = await _investmentService.GetInvestmentByIdAsync(investmentId, token);
                    ViewBag.Investment = investment;
                }
                catch (Exception ex)
                {
                    _logger.LogError(ex, "Erro ao carregar investimento para exibição de formulário com erros: {InvestmentId}", investmentId);
                }

                return View(model);
            }

            try
            {
                var token = HttpContext.GetJwtToken();
                await _transactionService.CreateTransactionAsync(investmentId, model, token);
                TempData["SuccessMessage"] = MessageHelper.GetCreationSuccessMessage(EntityNames.Transaction);
                return RedirectToAction("Details", "Investments", new { id = investmentId });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Erro ao criar transação para investimento: {InvestmentId}", investmentId);
                ModelState.AddModelError(string.Empty, MessageHelper.GetCreationErrorMessage(EntityNames.Transaction, ex));

                try
                {
                    var token = HttpContext.GetJwtToken();
                    var investment = await _investmentService.GetInvestmentByIdAsync(investmentId, token);
                    ViewBag.Investment = investment;
                }
                catch (Exception innerEx)
                {
                    _logger.LogError(innerEx, "Erro ao carregar investimento para exibição de formulário com erros: {InvestmentId}", investmentId);
                }

                return View(model);
            }
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        [RequirePermission("investments.transactions.delete")]
        public async Task<IActionResult> Delete(string id, string investmentId)
        {
            if (string.IsNullOrEmpty(id))
            {
                return BadRequest("ID da transação não fornecido");
            }

            if (string.IsNullOrEmpty(investmentId))
            {
                return BadRequest("ID do investimento não fornecido");
            }

            try
            {
                var token = HttpContext.GetJwtToken();
                await _transactionService.DeleteTransactionAsync(id, token);
                TempData["SuccessMessage"] = MessageHelper.GetDeletionSuccessMessage(EntityNames.Transaction);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Erro ao excluir transação: {Id}", id);
                TempData["ErrorMessage"] = MessageHelper.GetDeletionErrorMessage(EntityNames.Transaction, ex);
            }

            return RedirectToAction("Details", "Investments", new { id = investmentId });
        }
    }
}