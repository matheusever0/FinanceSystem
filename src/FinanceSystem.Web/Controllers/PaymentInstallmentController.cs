using FinanceSystem.Web.Extensions;
using FinanceSystem.Web.Filters;
using FinanceSystem.Web.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace FinanceSystem.Web.Controllers
{
    [Authorize]
    [RequirePermission("payments.view")]
    public class PaymentInstallmentsController : Controller
    {
        private readonly IPaymentService _paymentService;

        private const string ERROR_PARENT_PAYMENT_NOT_FOUND = "Não foi possível identificar o pagamento relacionado a esta parcela.";
        private const string ERROR_MARK_PAID = "Erro ao marcar parcela como paga: {0}";
        private const string ERROR_MARK_OVERDUE = "Erro ao marcar parcela como vencida: {0}";
        private const string ERROR_CANCEL = "Erro ao cancelar parcela: {0}";

        private const string SUCCESS_MARK_PAID = "Parcela marcada como paga com sucesso!";
        private const string SUCCESS_MARK_OVERDUE = "Parcela marcada como vencida com sucesso!";
        private const string SUCCESS_CANCEL = "Parcela cancelada com sucesso!";

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
                var token = HttpContext.GetJwtToken();

                var paymentId = TempData["PaymentId"]?.ToString() ??
                    (await _paymentService.GetInstallmentParentPaymentAsync(id, token));

                if (string.IsNullOrEmpty(paymentId))
                {
                    TempData["ErrorMessage"] = ERROR_PARENT_PAYMENT_NOT_FOUND;
                    return RedirectToAction("Index", "Payments");
                }

                await _paymentService.MarkInstallmentAsPaidAsync(id, paymentDate ?? DateTime.Now, token);

                TempData["SuccessMessage"] = SUCCESS_MARK_PAID;
                return RedirectToAction("Details", "Payments", new { id = paymentId });
            }
            catch (Exception ex)
            {
                TempData["ErrorMessage"] = string.Format(ERROR_MARK_PAID, ex.Message);

                var paymentId = TempData["PaymentId"]?.ToString();
                if (!string.IsNullOrEmpty(paymentId))
                {
                    return RedirectToAction("Details", "Payments", new { id = paymentId });
                }

                return RedirectToAction("Index", "Payments");
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
                var token = HttpContext.GetJwtToken();

                var paymentId = TempData["PaymentId"]?.ToString() ??
                    (await _paymentService.GetInstallmentParentPaymentAsync(id, token));

                if (string.IsNullOrEmpty(paymentId))
                {
                    TempData["ErrorMessage"] = ERROR_PARENT_PAYMENT_NOT_FOUND;
                    return RedirectToAction("Index", "Payments");
                }

                await _paymentService.MarkInstallmentAsOverdueAsync(id, token);

                TempData["SuccessMessage"] = SUCCESS_MARK_OVERDUE;
                return RedirectToAction("Details", "Payments", new { id = paymentId });
            }
            catch (Exception ex)
            {
                TempData["ErrorMessage"] = string.Format(ERROR_MARK_OVERDUE, ex.Message);

                var paymentId = TempData["PaymentId"]?.ToString();
                if (!string.IsNullOrEmpty(paymentId))
                {
                    return RedirectToAction("Details", "Payments", new { id = paymentId });
                }

                return RedirectToAction("Index", "Payments");
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
                var token = HttpContext.GetJwtToken();

                var paymentId = TempData["PaymentId"]?.ToString() ??
                    (await _paymentService.GetInstallmentParentPaymentAsync(id, token));

                if (string.IsNullOrEmpty(paymentId))
                {
                    TempData["ErrorMessage"] = ERROR_PARENT_PAYMENT_NOT_FOUND;
                    return RedirectToAction("Index", "Payments");
                }

                await _paymentService.CancelInstallmentAsync(id, token);

                TempData["SuccessMessage"] = SUCCESS_CANCEL;
                return RedirectToAction("Details", "Payments", new { id = paymentId });
            }
            catch (Exception ex)
            {
                TempData["ErrorMessage"] = string.Format(ERROR_CANCEL, ex.Message);

                var paymentId = TempData["PaymentId"]?.ToString();
                if (!string.IsNullOrEmpty(paymentId))
                {
                    return RedirectToAction("Details", "Payments", new { id = paymentId });
                }

                return RedirectToAction("Index", "Payments");
            }
        }
    }
}