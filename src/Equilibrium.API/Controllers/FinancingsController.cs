using Equilibrium.API.Extensions;
using Equilibrium.Application.DTOs.Financing;
using Equilibrium.Application.Interfaces;
using Equilibrium.Domain.Enums;
using Equilibrium.Domain.Interfaces.Services;
using Microsoft.AspNetCore.Mvc;

namespace Equilibrium.API.Controllers
{
    public class FinancingsController : AuthenticatedController<IFinancingService>
    {
        public FinancingsController(IUnitOfWork unitOfWork, IFinancingService service) : base(unitOfWork, service)
        {
        }

        [HttpGet]
        public async Task<ActionResult<IEnumerable<FinancingDto>>> GetAll()
        {
            var userId = HttpContext.GetCurrentUserId();
            var financings = await _service.GetAllByUserIdAsync(userId);
            return Ok(financings);
        }

        [HttpGet("active")]
        public async Task<ActionResult<IEnumerable<FinancingDto>>> GetActive()
        {
            var userId = HttpContext.GetCurrentUserId();
            var financings = await _service.GetActiveFinancingsByUserIdAsync(userId);
            return Ok(financings);
        }

        [HttpGet("status/{status}")]
        public async Task<ActionResult<IEnumerable<FinancingDto>>> GetByStatus(FinancingStatus status)
        {
            var userId = HttpContext.GetCurrentUserId();
            var financings = await _service.GetFinancingsByStatusAsync(userId, status);
            return Ok(financings);
        }

        [HttpGet("total-debt")]
        public async Task<ActionResult<decimal>> GetTotalRemainingDebt()
        {
            var userId = HttpContext.GetCurrentUserId();
            var totalDebt = await _service.GetTotalRemainingDebtByUserIdAsync(userId);
            return Ok(totalDebt);
        }

        [HttpGet("{id}")]
        public async Task<ActionResult<FinancingDto>> GetById(Guid id)
        {
            try
            {
                var financing = await _service.GetByIdAsync(id);

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
                var financing = await _service.GetDetailsByIdAsync(id);

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
                var financing = await _service.CreateAsync(createFinancingDto, userId);
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
                var existingFinancing = await _service.GetByIdAsync(id);

                if (existingFinancing.UserId != HttpContext.GetCurrentUserId())
                    return Forbid();

                var financing = await _service.UpdateAsync(id, updateFinancingDto);
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
                var existingFinancing = await _service.GetByIdAsync(id);

                if (existingFinancing.UserId != HttpContext.GetCurrentUserId())
                    return Forbid();

                await _service.CompleteAsync(id);
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
                var existingFinancing = await _service.GetByIdAsync(id);

                // Verificar se o financiamento pertence ao usuário atual
                if (existingFinancing.UserId != HttpContext.GetCurrentUserId())
                    return Forbid();

                await _service.CancelAsync(id);
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
                var simulation = await _service.SimulateAsync(simulationRequest);
                return Ok(simulation);
            }
            catch (Exception ex)
            {
                return BadRequest(new { message = ex.Message });
            }
        }
    }
}