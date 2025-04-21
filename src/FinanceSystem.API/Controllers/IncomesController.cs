using FinanceSystem.API.Extensions;
using FinanceSystem.Application.DTOs.Income;
using FinanceSystem.Application.Interfaces;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace FinanceSystem.API.Controllers
{
    [Route("api/incomes")]
    [ApiController]
    [Authorize]
    public class IncomesController : ControllerBase
    {
        private readonly IIncomeService _incomeService;

        public IncomesController(IIncomeService incomeService)
        {
            _incomeService = incomeService;
        }

        [HttpGet]
        public async Task<ActionResult<IEnumerable<IncomeDto>>> GetAll()
        {
            var userId = HttpContext.GetCurrentUserId();
            var incomes = await _incomeService.GetAllByUserIdAsync(userId);

            return Ok(incomes);
        }

        [HttpGet("{id}")]
        public async Task<ActionResult<IncomeDto>> GetById(Guid id)
        {
            try
            {
                var income = await _incomeService.GetByIdAsync(id);

                if (income.UserId != HttpContext.GetCurrentUserId())
                {
                    return Forbid();
                }

                return Ok(income);
            }
            catch (KeyNotFoundException ex)
            {
                return NotFound(new { message = ex.Message });
            }
        }

        [HttpGet("month/{year}/{month}")]
        public async Task<ActionResult<IEnumerable<IncomeDto>>> GetByMonth(int year, int month)
        {
            if (month < 1 || month > 12)
            {
                return BadRequest(new { message = "Mês deve estar entre 1 e 12" });
            }

            var userId = HttpContext.GetCurrentUserId();
            var incomes = await _incomeService.GetByMonthAsync(userId, month, year);

            return Ok(incomes);
        }

        [HttpGet("pending")]
        public async Task<ActionResult<IEnumerable<IncomeDto>>> GetPending()
        {
            var userId = HttpContext.GetCurrentUserId();
            var incomes = await _incomeService.GetPendingAsync(userId);

            return Ok(incomes);
        }

        [HttpGet("received")]
        public async Task<ActionResult<IEnumerable<IncomeDto>>> GetReceived()
        {
            var userId = HttpContext.GetCurrentUserId();
            var incomes = await _incomeService.GetReceivedAsync(userId);

            return Ok(incomes);
        }

        [HttpGet("type/{typeId}")]
        public async Task<ActionResult<IEnumerable<IncomeDto>>> GetByType(Guid typeId)
        {
            var userId = HttpContext.GetCurrentUserId();
            var incomes = await _incomeService.GetByTypeAsync(userId, typeId);

            return Ok(incomes);
        }

        [HttpPost]
        public async Task<ActionResult<IncomeDto>> Create(CreateIncomeDto createIncomeDto)
        {
            try
            {
                var userId = HttpContext.GetCurrentUserId();
                var income = await _incomeService.CreateAsync(createIncomeDto, userId);

                return CreatedAtAction(nameof(GetById), new { id = income.Id }, income);
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
        public async Task<ActionResult<IncomeDto>> Update(Guid id, UpdateIncomeDto updateIncomeDto)
        {
            try
            {
                var existingIncome = await _incomeService.GetByIdAsync(id);
                if (existingIncome.UserId != HttpContext.GetCurrentUserId())
                {
                    return Forbid();
                }

                var income = await _incomeService.UpdateAsync(id, updateIncomeDto);
                return Ok(income);
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
                var existingIncome = await _incomeService.GetByIdAsync(id);
                if (existingIncome.UserId != HttpContext.GetCurrentUserId())
                {
                    return Forbid();
                }

                await _incomeService.DeleteAsync(id);
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

        [HttpPost("{id}/received")]
        public async Task<ActionResult<IncomeDto>> MarkAsReceived(Guid id, [FromBody] DateTime? receivedDate)
        {
            try
            {
                var existingIncome = await _incomeService.GetByIdAsync(id);
                if (existingIncome.UserId != HttpContext.GetCurrentUserId())
                {
                    return Forbid();
                }

                var date = receivedDate ?? DateTime.UtcNow;
                var income = await _incomeService.MarkAsReceivedAsync(id, date);
                return Ok(income);
            }
            catch (KeyNotFoundException ex)
            {
                return NotFound(new { message = ex.Message });
            }
        }

        [HttpPost("{id}/cancel")]
        public async Task<ActionResult<IncomeDto>> Cancel(Guid id)
        {
            try
            {
                var existingIncome = await _incomeService.GetByIdAsync(id);
                if (existingIncome.UserId != HttpContext.GetCurrentUserId())
                {
                    return Forbid();
                }

                var income = await _incomeService.CancelAsync(id);
                return Ok(income);
            }
            catch (KeyNotFoundException ex)
            {
                return NotFound(new { message = ex.Message });
            }
        }
    }
}
