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
        private readonly ILogger<IncomesController> _logger;

        public IncomesController(IIncomeService incomeService, ILogger<IncomesController> logger)
        {
            _incomeService = incomeService;
            _logger = logger;
        }

        [HttpGet]
        public async Task<ActionResult<IEnumerable<IncomeDto>>> GetAll()
        {
            _logger.LogInformation("Obtendo todas as entradas para o usuário");

            var userId = HttpContext.GetCurrentUserId();
            var incomes = await _incomeService.GetAllByUserIdAsync(userId);

            return Ok(incomes);
        }

        [HttpGet("{id}")]
        public async Task<ActionResult<IncomeDto>> GetById(Guid id)
        {
            _logger.LogInformation("Obtendo entrada com ID: {IncomeId}", id);

            try
            {
                var income = await _incomeService.GetByIdAsync(id);

                if (income.UserId != HttpContext.GetCurrentUserId())
                {
                    _logger.LogWarning("Usuário tentou acessar entrada que não pertence a ele");
                    return Forbid();
                }

                return Ok(income);
            }
            catch (KeyNotFoundException ex)
            {
                _logger.LogWarning(ex, "Entrada não encontrada");
                return NotFound(new { message = ex.Message });
            }
        }

        [HttpGet("month/{year}/{month}")]
        public async Task<ActionResult<IEnumerable<IncomeDto>>> GetByMonth(int year, int month)
        {
            _logger.LogInformation("Obtendo entradas para {Month}/{Year}", month, year);

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
            _logger.LogInformation("Obtendo entradas pendentes");

            var userId = HttpContext.GetCurrentUserId();
            var incomes = await _incomeService.GetPendingAsync(userId);

            return Ok(incomes);
        }

        [HttpGet("received")]
        public async Task<ActionResult<IEnumerable<IncomeDto>>> GetReceived()
        {
            _logger.LogInformation("Obtendo entradas recebidas");

            var userId = HttpContext.GetCurrentUserId();
            var incomes = await _incomeService.GetReceivedAsync(userId);

            return Ok(incomes);
        }

        [HttpGet("type/{typeId}")]
        public async Task<ActionResult<IEnumerable<IncomeDto>>> GetByType(Guid typeId)
        {
            _logger.LogInformation("Obtendo entradas por tipo ID: {TypeId}", typeId);

            var userId = HttpContext.GetCurrentUserId();
            var incomes = await _incomeService.GetByTypeAsync(userId, typeId);

            return Ok(incomes);
        }

        [HttpPost]
        public async Task<ActionResult<IncomeDto>> Create(CreateIncomeDto createIncomeDto)
        {
            _logger.LogInformation("Criando nova entrada");

            try
            {
                var userId = HttpContext.GetCurrentUserId();
                var income = await _incomeService.CreateAsync(createIncomeDto, userId);

                return CreatedAtAction(nameof(GetById), new { id = income.Id }, income);
            }
            catch (KeyNotFoundException ex)
            {
                _logger.LogWarning(ex, "Entidade referenciada não encontrada");
                return NotFound(new { message = ex.Message });
            }
            catch (UnauthorizedAccessException ex)
            {
                _logger.LogWarning(ex, "Acesso não autorizado ao criar entrada");
                return Forbid();
            }
            catch (InvalidOperationException ex)
            {
                _logger.LogWarning(ex, "Operação inválida ao criar entrada");
                return BadRequest(new { message = ex.Message });
            }
        }

        [HttpPut("{id}")]
        public async Task<ActionResult<IncomeDto>> Update(Guid id, UpdateIncomeDto updateIncomeDto)
        {
            _logger.LogInformation("Atualizando entrada com ID: {IncomeId}", id);

            try
            {
                var existingIncome = await _incomeService.GetByIdAsync(id);
                if (existingIncome.UserId != HttpContext.GetCurrentUserId())
                {
                    _logger.LogWarning("Usuário tentou atualizar entrada que não pertence a ele");
                    return Forbid();
                }

                var income = await _incomeService.UpdateAsync(id, updateIncomeDto);
                return Ok(income);
            }
            catch (KeyNotFoundException ex)
            {
                _logger.LogWarning(ex, "Entrada não encontrada");
                return NotFound(new { message = ex.Message });
            }
            catch (UnauthorizedAccessException ex)
            {
                _logger.LogWarning(ex, "Acesso não autorizado ao atualizar entrada");
                return Forbid();
            }
            catch (InvalidOperationException ex)
            {
                _logger.LogWarning(ex, "Operação inválida ao atualizar entrada");
                return BadRequest(new { message = ex.Message });
            }
        }

        [HttpDelete("{id}")]
        public async Task<ActionResult> Delete(Guid id)
        {
            _logger.LogInformation("Excluindo entrada com ID: {IncomeId}", id);

            try
            {
                var existingIncome = await _incomeService.GetByIdAsync(id);
                if (existingIncome.UserId != HttpContext.GetCurrentUserId())
                {
                    _logger.LogWarning("Usuário tentou excluir entrada que não pertence a ele");
                    return Forbid();
                }

                await _incomeService.DeleteAsync(id);
                return NoContent();
            }
            catch (KeyNotFoundException ex)
            {
                _logger.LogWarning(ex, "Entrada não encontrada");
                return NotFound(new { message = ex.Message });
            }
            catch (InvalidOperationException ex)
            {
                _logger.LogWarning(ex, "Operação inválida ao excluir entrada");
                return BadRequest(new { message = ex.Message });
            }
        }

        [HttpPost("{id}/received")]
        public async Task<ActionResult<IncomeDto>> MarkAsReceived(Guid id, [FromBody] DateTime? receivedDate)
        {
            _logger.LogInformation("Marcando entrada com ID: {IncomeId} como recebida", id);

            try
            {
                var existingIncome = await _incomeService.GetByIdAsync(id);
                if (existingIncome.UserId != HttpContext.GetCurrentUserId())
                {
                    _logger.LogWarning("Usuário tentou marcar como recebida uma entrada que não pertence a ele");
                    return Forbid();
                }

                var date = receivedDate ?? DateTime.UtcNow;
                var income = await _incomeService.MarkAsReceivedAsync(id, date);
                return Ok(income);
            }
            catch (KeyNotFoundException ex)
            {
                _logger.LogWarning(ex, "Entrada não encontrada");
                return NotFound(new { message = ex.Message });
            }
        }

        [HttpPost("{id}/cancel")]
        public async Task<ActionResult<IncomeDto>> Cancel(Guid id)
        {
            _logger.LogInformation("Cancelando entrada com ID: {IncomeId}", id);

            try
            {
                var existingIncome = await _incomeService.GetByIdAsync(id);
                if (existingIncome.UserId != HttpContext.GetCurrentUserId())
                {
                    _logger.LogWarning("Usuário tentou cancelar uma entrada que não pertence a ele");
                    return Forbid();
                }

                var income = await _incomeService.CancelAsync(id);
                return Ok(income);
            }
            catch (KeyNotFoundException ex)
            {
                _logger.LogWarning(ex, "Entrada não encontrada");
                return NotFound(new { message = ex.Message });
            }
        }
    }
}