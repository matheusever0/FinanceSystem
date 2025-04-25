using FinanceSystem.Resources.Web.Enums;
using FinanceSystem.Resources.Web.Helpers;
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
                TempData["ErrorMessage"] = MessageHelper.GetParentEntityNotFoundMessage(EntityNames.Income);
                return RedirectToAction("Index", "Incomes");
            }

            try
            {
                var token = HttpContext.GetJwtToken();
                var incomeId = TempData["IncomeId"]?.ToString() ??
                    (await _incomeService.GetInstallmentParentIncomeAsync(id, token));

                if (string.IsNullOrEmpty(incomeId))
                {
                    TempData["ErrorMessage"] = MessageHelper.GetParentEntityNotFoundMessage(EntityNames.Income);
                    return RedirectToAction("Index", "Incomes");
                }

                await _incomeService.MarkInstallmentAsReceivedAsync(id, receivedDate ?? DateTime.Now, token);
                TempData["SuccessMessage"] = MessageHelper.GetStatusChangeSuccessMessage(EntityNames.IncomeInstallment, EntityStatus.Received);

                return RedirectToAction("Details", "Incomes", new { id = incomeId });
            }
            catch (Exception ex)
            {
                TempData["ErrorMessage"] = MessageHelper.GetStatusChangeErrorMessage(EntityNames.IncomeInstallment, EntityStatus.Received, ex);
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
                TempData["ErrorMessage"] = MessageHelper.GetParentEntityNotFoundMessage(EntityNames.Income);
                return RedirectToAction("Index", "Incomes");
            }

            try
            {
                var token = HttpContext.GetJwtToken();
                var incomeId = TempData["IncomeId"]?.ToString() ??
                    (await _incomeService.GetInstallmentParentIncomeAsync(id, token));

                if (string.IsNullOrEmpty(incomeId))
                {
                    TempData["ErrorMessage"] = MessageHelper.GetParentEntityNotFoundMessage(EntityNames.Income);
                    return RedirectToAction("Index", "Incomes");
                }

                await _incomeService.CancelInstallmentAsync(id, token);
                TempData["SuccessMessage"] = MessageHelper.GetCancelSuccessMessage(EntityNames.IncomeInstallment);

                return RedirectToAction("Details", "Incomes", new { id = incomeId });
            }
            catch (Exception ex)
            {
                TempData["ErrorMessage"] = MessageHelper.GetCancelErrorMessage(EntityNames.IncomeInstallment, ex);
                var incomeId = TempData["IncomeId"]?.ToString();

                return !string.IsNullOrEmpty(incomeId)
                    ? RedirectToAction("Details", "Incomes", new { id = incomeId })
                    : (IActionResult)RedirectToAction("Index", "Incomes");
            }
        }
    }
}