using FinanceSystem.API.Extensions;
using FinanceSystem.Application.DTOs.Payment;
using FinanceSystem.Application.Interfaces;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace FinanceSystem.API.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    [Authorize]
    public class PaymentsController : ControllerBase
    {
        private readonly IPaymentService _paymentService;

        public PaymentsController(IPaymentService paymentService)
        {
            _paymentService = paymentService;
        }

        [HttpGet]
        public async Task<ActionResult<IEnumerable<PaymentDto>>> GetAll()
        {
            var userId = HttpContext.GetCurrentUserId();
            var payments = await _paymentService.GetAllByUserIdAsync(userId);

            return Ok(payments);
        }

        [HttpGet("{id}")]
        public async Task<ActionResult<PaymentDto>> GetById(Guid id)
        {
            try
            {
                var payment = await _paymentService.GetByIdAsync(id);

                if (payment.UserId != HttpContext.GetCurrentUserId())
                {
                    return Forbid();
                }

                return Ok(payment);
            }
            catch (KeyNotFoundException ex)
            {
                return NotFound(new { message = ex.Message });
            }
        }

        [HttpGet("month/{year}/{month}")]
        public async Task<ActionResult<IEnumerable<PaymentDto>>> GetByMonth(int year, int month)
        {
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
            var userId = HttpContext.GetCurrentUserId();
            var payments = await _paymentService.GetPendingAsync(userId);

            return Ok(payments);
        }

        [HttpGet("overdue")]
        public async Task<ActionResult<IEnumerable<PaymentDto>>> GetOverdue()
        {
            var userId = HttpContext.GetCurrentUserId();
            var payments = await _paymentService.GetOverdueAsync(userId);

            return Ok(payments);
        }

        [HttpGet("type/{typeId}")]
        public async Task<ActionResult<IEnumerable<PaymentDto>>> GetByType(Guid typeId)
        {
            var userId = HttpContext.GetCurrentUserId();
            var payments = await _paymentService.GetByTypeAsync(userId, typeId);

            return Ok(payments);
        }

        [HttpGet("method/{methodId}")]
        public async Task<ActionResult<IEnumerable<PaymentDto>>> GetByMethod(Guid methodId)
        {
            var userId = HttpContext.GetCurrentUserId();
            var payments = await _paymentService.GetByMethodAsync(userId, methodId);

            return Ok(payments);
        }

        [HttpPost]
        public async Task<ActionResult<PaymentDto>> Create(CreatePaymentDto createPaymentDto)
        {
            try
            {
                var userId = HttpContext.GetCurrentUserId();
                var payment = await _paymentService.CreateAsync(createPaymentDto, userId);

                return CreatedAtAction(nameof(GetById), new { id = payment.Id }, payment);
            }
            catch (KeyNotFoundException ex)
            {
                return NotFound(new { message = ex.Message });
            }
            catch (UnauthorizedAccessException ex)
            {
                return Forbid();
            }
            catch (InvalidOperationException ex)
            {
                return BadRequest(new { message = ex.Message });
            }
        }

        [HttpPut("{id}")]
        public async Task<ActionResult<PaymentDto>> Update(Guid id, UpdatePaymentDto updatePaymentDto)
        {
            try
            {
                var existingPayment = await _paymentService.GetByIdAsync(id);
                if (existingPayment.UserId != HttpContext.GetCurrentUserId())
                {
                    return Forbid();
                }

                var payment = await _paymentService.UpdateAsync(id, updatePaymentDto);
                return Ok(payment);
            }
            catch (KeyNotFoundException ex)
            {
                return NotFound(new { message = ex.Message });
            }
            catch (UnauthorizedAccessException ex)
            {
                return Forbid();
            }
            catch (InvalidOperationException ex)
            {
                return BadRequest(new { message = ex.Message });
            }
        }

        [HttpDelete("{id}")]
        public async Task<ActionResult> Delete(Guid id)
        {
            try
            {
                var existingPayment = await _paymentService.GetByIdAsync(id);
                if (existingPayment.UserId != HttpContext.GetCurrentUserId())
                {
                    return Forbid();
                }

                await _paymentService.DeleteAsync(id);
                return NoContent();
            }
            catch (KeyNotFoundException ex)
            {
                return NotFound(new { message = ex.Message });
            }
            catch (InvalidOperationException ex)
            {
                return BadRequest(new { message = ex.Message });
            }
        }

        [HttpPost("{id}/paid")]
        public async Task<ActionResult<PaymentDto>> MarkAsPaid(Guid id, [FromBody] DateTime? paymentDate)
        {
            try
            {
                var existingPayment = await _paymentService.GetByIdAsync(id);
                if (existingPayment.UserId != HttpContext.GetCurrentUserId())
                {
                    return Forbid();
                }

                var date = paymentDate ?? DateTime.Now;
                var payment = await _paymentService.MarkAsPaidAsync(id, date);
                return Ok(payment);
            }
            catch (KeyNotFoundException ex)
            {
                return NotFound(new { message = ex.Message });
            }
        }

        [HttpPost("{id}/overdue")]
        public async Task<ActionResult<PaymentDto>> MarkAsOverdue(Guid id)
        {
            try
            {
                var existingPayment = await _paymentService.GetByIdAsync(id);
                if (existingPayment.UserId != HttpContext.GetCurrentUserId())
                {
                    return Forbid();
                }

                var payment = await _paymentService.MarkAsOverdueAsync(id);
                return Ok(payment);
            }
            catch (KeyNotFoundException ex)
            {
                return NotFound(new { message = ex.Message });
            }
        }

        [HttpPost("{id}/cancel")]
        public async Task<ActionResult<PaymentDto>> Cancel(Guid id)
        {
            try
            {
                var existingPayment = await _paymentService.GetByIdAsync(id);
                if (existingPayment.UserId != HttpContext.GetCurrentUserId())
                {
                    return Forbid();
                }

                var payment = await _paymentService.CancelAsync(id);
                return Ok(payment);
            }
            catch (KeyNotFoundException ex)
            {
                return NotFound(new { message = ex.Message });
            }
        }
    }
}
