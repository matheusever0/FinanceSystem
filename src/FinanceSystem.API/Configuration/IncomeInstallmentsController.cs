using FinanceSystem.Application.DTOs.IncomeInstallment;
using FinanceSystem.Application.Interfaces;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System.Security.Claims;

namespace FinanceSystem.API.Controllers
{
    [Route("api/income-installments")]
    [ApiController]
    [Authorize]
    public class IncomeInstallmentsController : ControllerBase
    {
        private readonly IIncomeInstallmentService _incomeInstallmentService;
        private readonly IIncomeService _incomeService;
        private readonly ILogger<IncomeInstallmentsController> _logger;

        public IncomeInstallmentsController(
            IIncomeInstallmentService incomeInstallmentService,
            IIncomeService incomeService,
            ILogger<IncomeInstallmentsController> logger)
        {
            _incomeInstallmentService = incomeInstallmentService;
            _incomeService = incomeService;
            _logger = logger;
        }

        [HttpGet("income/{incomeId}")]
        public async Task<ActionResult<IEnumerable<IncomeInstallmentDto>>> GetByIncome(Guid incomeId)
        {
            _logger.LogInformation("Obtendo parcelas para a entrada ID: {IncomeId}", incomeId);

            try
            {
                var income = await _incomeService.GetByIdAsync(incomeId);
                if (income.UserId != GetCurrentUserId())
                {
                    _logger.LogWarning("Usuário tentou acessar parcelas para uma entrada que não pertence a ele");
                    return Forbid();
                }

                var installments = await _incomeInstallmentService.GetByIncomeIdAsync(incomeId);
                return Ok(installments);
            }
            catch (KeyNotFoundException ex)
            {
                _logger.LogWarning(ex, "Entrada não encontrada");
                return NotFound(new { message = ex.Message });
            }
        }

        [HttpGet("due-date")]
        public async Task<ActionResult<IEnumerable<IncomeInstallmentDto>>> GetByDueDate([FromQuery] DateTime startDate, [FromQuery] DateTime endDate)
        {
            _logger.LogInformation("Obtendo parcelas entre {StartDate} e {EndDate}", startDate, endDate);

            var userId = GetCurrentUserId();
            var installments = await _incomeInstallmentService.GetByDueDateAsync(userId, startDate, endDate);

            return Ok(installments);
        }

        [HttpGet("pending")]
        public async Task<ActionResult<IEnumerable<IncomeInstallmentDto>>> GetPending()
        {
            _logger.LogInformation("Obtendo parcelas pendentes");

            var userId = GetCurrentUserId();
            var installments = await _incomeInstallmentService.GetPendingAsync(userId);

            return Ok(installments);
        }

        [HttpGet("received")]
        public async Task<ActionResult<IEnumerable<IncomeInstallmentDto>>> GetReceived()
        {
            _logger.LogInformation("Obtendo parcelas recebidas");

            var userId = GetCurrentUserId();
            var installments = await _incomeInstallmentService.GetReceivedAsync(userId);

            return Ok(installments);
        }

        [HttpGet("{id}")]
        public async Task<ActionResult<IncomeInstallmentDto>> GetById(Guid id)
        {
            _logger.LogInformation("Obtendo parcela com ID: {InstallmentId}", id);

            try
            {
                var installment = await _incomeInstallmentService.GetByIdAsync(id);

                var income = await _incomeService.GetByIdAsync(installment.IncomeId);
                if (income.UserId != GetCurrentUserId())
                {
                    _logger.LogWarning("Usuário tentou acessar parcela que não pertence a sua entrada");
                    return Forbid();
                }

                return Ok(installment);
            }
            catch (KeyNotFoundException ex)
            {
                _logger.LogWarning(ex, "Parcela não encontrada");
                return NotFound(new { message = ex.Message });
            }
        }

        [HttpPost("{id}/received")]
        public async Task<ActionResult<IncomeInstallmentDto>> MarkAsReceived(Guid id, [FromBody] DateTime? receivedDate)
        {
            _logger.LogInformation("Marcando parcela com ID: {InstallmentId} como recebida", id);

            try
            {
                var installment = await _incomeInstallmentService.GetByIdAsync(id);

                var income = await _incomeService.GetByIdAsync(installment.IncomeId);
                if (income.UserId != GetCurrentUserId())
                {
                    _logger.LogWarning("Usuário tentou marcar como recebida uma parcela que não pertence a sua entrada");
                    return Forbid();
                }

                var date = receivedDate ?? DateTime.UtcNow;
                var updatedInstallment = await _incomeInstallmentService.MarkAsReceivedAsync(id, date);
                return Ok(updatedInstallment);
            }
            catch (KeyNotFoundException ex)
            {
                _logger.LogWarning(ex, "Parcela não encontrada");
                return NotFound(new { message = ex.Message });
            }
        }

        [HttpPost("{id}/cancel")]
        public async Task<ActionResult<IncomeInstallmentDto>> Cancel(Guid id)
        {
            _logger.LogInformation("Cancelando parcela com ID: {InstallmentId}", id);

            try
            {
                var installment = await _incomeInstallmentService.GetByIdAsync(id);

                var income = await _incomeService.GetByIdAsync(installment.IncomeId);
                if (income.UserId != GetCurrentUserId())
                {
                    _logger.LogWarning("Usuário tentou cancelar uma parcela que não pertence a sua entrada");
                    return Forbid();
                }

                var updatedInstallment = await _incomeInstallmentService.CancelAsync(id);
                return Ok(updatedInstallment);
            }
            catch (KeyNotFoundException ex)
            {
                _logger.LogWarning(ex, "Parcela não encontrada");
                return NotFound(new { message = ex.Message });
            }
        }

        private Guid GetCurrentUserId()
        {
            var userIdClaim = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
            return Guid.Parse(userIdClaim);
        }
    }
}