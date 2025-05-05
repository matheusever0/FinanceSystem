using Equilibrium.API.Extensions;
using Equilibrium.Application.DTOs.InvestmentTransaction;
using Equilibrium.Application.Interfaces;
using Equilibrium.Domain.Interfaces.Services;
using Microsoft.AspNetCore.Mvc;

namespace Equilibrium.API.Controllers
{
    public class InvestmentTransactionsController(IUnitOfWork unitOfWork,
        IInvestmentTransactionService service,
        IInvestmentService investmentService
            ) : AuthenticatedController<IInvestmentTransactionService>(unitOfWork, service)
    {
        [HttpGet("investment/{investmentId}")]
        public async Task<ActionResult<IEnumerable<InvestmentTransactionDto>>> GetByInvestment(Guid investmentId)
        {
            try
            {
                var investment = await investmentService.GetByIdAsync(investmentId);
                if (investment.UserId != HttpContext.GetCurrentUserId())
                    return Forbid();

                var transactions = await _service.GetByInvestmentIdAsync(investmentId);
                return Ok(transactions);
            }
            catch (KeyNotFoundException ex)
            {
                return NotFound(new { message = ex.Message });
            }
        }

        [HttpGet("{id}")]
        public async Task<ActionResult<InvestmentTransactionDto>> GetById(Guid id)
        {
            try
            {
                var transaction = await _service.GetByIdAsync(id);
                var investment = await investmentService.GetByIdAsync(transaction.Id);

                return investment.UserId != HttpContext.GetCurrentUserId() ? (ActionResult<InvestmentTransactionDto>)Forbid() : (ActionResult<InvestmentTransactionDto>)Ok(transaction);
            }
            catch (KeyNotFoundException ex)
            {
                return NotFound(new { message = ex.Message });
            }
        }

        [HttpPost("investment/{investmentId}")]
        public async Task<ActionResult<InvestmentTransactionDto>> Create(
            Guid investmentId,
            CreateInvestmentTransactionDto createDto)
        {
            try
            {
                var investment = await investmentService.GetByIdAsync(investmentId);
                if (investment.UserId != HttpContext.GetCurrentUserId())
                    return Forbid();

                var transaction = await _service.CreateAsync(investmentId, createDto);
                return CreatedAtAction(nameof(GetById), new { id = transaction.Id }, transaction);
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
                var transaction = await _service.GetByIdAsync(id);
                var investment = await investmentService.GetByIdAsync(transaction.InvestmentId);

                if (investment.UserId != HttpContext.GetCurrentUserId())
                    return Forbid();

                await _service.DeleteAsync(id);
                return NoContent();
            }
            catch (KeyNotFoundException ex)
            {
                return NotFound(new { message = ex.Message });
            }
        }
    }
}
