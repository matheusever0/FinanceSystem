using FinanceSystem.Web.Extensions;
using FinanceSystem.Web.Filters;
using FinanceSystem.Web.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace FinanceSystem.Web.Controllers
{
    [Authorize]
    [RequirePermission("incomes.view")]
    public class IncomeInstallmentsController : Controller
    {
        private readonly IIncomeService _incomeService;
        private readonly ILogger<IncomeInstallmentsController> _logger;

        public IncomeInstallmentsController(
            IIncomeService incomeService,
            ILogger<IncomeInstallmentsController> logger)
        {
            _incomeService = incomeService;
            _logger = logger;
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        [RequirePermission("incomes.edit")]
        [Route("{id}/MarkAsReceived")]
        public async Task<IActionResult> MarkAsReceived(string id, DateTime? receivedDate = null)
        {
            try
            {
                var token = HttpContext.GetJwtToken();

                var incomeId = TempData["IncomeId"]?.ToString() ??
                    (await _incomeService.GetInstallmentParentIncomeAsync(id, token));

                if (string.IsNullOrEmpty(incomeId))
                {
                    TempData["ErrorMessage"] = "Não foi possível identificar a receita relacionada a esta parcela.";
                    return RedirectToAction("Index", "Incomes");
                }

                await _incomeService.MarkInstallmentAsReceivedAsync(id, receivedDate ?? DateTime.Now, token);

                TempData["SuccessMessage"] = "Parcela marcada como recebida com sucesso!";
                return RedirectToAction("Details", "Incomes", new { id = incomeId });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Erro ao marcar parcela como recebida");
                TempData["ErrorMessage"] = $"Erro ao marcar parcela como recebida: {ex.Message}";

                var incomeId = TempData["IncomeId"]?.ToString();
                if (!string.IsNullOrEmpty(incomeId))
                {
                    return RedirectToAction("Details", "Incomes", new { id = incomeId });
                }

                return RedirectToAction("Index", "Incomes");
            }
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        [RequirePermission("incomes.edit")]
        [Route("{id}/Cancel")]
        public async Task<IActionResult> Cancel(string id)
        {
            try
            {
                var token = HttpContext.GetJwtToken();

                var incomeId = TempData["IncomeId"]?.ToString() ??
                    (await _incomeService.GetInstallmentParentIncomeAsync(id, token));

                if (string.IsNullOrEmpty(incomeId))
                {
                    TempData["ErrorMessage"] = "Não foi possível identificar a receita relacionada a esta parcela.";
                    return RedirectToAction("Index", "Incomes");
                }

                await _incomeService.CancelInstallmentAsync(id, token);

                TempData["SuccessMessage"] = "Parcela cancelada com sucesso!";
                return RedirectToAction("Details", "Incomes", new { id = incomeId });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Erro ao cancelar parcela");
                TempData["ErrorMessage"] = $"Erro ao cancelar parcela: {ex.Message}";

                var incomeId = TempData["IncomeId"]?.ToString();
                if (!string.IsNullOrEmpty(incomeId))
                {
                    return RedirectToAction("Details", "Incomes", new { id = incomeId });
                }

                return RedirectToAction("Index", "Incomes");
            }
        }
    }
}