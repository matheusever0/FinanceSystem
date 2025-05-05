using Equilibrium.API.Extensions;
using Equilibrium.Application.DTOs.PaymentMethod;
using Equilibrium.Application.Interfaces;
using Equilibrium.Domain.Enums;
using Equilibrium.Domain.Interfaces.Services;
using Microsoft.AspNetCore.Mvc;

namespace Equilibrium.API.Controllers
{
    public class PaymentMethodsController(IUnitOfWork unitOfWork,
        IPaymentMethodService service) : AuthenticatedController<IPaymentMethodService>(unitOfWork, service)
    {
        [HttpGet]
        public async Task<ActionResult<IEnumerable<PaymentMethodDto>>> GetAll()
        {
            var userId = HttpContext.GetCurrentUserId();
            var paymentMethods = await _service.GetAllAvailableForUserAsync(userId);
            return Ok(paymentMethods);
        }

        [HttpGet("system")]
        public async Task<ActionResult<IEnumerable<PaymentMethodDto>>> GetAllSystem()
        {
            var paymentMethods = await _service.GetAllSystemMethodsAsync();
            return Ok(paymentMethods);
        }

        [HttpGet("user")]
        public async Task<ActionResult<IEnumerable<PaymentMethodDto>>> GetAllUser()
        {
            var userId = HttpContext.GetCurrentUserId();
            var paymentMethods = await _service.GetUserMethodsAsync(userId);
            return Ok(paymentMethods);
        }

        [HttpGet("type/{type}")]
        public async Task<ActionResult<IEnumerable<PaymentMethodDto>>> GetByType(PaymentMethodType type)
        {
            var paymentMethods = await _service.GetByTypeAsync(type);
            return Ok(paymentMethods);
        }

        [HttpGet("{id}")]
        public async Task<ActionResult<PaymentMethodDto>> GetById(Guid id)
        {
            try
            {
                var paymentMethod = await _service.GetByIdAsync(id);

                return !paymentMethod.IsSystem && paymentMethod.UserId != HttpContext.GetCurrentUserId() ? (ActionResult<PaymentMethodDto>)Forbid() : (ActionResult<PaymentMethodDto>)Ok(paymentMethod);
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
                var paymentMethod = await _service.CreateAsync(createPaymentMethodDto, userId);
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
                var existingMethod = await _service.GetByIdAsync(id);

                if (existingMethod.IsSystem)
                    return BadRequest(new { message = "Cannot update system payment method" });

                if (existingMethod.UserId != HttpContext.GetCurrentUserId())
                    return Forbid();

                var paymentMethod = await _service.UpdateAsync(id, updatePaymentMethodDto);
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
                var existingMethod = await _service.GetByIdAsync(id);

                if (existingMethod.IsSystem)
                    return BadRequest(new { message = "Cannot delete system payment method" });

                if (existingMethod.UserId != HttpContext.GetCurrentUserId())
                    return Forbid();

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
    }
}
