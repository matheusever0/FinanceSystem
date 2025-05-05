using Equilibrium.Application.DTOs.Role;
using Equilibrium.Application.Interfaces;
using Equilibrium.Domain.Interfaces.Services;
using Equilibrium.Resources;
using Microsoft.AspNetCore.Mvc;

namespace Equilibrium.API.Controllers
{
    public class RolesController(IUnitOfWork unitOfWork,
        IRoleService service) : AuthenticatedController<IRoleService>(unitOfWork, service)
    {
        [HttpGet]
        public async Task<ActionResult> GetAll()
        {
            var roles = await _service.GetAllAsync();
            return Ok(roles);
        }

        [HttpGet("{id}")]
        public async Task<ActionResult> GetById(Guid id)
        {
            try
            {
                var role = await _service.GetByIdAsync(id);
                return Ok(role);
            }
            catch (KeyNotFoundException ex)
            {
                return NotFound(ex.Message);
            }
        }

        [HttpPost]
        public async Task<ActionResult> Create(CreateRoleDto createRoleDto)
        {
            try
            {
                var role = await _service.CreateAsync(createRoleDto);
                return CreatedAtAction(nameof(GetById), new { id = role.Id }, role);
            }
            catch (InvalidOperationException ex)
            {
                return BadRequest(ex.Message);
            }
        }

        [HttpPut("{id}")]
        public async Task<ActionResult> Update(Guid id, UpdateRoleDto updateRoleDto)
        {
            try
            {
                var role = await _service.UpdateAsync(id, updateRoleDto);
                return Ok(role);
            }
            catch (KeyNotFoundException ex)
            {
                return NotFound(ex.Message);
            }
            catch (InvalidOperationException ex) when (ex.Message == ResourceFinanceApi.Role_NameExists)
            {
                return BadRequest(ex.Message);
            }
        }

        [HttpDelete("{id}")]
        public async Task<ActionResult> Delete(Guid id)
        {
            try
            {
                await _service.DeleteAsync(id);
                return NoContent();
            }
            catch (KeyNotFoundException ex)
            {
                return NotFound(ex.Message);
            }
            catch (InvalidOperationException ex)
            {
                return BadRequest(ex.Message);
            }
        }

        [HttpGet("{roleId}/has-permission/{permissionName}")]
        public async Task<ActionResult> HasPermission(Guid roleId, string permissionName)
        {
            try
            {
                var result = await _service.HasPermissionAsync(roleId, permissionName);
                return Ok(result);
            }
            catch (KeyNotFoundException ex)
            {
                return NotFound(ex.Message);
            }
        }

        [HttpPut("{roleId}/permissions")]
        public async Task<ActionResult> UpdateRolePermissions(Guid roleId, [FromBody] List<Guid> permissionIds)
        {
            try
            {
                var role = await _service.UpdateRolePermissionsAsync(roleId, permissionIds);
                return Ok(role);
            }
            catch (KeyNotFoundException ex)
            {
                return NotFound(ex.Message);
            }
        }
    }
}