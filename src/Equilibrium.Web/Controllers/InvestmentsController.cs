using Equilibrium.Resources.Web;
using Equilibrium.Resources.Web.Enums;
using Equilibrium.Resources.Web.Helpers;
using Equilibrium.Web.Extensions;
using Equilibrium.Web.Filters;
using Equilibrium.Web.Interfaces;
using Equilibrium.Web.Models.Filters;
using Equilibrium.Web.Models.Investment;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace Equilibrium.Web.Controllers
{
    [Authorize]
    [RequirePermission("investments.view")]
    public class InvestmentsController : Controller
    {
        private readonly IInvestmentService _investmentService;
        private readonly IInvestmentTransactionService _investmentTransactionService;
        private readonly ILogger<InvestmentsController> _logger;

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
                TempData["ErrorMessage"] = MessageHelper.GetLoadingErrorMessage(EntityNames.Investment, ex);
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
                TempData["ErrorMessage"] = MessageHelper.GetLoadingErrorMessage(EntityNames.Investment, ex);
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
                TempData["ErrorMessage"] = MessageHelper.GetLoadingErrorMessage(EntityNames.Investment, ex);
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
                TempData["ErrorMessage"] = ResourceFinanceWeb.Error_PreparingForm;
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
                TempData["SuccessMessage"] = MessageHelper.GetCreationSuccessMessage(EntityNames.Investment);
                return RedirectToAction(nameof(Details), new { id = investment.Id });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Erro ao criar investimento");
                ModelState.AddModelError(string.Empty, MessageHelper.GetCreationErrorMessage(EntityNames.Investment, ex));
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
                TempData["ErrorMessage"] = MessageHelper.GetLoadingErrorMessage(EntityNames.Investment, ex);
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
                TempData["SuccessMessage"] = MessageHelper.GetDeletionSuccessMessage(EntityNames.Investment);
                return RedirectToAction(nameof(Index));
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Erro ao excluir investimento: {Id}", id);
                TempData["ErrorMessage"] = MessageHelper.GetDeletionErrorMessage(EntityNames.Investment, ex);
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
                TempData["SuccessMessage"] = "Preço atualizado com sucesso!";
                return RedirectToAction(nameof(Details), new { id });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Erro ao atualizar preço do investimento: {Id}", id);
                TempData["ErrorMessage"] = ResourceFinanceWeb.Error_RefreshingPrice;
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
                TempData["ErrorMessage"] = ResourceFinanceWeb.Error_RefreshingPrice;
                return RedirectToAction(nameof(Index));
            }
        }
    }

    [HttpGet("filter")]
    [RequirePermission("investments.view")]
    public async Task<IActionResult> Filter(InvestmentFilter filter = null)
    {
        if (filter == null)
            filter = new InvestmentFilter();
        
        try
        {
            var token = HttpContext.GetJwtToken();
            var result = await _investmentService.GetFilteredAsync(filter, token);
            
            // Add pagination headers
            Response.Headers.Add("X-Pagination-Total", result.TotalCount.ToString());
            Response.Headers.Add("X-Pagination-Pages", result.TotalPages.ToString());
            Response.Headers.Add("X-Pagination-Page", result.PageNumber.ToString());
            Response.Headers.Add("X-Pagination-Size", result.PageSize.ToString());
            
            ViewBag.Filter = filter;
            ViewBag.TotalCount = result.TotalCount;
            ViewBag.TotalPages = result.TotalPages;
            ViewBag.CurrentPage = result.PageNumber;
            ViewBag.PageSize = result.PageSize;
            
            return View("Index", result.Items);
        }
        catch (Exception ex)
        {
            TempData["ErrorMessage"] = MessageHelper.GetLoadingErrorMessage(EntityNames.Investment, ex);
            return RedirectToAction("Index", "Home");
        }
    }

    [HttpGet("api/filter")]
    [RequirePermission("investments.view")]
    public async Task<IActionResult> FilterJson([FromQuery] InvestmentFilter filter)
    {
        if (filter == null)
            filter = new InvestmentFilter();
            
        try
        {
            var token = HttpContext.GetJwtToken();
            var result = await _investmentService.GetFilteredAsync(filter, token);
            
            return Json(new { 
                items = result.Items, 
                totalCount = result.TotalCount,
                pageNumber = result.PageNumber,
                pageSize = result.PageSize,
                totalPages = result.TotalPages,
                hasPreviousPage = result.HasPreviousPage,
                hasNextPage = result.HasNextPage
            });
        }
        catch (Exception ex)
        {
            return BadRequest(new { error = ex.Message });
        }
    }}
