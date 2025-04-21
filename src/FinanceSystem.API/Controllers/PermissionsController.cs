using FinanceSystem.Application.DTOs.Permission;
using FinanceSystem.Application.Interfaces;
using Microsoft.AspNetCore.Mvc;

namespace FinanceSystem.API.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class PermissionsController : ControllerBase
    {
        private readonly IPermissionService _permissionService;

        public PermissionsController(IPermissionService permissionService)
        {
            _permissionService = permissionService;
        }

        [HttpGet]
        public async Task<ActionResult> GetAll()
        {
            var permissions = await _permissionService.GetAllAsync();
            return Ok(permissions);
        }

        [HttpGet("{id}")]
        public async Task<ActionResult> GetById(Guid id)
        {
            var permission = await _permissionService.GetByIdAsync(id);
            return Ok(permission);
        }

        [HttpPost]
        public async Task<ActionResult> Create(CreatePermissionDto createPermissionDto)
        {
            var permission = await _permissionService.CreateAsync(createPermissionDto);
            return CreatedAtAction(nameof(GetById), new { id = permission.Id }, permission);
        }

        [HttpPut("{id}")]
        public async Task<ActionResult> Update(Guid id, UpdatePermissionDto updatePermissionDto)
        {
            var permission = await _permissionService.UpdateAsync(id, updatePermissionDto);
            return Ok(permission);
        }

        [HttpDelete("{id}")]
        public async Task<ActionResult> Delete(Guid id)
        {
            await _permissionService.DeleteAsync(id);
            return NoContent();
        }

        [HttpGet("role/{roleId}")]
        public async Task<ActionResult> GetPermissionsByRoleId(Guid roleId)
        {
            var permissions = await _permissionService.GetPermissionsByRoleIdAsync(roleId);
            return Ok(permissions);
        }

        [HttpPost("role/{roleId}/permission/{permissionId}")]
        public async Task<ActionResult> AssignPermissionToRole(Guid roleId, Guid permissionId)
        {
            var result = await _permissionService.AssignPermissionToRoleAsync(roleId, permissionId);
            return Ok(result);
        }

        [HttpDelete("role/{roleId}/permission/{permissionId}")]

        public async Task<ActionResult> RemovePermissionFromRole(Guid roleId, Guid permissionId)
        {
            var result = await _permissionService.RemovePermissionFromRoleAsync(roleId, permissionId);
            return Ok(result);
        }

        [HttpGet("user/{userId}")]
        public async Task<ActionResult> GetPermissionsByUserId(Guid userId)
        {
            var permissions = await _permissionService.GetPermissionsByUserIdAsync(userId);
            return Ok(permissions);
        }
    }
}