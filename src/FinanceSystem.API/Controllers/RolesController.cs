using FinanceSystem.Application.DTOs.Role;
using FinanceSystem.Application.Interfaces;
using FinanceSystem.Resources;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace FinanceSystem.API.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class RolesController : ControllerBase
    {
        private readonly IRoleService _roleService;

        public RolesController(IRoleService roleService)
        {
            _roleService = roleService;
        }

        [HttpGet]
        [Authorize]
        public async Task<ActionResult> GetAll()
        {
            var roles = await _roleService.GetAllAsync();
            return Ok(roles);
        }

        [HttpGet("{id}")]
        [Authorize]
        public async Task<ActionResult> GetById(Guid id)
        {
            try
            {
                var role = await _roleService.GetByIdAsync(id);
                return Ok(role);
            }
            catch (KeyNotFoundException ex)
            {
                return NotFound(ex.Message);
            }
        }

        [HttpPost]
        [Authorize]
        public async Task<ActionResult> Create(CreateRoleDto createRoleDto)
        {
            try
            {
                var role = await _roleService.CreateAsync(createRoleDto);
                return CreatedAtAction(nameof(GetById), new { id = role.Id }, role);
            }
            catch (InvalidOperationException ex)
            {
                return BadRequest(ex.Message);
            }
        }

        [HttpPut("{id}")]
        [Authorize]
        public async Task<ActionResult> Update(Guid id, UpdateRoleDto updateRoleDto)
        {
            try
            {
                var role = await _roleService.UpdateAsync(id, updateRoleDto);
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
        [Authorize]
        public async Task<ActionResult> Delete(Guid id)
        {
            try
            {
                await _roleService.DeleteAsync(id);
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
        [Authorize]
        public async Task<ActionResult> HasPermission(Guid roleId, string permissionName)
        {
            try
            {
                var result = await _roleService.HasPermissionAsync(roleId, permissionName);
                return Ok(result);
            }
            catch (KeyNotFoundException ex)
            {
                return NotFound(ex.Message);
            }
        }

        [HttpPut("{roleId}/permissions")]
        [Authorize]
        public async Task<ActionResult> UpdateRolePermissions(Guid roleId, [FromBody] List<Guid> permissionIds)
        {
            try
            {
                var role = await _roleService.UpdateRolePermissionsAsync(roleId, permissionIds);
                return Ok(role);
            }
            catch (KeyNotFoundException ex)
            {
                return NotFound(ex.Message);
            }
        }
    }
}