using Equilibrium.API.Extensions;
using Equilibrium.Application.DTOs.CreditCard;
using Equilibrium.Application.Interfaces;
using Equilibrium.Domain.Interfaces.Services;
using Microsoft.AspNetCore.Mvc;

namespace Equilibrium.API.Controllers
{
    public class CreditCardsController(IUnitOfWork unitOfWork, 
        ICreditCardService service) : AuthenticatedController<ICreditCardService>(unitOfWork, service)
    {
        [HttpGet]
        public async Task<ActionResult<IEnumerable<CreditCardDto>>> GetAll()
        {
            var userId = HttpContext.GetCurrentUserId();
            var creditCards = await _service.GetByUserIdAsync(userId);

            return Ok(creditCards);
        }

        [HttpGet("{id}")]
        public async Task<ActionResult<CreditCardDto>> GetById(Guid id)
        {
            try
            {
                var creditCard = await _service.GetByIdAsync(id);

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
                var creditCard = await _service.CreateAsync(createCreditCardDto, userId);

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
                var existingCard = await _service.GetByIdAsync(id);
                if (existingCard.UserId != HttpContext.GetCurrentUserId())
                {
                    return Forbid();
                }

                var creditCard = await _service.UpdateAsync(id, updateCreditCardDto);
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
                var existingCard = await _service.GetByIdAsync(id);
                if (existingCard.UserId != HttpContext.GetCurrentUserId())
                {
                    return Forbid();
                }

                await _service.DeleteAsync(id);
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


        [HttpGet("{id}/current-invoice")]
        public async Task<ActionResult<CreditCardInvoiceDto>> GetCurrentInvoice(Guid id)
        {
            try
            {
                var creditCard = await _service.GetByIdAsync(id);
                if (creditCard.UserId != HttpContext.GetCurrentUserId())
                    return Forbid();

                // Aqui você injetaria o ICreditCardInvoiceService
                // var invoice = await _invoiceService.GetCurrentInvoiceAsync(id);
                // return Ok(invoice);

                return NoContent(); // Temporário até injetar o serviço
            }
            catch (KeyNotFoundException ex)
            {
                return NotFound(new { message = ex.Message });
            }
        }

        [HttpPost("{id}/pay-invoice")]
        public async Task<ActionResult<CreditCardDto>> PayInvoice(Guid id, [FromBody] PayInvoiceDto payInvoiceDto)
        {
            try
            {
                var creditCard = await _service.GetByIdAsync(id);
                if (creditCard.UserId != HttpContext.GetCurrentUserId())
                    return Forbid();

                // Aqui você injetaria o ICreditCardInvoiceService
                // var updatedCard = await _invoiceService.PayInvoiceAsync(id, payInvoiceDto);
                // return Ok(updatedCard);

                return NoContent(); // Temporário até injetar o serviço
            }
            catch (KeyNotFoundException ex)
            {
                return NotFound(new { message = ex.Message });
            }
        }
    }
}

