using FinanceSystem.Web.Extensions;
using FinanceSystem.Web.Filters;
using FinanceSystem.Web.Interfaces;
using FinanceSystem.Web.Models.Investment;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace FinanceSystem.Web.Controllers
{
    [Authorize]
    [RequirePermission("investments.view")]
    public class InvestmentTransactionsController : Controller
    {
        private readonly IInvestmentService _investmentService;
        private readonly IInvestmentTransactionService _transactionService;
        private readonly ILogger<InvestmentTransactionsController> _logger;

        private const string ERROR_PREPARING_FORM = "Erro ao preparar formulário: {0}";
        private const string ERROR_CREATING_TRANSACTION = "Erro ao criar transação: {0}";
        private const string ERROR_DELETING_TRANSACTION = "Erro ao excluir transação: {0}";

        private const string SUCCESS_CREATE_TRANSACTION = "Transação criada com sucesso!";
        private const string SUCCESS_DELETE_TRANSACTION = "Transação excluída com sucesso!";

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
                TempData["ErrorMessage"] = string.Format(ERROR_PREPARING_FORM, ex.Message);
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
                TempData["SuccessMessage"] = SUCCESS_CREATE_TRANSACTION;
                return RedirectToAction("Details", "Investments", new { id = investmentId });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Erro ao criar transação para investimento: {InvestmentId}", investmentId);
                ModelState.AddModelError(string.Empty, string.Format(ERROR_CREATING_TRANSACTION, ex.Message));

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
                TempData["SuccessMessage"] = SUCCESS_DELETE_TRANSACTION;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Erro ao excluir transação: {Id}", id);
                TempData["ErrorMessage"] = string.Format(ERROR_DELETING_TRANSACTION, ex.Message);
            }

            return RedirectToAction("Details", "Investments", new { id = investmentId });
        }
    }
}