using FinanceSystem.Application.DTOs;
using FinanceSystem.Application.Interfaces;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System.Security.Claims;

namespace FinanceSystem.API.Controllers
{
    [Route("api/credit-cards")]
    [ApiController]
    [Authorize]
    public class CreditCardsController : ControllerBase
    {
        private readonly ICreditCardService _creditCardService;
        private readonly ILogger<CreditCardsController> _logger;

        public CreditCardsController(ICreditCardService creditCardService, ILogger<CreditCardsController> logger)
        {
            _creditCardService = creditCardService;
            _logger = logger;
        }

        [HttpGet]
        public async Task<ActionResult<IEnumerable<CreditCardDto>>> GetAll()
        {
            _logger.LogInformation("Getting all credit cards for user");

            var userId = GetCurrentUserId();
            var creditCards = await _creditCardService.GetByUserIdAsync(userId);

            return Ok(creditCards);
        }

        [HttpGet("{id}")]
        public async Task<ActionResult<CreditCardDto>> GetById(Guid id)
        {
            _logger.LogInformation("Getting credit card with ID: {CardId}", id);

            try
            {
                var creditCard = await _creditCardService.GetByIdAsync(id);

                //if (creditCard.UserId != GetCurrentUserId())
                //{
                //    _logger.LogWarning("User attempted to access credit card that doesn't belong to them");
                //    return Forbid();
                //}

                return Ok(creditCard);
            }
            catch (KeyNotFoundException ex)
            {
                _logger.LogWarning(ex, "Credit card not found");
                return NotFound(new { message = ex.Message });
            }
        }

        [HttpPost]
        public async Task<ActionResult<CreditCardDto>> Create(CreateCreditCardDto createCreditCardDto)
        {
            _logger.LogInformation("Creating new credit card");

            try
            {
                var userId = GetCurrentUserId();
                var creditCard = await _creditCardService.CreateAsync(createCreditCardDto, userId);

                return CreatedAtAction(nameof(GetById), new { id = creditCard.Id }, creditCard);
            }
            catch (KeyNotFoundException ex)
            {
                _logger.LogWarning(ex, "Referenced entity not found");
                return NotFound(new { message = ex.Message });
            }
            catch (UnauthorizedAccessException ex)
            {
                _logger.LogWarning(ex, "Unauthorized access when creating credit card");
                return Forbid();
            }
            catch (InvalidOperationException ex)
            {
                _logger.LogWarning(ex, "Invalid operation when creating credit card");
                return BadRequest(new { message = ex.Message });
            }
            catch (ArgumentException ex)
            {
                _logger.LogWarning(ex, "Invalid argument when creating credit card");
                return BadRequest(new { message = ex.Message });
            }
        }

        [HttpPut("{id}")]
        public async Task<ActionResult<CreditCardDto>> Update(Guid id, UpdateCreditCardDto updateCreditCardDto)
        {
            _logger.LogInformation("Updating credit card with ID: {CardId}", id);

            try
            {
                var existingCard = await _creditCardService.GetByIdAsync(id);
                //if (existingCard.UserId != GetCurrentUserId())
                //{
                //    _logger.LogWarning("User attempted to update credit card that doesn't belong to them");
                //    return Forbid();
                //}

                var creditCard = await _creditCardService.UpdateAsync(id, updateCreditCardDto);
                return Ok(creditCard);
            }
            catch (KeyNotFoundException ex)
            {
                _logger.LogWarning(ex, "Credit card not found");
                return NotFound(new { message = ex.Message });
            }
            catch (ArgumentException ex)
            {
                _logger.LogWarning(ex, "Invalid argument when updating credit card");
                return BadRequest(new { message = ex.Message });
            }
        }

        [HttpDelete("{id}")]
        public async Task<ActionResult> Delete(Guid id)
        {
            _logger.LogInformation("Deleting credit card with ID: {CardId}", id);

            try
            {
                var existingCard = await _creditCardService.GetByIdAsync(id);
                //if (existingCard.UserId != GetCurrentUserId())
                //{
                //    _logger.LogWarning("User attempted to delete credit card that doesn't belong to them");
                //    return Forbid();
                //}

                await _creditCardService.DeleteAsync(id);
                return NoContent();
            }
            catch (KeyNotFoundException ex)
            {
                _logger.LogWarning(ex, "Credit card not found");
                return NotFound(new { message = ex.Message });
            }
            catch (InvalidOperationException ex)
            {
                _logger.LogWarning(ex, "Invalid operation when deleting credit card");
                return BadRequest(new { message = ex.Message });
            }
        }

        private Guid GetCurrentUserId()
        {
            var userIdClaim = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
            return Guid.Parse(userIdClaim);
        }
    }
}
