using FinanceSystem.Application.DTOs.PaymentMethod;
using FinanceSystem.Application.Interfaces;
using FinanceSystem.Domain.Enums;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System.Security.Claims;

namespace FinanceSystem.API.Controllers
{
    [Route("api/payment-methods")]
    [ApiController]
    [Authorize]
    public class PaymentMethodsController : ControllerBase
    {
        private readonly IPaymentMethodService _paymentMethodService;
        private readonly ILogger<PaymentMethodsController> _logger;

        public PaymentMethodsController(IPaymentMethodService paymentMethodService, ILogger<PaymentMethodsController> logger)
        {
            _paymentMethodService = paymentMethodService;
            _logger = logger;
        }

        [HttpGet]
        public async Task<ActionResult<IEnumerable<PaymentMethodDto>>> GetAll()
        {
            _logger.LogInformation("Getting all payment methods for user");

            var userId = GetCurrentUserId();
            var paymentMethods = await _paymentMethodService.GetAllAvailableForUserAsync(userId);

            return Ok(paymentMethods);
        }

        [HttpGet("system")]
        public async Task<ActionResult<IEnumerable<PaymentMethodDto>>> GetAllSystem()
        {
            _logger.LogInformation("Getting all system payment methods");

            var paymentMethods = await _paymentMethodService.GetAllSystemMethodsAsync();

            return Ok(paymentMethods);
        }

        [HttpGet("user")]
        public async Task<ActionResult<IEnumerable<PaymentMethodDto>>> GetAllUser()
        {
            _logger.LogInformation("Getting all user-defined payment methods");

            var userId = GetCurrentUserId();
            var paymentMethods = await _paymentMethodService.GetUserMethodsAsync(userId);

            return Ok(paymentMethods);
        }

        [HttpGet("type/{type}")]
        public async Task<ActionResult<IEnumerable<PaymentMethodDto>>> GetByType(PaymentMethodType type)
        {
            _logger.LogInformation("Getting payment methods by type: {Type}", type);

            var paymentMethods = await _paymentMethodService.GetByTypeAsync(type);

            return Ok(paymentMethods);
        }

        [HttpGet("{id}")]
        public async Task<ActionResult<PaymentMethodDto>> GetById(Guid id)
        {
            _logger.LogInformation("Getting payment method with ID: {MethodId}", id);

            try
            {
                var paymentMethod = await _paymentMethodService.GetByIdAsync(id);

                if (!paymentMethod.IsSystem && paymentMethod.UserId != GetCurrentUserId())
                {
                    _logger.LogWarning("User attempted to access payment method that doesn't belong to them");
                    return Forbid();
                }

                return Ok(paymentMethod);
            }
            catch (KeyNotFoundException ex)
            {
                _logger.LogWarning(ex, "Payment method not found");
                return NotFound(new { message = ex.Message });
            }
        }

        [HttpPost]
        public async Task<ActionResult<PaymentMethodDto>> Create(CreatePaymentMethodDto createPaymentMethodDto)
        {
            _logger.LogInformation("Creating new payment method");

            try
            {
                var userId = GetCurrentUserId();
                var paymentMethod = await _paymentMethodService.CreateAsync(createPaymentMethodDto, userId);

                return CreatedAtAction(nameof(GetById), new { id = paymentMethod.Id }, paymentMethod);
            }
            catch (KeyNotFoundException ex)
            {
                _logger.LogWarning(ex, "User not found");
                return NotFound(new { message = ex.Message });
            }
            catch (InvalidOperationException ex)
            {
                _logger.LogWarning(ex, "Invalid operation when creating payment method");
                return BadRequest(new { message = ex.Message });
            }
        }

        [HttpPut("{id}")]
        public async Task<ActionResult<PaymentMethodDto>> Update(Guid id, UpdatePaymentMethodDto updatePaymentMethodDto)
        {
            _logger.LogInformation("Updating payment method with ID: {MethodId}", id);

            try
            {
                var existingMethod = await _paymentMethodService.GetByIdAsync(id);

                if (existingMethod.IsSystem)
                {
                    _logger.LogWarning("User attempted to update a system payment method");
                    return BadRequest(new { message = "Cannot update system payment method" });
                }

                if (existingMethod.UserId != GetCurrentUserId())
                {
                    _logger.LogWarning("User attempted to update payment method that doesn't belong to them");
                    return Forbid();
                }

                var paymentMethod = await _paymentMethodService.UpdateAsync(id, updatePaymentMethodDto);
                return Ok(paymentMethod);
            }
            catch (KeyNotFoundException ex)
            {
                _logger.LogWarning(ex, "Payment method not found");
                return NotFound(new { message = ex.Message });
            }
            catch (InvalidOperationException ex)
            {
                _logger.LogWarning(ex, "Invalid operation when updating payment method");
                return BadRequest(new { message = ex.Message });
            }
        }

        [HttpDelete("{id}")]
        public async Task<ActionResult> Delete(Guid id)
        {
            _logger.LogInformation("Deleting payment method with ID: {MethodId}", id);

            try
            {
                var existingMethod = await _paymentMethodService.GetByIdAsync(id);

                if (existingMethod.IsSystem)
                {
                    _logger.LogWarning("User attempted to delete a system payment method");
                    return BadRequest(new { message = "Cannot delete system payment method" });
                }

                if (existingMethod.UserId != GetCurrentUserId())
                {
                    _logger.LogWarning("User attempted to delete payment method that doesn't belong to them");
                    return Forbid();
                }

                await _paymentMethodService.DeleteAsync(id);
                return NoContent();
            }
            catch (KeyNotFoundException ex)
            {
                _logger.LogWarning(ex, "Payment method not found");
                return NotFound(new { message = ex.Message });
            }
            catch (InvalidOperationException ex)
            {
                _logger.LogWarning(ex, "Invalid operation when deleting payment method");
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
