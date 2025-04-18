using FinanceSystem.API.Extensions;
using FinanceSystem.Application.DTOs.Payment;
using FinanceSystem.Application.Interfaces;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System.Security.Claims;

namespace FinanceSystem.API.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    [Authorize]
    public class PaymentsController : ControllerBase
    {
        private readonly IPaymentService _paymentService;
        private readonly ILogger<PaymentsController> _logger;

        public PaymentsController(IPaymentService paymentService, ILogger<PaymentsController> logger)
        {
            _paymentService = paymentService;
            _logger = logger;
        }

        [HttpGet]
        public async Task<ActionResult<IEnumerable<PaymentDto>>> GetAll()
        {
            _logger.LogInformation("Getting all payments for user");

            var userId = HttpContext.GetCurrentUserId();
            var payments = await _paymentService.GetAllByUserIdAsync(userId);

            return Ok(payments);
        }

        [HttpGet("{id}")]
        public async Task<ActionResult<PaymentDto>> GetById(Guid id)
        {
            _logger.LogInformation("Getting payment with ID: {PaymentId}", id);

            try
            {
                var payment = await _paymentService.GetByIdAsync(id);

                if (payment.UserId != HttpContext.GetCurrentUserId())
                {
                    _logger.LogWarning("User attempted to access payment that doesn't belong to them");
                    return Forbid();
                }

                return Ok(payment);
            }
            catch (KeyNotFoundException ex)
            {
                _logger.LogWarning(ex, "Payment not found");
                return NotFound(new { message = ex.Message });
            }
        }

        [HttpGet("month/{year}/{month}")]
        public async Task<ActionResult<IEnumerable<PaymentDto>>> GetByMonth(int year, int month)
        {
            _logger.LogInformation("Getting payments for {Month}/{Year}", month, year);

            if (month < 1 || month > 12)
            {
                return BadRequest(new { message = "Month must be between 1 and 12" });
            }

            var userId = HttpContext.GetCurrentUserId();
            var payments = await _paymentService.GetByMonthAsync(userId, month, year);

            return Ok(payments);
        }

        [HttpGet("pending")]
        public async Task<ActionResult<IEnumerable<PaymentDto>>> GetPending()
        {
            _logger.LogInformation("Getting pending payments");

            var userId = HttpContext.GetCurrentUserId();
            var payments = await _paymentService.GetPendingAsync(userId);

            return Ok(payments);
        }

        [HttpGet("overdue")]
        public async Task<ActionResult<IEnumerable<PaymentDto>>> GetOverdue()
        {
            _logger.LogInformation("Getting overdue payments");

            var userId = HttpContext.GetCurrentUserId();
            var payments = await _paymentService.GetOverdueAsync(userId);

            return Ok(payments);
        }

        [HttpGet("type/{typeId}")]
        public async Task<ActionResult<IEnumerable<PaymentDto>>> GetByType(Guid typeId)
        {
            _logger.LogInformation("Getting payments by type ID: {TypeId}", typeId);

            var userId = HttpContext.GetCurrentUserId();
            var payments = await _paymentService.GetByTypeAsync(userId, typeId);

            return Ok(payments);
        }

        [HttpGet("method/{methodId}")]
        public async Task<ActionResult<IEnumerable<PaymentDto>>> GetByMethod(Guid methodId)
        {
            _logger.LogInformation("Getting payments by method ID: {MethodId}", methodId);

            var userId = HttpContext.GetCurrentUserId();
            var payments = await _paymentService.GetByMethodAsync(userId, methodId);

            return Ok(payments);
        }

        [HttpPost]
        public async Task<ActionResult<PaymentDto>> Create(CreatePaymentDto createPaymentDto)
        {
            _logger.LogInformation("Creating new payment");

            try
            {
                var userId = HttpContext.GetCurrentUserId();
                var payment = await _paymentService.CreateAsync(createPaymentDto, userId);

                return CreatedAtAction(nameof(GetById), new { id = payment.Id }, payment);
            }
            catch (KeyNotFoundException ex)
            {
                _logger.LogWarning(ex, "Failed to create payment due to missing referenced entity");
                return NotFound(new { message = ex.Message });
            }
            catch (UnauthorizedAccessException ex)
            {
                _logger.LogWarning(ex, "Unauthorized access when creating payment");
                return Forbid();
            }
            catch (InvalidOperationException ex)
            {
                _logger.LogWarning(ex, "Invalid operation when creating payment");
                return BadRequest(new { message = ex.Message });
            }
        }

        [HttpPut("{id}")]
        public async Task<ActionResult<PaymentDto>> Update(Guid id, UpdatePaymentDto updatePaymentDto)
        {
            _logger.LogInformation("Updating payment with ID: {PaymentId}", id);

            try
            {
                var existingPayment = await _paymentService.GetByIdAsync(id);
                if (existingPayment.UserId != HttpContext.GetCurrentUserId())
                {
                    _logger.LogWarning("User attempted to update payment that doesn't belong to them");
                    return Forbid();
                }

                var payment = await _paymentService.UpdateAsync(id, updatePaymentDto);
                return Ok(payment);
            }
            catch (KeyNotFoundException ex)
            {
                _logger.LogWarning(ex, "Payment not found");
                return NotFound(new { message = ex.Message });
            }
            catch (UnauthorizedAccessException ex)
            {
                _logger.LogWarning(ex, "Unauthorized access when updating payment");
                return Forbid();
            }
            catch (InvalidOperationException ex)
            {
                _logger.LogWarning(ex, "Invalid operation when updating payment");
                return BadRequest(new { message = ex.Message });
            }
        }

        [HttpDelete("{id}")]
        public async Task<ActionResult> Delete(Guid id)
        {
            _logger.LogInformation("Deleting payment with ID: {PaymentId}", id);

            try
            {
                var existingPayment = await _paymentService.GetByIdAsync(id);
                if (existingPayment.UserId != HttpContext.GetCurrentUserId())
                {
                    _logger.LogWarning("User attempted to delete payment that doesn't belong to them");
                    return Forbid();
                }

                await _paymentService.DeleteAsync(id);
                return NoContent();
            }
            catch (KeyNotFoundException ex)
            {
                _logger.LogWarning(ex, "Payment not found");
                return NotFound(new { message = ex.Message });
            }
            catch (InvalidOperationException ex)
            {
                _logger.LogWarning(ex, "Invalid operation when deleting payment");
                return BadRequest(new { message = ex.Message });
            }
        }

        [HttpPost("{id}/paid")]
        public async Task<ActionResult<PaymentDto>> MarkAsPaid(Guid id, [FromBody] DateTime? paymentDate)
        {
            _logger.LogInformation("Marking payment with ID: {PaymentId} as paid", id);

            try
            {
                var existingPayment = await _paymentService.GetByIdAsync(id);
                if (existingPayment.UserId != HttpContext.GetCurrentUserId())
                {
                    _logger.LogWarning("User attempted to mark as paid a payment that doesn't belong to them");
                    return Forbid();
                }

                var date = paymentDate ?? DateTime.UtcNow;
                var payment = await _paymentService.MarkAsPaidAsync(id, date);
                return Ok(payment);
            }
            catch (KeyNotFoundException ex)
            {
                _logger.LogWarning(ex, "Payment not found");
                return NotFound(new { message = ex.Message });
            }
        }

        [HttpPost("{id}/overdue")]
        public async Task<ActionResult<PaymentDto>> MarkAsOverdue(Guid id)
        {
            _logger.LogInformation("Marking payment with ID: {PaymentId} as overdue", id);

            try
            {
                var existingPayment = await _paymentService.GetByIdAsync(id);
                if (existingPayment.UserId != HttpContext.GetCurrentUserId())
                {
                    _logger.LogWarning("User attempted to mark as overdue a payment that doesn't belong to them");
                    return Forbid();
                }

                var payment = await _paymentService.MarkAsOverdueAsync(id);
                return Ok(payment);
            }
            catch (KeyNotFoundException ex)
            {
                _logger.LogWarning(ex, "Payment not found");
                return NotFound(new { message = ex.Message });
            }
        }

        [HttpPost("{id}/cancel")]
        public async Task<ActionResult<PaymentDto>> Cancel(Guid id)
        {
            _logger.LogInformation("Cancelling payment with ID: {PaymentId}", id);

            try
            {
                var existingPayment = await _paymentService.GetByIdAsync(id);
                if (existingPayment.UserId != HttpContext.GetCurrentUserId())
                {
                    _logger.LogWarning("User attempted to cancel a payment that doesn't belong to them");
                    return Forbid();
                }

                var payment = await _paymentService.CancelAsync(id);
                return Ok(payment);
            }
            catch (KeyNotFoundException ex)
            {
                _logger.LogWarning(ex, "Payment not found");
                return NotFound(new { message = ex.Message });
            }
        }
    }
}
