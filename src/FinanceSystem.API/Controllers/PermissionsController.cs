using FinanceSystem.Application.DTOs.Permission;
using FinanceSystem.Application.Interfaces;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace FinanceSystem.API.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class PermissionsController : ControllerBase
    {
        private readonly IPermissionService _permissionService;
        private readonly ILogger<PermissionsController> _logger;

        public PermissionsController(IPermissionService permissionService, ILogger<PermissionsController> logger)
        {
            _permissionService = permissionService;
            _logger = logger;
        }

        [HttpGet]
        [Authorize(Roles = "Admin")]
        public async Task<ActionResult> GetAll()
        {
            var permissions = await _permissionService.GetAllAsync();
            return Ok(permissions);
        }

        [HttpGet("{id}")]
        [Authorize(Roles = "Admin")]
        public async Task<ActionResult> GetById(Guid id)
        {
            var permission = await _permissionService.GetByIdAsync(id);
            return Ok(permission);
        }

        [HttpPost]
        [Authorize(Roles = "Admin")]
        public async Task<ActionResult> Create(CreatePermissionDto createPermissionDto)
        {
            var permission = await _permissionService.CreateAsync(createPermissionDto);
            return CreatedAtAction(nameof(GetById), new { id = permission.Id }, permission);
        }

        [HttpPut("{id}")]
        [Authorize(Roles = "Admin")]
        public async Task<ActionResult> Update(Guid id, UpdatePermissionDto updatePermissionDto)
        {
            var permission = await _permissionService.UpdateAsync(id, updatePermissionDto);
            return Ok(permission);
        }

        [HttpDelete("{id}")]
        [Authorize(Roles = "Admin")]
        public async Task<ActionResult> Delete(Guid id)
        {
            await _permissionService.DeleteAsync(id);
            return NoContent();
        }

        [HttpGet("role/{roleId}")]
        [Authorize]
        public async Task<ActionResult> GetPermissionsByRoleId(Guid roleId)
        {
            _logger.LogInformation("Obtendo permissões para o perfil {RoleId}", roleId);
            var permissions = await _permissionService.GetPermissionsByRoleIdAsync(roleId);
            return Ok(permissions);
        }

        [HttpPost("role/{roleId}/permission/{permissionId}")]
        [Authorize(Roles = "Admin")]
        public async Task<ActionResult> AssignPermissionToRole(Guid roleId, Guid permissionId)
        {
            var result = await _permissionService.AssignPermissionToRoleAsync(roleId, permissionId);
            return Ok(result);
        }

        [HttpDelete("role/{roleId}/permission/{permissionId}")]
        [Authorize(Roles = "Admin")]
        public async Task<ActionResult> RemovePermissionFromRole(Guid roleId, Guid permissionId)
        {
            var result = await _permissionService.RemovePermissionFromRoleAsync(roleId, permissionId);
            return Ok(result);
        }

        [HttpGet("user/{userId}")]
        [Authorize]
        public async Task<ActionResult> GetPermissionsByUserId(Guid userId)
        {
            _logger.LogInformation("Obtendo permissões para o usuário {UserId}", userId);
            try
            {
                var permissions = await _permissionService.GetPermissionsByUserIdAsync(userId);
                return Ok(permissions);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Erro ao obter permissões para o usuário {UserId}", userId);
                throw;
            }
        }
    }
}