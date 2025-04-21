using FinanceSystem.API.Extensions;
using FinanceSystem.Application.DTOs.PaymentType;
using FinanceSystem.Application.Interfaces;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace FinanceSystem.API.Controllers
{
    [Route("api/payment-types")]
    [ApiController]
    [Authorize]
    public class PaymentTypesController : ControllerBase
    {
        private readonly IPaymentTypeService _paymentTypeService;

        public PaymentTypesController(IPaymentTypeService paymentTypeService)
        {
            _paymentTypeService = paymentTypeService;
        }

        [HttpGet]
        public async Task<ActionResult<IEnumerable<PaymentTypeDto>>> GetAll()
        {
            var userId = HttpContext.GetCurrentUserId();
            var paymentTypes = await _paymentTypeService.GetAllAvailableForUserAsync(userId);
            return Ok(paymentTypes);
        }

        [HttpGet("system")]
        public async Task<ActionResult<IEnumerable<PaymentTypeDto>>> GetAllSystem()
        {
            var paymentTypes = await _paymentTypeService.GetAllSystemTypesAsync();
            return Ok(paymentTypes);
        }

        [HttpGet("user")]
        public async Task<ActionResult<IEnumerable<PaymentTypeDto>>> GetAllUser()
        {
            var userId = HttpContext.GetCurrentUserId();
            var paymentTypes = await _paymentTypeService.GetUserTypesAsync(userId);
            return Ok(paymentTypes);
        }

        [HttpGet("{id}")]
        public async Task<ActionResult<PaymentTypeDto>> GetById(Guid id)
        {
            try
            {
                var paymentType = await _paymentTypeService.GetByIdAsync(id);

                if (!paymentType.IsSystem && paymentType.UserId != HttpContext.GetCurrentUserId())
                    return Forbid();

                return Ok(paymentType);
            }
            catch (KeyNotFoundException ex)
            {
                return NotFound(new { message = ex.Message });
            }
        }

        [HttpPost]
        public async Task<ActionResult<PaymentTypeDto>> Create(CreatePaymentTypeDto createPaymentTypeDto)
        {
            try
            {
                var userId = HttpContext.GetCurrentUserId();
                var paymentType = await _paymentTypeService.CreateAsync(createPaymentTypeDto, userId);
                return CreatedAtAction(nameof(GetById), new { id = paymentType.Id }, paymentType);
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
        public async Task<ActionResult<PaymentTypeDto>> Update(Guid id, UpdatePaymentTypeDto updatePaymentTypeDto)
        {
            try
            {
                var existingType = await _paymentTypeService.GetByIdAsync(id);

                if (existingType.IsSystem)
                    return BadRequest(new { message = "Cannot update system payment type" });

                if (existingType.UserId != HttpContext.GetCurrentUserId())
                    return Forbid();

                var paymentType = await _paymentTypeService.UpdateAsync(id, updatePaymentTypeDto);
                return Ok(paymentType);
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
                var existingType = await _paymentTypeService.GetByIdAsync(id);

                if (existingType.IsSystem)
                    return BadRequest(new { message = "Cannot delete system payment type" });

                if (existingType.UserId != HttpContext.GetCurrentUserId())
                    return Forbid();

                await _paymentTypeService.DeleteAsync(id);
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
