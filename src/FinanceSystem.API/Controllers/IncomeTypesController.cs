using FinanceSystem.API.Extensions;
using FinanceSystem.Application.DTOs.IncomeType;
using FinanceSystem.Application.Interfaces;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace FinanceSystem.API.Controllers
{
    [Route("api/income-types")]
    [ApiController]
    [Authorize]
    public class IncomeTypesController : ControllerBase
    {
        private readonly IIncomeTypeService _incomeTypeService;
        private readonly ILogger<IncomeTypesController> _logger;

        public IncomeTypesController(IIncomeTypeService incomeTypeService, ILogger<IncomeTypesController> logger)
        {
            _incomeTypeService = incomeTypeService;
            _logger = logger;
        }

        [HttpGet]
        public async Task<ActionResult<IEnumerable<IncomeTypeDto>>> GetAll()
        {
            _logger.LogInformation("Obtendo todos os tipos de entrada para o usuário");

            var userId = HttpContext.GetCurrentUserId();
            var incomeTypes = await _incomeTypeService.GetAllAvailableForUserAsync(userId);

            return Ok(incomeTypes);
        }

        [HttpGet("system")]
        public async Task<ActionResult<IEnumerable<IncomeTypeDto>>> GetAllSystem()
        {
            _logger.LogInformation("Obtendo todos os tipos de entrada do sistema");

            var incomeTypes = await _incomeTypeService.GetAllSystemTypesAsync();

            return Ok(incomeTypes);
        }

        [HttpGet("user")]
        public async Task<ActionResult<IEnumerable<IncomeTypeDto>>> GetAllUser()
        {
            _logger.LogInformation("Obtendo todos os tipos de entrada personalizados do usuário");

            var userId = HttpContext.GetCurrentUserId();
            var incomeTypes = await _incomeTypeService.GetUserTypesAsync(userId);

            return Ok(incomeTypes);
        }

        [HttpGet("{id}")]
        public async Task<ActionResult<IncomeTypeDto>> GetById(Guid id)
        {
            _logger.LogInformation("Obtendo tipo de entrada com ID: {TypeId}", id);

            try
            {
                var incomeType = await _incomeTypeService.GetByIdAsync(id);

                if (!incomeType.IsSystem && incomeType.UserId != HttpContext.GetCurrentUserId())
                {
                    _logger.LogWarning("Usuário tentou acessar tipo de entrada que não pertence a ele");
                    return Forbid();
                }

                return Ok(incomeType);
            }
            catch (KeyNotFoundException ex)
            {
                _logger.LogWarning(ex, "Tipo de entrada não encontrado");
                return NotFound(new { message = ex.Message });
            }
        }

        [HttpPost]
        public async Task<ActionResult<IncomeTypeDto>> Create(CreateIncomeTypeDto createIncomeTypeDto)
        {
            _logger.LogInformation("Criando novo tipo de entrada");

            try
            {
                var userId = HttpContext.GetCurrentUserId();
                var incomeType = await _incomeTypeService.CreateAsync(createIncomeTypeDto, userId);

                return CreatedAtAction(nameof(GetById), new { id = incomeType.Id }, incomeType);
            }
            catch (KeyNotFoundException ex)
            {
                _logger.LogWarning(ex, "Usuário não encontrado");
                return NotFound(new { message = ex.Message });
            }
            catch (InvalidOperationException ex)
            {
                _logger.LogWarning(ex, "Operação inválida ao criar tipo de entrada");
                return BadRequest(new { message = ex.Message });
            }
        }

        [HttpPut("{id}")]
        public async Task<ActionResult<IncomeTypeDto>> Update(Guid id, UpdateIncomeTypeDto updateIncomeTypeDto)
        {
            _logger.LogInformation("Atualizando tipo de entrada com ID: {TypeId}", id);

            try
            {
                var existingType = await _incomeTypeService.GetByIdAsync(id);

                if (existingType.IsSystem)
                {
                    _logger.LogWarning("Usuário tentou atualizar um tipo de entrada do sistema");
                    return BadRequest(new { message = "Não é possível atualizar tipos de entrada do sistema" });
                }

                if (existingType.UserId != HttpContext.GetCurrentUserId())
                {
                    _logger.LogWarning("Usuário tentou atualizar tipo de entrada que não pertence a ele");
                    return Forbid();
                }

                var incomeType = await _incomeTypeService.UpdateAsync(id, updateIncomeTypeDto);
                return Ok(incomeType);
            }
            catch (KeyNotFoundException ex)
            {
                _logger.LogWarning(ex, "Tipo de entrada não encontrado");
                return NotFound(new { message = ex.Message });
            }
            catch (InvalidOperationException ex)
            {
                _logger.LogWarning(ex, "Operação inválida ao atualizar tipo de entrada");
                return BadRequest(new { message = ex.Message });
            }
        }

        [HttpDelete("{id}")]
        public async Task<ActionResult> Delete(Guid id)
        {
            _logger.LogInformation("Excluindo tipo de entrada com ID: {TypeId}", id);

            try
            {
                var existingType = await _incomeTypeService.GetByIdAsync(id);

                if (existingType.IsSystem)
                {
                    _logger.LogWarning("Usuário tentou excluir um tipo de entrada do sistema");
                    return BadRequest(new { message = "Não é possível excluir tipos de entrada do sistema" });
                }

                if (existingType.UserId != HttpContext.GetCurrentUserId())
                {
                    _logger.LogWarning("Usuário tentou excluir tipo de entrada que não pertence a ele");
                    return Forbid();
                }

                await _incomeTypeService.DeleteAsync(id);
                return NoContent();
            }
            catch (KeyNotFoundException ex)
            {
                _logger.LogWarning(ex, "Tipo de entrada não encontrado");
                return NotFound(new { message = ex.Message });
            }
            catch (InvalidOperationException ex)
            {
                _logger.LogWarning(ex, "Operação inválida ao excluir tipo de entrada");
                return BadRequest(new { message = ex.Message });
            }
        }
    }
}