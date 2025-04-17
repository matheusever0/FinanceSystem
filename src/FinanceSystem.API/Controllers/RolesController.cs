using FinanceSystem.Application.DTOs.Role;
using FinanceSystem.Application.Interfaces;
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
            _logger.LogInformation("Obtendo todos os perfis");
            var roles = await _roleService.GetAllAsync();
            return Ok(roles);
        }

        [HttpGet("{id}")]
        [Authorize]
        public async Task<ActionResult> GetById(Guid id)
        {
            _logger.LogInformation("Obtendo perfil por ID: {RoleId}", id);
            var role = await _roleService.GetByIdAsync(id);
            return Ok(role);
        }

        [HttpPost]
        [Authorize]
        public async Task<ActionResult> Create(CreateRoleDto createRoleDto)
        {
            _logger.LogInformation("Criando novo perfil: {Name}", createRoleDto.Name);
            var role = await _roleService.CreateAsync(createRoleDto);
            return CreatedAtAction(nameof(GetById), new { id = role.Id }, role);
        }

        [HttpPut("{id}")]
        [Authorize]
        public async Task<ActionResult> Update(Guid id, UpdateRoleDto updateRoleDto)
        {
            _logger.LogInformation("Atualizando perfil: {RoleId}", id);
            var role = await _roleService.UpdateAsync(id, updateRoleDto);
            return Ok(role);
        }

        [HttpDelete("{id}")]
        [Authorize]
        public async Task<ActionResult> Delete(Guid id)
        {
            _logger.LogInformation("Excluindo perfil: {RoleId}", id);
            await _roleService.DeleteAsync(id);
            return NoContent();
        }

        [HttpGet("{roleId}/has-permission/{permissionName}")]
        [Authorize]
        public async Task<ActionResult> HasPermission(Guid roleId, string permissionName)
        {
            _logger.LogInformation("Verificando se perfil {RoleId} tem permissão {Permission}", roleId, permissionName);
            var result = await _roleService.HasPermissionAsync(roleId, permissionName);
            return Ok(result);
        }

        [HttpPut("{roleId}/permissions")]
        [Authorize]
        public async Task<ActionResult> UpdateRolePermissions(Guid roleId, [FromBody] List<Guid> permissionIds)
        {
            _logger.LogInformation("Atualizando permissões do perfil {RoleId}", roleId);
            var role = await _roleService.UpdateRolePermissionsAsync(roleId, permissionIds);
            return Ok(role);
        }
    }
}