using FinanceSystem.Application.DTOs.PaymentType;
using FinanceSystem.Application.Interfaces;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System.Security.Claims;

namespace FinanceSystem.API.Controllers
{
    [Route("api/payment-types")]
    [ApiController]
    [Authorize]
    public class PaymentTypesController : ControllerBase
    {
        private readonly IPaymentTypeService _paymentTypeService;
        private readonly ILogger<PaymentTypesController> _logger;

        public PaymentTypesController(IPaymentTypeService paymentTypeService, ILogger<PaymentTypesController> logger)
        {
            _paymentTypeService = paymentTypeService;
            _logger = logger;
        }

        [HttpGet]
        public async Task<ActionResult<IEnumerable<PaymentTypeDto>>> GetAll()
        {
            _logger.LogInformation("Getting all payment types for user");

            var userId = GetCurrentUserId();
            var paymentTypes = await _paymentTypeService.GetAllAvailableForUserAsync(userId);

            return Ok(paymentTypes);
        }

        [HttpGet("system")]
        public async Task<ActionResult<IEnumerable<PaymentTypeDto>>> GetAllSystem()
        {
            _logger.LogInformation("Getting all system payment types");

            var paymentTypes = await _paymentTypeService.GetAllSystemTypesAsync();

            return Ok(paymentTypes);
        }

        [HttpGet("user")]
        public async Task<ActionResult<IEnumerable<PaymentTypeDto>>> GetAllUser()
        {
            _logger.LogInformation("Getting all user-defined payment types");

            var userId = GetCurrentUserId();
            var paymentTypes = await _paymentTypeService.GetUserTypesAsync(userId);

            return Ok(paymentTypes);
        }

        [HttpGet("{id}")]
        public async Task<ActionResult<PaymentTypeDto>> GetById(Guid id)
        {
            _logger.LogInformation("Getting payment type with ID: {TypeId}", id);

            try
            {
                var paymentType = await _paymentTypeService.GetByIdAsync(id);

                if (!paymentType.IsSystem && paymentType.UserId != GetCurrentUserId())
                {
                    _logger.LogWarning("User attempted to access payment type that doesn't belong to them");
                    return Forbid();
                }

                return Ok(paymentType);
            }
            catch (KeyNotFoundException ex)
            {
                _logger.LogWarning(ex, "Payment type not found");
                return NotFound(new { message = ex.Message });
            }
        }

        [HttpPost]
        public async Task<ActionResult<PaymentTypeDto>> Create(CreatePaymentTypeDto createPaymentTypeDto)
        {
            _logger.LogInformation("Creating new payment type");

            try
            {
                var userId = GetCurrentUserId();
                var paymentType = await _paymentTypeService.CreateAsync(createPaymentTypeDto, userId);

                return CreatedAtAction(nameof(GetById), new { id = paymentType.Id }, paymentType);
            }
            catch (KeyNotFoundException ex)
            {
                _logger.LogWarning(ex, "User not found");
                return NotFound(new { message = ex.Message });
            }
            catch (InvalidOperationException ex)
            {
                _logger.LogWarning(ex, "Invalid operation when creating payment type");
                return BadRequest(new { message = ex.Message });
            }
        }

        [HttpPut("{id}")]
        public async Task<ActionResult<PaymentTypeDto>> Update(Guid id, UpdatePaymentTypeDto updatePaymentTypeDto)
        {
            _logger.LogInformation("Updating payment type with ID: {TypeId}", id);

            try
            {
                var existingType = await _paymentTypeService.GetByIdAsync(id);

                if (existingType.IsSystem)
                {
                    _logger.LogWarning("User attempted to update a system payment type");
                    return BadRequest(new { message = "Cannot update system payment type" });
                }

                if (existingType.UserId != GetCurrentUserId())
                {
                    _logger.LogWarning("User attempted to update payment type that doesn't belong to them");
                    return Forbid();
                }

                var paymentType = await _paymentTypeService.UpdateAsync(id, updatePaymentTypeDto);
                return Ok(paymentType);
            }
            catch (KeyNotFoundException ex)
            {
                _logger.LogWarning(ex, "Payment type not found");
                return NotFound(new { message = ex.Message });
            }
            catch (InvalidOperationException ex)
            {
                _logger.LogWarning(ex, "Invalid operation when updating payment type");
                return BadRequest(new { message = ex.Message });
            }
        }

        [HttpDelete("{id}")]
        public async Task<ActionResult> Delete(Guid id)
        {
            _logger.LogInformation("Deleting payment type with ID: {TypeId}", id);

            try
            {
                var existingType = await _paymentTypeService.GetByIdAsync(id);

                if (existingType.IsSystem)
                {
                    _logger.LogWarning("User attempted to delete a system payment type");
                    return BadRequest(new { message = "Cannot delete system payment type" });
                }

                if (existingType.UserId != GetCurrentUserId())
                {
                    _logger.LogWarning("User attempted to delete payment type that doesn't belong to them");
                    return Forbid();
                }

                await _paymentTypeService.DeleteAsync(id);
                return NoContent();
            }
            catch (KeyNotFoundException ex)
            {
                _logger.LogWarning(ex, "Payment type not found");
                return NotFound(new { message = ex.Message });
            }
            catch (InvalidOperationException ex)
            {
                _logger.LogWarning(ex, "Invalid operation when deleting payment type");
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
