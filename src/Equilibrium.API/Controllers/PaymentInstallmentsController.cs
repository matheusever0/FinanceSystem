using Equilibrium.API.Extensions;
using Equilibrium.Application.DTOs.PaymentInstallment;
using Equilibrium.Application.Interfaces;
using Equilibrium.Domain.Interfaces.Services;
using Equilibrium.Application.DTOs.Common;
using Equilibrium.Domain.DTOs.Filters;
using Equilibrium.Application.Validations.Filters;
using Microsoft.AspNetCore.Mvc;

namespace Equilibrium.API.Controllers
{
    public class PaymentInstallmentsController(IUnitOfWork unitOfWork,
        IPaymentInstallmentService service,
        IPaymentService paymentService) : AuthenticatedController<IPaymentInstallmentService>(unitOfWork, service)
    {
                [HttpGet("filter")]
        public async Task<ActionResult<PagedResult<PaymentInstallmentDto>>> GetFiltered([FromQuery] PaymentInstallmentFilter filter)
        {
            if (filter == null)
                filter = new PaymentInstallmentFilter();
                
            var validator = new PaymentInstallmentFilterValidator();
            var validationResult = await validator.ValidateAsync(filter);
            
            if (!validationResult.IsValid)
                return BadRequest(validationResult.Errors);
                
            var userId = HttpContext.GetCurrentUserId();
            var pagedResult = await _service.GetFilteredAsync(filter, userId);
            
            // Add pagination headers
            Response.Headers.Add("X-Pagination-Total", pagedResult.TotalCount.ToString());
            Response.Headers.Add("X-Pagination-Pages", pagedResult.TotalPages.ToString());
            Response.Headers.Add("X-Pagination-Page", pagedResult.PageNumber.ToString());
            Response.Headers.Add("X-Pagination-Size", pagedResult.PageSize.ToString());
            
            return Ok(pagedResult);
        }
        [HttpGet("payment/{paymentId}")]
        public async Task<ActionResult<IEnumerable<PaymentInstallmentDto>>> GetByPayment(Guid paymentId)
        {
            try
            {
                var payment = await paymentService.GetByIdAsync(paymentId);
                if (payment.UserId != HttpContext.GetCurrentUserId())
                {
                    return Forbid();
                }

                var installments = await _service.GetByPaymentIdAsync(paymentId);
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
            var installments = await _service.GetByDueDateAsync(userId, startDate, endDate);

            return Ok(installments);
        }

        [HttpGet("pending")]
        public async Task<ActionResult<IEnumerable<PaymentInstallmentDto>>> GetPending()
        {
            var userId = HttpContext.GetCurrentUserId();
            var installments = await _service.GetPendingAsync(userId);

            return Ok(installments);
        }

        [HttpGet("overdue")]
        public async Task<ActionResult<IEnumerable<PaymentInstallmentDto>>> GetOverdue()
        {
            var userId = HttpContext.GetCurrentUserId();
            var installments = await _service.GetOverdueAsync(userId);

            return Ok(installments);
        }

        [HttpGet("{id}")]
        public async Task<ActionResult<PaymentInstallmentDto>> GetById(Guid id)
        {
            try
            {
                var installment = await _service.GetByIdAsync(id);

                var payment = await paymentService.GetByIdAsync(installment.PaymentId);
                return payment.UserId != HttpContext.GetCurrentUserId() ? (ActionResult<PaymentInstallmentDto>)Forbid() : (ActionResult<PaymentInstallmentDto>)Ok(installment);
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
                var installment = await _service.GetByIdAsync(id);

                var payment = await paymentService.GetByIdAsync(installment.PaymentId);
                if (payment.UserId != HttpContext.GetCurrentUserId())
                {
                    return Forbid();
                }

                var date = paymentDate ?? DateTime.Now;
                var updatedInstallment = await _service.MarkAsPaidAsync(id, date);
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
                var installment = await _service.GetByIdAsync(id);

                var payment = await paymentService.GetByIdAsync(installment.PaymentId);
                if (payment.UserId != HttpContext.GetCurrentUserId())
                {
                    return Forbid();
                }

                var updatedInstallment = await _service.MarkAsOverdueAsync(id);
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
                var installment = await _service.GetByIdAsync(id);

                var payment = await paymentService.GetByIdAsync(installment.PaymentId);
                if (payment.UserId != HttpContext.GetCurrentUserId())
                {
                    return Forbid();
                }

                var updatedInstallment = await _service.CancelAsync(id);
                return Ok(updatedInstallment);
            }
            catch (KeyNotFoundException ex)
            {
                return NotFound(new { message = ex.Message });
            }
        }
    }
}

