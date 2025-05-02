using Equilibrium.API.Extensions;
using Equilibrium.Application.DTOs.Financing;
using Equilibrium.Application.Interfaces;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace Equilibrium.API.Controllers
{
    [Route("api/financing-installments")]
    [ApiController]
    [Authorize]
    public class FinancingInstallmentsController : ControllerBase
    {
        private readonly IFinancingInstallmentService _installmentService;
        private readonly IFinancingService _financingService;

        public FinancingInstallmentsController(
            IFinancingInstallmentService installmentService,
            IFinancingService financingService)
        {
            _installmentService = installmentService;
            _financingService = financingService;
        }

        [HttpGet("financing/{financingId}")]
        public async Task<ActionResult<IEnumerable<FinancingInstallmentDto>>> GetByFinancingId(Guid financingId)
        {
            try
            {
                var financing = await _financingService.GetByIdAsync(financingId);
                if (financing.UserId != HttpContext.GetCurrentUserId())
                    return Forbid();

                var installments = await _installmentService.GetByFinancingIdAsync(financingId);
                return Ok(installments);
            }
            catch (KeyNotFoundException ex)
            {
                return NotFound(new { message = ex.Message });
            }
        }

        [HttpGet("due-date")]
        public async Task<ActionResult<IEnumerable<FinancingInstallmentDto>>> GetByDueDate([FromQuery] DateTime startDate, [FromQuery] DateTime endDate)
        {
            var userId = HttpContext.GetCurrentUserId();
            var installments = await _installmentService.GetByDueDateAsync(userId, startDate, endDate);
            return Ok(installments);
        }

        [HttpGet("pending")]
        public async Task<ActionResult<IEnumerable<FinancingInstallmentDto>>> GetPending()
        {
            var userId = HttpContext.GetCurrentUserId();
            var installments = await _installmentService.GetPendingAsync(userId);
            return Ok(installments);
        }

        [HttpGet("overdue")]
        public async Task<ActionResult<IEnumerable<FinancingInstallmentDto>>> GetOverdue()
        {
            var userId = HttpContext.GetCurrentUserId();
            var installments = await _installmentService.GetOverdueAsync(userId);
            return Ok(installments);
        }

        [HttpGet("{id}")]
        public async Task<ActionResult<FinancingInstallmentDto>> GetById(Guid id)
        {
            try
            {
                var installment = await _installmentService.GetByIdAsync(id);
                var financing = await _financingService.GetByIdAsync(installment.FinancingId);

                if (financing.UserId != HttpContext.GetCurrentUserId())
                    return Forbid();

                return Ok(installment);
            }
            catch (KeyNotFoundException ex)
            {
                return NotFound(new { message = ex.Message });
            }
        }

        [HttpGet("{id}/details")]
        public async Task<ActionResult<FinancingInstallmentDetailDto>> GetDetails(Guid id)
        {
            try
            {
                var details = await _installmentService.GetDetailsByIdAsync(id);
                var financing = await _financingService.GetByIdAsync(details.FinancingId);

                if (financing.UserId != HttpContext.GetCurrentUserId())
                    return Forbid();

                return Ok(details);
            }
            catch (KeyNotFoundException ex)
            {
                return NotFound(new { message = ex.Message });
            }
        }
    }
}