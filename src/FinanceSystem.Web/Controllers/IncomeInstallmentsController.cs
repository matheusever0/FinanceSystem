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

        private const string ERROR_PARENT_INCOME_NOT_FOUND = "Não foi possível identificar a receita relacionada a esta parcela.";
        private const string ERROR_MARK_RECEIVED = "Erro ao marcar parcela como recebida: {0}";
        private const string ERROR_CANCEL = "Erro ao cancelar parcela: {0}";

        private const string SUCCESS_MARK_RECEIVED = "Parcela marcada como recebida com sucesso!";
        private const string SUCCESS_CANCEL = "Parcela cancelada com sucesso!";

        public IncomeInstallmentsController(IIncomeService incomeService)
        {
            _incomeService = incomeService ?? throw new ArgumentNullException(nameof(incomeService));
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        [RequirePermission("incomes.edit")]
        public async Task<IActionResult> MarkAsReceived(string id, DateTime? receivedDate = null)
        {
            if (string.IsNullOrEmpty(id))
            {
                TempData["ErrorMessage"] = ERROR_PARENT_INCOME_NOT_FOUND;
                return RedirectToAction("Index", "Incomes");
            }

            try
            {
                var token = HttpContext.GetJwtToken();
                var incomeId = TempData["IncomeId"]?.ToString() ??
                    (await _incomeService.GetInstallmentParentIncomeAsync(id, token));

                if (string.IsNullOrEmpty(incomeId))
                {
                    TempData["ErrorMessage"] = ERROR_PARENT_INCOME_NOT_FOUND;
                    return RedirectToAction("Index", "Incomes");
                }

                await _incomeService.MarkInstallmentAsReceivedAsync(id, receivedDate ?? DateTime.Now, token);
                TempData["SuccessMessage"] = SUCCESS_MARK_RECEIVED;

                return RedirectToAction("Details", "Incomes", new { id = incomeId });
            }
            catch (Exception ex)
            {
                TempData["ErrorMessage"] = string.Format(ERROR_MARK_RECEIVED, ex.Message);
                var incomeId = TempData["IncomeId"]?.ToString();

                return !string.IsNullOrEmpty(incomeId)
                    ? RedirectToAction("Details", "Incomes", new { id = incomeId })
                    : (IActionResult)RedirectToAction("Index", "Incomes");
            }
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        [RequirePermission("incomes.edit")]
        public async Task<IActionResult> Cancel(string id)
        {
            if (string.IsNullOrEmpty(id))
            {
                TempData["ErrorMessage"] = ERROR_PARENT_INCOME_NOT_FOUND;
                return RedirectToAction("Index", "Incomes");
            }

            try
            {
                var token = HttpContext.GetJwtToken();
                var incomeId = TempData["IncomeId"]?.ToString() ??
                    (await _incomeService.GetInstallmentParentIncomeAsync(id, token));

                if (string.IsNullOrEmpty(incomeId))
                {
                    TempData["ErrorMessage"] = ERROR_PARENT_INCOME_NOT_FOUND;
                    return RedirectToAction("Index", "Incomes");
                }

                await _incomeService.CancelInstallmentAsync(id, token);
                TempData["SuccessMessage"] = SUCCESS_CANCEL;

                return RedirectToAction("Details", "Incomes", new { id = incomeId });
            }
            catch (Exception ex)
            {
                TempData["ErrorMessage"] = string.Format(ERROR_CANCEL, ex.Message);
                var incomeId = TempData["IncomeId"]?.ToString();

                return !string.IsNullOrEmpty(incomeId)
                    ? RedirectToAction("Details", "Incomes", new { id = incomeId })
                    : (IActionResult)RedirectToAction("Index", "Incomes");
            }
        }
    }
}