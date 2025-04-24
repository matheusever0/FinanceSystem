using FinanceSystem.API.Extensions;
using FinanceSystem.Application.DTOs.Investment;
using FinanceSystem.Application.Interfaces;
using FinanceSystem.Domain.Enums;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace FinanceSystem.API.Controllers
{
    [Route("api/investments")]
    [ApiController]
    [Authorize]
    public class InvestmentsController : ControllerBase
    {
        private readonly IInvestmentService _investmentService;

        public InvestmentsController(IInvestmentService investmentService)
        {
            _investmentService = investmentService;
        }

        [HttpGet]
        public async Task<ActionResult<IEnumerable<InvestmentDto>>> GetAll()
        {
            var userId = HttpContext.GetCurrentUserId();
            var investments = await _investmentService.GetAllByUserIdAsync(userId);
            return Ok(investments);
        }

        [HttpGet("{id}")]
        public async Task<ActionResult<InvestmentDto>> GetById(Guid id)
        {
            try
            {
                var investment = await _investmentService.GetByIdAsync(id);

                return investment.UserId != HttpContext.GetCurrentUserId() ? (ActionResult<InvestmentDto>)Forbid() : (ActionResult<InvestmentDto>)Ok(investment);
            }
            catch (KeyNotFoundException ex)
            {
                return NotFound(new { message = ex.Message });
            }
        }

        [HttpGet("type/{type}")]
        public async Task<ActionResult<IEnumerable<InvestmentDto>>> GetByType(InvestmentType type)
        {
            var userId = HttpContext.GetCurrentUserId();
            var investments = await _investmentService.GetByTypeAsync(userId, type);
            return Ok(investments);
        }

        [HttpPost]
        public async Task<ActionResult<InvestmentDto>> Create(CreateInvestmentDto createInvestmentDto)
        {
            try
            {
                var userId = HttpContext.GetCurrentUserId();
                var investment = await _investmentService.CreateAsync(createInvestmentDto, userId);
                return CreatedAtAction(nameof(GetById), new { id = investment.Id }, investment);
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
        public async Task<ActionResult<InvestmentDto>> Update(Guid id, UpdateInvestmentDto updateInvestmentDto)
        {
            try
            {
                var investment = await _investmentService.GetByIdAsync(id);
                if (investment.UserId != HttpContext.GetCurrentUserId())
                    return Forbid();

                var updatedInvestment = await _investmentService.UpdateAsync(id, updateInvestmentDto);
                return Ok(updatedInvestment);
            }
            catch (KeyNotFoundException ex)
            {
                return NotFound(new { message = ex.Message });
            }
        }

        [HttpDelete("{id}")]
        public async Task<ActionResult> Delete(Guid id)
        {
            try
            {
                var investment = await _investmentService.GetByIdAsync(id);
                if (investment.UserId != HttpContext.GetCurrentUserId())
                    return Forbid();

                await _investmentService.DeleteAsync(id);
                return NoContent();
            }
            catch (KeyNotFoundException ex)
            {
                return NotFound(new { message = ex.Message });
            }
        }

        [HttpPost("{id}/refresh")]
        public async Task<ActionResult<InvestmentDto>> RefreshPrice(Guid id)
        {
            try
            {
                var investment = await _investmentService.GetByIdAsync(id);
                if (investment.UserId != HttpContext.GetCurrentUserId())
                    return Forbid();

                var updatedInvestment = await _investmentService.RefreshPriceAsync(id);
                return Ok(updatedInvestment);
            }
            catch (KeyNotFoundException ex)
            {
                return NotFound(new { message = ex.Message });
            }
            catch (Exception ex)
            {
                return BadRequest(new { message = ex.Message });
            }
        }

        [HttpPost("refresh-all")]
        public async Task<ActionResult<IEnumerable<InvestmentDto>>> RefreshAllPrices()
        {
            try
            {
                var userId = HttpContext.GetCurrentUserId();
                var updatedInvestments = await _investmentService.RefreshAllPricesAsync(userId);
                return Ok(updatedInvestments);
            }
            catch (Exception ex)
            {
                return BadRequest(new { message = ex.Message });
            }
        }
    }
}
