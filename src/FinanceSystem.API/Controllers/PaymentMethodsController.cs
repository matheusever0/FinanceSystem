using FinanceSystem.API.Extensions;
using FinanceSystem.Application.DTOs.PaymentMethod;
using FinanceSystem.Application.Interfaces;
using FinanceSystem.Domain.Enums;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace FinanceSystem.API.Controllers
{
    [Route("api/payment-methods")]
    [ApiController]
    [Authorize]
    public class PaymentMethodsController : ControllerBase
    {
        private readonly IPaymentMethodService _paymentMethodService;

        public PaymentMethodsController(IPaymentMethodService paymentMethodService)
        {
            _paymentMethodService = paymentMethodService;
        }

        [HttpGet]
        public async Task<ActionResult<IEnumerable<PaymentMethodDto>>> GetAll()
        {
            var userId = HttpContext.GetCurrentUserId();
            var paymentMethods = await _paymentMethodService.GetAllAvailableForUserAsync(userId);
            return Ok(paymentMethods);
        }

        [HttpGet("system")]
        public async Task<ActionResult<IEnumerable<PaymentMethodDto>>> GetAllSystem()
        {
            var paymentMethods = await _paymentMethodService.GetAllSystemMethodsAsync();
            return Ok(paymentMethods);
        }

        [HttpGet("user")]
        public async Task<ActionResult<IEnumerable<PaymentMethodDto>>> GetAllUser()
        {
            var userId = HttpContext.GetCurrentUserId();
            var paymentMethods = await _paymentMethodService.GetUserMethodsAsync(userId);
            return Ok(paymentMethods);
        }

        [HttpGet("type/{type}")]
        public async Task<ActionResult<IEnumerable<PaymentMethodDto>>> GetByType(PaymentMethodType type)
        {
            var paymentMethods = await _paymentMethodService.GetByTypeAsync(type);
            return Ok(paymentMethods);
        }

        [HttpGet("{id}")]
        public async Task<ActionResult<PaymentMethodDto>> GetById(Guid id)
        {
            try
            {
                var paymentMethod = await _paymentMethodService.GetByIdAsync(id);

                if (!paymentMethod.IsSystem && paymentMethod.UserId != HttpContext.GetCurrentUserId())
                    return Forbid();

                return Ok(paymentMethod);
            }
            catch (KeyNotFoundException ex)
            {
                return NotFound(new { message = ex.Message });
            }
        }

        [HttpPost]
        public async Task<ActionResult<PaymentMethodDto>> Create(CreatePaymentMethodDto createPaymentMethodDto)
        {
            try
            {
                var userId = HttpContext.GetCurrentUserId();
                var paymentMethod = await _paymentMethodService.CreateAsync(createPaymentMethodDto, userId);
                return CreatedAtAction(nameof(GetById), new { id = paymentMethod.Id }, paymentMethod);
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
        public async Task<ActionResult<PaymentMethodDto>> Update(Guid id, UpdatePaymentMethodDto updatePaymentMethodDto)
        {
            try
            {
                var existingMethod = await _paymentMethodService.GetByIdAsync(id);

                if (existingMethod.IsSystem)
                    return BadRequest(new { message = "Cannot update system payment method" });

                if (existingMethod.UserId != HttpContext.GetCurrentUserId())
                    return Forbid();

                var paymentMethod = await _paymentMethodService.UpdateAsync(id, updatePaymentMethodDto);
                return Ok(paymentMethod);
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
                var existingMethod = await _paymentMethodService.GetByIdAsync(id);

                if (existingMethod.IsSystem)
                    return BadRequest(new { message = "Cannot delete system payment method" });

                if (existingMethod.UserId != HttpContext.GetCurrentUserId())
                    return Forbid();

                await _paymentMethodService.DeleteAsync(id);
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
