using FinanceSystem.API.Extensions;
using FinanceSystem.Application.DTOs.IncomeInstallment;
using FinanceSystem.Application.Interfaces;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace FinanceSystem.API.Controllers
{
    [Route("api/income-installments")]
    [ApiController]
    [Authorize]
    public class IncomeInstallmentsController : ControllerBase
    {
        private readonly IIncomeInstallmentService _incomeInstallmentService;
        private readonly IIncomeService _incomeService;

        public IncomeInstallmentsController(
            IIncomeInstallmentService incomeInstallmentService,
            IIncomeService incomeService)
        {
            _incomeInstallmentService = incomeInstallmentService;
            _incomeService = incomeService;
        }

        [HttpGet("income/{incomeId}")]
        public async Task<ActionResult<IEnumerable<IncomeInstallmentDto>>> GetByIncome(Guid incomeId)
        {
            try
            {
                var income = await _incomeService.GetByIdAsync(incomeId);
                if (income.UserId != HttpContext.GetCurrentUserId())
                {
                    return Forbid();
                }

                var installments = await _incomeInstallmentService.GetByIncomeIdAsync(incomeId);
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
            var installments = await _incomeInstallmentService.GetByDueDateAsync(userId, startDate, endDate);

            return Ok(installments);
        }

        [HttpGet("pending")]
        public async Task<ActionResult<IEnumerable<IncomeInstallmentDto>>> GetPending()
        {
            var userId = HttpContext.GetCurrentUserId();
            var installments = await _incomeInstallmentService.GetPendingAsync(userId);

            return Ok(installments);
        }

        [HttpGet("received")]
        public async Task<ActionResult<IEnumerable<IncomeInstallmentDto>>> GetReceived()
        {
            var userId = HttpContext.GetCurrentUserId();
            var installments = await _incomeInstallmentService.GetReceivedAsync(userId);

            return Ok(installments);
        }

        [HttpGet("{id}")]
        public async Task<ActionResult<IncomeInstallmentDto>> GetById(Guid id)
        {
            try
            {
                var installment = await _incomeInstallmentService.GetByIdAsync(id);

                var income = await _incomeService.GetByIdAsync(installment.IncomeId);
                if (income.UserId != HttpContext.GetCurrentUserId())
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

        [HttpPost("{id}/received")]
        public async Task<ActionResult<IncomeInstallmentDto>> MarkAsReceived(Guid id, [FromBody] DateTime? receivedDate)
        {
            try
            {
                var installment = await _incomeInstallmentService.GetByIdAsync(id);

                var income = await _incomeService.GetByIdAsync(installment.IncomeId);
                if (income.UserId != HttpContext.GetCurrentUserId())
                {
                    return Forbid();
                }

                var date = receivedDate ?? DateTime.Now;
                var updatedInstallment = await _incomeInstallmentService.MarkAsReceivedAsync(id, date);
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
                var installment = await _incomeInstallmentService.GetByIdAsync(id);

                var income = await _incomeService.GetByIdAsync(installment.IncomeId);
                if (income.UserId != HttpContext.GetCurrentUserId())
                {
                    return Forbid();
                }

                var updatedInstallment = await _incomeInstallmentService.CancelAsync(id);
                return Ok(updatedInstallment);
            }
            catch (KeyNotFoundException ex)
            {
                return NotFound(new { message = ex.Message });
            }
        }
    }
}
