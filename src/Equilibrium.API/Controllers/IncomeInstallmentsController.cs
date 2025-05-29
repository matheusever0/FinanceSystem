using Equilibrium.API.Extensions;
using Equilibrium.Application.DTOs.IncomeInstallment;
using Equilibrium.Application.Interfaces;
using Equilibrium.Application.Services;
using Equilibrium.Domain.Interfaces.Services;
using Microsoft.AspNetCore.Mvc;

namespace Equilibrium.API.Controllers
{
    public class IncomeInstallmentsController(IUnitOfWork unitOfWork,
        IncomeInstallmentService service,
        IIncomeService incomeService) : AuthenticatedController<IncomeInstallmentService>(unitOfWork, service)
    {
       [HttpGet("income/{incomeId}")]
        public async Task<ActionResult<IEnumerable<IncomeInstallmentDto>>> GetByIncome(Guid incomeId)
        {
            try
            {
                var income = await incomeService.GetByIdAsync(incomeId);
                if (income.UserId != HttpContext.GetCurrentUserId())
                {
                    return Forbid();
                }

                var installments = await _service.GetByIncomeIdAsync(incomeId);
                return Ok(installments);
            }
            catch (KeyNotFoundException ex)
            {
                return NotFound(new { message = ex.Message });
            }
        }

        [HttpGet("due-date")]
        public async Task<ActionResult<IEnumerable<IncomeInstallmentDto>>> GetByDueDate([FromQuery] DateTime startDate, [FromQuery] DateTime endDate)
        {
            var userId = HttpContext.GetCurrentUserId();
            var installments = await _service.GetByDueDateAsync(userId, startDate, endDate);

            return Ok(installments);
        }

        [HttpGet("pending")]
        public async Task<ActionResult<IEnumerable<IncomeInstallmentDto>>> GetPending()
        {
            var userId = HttpContext.GetCurrentUserId();
            var installments = await _service.GetPendingAsync(userId);

            return Ok(installments);
        }

        [HttpGet("received")]
        public async Task<ActionResult<IEnumerable<IncomeInstallmentDto>>> GetReceived()
        {
            var userId = HttpContext.GetCurrentUserId();
            var installments = await _service.GetReceivedAsync(userId);

            return Ok(installments);
        }

        [HttpGet("{id}")]
        public async Task<ActionResult<IncomeInstallmentDto>> GetById(Guid id)
        {
            try
            {
                var installment = await _service.GetByIdAsync(id);

                var income = await incomeService.GetByIdAsync(installment.IncomeId);
                return income.UserId != HttpContext.GetCurrentUserId() ? (ActionResult<IncomeInstallmentDto>)Forbid() : (ActionResult<IncomeInstallmentDto>)Ok(installment);
            }
            catch (KeyNotFoundException ex)
            {
                return NotFound(new { message = ex.Message });
            }
        }

        [HttpPost("{id}/received")]
        public async Task<ActionResult<IncomeInstallmentDto>> MarkAsReceived(Guid id, [FromBody] DateTime? receivedDate)
        {
            try
            {
                var installment = await _service.GetByIdAsync(id);

                var income = await incomeService.GetByIdAsync(installment.IncomeId);
                if (income.UserId != HttpContext.GetCurrentUserId())
                {
                    return Forbid();
                }

                var date = receivedDate ?? DateTime.Now;
                var updatedInstallment = await _service.MarkAsReceivedAsync(id, date);
                return Ok(updatedInstallment);
            }
            catch (KeyNotFoundException ex)
            {
                return NotFound(new { message = ex.Message });
            }
        }

        [HttpPost("{id}/cancel")]
        public async Task<ActionResult<IncomeInstallmentDto>> Cancel(Guid id)
        {
            try
            {
                var installment = await _service.GetByIdAsync(id);

                var income = await incomeService.GetByIdAsync(installment.IncomeId);
                if (income.UserId != HttpContext.GetCurrentUserId())
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

