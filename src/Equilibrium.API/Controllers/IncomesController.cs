using Equilibrium.API.Extensions;
using Equilibrium.Application.DTOs.Income;
using Equilibrium.Application.Interfaces;
using Equilibrium.Domain.Interfaces.Services;
using Microsoft.AspNetCore.Mvc;

namespace Equilibrium.API.Controllers
{
    public class IncomesController(IUnitOfWork unitOfWork,
        IIncomeService service) : AuthenticatedController<IIncomeService>(unitOfWork, service)
    {
        /// <summary>
        /// Busca receitas com filtros diversos
        /// </summary>
        /// <param name="filter">Filtros para busca</param>
        /// <returns>Lista de receitas filtradas</returns>
        [HttpGet]
        public async Task<ActionResult<IEnumerable<IncomeDto>>> Get([FromQuery] IncomeFilter filter)
        {
            // Validar mês se fornecido
            if (!filter.IsValidMonth())
            {
                return BadRequest(new { message = "Mês deve estar entre 1 e 12" });
            }

            var userId = HttpContext.GetCurrentUserId();
            var incomes = await _service.GetFilteredAsync(userId, filter);

            return Ok(incomes);
        }

        /// <summary>
        /// Busca receita específica por ID
        /// </summary>
        [HttpGet("{id}")]
        public async Task<ActionResult<IncomeDto>> GetById(Guid id)
        {
            try
            {
                var income = await _service.GetByIdAsync(id);
                return income.UserId != HttpContext.GetCurrentUserId()
                    ? Forbid()
                    : Ok(income);
            }
            catch (KeyNotFoundException ex)
            {
                return NotFound(new { message = ex.Message });
            }
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

                var income = await _service.MarkAsReceivedAsync(id, receivedDate);
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