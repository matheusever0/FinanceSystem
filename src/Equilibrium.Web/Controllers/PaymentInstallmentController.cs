using Equilibrium.Resources.Web.Enums;
using Equilibrium.Resources.Web.Helpers;
using Equilibrium.Web.Filters;
using Equilibrium.Web.Interfaces;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace Equilibrium.Web.Controllers
{
    [Authorize]
    [RequirePermission("payments.view")]
    public class PaymentInstallmentsController : BaseController
    {
        private readonly IPaymentService _paymentService;

        public PaymentInstallmentsController(IPaymentService paymentService)
        {
            _paymentService = paymentService ?? throw new ArgumentNullException(nameof(paymentService));
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        [RequirePermission("payments.edit")]
        public async Task<IActionResult> MarkAsPaid(string id, DateTime? paymentDate = null)
        {
            if (string.IsNullOrEmpty(id))
            {
                return BadRequest("ID da parcela não fornecido");
            }

            try
            {
                var token = GetToken();

                var paymentId = TempData["PaymentId"]?.ToString() ??
                    (await _paymentService.GetInstallmentParentPaymentAsync(id, token));

                if (string.IsNullOrEmpty(paymentId))
                {
                    TempData["ErrorMessage"] = MessageHelper.GetParentEntityNotFoundMessage(EntityNames.Payment);
                    return RedirectToAction("Index", "Payments");
                }

                await _paymentService.MarkInstallmentAsPaidAsync(id, paymentDate ?? DateTime.Now, token);

                TempData["SuccessMessage"] = MessageHelper.GetStatusChangeSuccessMessage(EntityNames.PaymentInstallment, EntityStatus.Paid);
                return RedirectToAction("Details", "Payments", new { id = paymentId });
            }
            catch (Exception ex)
            {
                TempData["ErrorMessage"] = MessageHelper.GetStatusChangeErrorMessage(EntityNames.PaymentInstallment, EntityStatus.Paid, ex);

                var paymentId = TempData["PaymentId"]?.ToString();
                return !string.IsNullOrEmpty(paymentId)
                    ? RedirectToAction("Details", "Payments", new { id = paymentId })
                    : (IActionResult)RedirectToAction("Index", "Payments");
            }
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        [RequirePermission("payments.edit")]
        public async Task<IActionResult> MarkAsOverdue(string id)
        {
            if (string.IsNullOrEmpty(id))
            {
                return BadRequest("ID da parcela não fornecido");
            }

            try
            {
                var token = GetToken();

                var paymentId = TempData["PaymentId"]?.ToString() ??
                    (await _paymentService.GetInstallmentParentPaymentAsync(id, token));

                if (string.IsNullOrEmpty(paymentId))
                {
                    TempData["ErrorMessage"] = MessageHelper.GetParentEntityNotFoundMessage(EntityNames.Payment);
                    return RedirectToAction("Index", "Payments");
                }

                await _paymentService.MarkInstallmentAsOverdueAsync(id, token);

                TempData["SuccessMessage"] = MessageHelper.GetStatusChangeSuccessMessage(EntityNames.PaymentInstallment, EntityStatus.Overdue);
                return RedirectToAction("Details", "Payments", new { id = paymentId });
            }
            catch (Exception ex)
            {
                TempData["ErrorMessage"] = MessageHelper.GetStatusChangeErrorMessage(EntityNames.PaymentInstallment, EntityStatus.Overdue, ex);

                var paymentId = TempData["PaymentId"]?.ToString();
                return !string.IsNullOrEmpty(paymentId)
                    ? RedirectToAction("Details", "Payments", new { id = paymentId })
                    : (IActionResult)RedirectToAction("Index", "Payments");
            }
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        [RequirePermission("payments.edit")]
        public async Task<IActionResult> Cancel(string id)
        {
            if (string.IsNullOrEmpty(id))
            {
                return BadRequest("ID da parcela não fornecido");
            }

            try
            {
                var token = GetToken();

                var paymentId = TempData["PaymentId"]?.ToString() ??
                    (await _paymentService.GetInstallmentParentPaymentAsync(id, token));

                if (string.IsNullOrEmpty(paymentId))
                {
                    TempData["ErrorMessage"] = MessageHelper.GetParentEntityNotFoundMessage(EntityNames.Payment);
                    return RedirectToAction("Index", "Payments");
                }

                await _paymentService.CancelInstallmentAsync(id, token);

                TempData["SuccessMessage"] = MessageHelper.GetCancelSuccessMessage(EntityNames.PaymentInstallment);
                return RedirectToAction("Details", "Payments", new { id = paymentId });
            }
            catch (Exception ex)
            {
                TempData["ErrorMessage"] = MessageHelper.GetCancelErrorMessage(EntityNames.PaymentInstallment, ex);

                var paymentId = TempData["PaymentId"]?.ToString();
                return !string.IsNullOrEmpty(paymentId)
                    ? RedirectToAction("Details", "Payments", new { id = paymentId })
                    : (IActionResult)RedirectToAction("Index", "Payments");
            }
        }
    }
}