using Equilibrium.API.Extensions;
using Equilibrium.Application.DTOs.IncomeType;
using Equilibrium.Application.Interfaces;
using Equilibrium.Domain.Interfaces.Services;
using Microsoft.AspNetCore.Mvc;

namespace Equilibrium.API.Controllers
{
    public class IncomeTypesController(IUnitOfWork unitOfWork,
        IIncomeTypeService service) : AuthenticatedController<IIncomeTypeService>(unitOfWork, service)
    {
        [HttpGet]
        public async Task<ActionResult<IEnumerable<IncomeTypeDto>>> GetAll()
        {
            var userId = HttpContext.GetCurrentUserId();
            var incomeTypes = await _service.GetAllAvailableForUserAsync(userId);

            return Ok(incomeTypes);
        }

        [HttpGet("system")]
        public async Task<ActionResult<IEnumerable<IncomeTypeDto>>> GetAllSystem()
        {
            var incomeTypes = await _service.GetAllSystemTypesAsync();

            return Ok(incomeTypes);
        }

        [HttpGet("user")]
        public async Task<ActionResult<IEnumerable<IncomeTypeDto>>> GetAllUser()
        {
            var userId = HttpContext.GetCurrentUserId();
            var incomeTypes = await _service.GetUserTypesAsync(userId);

            return Ok(incomeTypes);
        }

        [HttpGet("{id}")]
        public async Task<ActionResult<IncomeTypeDto>> GetById(Guid id)
        {
            try
            {
                var incomeType = await _service.GetByIdAsync(id);

                return !incomeType.IsSystem && incomeType.UserId != HttpContext.GetCurrentUserId() ? (ActionResult<IncomeTypeDto>)Forbid() : (ActionResult<IncomeTypeDto>)Ok(incomeType);
            }
            catch (KeyNotFoundException ex)
            {
                return NotFound(new { message = ex.Message });
            }
        }

        [HttpPost]
        public async Task<ActionResult<IncomeTypeDto>> Create(CreateIncomeTypeDto createIncomeTypeDto)
        {
            try
            {
                var userId = HttpContext.GetCurrentUserId();
                var incomeType = await _service.CreateAsync(createIncomeTypeDto, userId);

                return CreatedAtAction(nameof(GetById), new { id = incomeType.Id }, incomeType);
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

        [HttpPut("{id}")]
        public async Task<ActionResult<IncomeTypeDto>> Update(Guid id, UpdateIncomeTypeDto updateIncomeTypeDto)
        {
            try
            {
                var existingType = await _service.GetByIdAsync(id);

                if (existingType.IsSystem)
                {
                    return BadRequest(new { message = "Não é possível atualizar tipos de entrada do sistema" });
                }

                if (existingType.UserId != HttpContext.GetCurrentUserId())
                {
                    return Forbid();
                }

                var incomeType = await _service.UpdateAsync(id, updateIncomeTypeDto);
                return Ok(incomeType);
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

        [HttpDelete("{id}")]
        public async Task<ActionResult> Delete(Guid id)
        {
            try
            {
                var existingType = await _service.GetByIdAsync(id);

                if (existingType.IsSystem)
                {
                    return BadRequest(new { message = "Não é possível excluir tipos de entrada do sistema" });
                }

                if (existingType.UserId != HttpContext.GetCurrentUserId())
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
    }
}

