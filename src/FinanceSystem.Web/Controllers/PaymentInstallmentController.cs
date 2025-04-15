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
        private readonly ILogger<PaymentInstallmentsController> _logger;

        public PaymentInstallmentsController(
            IPaymentService paymentService,
            ILogger<PaymentInstallmentsController> logger)
        {
            _paymentService = paymentService;
            _logger = logger;
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        [RequirePermission("payments.edit")]
        [Route("{id}/MarkAsPaid")]
        public async Task<IActionResult> MarkAsPaid(string id, DateTime? paymentDate = null)
        {
            try
            {
                var token = HttpContext.GetJwtToken();

                // Aqui usaria um serviço de instalação de pagamento, mas como não foi implementado,
                // usamos o paymentService para redirecionar ao pagamento principal
                var paymentId = TempData["PaymentId"]?.ToString() ??
                    (await _paymentService.GetInstallmentParentPaymentAsync(id, token));

                if (string.IsNullOrEmpty(paymentId))
                {
                    TempData["ErrorMessage"] = "Não foi possível identificar o pagamento relacionado a esta parcela.";
                    return RedirectToAction("Index", "Payments");
                }

                await _paymentService.MarkInstallmentAsPaidAsync(id, paymentDate ?? DateTime.Now, token);

                TempData["SuccessMessage"] = "Parcela marcada como paga com sucesso!";
                return RedirectToAction("Details", "Payments", new { id = paymentId });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Erro ao marcar parcela como paga");
                TempData["ErrorMessage"] = $"Erro ao marcar parcela como paga: {ex.Message}";

                // Tentar retornar para o pagamento principal
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
        [Route("{id}/MarkAsOverdue")]
        public async Task<IActionResult> MarkAsOverdue(string id)
        {
            try
            {
                var token = HttpContext.GetJwtToken();

                // Aqui usaria um serviço de instalação de pagamento, mas como não foi implementado,
                // usamos o paymentService para redirecionar ao pagamento principal
                var paymentId = TempData["PaymentId"]?.ToString() ??
                    (await _paymentService.GetInstallmentParentPaymentAsync(id, token));

                if (string.IsNullOrEmpty(paymentId))
                {
                    TempData["ErrorMessage"] = "Não foi possível identificar o pagamento relacionado a esta parcela.";
                    return RedirectToAction("Index", "Payments");
                }

                await _paymentService.MarkInstallmentAsOverdueAsync(id, token);

                TempData["SuccessMessage"] = "Parcela marcada como vencida com sucesso!";
                return RedirectToAction("Details", "Payments", new { id = paymentId });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Erro ao marcar parcela como vencida");
                TempData["ErrorMessage"] = $"Erro ao marcar parcela como vencida: {ex.Message}";

                // Tentar retornar para o pagamento principal
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
        [Route("{id}/Cancel")]
        public async Task<IActionResult> Cancel(string id)
        {
            try
            {
                var token = HttpContext.GetJwtToken();

                // Aqui usaria um serviço de instalação de pagamento, mas como não foi implementado,
                // usamos o paymentService para redirecionar ao pagamento principal
                var paymentId = TempData["PaymentId"]?.ToString() ??
                    (await _paymentService.GetInstallmentParentPaymentAsync(id, token));

                if (string.IsNullOrEmpty(paymentId))
                {
                    TempData["ErrorMessage"] = "Não foi possível identificar o pagamento relacionado a esta parcela.";
                    return RedirectToAction("Index", "Payments");
                }

                await _paymentService.CancelInstallmentAsync(id, token);

                TempData["SuccessMessage"] = "Parcela cancelada com sucesso!";
                return RedirectToAction("Details", "Payments", new { id = paymentId });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Erro ao cancelar parcela");
                TempData["ErrorMessage"] = $"Erro ao cancelar parcela: {ex.Message}";

                // Tentar retornar para o pagamento principal
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