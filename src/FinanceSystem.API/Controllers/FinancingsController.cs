using FinanceSystem.API.Extensions;
using FinanceSystem.Application.DTOs.Financing;
using FinanceSystem.Application.Interfaces;
using FinanceSystem.Domain.Enums;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace FinanceSystem.API.Controllers
{
    [Route("api/financings")]
    [ApiController]
    [Authorize]
    public class FinancingsController : ControllerBase
    {
        private readonly IFinancingService _financingService;

        public FinancingsController(IFinancingService financingService)
        {
            _financingService = financingService;
        }

        [HttpGet]
        public async Task<ActionResult<IEnumerable<FinancingDto>>> GetAll()
        {
            var userId = HttpContext.GetCurrentUserId();
            var financings = await _financingService.GetAllByUserIdAsync(userId);
            return Ok(financings);
        }

        [HttpGet("active")]
        public async Task<ActionResult<IEnumerable<FinancingDto>>> GetActive()
        {
            var userId = HttpContext.GetCurrentUserId();
            var financings = await _financingService.GetActiveFinancingsByUserIdAsync(userId);
            return Ok(financings);
        }

        [HttpGet("status/{status}")]
        public async Task<ActionResult<IEnumerable<FinancingDto>>> GetByStatus(FinancingStatus status)
        {
            var userId = HttpContext.GetCurrentUserId();
            var financings = await _financingService.GetFinancingsByStatusAsync(userId, status);
            return Ok(financings);
        }

        [HttpGet("total-debt")]
        public async Task<ActionResult<decimal>> GetTotalRemainingDebt()
        {
            var userId = HttpContext.GetCurrentUserId();
            var totalDebt = await _financingService.GetTotalRemainingDebtByUserIdAsync(userId);
            return Ok(totalDebt);
        }

        [HttpGet("{id}")]
        public async Task<ActionResult<FinancingDto>> GetById(Guid id)
        {
            try
            {
                var financing = await _financingService.GetByIdAsync(id);

                if (financing.UserId != HttpContext.GetCurrentUserId())
                    return Forbid();

                return Ok(financing);
            }
            catch (KeyNotFoundException ex)
            {
                return NotFound(new { message = ex.Message });
            }
        }

        [HttpGet("{id}/details")]
        public async Task<ActionResult<FinancingDetailDto>> GetDetails(Guid id)
        {
            try
            {
                var financing = await _financingService.GetDetailsByIdAsync(id);

                if (financing.UserId != HttpContext.GetCurrentUserId())
                    return Forbid();

                return Ok(financing);
            }
            catch (KeyNotFoundException ex)
            {
                return NotFound(new { message = ex.Message });
            }
        }

        [HttpPost]
        public async Task<ActionResult<FinancingDto>> Create(CreateFinancingDto createFinancingDto)
        {
            try
            {
                var userId = HttpContext.GetCurrentUserId();
                var financing = await _financingService.CreateAsync(createFinancingDto, userId);
                return CreatedAtAction(nameof(GetById), new { id = financing.Id }, financing);
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
        public async Task<ActionResult<FinancingDto>> Update(Guid id, UpdateFinancingDto updateFinancingDto)
        {
            try
            {
                var existingFinancing = await _financingService.GetByIdAsync(id);

                if (existingFinancing.UserId != HttpContext.GetCurrentUserId())
                    return Forbid();

                var financing = await _financingService.UpdateAsync(id, updateFinancingDto);
                return Ok(financing);
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

        [HttpPost("{id}/complete")]
        public async Task<ActionResult> Complete(Guid id)
        {
            try
            {
                var existingFinancing = await _financingService.GetByIdAsync(id);

                if (existingFinancing.UserId != HttpContext.GetCurrentUserId())
                    return Forbid();

                await _financingService.CompleteAsync(id);
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

        [HttpPost("{id}/cancel")]
        public async Task<ActionResult> Cancel(Guid id)
        {
            try
            {
                var existingFinancing = await _financingService.GetByIdAsync(id);

                // Verificar se o financiamento pertence ao usuário atual
                if (existingFinancing.UserId != HttpContext.GetCurrentUserId())
                    return Forbid();

                await _financingService.CancelAsync(id);
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

        [HttpPost("simulate")]
        public async Task<ActionResult<FinancingSimulationDto>> Simulate(FinancingSimulationRequestDto simulationRequest)
        {
            try
            {
                var simulation = await _financingService.SimulateAsync(simulationRequest);
                return Ok(simulation);
            }
            catch (Exception ex)
            {
                return BadRequest(new { message = ex.Message });
            }
        }

        [HttpPost("forecast")]
        public async Task<ActionResult<FinancingForecastDto>> Forecast(FinancingForecastRequestDto forecastRequest)
        {
            try
            {
                var existingFinancing = await _financingService.GetByIdAsync(forecastRequest.FinancingId);

                if (existingFinancing.UserId != HttpContext.GetCurrentUserId())
                    return Forbid();

                var forecast = await _financingService.ForecastAsync(forecastRequest);
                return Ok(forecast);
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