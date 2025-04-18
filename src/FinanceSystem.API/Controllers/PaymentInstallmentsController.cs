using FinanceSystem.API.Extensions;
using FinanceSystem.Application.DTOs.PaymentInstallment;
using FinanceSystem.Application.Interfaces;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System.Security.Claims;

namespace FinanceSystem.API.Controllers
{
    [Route("api/payment-installments")]
    [ApiController]
    [Authorize]
    public class PaymentInstallmentsController : ControllerBase
    {
        private readonly IPaymentInstallmentService _paymentInstallmentService;
        private readonly IPaymentService _paymentService;
        private readonly ILogger<PaymentInstallmentsController> _logger;

        public PaymentInstallmentsController(
            IPaymentInstallmentService paymentInstallmentService,
            IPaymentService paymentService,
            ILogger<PaymentInstallmentsController> logger)
        {
            _paymentInstallmentService = paymentInstallmentService;
            _paymentService = paymentService;
            _logger = logger;
        }

        [HttpGet("payment/{paymentId}")]
        public async Task<ActionResult<IEnumerable<PaymentInstallmentDto>>> GetByPayment(Guid paymentId)
        {
            _logger.LogInformation("Getting installments for payment ID: {PaymentId}", paymentId);

            try
            {
                var payment = await _paymentService.GetByIdAsync(paymentId);
                if (payment.UserId != HttpContext.GetCurrentUserId())
                {
                    _logger.LogWarning("User attempted to access installments for a payment that doesn't belong to them");
                    return Forbid();
                }

                var installments = await _paymentInstallmentService.GetByPaymentIdAsync(paymentId);
                return Ok(installments);
            }
            catch (KeyNotFoundException ex)
            {
                _logger.LogWarning(ex, "Payment not found");
                return NotFound(new { message = ex.Message });
            }
        }

        [HttpGet("due-date")]
        public async Task<ActionResult<IEnumerable<PaymentInstallmentDto>>> GetByDueDate([FromQuery] DateTime startDate, [FromQuery] DateTime endDate)
        {
            _logger.LogInformation("Getting installments between {StartDate} and {EndDate}", startDate, endDate);

            var userId = HttpContext.GetCurrentUserId();
            var installments = await _paymentInstallmentService.GetByDueDateAsync(userId, startDate, endDate);

            return Ok(installments);
        }

        [HttpGet("pending")]
        public async Task<ActionResult<IEnumerable<PaymentInstallmentDto>>> GetPending()
        {
            _logger.LogInformation("Getting pending installments");

            var userId = HttpContext.GetCurrentUserId();
            var installments = await _paymentInstallmentService.GetPendingAsync(userId);

            return Ok(installments);
        }

        [HttpGet("overdue")]
        public async Task<ActionResult<IEnumerable<PaymentInstallmentDto>>> GetOverdue()
        {
            _logger.LogInformation("Getting overdue installments");

            var userId = HttpContext.GetCurrentUserId();
            var installments = await _paymentInstallmentService.GetOverdueAsync(userId);

            return Ok(installments);
        }

        [HttpGet("{id}")]
        public async Task<ActionResult<PaymentInstallmentDto>> GetById(Guid id)
        {
            _logger.LogInformation("Getting installment with ID: {InstallmentId}", id);

            try
            {
                var installment = await _paymentInstallmentService.GetByIdAsync(id);

                var payment = await _paymentService.GetByIdAsync(installment.PaymentId);
                if (payment.UserId != HttpContext.GetCurrentUserId())
                {
                    _logger.LogWarning("User attempted to access installment that doesn't belong to their payment");
                    return Forbid();
                }

                return Ok(installment);
            }
            catch (KeyNotFoundException ex)
            {
                _logger.LogWarning(ex, "Installment not found");
                return NotFound(new { message = ex.Message });
            }
        }

        [HttpPost("{id}/paid")]
        public async Task<ActionResult<PaymentInstallmentDto>> MarkAsPaid(Guid id, [FromBody] DateTime? paymentDate)
        {
            _logger.LogInformation("Marking installment with ID: {InstallmentId} as paid", id);

            try
            {
                var installment = await _paymentInstallmentService.GetByIdAsync(id);

                var payment = await _paymentService.GetByIdAsync(installment.PaymentId);
                if (payment.UserId != HttpContext.GetCurrentUserId())
                {
                    _logger.LogWarning("User attempted to mark as paid an installment that doesn't belong to their payment");
                    return Forbid();
                }

                var date = paymentDate ?? DateTime.UtcNow;
                var updatedInstallment = await _paymentInstallmentService.MarkAsPaidAsync(id, date);
                return Ok(updatedInstallment);
            }
            catch (KeyNotFoundException ex)
            {
                _logger.LogWarning(ex, "Installment not found");
                return NotFound(new { message = ex.Message });
            }
        }

        [HttpPost("{id}/overdue")]
        public async Task<ActionResult<PaymentInstallmentDto>> MarkAsOverdue(Guid id)
        {
            _logger.LogInformation("Marking installment with ID: {InstallmentId} as overdue", id);

            try
            {
                var installment = await _paymentInstallmentService.GetByIdAsync(id);

                var payment = await _paymentService.GetByIdAsync(installment.PaymentId);
                if (payment.UserId != HttpContext.GetCurrentUserId())
                {
                    _logger.LogWarning("User attempted to mark as overdue an installment that doesn't belong to their payment");
                    return Forbid();
                }

                var updatedInstallment = await _paymentInstallmentService.MarkAsOverdueAsync(id);
                return Ok(updatedInstallment);
            }
            catch (KeyNotFoundException ex)
            {
                _logger.LogWarning(ex, "Installment not found");
                return NotFound(new { message = ex.Message });
            }
        }

        [HttpPost("{id}/cancel")]
        public async Task<ActionResult<PaymentInstallmentDto>> Cancel(Guid id)
        {
            _logger.LogInformation("Cancelling installment with ID: {InstallmentId}", id);

            try
            {
                var installment = await _paymentInstallmentService.GetByIdAsync(id);

                var payment = await _paymentService.GetByIdAsync(installment.PaymentId);
                if (payment.UserId != HttpContext.GetCurrentUserId())
                {
                    _logger.LogWarning("User attempted to cancel an installment that doesn't belong to their payment");
                    return Forbid();
                }

                var updatedInstallment = await _paymentInstallmentService.CancelAsync(id);
                return Ok(updatedInstallment);
            }
            catch (KeyNotFoundException ex)
            {
                _logger.LogWarning(ex, "Installment not found");
                return NotFound(new { message = ex.Message });
            }
        }
    }
}
