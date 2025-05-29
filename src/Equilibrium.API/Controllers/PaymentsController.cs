using Equilibrium.API.Extensions;
using Equilibrium.Application.DTOs.Financing;
using Equilibrium.Application.DTOs.Payment;
using Equilibrium.Application.Interfaces;
using Equilibrium.Domain.Enums;
using Equilibrium.Domain.Interfaces.Services;
using Microsoft.AspNetCore.Mvc;

namespace Equilibrium.API.Controllers
{
    public class PaymentsController(IUnitOfWork unitOfWork,
        IPaymentService service) : AuthenticatedController<IPaymentService>(unitOfWork, service)
    {
        [HttpGet]
        public async Task<ActionResult<IEnumerable<PaymentDto>>> GetAll()
        {
            var userId = HttpContext.GetCurrentUserId();
            var payments = await _service.GetAllByUserIdAsync(userId);

            return Ok(payments);
        }

        [HttpGet("{id}")]
        public async Task<ActionResult<PaymentDto>> GetById(Guid id)
        {
            try
            {
                var payment = await _service.GetByIdAsync(id);

                return payment.UserId != HttpContext.GetCurrentUserId() ? (ActionResult<PaymentDto>)Forbid() : (ActionResult<PaymentDto>)Ok(payment);
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
            var payments = await _service.GetByMonthAsync(userId, month, year);

            return Ok(payments);
        }

        [HttpGet("pending")]
        public async Task<ActionResult<IEnumerable<PaymentDto>>> GetPending()
        {
            var userId = HttpContext.GetCurrentUserId();
            var payments = await _service.GetPendingAsync(userId);

            return Ok(payments);
        }

        [HttpGet("overdue")]
        public async Task<ActionResult<IEnumerable<PaymentDto>>> GetOverdue()
        {
            var userId = HttpContext.GetCurrentUserId();
            var payments = await _service.GetOverdueAsync(userId);

            return Ok(payments);
        }

        [HttpGet("type/{typeId}")]
        public async Task<ActionResult<IEnumerable<PaymentDto>>> GetByType(Guid typeId)
        {
            var userId = HttpContext.GetCurrentUserId();
            var payments = await _service.GetByTypeAsync(userId, typeId);

            return Ok(payments);
        }

        [HttpGet("method/{methodId}")]
        public async Task<ActionResult<IEnumerable<PaymentDto>>> GetByMethod(Guid methodId)
        {
            var userId = HttpContext.GetCurrentUserId();
            var payments = await _service.GetByMethodAsync(userId, methodId);

            return Ok(payments);
        }

        [HttpPost]
        public async Task<ActionResult<PaymentDto>> Create(CreatePaymentDto createPaymentDto)
        {
            try
            {
                var userId = HttpContext.GetCurrentUserId();
                var payment = await _service.CreateAsync(createPaymentDto, userId);

                return CreatedAtAction(nameof(GetById), new { id = payment.Id }, payment);
            }
            catch (KeyNotFoundException ex)
            {
                return NotFound(new { message = ex.Message });
            }
            catch (UnauthorizedAccessException)
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
                var existingPayment = await _service.GetByIdAsync(id);
                if (existingPayment.UserId != HttpContext.GetCurrentUserId())
                {
                    return Forbid();
                }

                var payment = await _service.UpdateAsync(id, updatePaymentDto);
                return Ok(payment);
            }
            catch (KeyNotFoundException ex)
            {
                return NotFound(new { message = ex.Message });
            }
            catch (UnauthorizedAccessException)
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
                var existingPayment = await _service.GetByIdAsync(id);
                if (existingPayment.UserId != HttpContext.GetCurrentUserId())
                {
                    return Forbid();
                }

                await _service.DeleteAsync(id);
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
                var existingPayment = await _service.GetByIdAsync(id);
                if (existingPayment.UserId != HttpContext.GetCurrentUserId())
                {
                    return Forbid();
                }

                var date = paymentDate ?? DateTime.Now;
                var payment = await _service.MarkAsPaidAsync(id, date);
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
                var existingPayment = await _service.GetByIdAsync(id);
                if (existingPayment.UserId != HttpContext.GetCurrentUserId())
                {
                    return Forbid();
                }

                var payment = await _service.MarkAsOverdueAsync(id);
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
                var existingPayment = await _service.GetByIdAsync(id);
                if (existingPayment.UserId != HttpContext.GetCurrentUserId())
                {
                    return Forbid();
                }

                var payment = await _service.CancelAsync(id);
                return Ok(payment);
            }
            catch (KeyNotFoundException ex)
            {
                return NotFound(new { message = ex.Message });
            }
        }

        [HttpGet("financing-options")]
        public async Task<ActionResult<IEnumerable<FinancingDto>>> GetFinancingOptions()
        {
            var userId = HttpContext.GetCurrentUserId();
            var financingService = HttpContext.RequestServices.GetRequiredService<IFinancingService>();
            var financings = await financingService.GetActiveFinancingsByUserIdAsync(userId);
            return Ok(financings);
        }

        // Adicionar um endpoint para listar parcelas pendentes de um financiamento
        [HttpGet("financing/{financingId}/installments")]
        public async Task<ActionResult<IEnumerable<FinancingInstallmentDto>>> GetFinancingInstallments(Guid financingId)
        {
            var userId = HttpContext.GetCurrentUserId();
            var financingService = HttpContext.RequestServices.GetRequiredService<IFinancingService>();

            try
            {
                var financing = await financingService.GetByIdAsync(financingId);
                if (financing.UserId != userId)
                    return Forbid();

                var installmentService = HttpContext.RequestServices.GetRequiredService<IFinancingInstallmentService>();
                var installments = await installmentService.GetByFinancingIdAsync(financingId);

                // Filtrar apenas parcelas pendentes ou parcialmente pagas
                var pendingInstallments = installments.Where(i =>
                    i.Status == FinancingInstallmentStatus.Pending ||
                    i.Status == FinancingInstallmentStatus.PartiallyPaid).ToList();

                return Ok(pendingInstallments);
            }
            catch (KeyNotFoundException ex)
            {
                return NotFound(new { message = ex.Message });
            }
        }
    }
}

