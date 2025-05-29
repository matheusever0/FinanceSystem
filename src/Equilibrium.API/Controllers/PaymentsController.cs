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
        /// <summary>
        /// Busca pagamentos com filtros diversos
        /// </summary>
        /// <param name="filter">Filtros para busca</param>
        /// <returns>Lista de pagamentos filtrados</returns>
        [HttpGet]
        public async Task<ActionResult<IEnumerable<PaymentDto>>> Get([FromQuery] PaymentFilter filter)
        {
            // Validar mês se fornecido
            if (!filter.IsValidMonth())
            {
                return BadRequest(new { message = "Mês deve estar entre 1 e 12" });
            }

            var userId = HttpContext.GetCurrentUserId();
            var payments = await _service.GetFilteredAsync(userId, filter);

            return Ok(payments);
        }

        /// <summary>
        /// Busca pagamento específico por ID
        /// </summary>
        [HttpGet("{id}")]
        public async Task<ActionResult<PaymentDto>> GetById(Guid id)
        {
            try
            {
                var payment = await _service.GetByIdAsync(id);
                return payment.UserId != HttpContext.GetCurrentUserId()
                    ? Forbid()
                    : Ok(payment);
            }
            catch (KeyNotFoundException ex)
            {
                return NotFound(new { message = ex.Message });
            }
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