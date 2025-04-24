using FinanceSystem.API.Extensions;
using FinanceSystem.Application.DTOs.InvestmentTransaction;
using FinanceSystem.Application.Interfaces;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace FinanceSystem.API.Controllers
{
    [Route("api/investment-transactions")]
    [ApiController]
    [Authorize]
    public class InvestmentTransactionsController : ControllerBase
    {
        private readonly IInvestmentTransactionService _transactionService;
        private readonly IInvestmentService _investmentService;

        public InvestmentTransactionsController(
            IInvestmentTransactionService transactionService,
            IInvestmentService investmentService)
        {
            _transactionService = transactionService;
            _investmentService = investmentService;
        }

        [HttpGet("investment/{investmentId}")]
        public async Task<ActionResult<IEnumerable<InvestmentTransactionDto>>> GetByInvestment(Guid investmentId)
        {
            try
            {
                var investment = await _investmentService.GetByIdAsync(investmentId);
                if (investment.UserId != HttpContext.GetCurrentUserId())
                    return Forbid();

                var transactions = await _transactionService.GetByInvestmentIdAsync(investmentId);
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
                var transaction = await _transactionService.GetByIdAsync(id);
                var investment = await _investmentService.GetByIdAsync(transaction.Id);

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
                var investment = await _investmentService.GetByIdAsync(investmentId);
                if (investment.UserId != HttpContext.GetCurrentUserId())
                    return Forbid();

                var transaction = await _transactionService.CreateAsync(investmentId, createDto);
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
                var transaction = await _transactionService.GetByIdAsync(id);
                var investment = await _investmentService.GetByIdAsync(transaction.InvestmentId);

                if (investment.UserId != HttpContext.GetCurrentUserId())
                    return Forbid();

                await _transactionService.DeleteAsync(id);
                return NoContent();
            }
            catch (KeyNotFoundException ex)
            {
                return NotFound(new { message = ex.Message });
            }
        }
    }
}
