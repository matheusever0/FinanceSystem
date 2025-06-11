using Equilibrium.Resources.Web.Enums;
using Equilibrium.Resources.Web.Helpers;
using Equilibrium.Web.Extensions;
using Equilibrium.Web.Filters;
using Equilibrium.Web.Interfaces;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace Equilibrium.Web.Controllers
{
    [Authorize]
    [RequirePermission("incomes.view")]
    public class IncomeInstallmentsController : BaseController
    {
        private readonly IIncomeService _incomeService;

        public IncomeInstallmentsController(IIncomeService incomeService)
        {
            _incomeService = incomeService ?? throw new ArgumentNullException(nameof(incomeService));
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        [RequirePermission("incomes.edit")]
        public async Task<IActionResult> MarkAsReceived(string id, DateTime receivedDate)
        {
            if (string.IsNullOrEmpty(id))
            {
                return BadRequest("ID da parcela não fornecido");
            }

            try
            {
                var token = GetToken();

                var incomeId = TempData["IncomeId"]?.ToString() ??
                    (await _incomeService.GetInstallmentParentIncomeAsync(id, token));

                if (string.IsNullOrEmpty(incomeId))
                {
                    TempData["ErrorMessage"] = "Receita pai não encontrada.";
                    return RedirectToAction("Index", "Incomes");
                }

                // Validar data de recebimento
                if (receivedDate > DateTime.Today)
                {
                    TempData["ErrorMessage"] = "A data de recebimento não pode ser futura.";
                    return RedirectToAction("Details", "Incomes", new { id = incomeId });
                }

                await _incomeService.MarkInstallmentAsReceivedAsync(id, receivedDate, token);

                TempData["SuccessMessage"] = "Parcela marcada como recebida com sucesso.";
                return RedirectToAction("Details", "Incomes", new { id = incomeId });
            }
            catch (Exception ex)
            {
                TempData["ErrorMessage"] = $"Erro ao marcar parcela como recebida: {ex.Message}";

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
                var token = GetToken();
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