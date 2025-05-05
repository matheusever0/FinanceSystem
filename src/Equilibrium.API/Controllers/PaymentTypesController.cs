using Equilibrium.API.Extensions;
using Equilibrium.Application.DTOs.PaymentType;
using Equilibrium.Application.Interfaces;
using Equilibrium.Domain.Interfaces.Services;
using Microsoft.AspNetCore.Mvc;

namespace Equilibrium.API.Controllers
{
    public class PaymentTypesController(IUnitOfWork unitOfWork,
        IPaymentTypeService service) : AuthenticatedController<IPaymentTypeService>(unitOfWork, service)
    {
        [HttpGet]
        public async Task<ActionResult<IEnumerable<PaymentTypeDto>>> GetAll()
        {
            var userId = HttpContext.GetCurrentUserId();
            var paymentTypes = await _service.GetAllAvailableForUserAsync(userId);
            return Ok(paymentTypes);
        }

        [HttpGet("system")]
        public async Task<ActionResult<IEnumerable<PaymentTypeDto>>> GetAllSystem()
        {
            var paymentTypes = await _service.GetAllSystemTypesAsync();
            return Ok(paymentTypes);
        }

        [HttpGet("user")]
        public async Task<ActionResult<IEnumerable<PaymentTypeDto>>> GetAllUser()
        {
            var userId = HttpContext.GetCurrentUserId();
            var paymentTypes = await _service.GetUserTypesAsync(userId);
            return Ok(paymentTypes);
        }

        [HttpGet("{id}")]
        public async Task<ActionResult<PaymentTypeDto>> GetById(Guid id)
        {
            try
            {
                var paymentType = await _service.GetByIdAsync(id);

                return !paymentType.IsSystem && paymentType.UserId != HttpContext.GetCurrentUserId() ? (ActionResult<PaymentTypeDto>)Forbid() : (ActionResult<PaymentTypeDto>)Ok(paymentType);
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
                var paymentType = await _service.CreateAsync(createPaymentTypeDto, userId);
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
                var existingType = await _service.GetByIdAsync(id);

                if (existingType.IsSystem)
                    return BadRequest(new { message = "Cannot update system payment type" });

                if (existingType.UserId != HttpContext.GetCurrentUserId())
                    return Forbid();

                var paymentType = await _service.UpdateAsync(id, updatePaymentTypeDto);
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
                var existingType = await _service.GetByIdAsync(id);

                if (existingType.IsSystem)
                    return BadRequest(new { message = "Cannot delete system payment type" });

                if (existingType.UserId != HttpContext.GetCurrentUserId())
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
