using FinanceSystem.API.Extensions;
using FinanceSystem.Application.DTOs.PaymentInstallment;
using FinanceSystem.Application.Interfaces;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace FinanceSystem.API.Controllers
{
    [Route("api/payment-installments")]
    [ApiController]
    [Authorize]
    public class PaymentInstallmentsController : ControllerBase
    {
        private readonly IPaymentInstallmentService _paymentInstallmentService;
        private readonly IPaymentService _paymentService;

        public PaymentInstallmentsController(
            IPaymentInstallmentService paymentInstallmentService,
            IPaymentService paymentService)
        {
            _paymentInstallmentService = paymentInstallmentService;
            _paymentService = paymentService;
        }

        [HttpGet("payment/{paymentId}")]
        public async Task<ActionResult<IEnumerable<PaymentInstallmentDto>>> GetByPayment(Guid paymentId)
        {
            try
            {
                var payment = await _paymentService.GetByIdAsync(paymentId);
                if (payment.UserId != HttpContext.GetCurrentUserId())
                {
                    return Forbid();
                }

                var installments = await _paymentInstallmentService.GetByPaymentIdAsync(paymentId);
                return Ok(installments);
            }
            catch (KeyNotFoundException ex)
            {
                return NotFound(new { message = ex.Message });
            }
        }

        [HttpGet("due-date")]
        public async Task<ActionResult<IEnumerable<PaymentInstallmentDto>>> GetByDueDate([FromQuery] DateTime startDate, [FromQuery] DateTime endDate)
        {
            var userId = HttpContext.GetCurrentUserId();
            var installments = await _paymentInstallmentService.GetByDueDateAsync(userId, startDate, endDate);

            return Ok(installments);
        }

        [HttpGet("pending")]
        public async Task<ActionResult<IEnumerable<PaymentInstallmentDto>>> GetPending()
        {
            var userId = HttpContext.GetCurrentUserId();
            var installments = await _paymentInstallmentService.GetPendingAsync(userId);

            return Ok(installments);
        }

        [HttpGet("overdue")]
        public async Task<ActionResult<IEnumerable<PaymentInstallmentDto>>> GetOverdue()
        {
            var userId = HttpContext.GetCurrentUserId();
            var installments = await _paymentInstallmentService.GetOverdueAsync(userId);

            return Ok(installments);
        }

        [HttpGet("{id}")]
        public async Task<ActionResult<PaymentInstallmentDto>> GetById(Guid id)
        {
            try
            {
                var installment = await _paymentInstallmentService.GetByIdAsync(id);

                var payment = await _paymentService.GetByIdAsync(installment.PaymentId);
                if (payment.UserId != HttpContext.GetCurrentUserId())
                {
                    return Forbid();
                }

                return Ok(installment);
            }
            catch (KeyNotFoundException ex)
            {
                return NotFound(new { message = ex.Message });
            }
        }

        [HttpPost("{id}/paid")]
        public async Task<ActionResult<PaymentInstallmentDto>> MarkAsPaid(Guid id, [FromBody] DateTime? paymentDate)
        {
            try
            {
                var installment = await _paymentInstallmentService.GetByIdAsync(id);

                var payment = await _paymentService.GetByIdAsync(installment.PaymentId);
                if (payment.UserId != HttpContext.GetCurrentUserId())
                {
                    return Forbid();
                }

                var date = paymentDate ?? DateTime.UtcNow;
                var updatedInstallment = await _paymentInstallmentService.MarkAsPaidAsync(id, date);
                return Ok(updatedInstallment);
            }
            catch (KeyNotFoundException ex)
            {
                return NotFound(new { message = ex.Message });
            }
        }

        [HttpPost("{id}/overdue")]
        public async Task<ActionResult<PaymentInstallmentDto>> MarkAsOverdue(Guid id)
        {
            try
            {
                var installment = await _paymentInstallmentService.GetByIdAsync(id);

                var payment = await _paymentService.GetByIdAsync(installment.PaymentId);
                if (payment.UserId != HttpContext.GetCurrentUserId())
                {
                    return Forbid();
                }

                var updatedInstallment = await _paymentInstallmentService.MarkAsOverdueAsync(id);
                return Ok(updatedInstallment);
            }
            catch (KeyNotFoundException ex)
            {
                return NotFound(new { message = ex.Message });
            }
        }

        [HttpPost("{id}/cancel")]
        public async Task<ActionResult<PaymentInstallmentDto>> Cancel(Guid id)
        {
            try
            {
                var installment = await _paymentInstallmentService.GetByIdAsync(id);

                var payment = await _paymentService.GetByIdAsync(installment.PaymentId);
                if (payment.UserId != HttpContext.GetCurrentUserId())
                {
                    return Forbid();
                }

                var updatedInstallment = await _paymentInstallmentService.CancelAsync(id);
                return Ok(updatedInstallment);
            }
            catch (KeyNotFoundException ex)
            {
                return NotFound(new { message = ex.Message });
            }
        }
    }
}
