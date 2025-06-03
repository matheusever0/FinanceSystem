// src/Equilibrium.API/Controllers/CreditCardInvoicesController.cs
using Equilibrium.API.Extensions;
using Equilibrium.Application.DTOs.CreditCard;
using Equilibrium.Application.Interfaces;
using Equilibrium.Domain.Interfaces.Services;
using Microsoft.AspNetCore.Mvc;

namespace Equilibrium.API.Controllers
{
    [Route("api/creditcards/{creditCardId}/invoices")]
    public class CreditCardInvoicesController : AuthenticatedController<ICreditCardInvoiceService>
    {
        private readonly ICreditCardService _creditCardService;

        public CreditCardInvoicesController(
            IUnitOfWork unitOfWork,
            ICreditCardInvoiceService service,
            ICreditCardService creditCardService) : base(unitOfWork, service)
        {
            _creditCardService = creditCardService;
        }

        [HttpGet("current")]
        public async Task<ActionResult<CreditCardInvoiceDto>> GetCurrentInvoice(Guid creditCardId)
        {
            try
            {
                // Verificar se o cartão pertence ao usuário
                var creditCard = await _creditCardService.GetByIdAsync(creditCardId);
                if (creditCard.UserId != HttpContext.GetCurrentUserId())
                    return Forbid();

                var invoice = await _service.GetCurrentInvoiceAsync(creditCardId);
                return Ok(invoice);
            }
            catch (KeyNotFoundException ex)
            {
                return NotFound(new { message = ex.Message });
            }
        }

        [HttpGet("history")]
        public async Task<ActionResult<IEnumerable<CreditCardInvoiceDto>>> GetInvoiceHistory(
            Guid creditCardId,
            [FromQuery] int months = 12)
        {
            try
            {
                // Verificar se o cartão pertence ao usuário
                var creditCard = await _creditCardService.GetByIdAsync(creditCardId);
                if (creditCard.UserId != HttpContext.GetCurrentUserId())
                    return Forbid();

                var invoices = await _service.GetInvoiceHistoryAsync(creditCardId, months);
                return Ok(invoices);
            }
            catch (KeyNotFoundException ex)
            {
                return NotFound(new { message = ex.Message });
            }
        }

        [HttpGet("period")]
        public async Task<ActionResult<CreditCardInvoiceDto>> GetInvoiceByPeriod(
            Guid creditCardId,
            [FromQuery] DateTime startDate,
            [FromQuery] DateTime endDate)
        {
            try
            {
                // Verificar se o cartão pertence ao usuário
                var creditCard = await _creditCardService.GetByIdAsync(creditCardId);
                if (creditCard.UserId != HttpContext.GetCurrentUserId())
                    return Forbid();

                var invoice = await _service.GetInvoiceByPeriodAsync(creditCardId, startDate, endDate);
                return Ok(invoice);
            }
            catch (KeyNotFoundException ex)
            {
                return NotFound(new { message = ex.Message });
            }
        }

        [HttpGet("details")]
        public async Task<ActionResult<CreditCardInvoiceDetailDto>> GetInvoiceDetails(
            Guid creditCardId,
            [FromQuery] DateTime? referenceDate = null)
        {
            try
            {
                // Verificar se o cartão pertence ao usuário
                var creditCard = await _creditCardService.GetByIdAsync(creditCardId);
                if (creditCard.UserId != HttpContext.GetCurrentUserId())
                    return Forbid();

                var reference = referenceDate ?? DateTime.Now;
                var details = await _service.GetInvoiceDetailsAsync(creditCardId, reference);
                return Ok(details);
            }
            catch (KeyNotFoundException ex)
            {
                return NotFound(new { message = ex.Message });
            }
        }

        [HttpPost("pay")]
        public async Task<ActionResult<CreditCardDto>> PayInvoice(
            Guid creditCardId,
            [FromBody] PayInvoiceDto payInvoiceDto)
        {
            try
            {
                // Verificar se o cartão pertence ao usuário
                var creditCard = await _creditCardService.GetByIdAsync(creditCardId);
                if (creditCard.UserId != HttpContext.GetCurrentUserId())
                    return Forbid();

                var updatedCreditCard = await _service.PayInvoiceAsync(creditCardId, payInvoiceDto);
                return Ok(updatedCreditCard);
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