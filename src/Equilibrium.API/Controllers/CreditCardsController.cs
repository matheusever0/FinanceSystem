using Equilibrium.API.Extensions;
using Equilibrium.Application.DTOs.CreditCard;
using Equilibrium.Application.Interfaces;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace Equilibrium.API.Controllers
{
    [Route("api/credit-cards")]
    [ApiController]
    [Authorize]
    public class CreditCardsController : ControllerBase
    {
        private readonly ICreditCardService _creditCardService;

        public CreditCardsController(ICreditCardService creditCardService)
        {
            _creditCardService = creditCardService;
        }

        [HttpGet]
        public async Task<ActionResult<IEnumerable<CreditCardDto>>> GetAll()
        {
            var userId = HttpContext.GetCurrentUserId();
            var creditCards = await _creditCardService.GetByUserIdAsync(userId);

            return Ok(creditCards);
        }

        [HttpGet("{id}")]
        public async Task<ActionResult<CreditCardDto>> GetById(Guid id)
        {
            try
            {
                var creditCard = await _creditCardService.GetByIdAsync(id);

                return creditCard.UserId != HttpContext.GetCurrentUserId() ? (ActionResult<CreditCardDto>)Forbid() : (ActionResult<CreditCardDto>)Ok(creditCard);
            }
            catch (KeyNotFoundException ex)
            {
                return NotFound(new { message = ex.Message });
            }
        }

        [HttpPost]
        public async Task<ActionResult<CreditCardDto>> Create(CreateCreditCardDto createCreditCardDto)
        {
            try
            {
                var userId = HttpContext.GetCurrentUserId();
                var creditCard = await _creditCardService.CreateAsync(createCreditCardDto, userId);

                return CreatedAtAction(nameof(GetById), new { id = creditCard.Id }, creditCard);
            }
            catch (KeyNotFoundException ex)
            {
                return NotFound(new { message = ex.Message });
            }
            catch (UnauthorizedAccessException)
            {
                return Forbid();
            }
            catch (InvalidOperationException ex)
            {
                return BadRequest(new { message = ex.Message });
            }
            catch (ArgumentException ex)
            {
                return BadRequest(new { message = ex.Message });
            }
        }

        [HttpPut("{id}")]
        public async Task<ActionResult<CreditCardDto>> Update(Guid id, UpdateCreditCardDto updateCreditCardDto)
        {
            try
            {
                var existingCard = await _creditCardService.GetByIdAsync(id);
                if (existingCard.UserId != HttpContext.GetCurrentUserId())
                {
                    return Forbid();
                }

                var creditCard = await _creditCardService.UpdateAsync(id, updateCreditCardDto);
                return Ok(creditCard);
            }
            catch (KeyNotFoundException ex)
            {
                return NotFound(new { message = ex.Message });
            }
            catch (ArgumentException ex)
            {
                return BadRequest(new { message = ex.Message });
            }
        }

        [HttpDelete("{id}")]
        public async Task<ActionResult> Delete(Guid id)
        {
            try
            {
                var existingCard = await _creditCardService.GetByIdAsync(id);
                if (existingCard.UserId != HttpContext.GetCurrentUserId())
                {
                    return Forbid();
                }

                await _creditCardService.DeleteAsync(id);
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
