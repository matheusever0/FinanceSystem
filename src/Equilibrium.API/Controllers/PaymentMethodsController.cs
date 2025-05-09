using Equilibrium.API.Extensions;
using Equilibrium.Application.DTOs.PaymentMethod;
using Equilibrium.Application.Interfaces;
using Equilibrium.Domain.Enums;
using Equilibrium.Domain.Interfaces.Services;
using Equilibrium.Application.DTOs.Common;
using Equilibrium.Domain.DTOs.Filters;
using Equilibrium.Application.Validations.Filters;
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

                [HttpGet("filter")]
        public async Task<ActionResult<PagedResult<PaymentMethodDto>>> GetFiltered([FromQuery] PaymentMethodFilter filter)
        {
            if (filter == null)
                filter = new PaymentMethodFilter();
                
            var validator = new PaymentMethodFilterValidator();
            var validationResult = await validator.ValidateAsync(filter);
            
            if (!validationResult.IsValid)
                return BadRequest(validationResult.Errors);
                
            var userId = HttpContext.GetCurrentUserId();
            var pagedResult = await _service.GetFilteredAsync(filter, userId);
            
            // Add pagination headers
            Response.Headers.Add("X-Pagination-Total", pagedResult.TotalCount.ToString());
            Response.Headers.Add("X-Pagination-Pages", pagedResult.TotalPages.ToString());
            Response.Headers.Add("X-Pagination-Page", pagedResult.PageNumber.ToString());
            Response.Headers.Add("X-Pagination-Size", pagedResult.PageSize.ToString());
            
            return Ok(pagedResult);
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

