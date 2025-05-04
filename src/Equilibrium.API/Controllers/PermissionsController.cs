using Equilibrium.Application.DTOs.Permission;
using Equilibrium.Application.Interfaces;
using Equilibrium.Domain.Interfaces.Services;
using Microsoft.AspNetCore.Mvc;

namespace Equilibrium.API.Controllers
{
    public class PermissionsController : AuthenticatedController<IPermissionService>
    {
        public PermissionsController(IUnitOfWork unitOfWork, 
            IPermissionService service) : base(unitOfWork, service)
        {
        }

        [HttpGet]
        public async Task<ActionResult> GetAll()
        {
            var permissions = await _service.GetAllAsync();
            return Ok(permissions);
        }

        [HttpGet("{id}")]
        public async Task<ActionResult> GetById(Guid id)
        {
            var permission = await _service.GetByIdAsync(id);
            return Ok(permission);
        }

        [HttpPost]
        public async Task<ActionResult> Create(CreatePermissionDto createPermissionDto)
        {
            var permission = await _service.CreateAsync(createPermissionDto);
            return CreatedAtAction(nameof(GetById), new { id = permission.Id }, permission);
        }

        [HttpPut("{id}")]
        public async Task<ActionResult> Update(Guid id, UpdatePermissionDto updatePermissionDto)
        {
            var permission = await _service.UpdateAsync(id, updatePermissionDto);
            return Ok(permission);
        }

        [HttpDelete("{id}")]
        public async Task<ActionResult> Delete(Guid id)
        {
            await _service.DeleteAsync(id);
            return NoContent();
        }

        [HttpGet("role/{roleId}")]
        public async Task<ActionResult> GetPermissionsByRoleId(Guid roleId)
        {
            var permissions = await _service.GetPermissionsByRoleIdAsync(roleId);
            return Ok(permissions);
        }

        [HttpPost("role/{roleId}/permission/{permissionId}")]
        public async Task<ActionResult> AssignPermissionToRole(Guid roleId, Guid permissionId)
        {
            var result = await _service.AssignPermissionToRoleAsync(roleId, permissionId);
            return Ok(result);
        }

        [HttpDelete("role/{roleId}/permission/{permissionId}")]

        public async Task<ActionResult> RemovePermissionFromRole(Guid roleId, Guid permissionId)
        {
            var result = await _service.RemovePermissionFromRoleAsync(roleId, permissionId);
            return Ok(result);
        }

        [HttpGet("user/{userId}")]
        public async Task<ActionResult> GetPermissionsByUserId(Guid userId)
        {
            var permissions = await _service.GetPermissionsByUserIdAsync(userId);
            return Ok(permissions);
        }
    }
}