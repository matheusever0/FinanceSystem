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
    public class InvestmentsController : Controller
    {
        private readonly IInvestmentService _investmentService;
        private readonly IInvestmentTransactionService _investmentTransactionService;
        private readonly ILogger<InvestmentsController> _logger;

        private const string ERROR_LOADING_INVESTMENTS = "Erro ao carregar investimentos: {0}";
        private const string ERROR_LOADING_INVESTMENT_DETAILS = "Erro ao carregar detalhes do investimento: {0}";
        private const string ERROR_PREPARING_FORM = "Erro ao preparar formulário: {0}";
        private const string ERROR_CREATING_INVESTMENT = "Erro ao criar investimento: {0}";
        private const string ERROR_UPDATING_INVESTMENT = "Erro ao atualizar investimento: {0}";
        private const string ERROR_DELETING_INVESTMENT = "Erro ao excluir investimento: {0}";
        private const string ERROR_REFRESHING_PRICE = "Erro ao atualizar preço: {0}";

        private const string SUCCESS_CREATE_INVESTMENT = "Investimento criado com sucesso!";
        private const string SUCCESS_UPDATE_INVESTMENT = "Investimento atualizado com sucesso!";
        private const string SUCCESS_DELETE_INVESTMENT = "Investimento excluído com sucesso!";
        private const string SUCCESS_REFRESH_PRICE = "Preço atualizado com sucesso!";

        public InvestmentsController(
            IInvestmentService investmentService,
            IInvestmentTransactionService investmentTransactionService,
            ILogger<InvestmentsController> logger)
        {
            _investmentService = investmentService ?? throw new ArgumentNullException(nameof(investmentService));
            _investmentTransactionService = investmentTransactionService ?? throw new ArgumentNullException(nameof(investmentTransactionService));
            _logger = logger ?? throw new ArgumentNullException(nameof(logger));
        }

        public async Task<IActionResult> Index()
        {
            try
            {
                var token = HttpContext.GetJwtToken();
                var investments = await _investmentService.GetAllInvestmentsAsync(token);
                return View(investments);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Erro ao carregar investimentos");
                TempData["ErrorMessage"] = string.Format(ERROR_LOADING_INVESTMENTS, ex.Message);
                return RedirectToAction("Index", "Home");
            }
        }

        public async Task<IActionResult> ByType(int type)
        {
            try
            {
                var token = HttpContext.GetJwtToken();
                var investments = await _investmentService.GetInvestmentsByTypeAsync(type, token);

                string typeDescription = type switch
                {
                    1 => "Ações",
                    2 => "Fundos Imobiliários",
                    3 => "ETFs",
                    4 => "Ações Estrangeiras",
                    5 => "Renda Fixa",
                    _ => "Não Categorizado"
                };

                ViewBag.Title = $"Investimentos por Tipo: {typeDescription}";
                ViewBag.Type = type;

                return View("Index", investments);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Erro ao carregar investimentos por tipo: {Type}", type);
                TempData["ErrorMessage"] = string.Format(ERROR_LOADING_INVESTMENTS, ex.Message);
                return RedirectToAction("Index", "Home");
            }
        }

        public async Task<IActionResult> Details(string id)
        {
            if (string.IsNullOrEmpty(id))
            {
                return BadRequest("ID do investimento não fornecido");
            }

            try
            {
                var token = HttpContext.GetJwtToken();
                var investment = await _investmentService.GetInvestmentByIdAsync(id, token);

                if (investment == null)
                {
                    return NotFound("Investimento não encontrado");
                }

                // Carregar transações para este investimento
                var transactions = await _investmentTransactionService.GetTransactionsByInvestmentIdAsync(id, token);
                investment.Transactions = [.. transactions];

                return View(investment);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Erro ao carregar detalhes do investimento: {Id}", id);
                TempData["ErrorMessage"] = string.Format(ERROR_LOADING_INVESTMENT_DETAILS, ex.Message);
                return RedirectToAction(nameof(Index));
            }
        }

        [RequirePermission("investments.create")]
        public IActionResult Create()
        {
            try
            {
                var model = new CreateInvestmentModel
                {
                    TransactionDate = DateTime.Now
                };

                return View(model);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Erro ao preparar formulário de criação de investimento");
                TempData["ErrorMessage"] = string.Format(ERROR_PREPARING_FORM, ex.Message);
                return RedirectToAction(nameof(Index));
            }
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        [RequirePermission("investments.create")]
        public async Task<IActionResult> Create(CreateInvestmentModel model)
        {
            if (!ModelState.IsValid)
            {
                return View(model);
            }

            try
            {
                var token = HttpContext.GetJwtToken();
                var investment = await _investmentService.CreateInvestmentAsync(model, token);
                TempData["SuccessMessage"] = SUCCESS_CREATE_INVESTMENT;
                return RedirectToAction(nameof(Details), new { id = investment.Id });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Erro ao criar investimento");
                ModelState.AddModelError(string.Empty, string.Format(ERROR_CREATING_INVESTMENT, ex.Message));
                return View(model);
            }
        }

        [RequirePermission("investments.edit")]
        public async Task<IActionResult> Edit(string id)
        {
            if (string.IsNullOrEmpty(id))
            {
                return BadRequest("ID do investimento não fornecido");
            }

            try
            {
                var token = HttpContext.GetJwtToken();
                var investment = await _investmentService.GetInvestmentByIdAsync(id, token);

                if (investment == null)
                {
                    return NotFound("Investimento não encontrado");
                }

                var model = new UpdateInvestmentModel
                {
                    Name = investment.Name
                };

                ViewBag.InvestmentId = id;
                ViewBag.Symbol = investment.Symbol;
                ViewBag.CurrentName = investment.Name;

                return View(model);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Erro ao carregar investimento para edição: {Id}", id);
                TempData["ErrorMessage"] = string.Format(ERROR_LOADING_INVESTMENT_DETAILS, ex.Message);
                return RedirectToAction(nameof(Index));
            }
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        [RequirePermission("investments.edit")]
        public async Task<IActionResult> Edit(string id, UpdateInvestmentModel model)
        {
            if (string.IsNullOrEmpty(id))
            {
                return BadRequest("ID do investimento não fornecido");
            }

            if (!ModelState.IsValid)
            {
                return View(model);
            }

            try
            {
                var token = HttpContext.GetJwtToken();
                await _investmentService.UpdateInvestmentAsync(id, model, token);
                TempData["SuccessMessage"] = SUCCESS_UPDATE_INVESTMENT;
                return RedirectToAction(nameof(Details), new { id });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Erro ao atualizar investimento: {Id}", id);
                ModelState.AddModelError(string.Empty, string.Format(ERROR_UPDATING_INVESTMENT, ex.Message));
                return View(model);
            }
        }

        [RequirePermission("investments.delete")]
        public async Task<IActionResult> Delete(string id)
        {
            if (string.IsNullOrEmpty(id))
            {
                return BadRequest("ID do investimento não fornecido");
            }

            try
            {
                var token = HttpContext.GetJwtToken();
                var investment = await _investmentService.GetInvestmentByIdAsync(id, token);

                return investment == null ? NotFound("Investimento não encontrado") : View(investment);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Erro ao carregar investimento para exclusão: {Id}", id);
                TempData["ErrorMessage"] = string.Format(ERROR_LOADING_INVESTMENT_DETAILS, ex.Message);
                return RedirectToAction(nameof(Index));
            }
        }

        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        [RequirePermission("investments.delete")]
        public async Task<IActionResult> DeleteConfirmed(string id)
        {
            if (string.IsNullOrEmpty(id))
            {
                return BadRequest("ID do investimento não fornecido");
            }

            try
            {
                var token = HttpContext.GetJwtToken();
                await _investmentService.DeleteInvestmentAsync(id, token);
                TempData["SuccessMessage"] = SUCCESS_DELETE_INVESTMENT;
                return RedirectToAction(nameof(Index));
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Erro ao excluir investimento: {Id}", id);
                TempData["ErrorMessage"] = string.Format(ERROR_DELETING_INVESTMENT, ex.Message);
                return RedirectToAction(nameof(Index));
            }
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        [RequirePermission("investments.edit")]
        public async Task<IActionResult> RefreshPrice(string id)
        {
            if (string.IsNullOrEmpty(id))
            {
                return BadRequest("ID do investimento não fornecido");
            }

            try
            {
                var token = HttpContext.GetJwtToken();
                await _investmentService.RefreshPriceAsync(id, token);
                TempData["SuccessMessage"] = SUCCESS_REFRESH_PRICE;
                return RedirectToAction(nameof(Details), new { id });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Erro ao atualizar preço do investimento: {Id}", id);
                TempData["ErrorMessage"] = string.Format(ERROR_REFRESHING_PRICE, ex.Message);
                return RedirectToAction(nameof(Details), new { id });
            }
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        [RequirePermission("investments.edit")]
        public async Task<IActionResult> RefreshAllPrices()
        {
            try
            {
                var token = HttpContext.GetJwtToken();
                await _investmentService.RefreshAllPricesAsync(token);
                TempData["SuccessMessage"] = "Todos os preços foram atualizados com sucesso!";
                return RedirectToAction(nameof(Index));
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Erro ao atualizar preços de todos os investimentos");
                TempData["ErrorMessage"] = string.Format(ERROR_REFRESHING_PRICE, ex.Message);
                return RedirectToAction(nameof(Index));
            }
        }
    }
}