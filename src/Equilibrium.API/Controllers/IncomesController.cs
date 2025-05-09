using Equilibrium.API.Extensions;
using Equilibrium.Application.DTOs.Income;
using Equilibrium.Application.DTOs.Payment;
using Equilibrium.Application.Interfaces;
using Equilibrium.Domain.Interfaces.Services;
using Equilibrium.Application.DTOs.Common;
using Equilibrium.Application.Validations.Filters;
using Microsoft.AspNetCore.Mvc;
using Equilibrium.Domain.DTOs.Filters;

namespace Equilibrium.API.Controllers
{
    public class IncomesController(IUnitOfWork unitOfWork,
        IIncomeService service) : AuthenticatedController<IIncomeService>(unitOfWork, service)
    {
        [HttpGet]
        public async Task<ActionResult<IEnumerable<IncomeDto>>> GetAll()
        {
            var userId = HttpContext.GetCurrentUserId();
            var incomes = await _service.GetAllByUserIdAsync(userId);

            return Ok(incomes);
        }

                [HttpGet("filter")]
        public async Task<ActionResult<PagedResult<IncomeDto>>> GetFiltered([FromQuery] IncomeFilter filter)
        {
            if (filter == null)
                filter = new IncomeFilter();
                
            var validator = new IncomeFilterValidator();
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
        [HttpGet("{id}")]
        public async Task<ActionResult<IncomeDto>> GetById(Guid id)
        {
            try
            {
                var income = await _service.GetByIdAsync(id);

                return income.UserId != HttpContext.GetCurrentUserId() ? (ActionResult<IncomeDto>)Forbid() : (ActionResult<IncomeDto>)Ok(income);
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
            var incomes = await _service.GetByMonthAsync(userId, month, year);

            return Ok(incomes);
        }

        [HttpGet("pending")]
        public async Task<ActionResult<IEnumerable<IncomeDto>>> GetPending()
        {
            var userId = HttpContext.GetCurrentUserId();
            var incomes = await _service.GetPendingAsync(userId);

            return Ok(incomes);
        }

        [HttpGet("overdue")]
        public async Task<ActionResult<IEnumerable<PaymentDto>>> GetOverdue()
        {
            var userId = HttpContext.GetCurrentUserId();
            var incomes = await _service.GetOverdueAsync(userId);

            return Ok(incomes);
        }

        [HttpGet("received")]
        public async Task<ActionResult<IEnumerable<IncomeDto>>> GetReceived()
        {
            var userId = HttpContext.GetCurrentUserId();
            var incomes = await _service.GetReceivedAsync(userId);

            return Ok(incomes);
        }

        [HttpGet("type/{typeId}")]
        public async Task<ActionResult<IEnumerable<IncomeDto>>> GetByType(Guid typeId)
        {
            var userId = HttpContext.GetCurrentUserId();
            var incomes = await _service.GetByTypeAsync(userId, typeId);

            return Ok(incomes);
        }

        [HttpPost]
        public async Task<ActionResult<IncomeDto>> Create(CreateIncomeDto createIncomeDto)
        {
            try
            {
                var userId = HttpContext.GetCurrentUserId();
                var income = await _service.CreateAsync(createIncomeDto, userId);

                return CreatedAtAction(nameof(GetById), new { id = income.Id }, income);
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
        public async Task<ActionResult<IncomeDto>> Update(Guid id, UpdateIncomeDto updateIncomeDto)
        {
            try
            {
                var existingIncome = await _service.GetByIdAsync(id);
                if (existingIncome.UserId != HttpContext.GetCurrentUserId())
                {
                    return Forbid();
                }

                var income = await _service.UpdateAsync(id, updateIncomeDto);
                return Ok(income);
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
                var existingIncome = await _service.GetByIdAsync(id);
                if (existingIncome.UserId != HttpContext.GetCurrentUserId())
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

        [HttpPost("{id}/received")]
        public async Task<ActionResult<IncomeDto>> MarkAsReceived(Guid id, [FromBody] DateTime? receivedDate)
        {
            try
            {
                var existingIncome = await _service.GetByIdAsync(id);
                if (existingIncome.UserId != HttpContext.GetCurrentUserId())
                {
                    return Forbid();
                }

                var date = receivedDate ?? DateTime.Now;
                var income = await _service.MarkAsReceivedAsync(id, date);
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
                var existingIncome = await _service.GetByIdAsync(id);
                if (existingIncome.UserId != HttpContext.GetCurrentUserId())
                {
                    return Forbid();
                }

                var income = await _service.CancelAsync(id);
                return Ok(income);
            }
            catch (KeyNotFoundException ex)
            {
                return NotFound(new { message = ex.Message });
            }
        }
    }
}

