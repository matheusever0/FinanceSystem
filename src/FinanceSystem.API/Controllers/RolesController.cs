using FinanceSystem.API.Extensions;
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
        private readonly ILogger<RolesController> _logger;

        public RolesController(IRoleService roleService, ILogger<RolesController> logger)
        {
            _roleService = roleService;
            _logger = logger;
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
            catch (KeyNotFoundException)
            {
                return this.ApiNotFound<RoleDto>(ResourceFinanceApi.Role_NotFound);
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
            catch (InvalidOperationException ex) when (ex.Message == ResourceFinanceApi.Role_NameExists)
            {
                return this.ApiBadRequest<RoleDto>(ex.Message);
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
            catch (KeyNotFoundException)
            {
                return this.ApiNotFound<RoleDto>(ResourceFinanceApi.Role_NotFound);
            }
            catch (InvalidOperationException ex) when (ex.Message == ResourceFinanceApi.Role_NameExists)
            {
                return this.ApiBadRequest<RoleDto>(ex.Message);
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
            catch (KeyNotFoundException)
            {
                return this.ApiNotFound<RoleDto>(ResourceFinanceApi.Role_NotFound);
            }
            catch (InvalidOperationException ex) when (ex.Message == ResourceFinanceApi.Role_HasUsers)
            {
                return this.ApiBadRequest<RoleDto>(ex.Message);
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
            catch (KeyNotFoundException)
            {
                return this.ApiNotFound<bool>(ResourceFinanceApi.Role_NotFound);
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
            catch (KeyNotFoundException)
            {
                return this.ApiNotFound<RoleDto>(ResourceFinanceApi.Role_NotFound);
            }
        }
    }
}