using FinanceSystem.Application.DTOs;
using FinanceSystem.Application.Services.Interfaces;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace FinanceSystem.API.Controllers
{
    [Authorize(Roles = "Admin")]
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
        public async Task<ActionResult> GetAll()
        {
            var roles = await _roleService.GetAllAsync();
            return Ok(roles);
        }

        [HttpGet("{id}")]
        public async Task<ActionResult> GetById(Guid id)
        {
            var role = await _roleService.GetByIdAsync(id);
            return Ok(role);
        }

        [HttpPost]
        public async Task<ActionResult> Create(CreateRoleDto createRoleDto)
        {
            var role = await _roleService.CreateAsync(createRoleDto);
            return CreatedAtAction(nameof(GetById), new { id = role.Id }, role);
        }

        [HttpPut("{id}")]
        public async Task<ActionResult> Update(Guid id, UpdateRoleDto updateRoleDto)
        {
            var role = await _roleService.UpdateAsync(id, updateRoleDto);
            return Ok(role);
        }

        [HttpDelete("{id}")]
        public async Task<ActionResult> Delete(Guid id)
        {
            await _roleService.DeleteAsync(id);
            return NoContent();
        }
    }
}