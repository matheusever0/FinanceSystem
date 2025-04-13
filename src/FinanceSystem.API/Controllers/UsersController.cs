using FinanceSystem.Application.DTOs;
using FinanceSystem.Application.Interfaces;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace FinanceSystem.API.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class UsersController : ControllerBase
    {
        private readonly IUserService _userService;
        private readonly ILogger<UsersController> _logger;

        public UsersController(IUserService userService, ILogger<UsersController> logger)
        {
            _userService = userService;
            _logger = logger;
        }

        [HttpGet]
        [Authorize]
        public async Task<ActionResult> GetAll()
        {
            _logger.LogInformation("Obtendo todos os usuários");
            var users = await _userService.GetAllAsync();
            return Ok(users);
        }

        [HttpGet("{id}")]
        [Authorize]
        public async Task<ActionResult> GetById(Guid id)
        {
            _logger.LogInformation("Obtendo usuário por ID: {UserId}", id);
            var user = await _userService.GetByIdAsync(id);
            return Ok(user);
        }

        [HttpPost]
        [Authorize]
        public async Task<ActionResult> Create(CreateUserDto createUserDto)
        {
            _logger.LogInformation("Criando novo usuário: {Username}", createUserDto.Username);
            var user = await _userService.CreateAsync(createUserDto);
            return CreatedAtAction(nameof(GetById), new { id = user.Id }, user);
        }

        [HttpPut("{id}")]
        [Authorize]
        public async Task<ActionResult> Update(Guid id, UpdateUserDto updateUserDto)
        {
            _logger.LogInformation("Atualizando usuário: {UserId}", id);
            var user = await _userService.UpdateAsync(id, updateUserDto);
            return Ok(user);
        }

        [HttpDelete("{id}")]
        [Authorize]
        public async Task<ActionResult> Delete(Guid id)
        {
            _logger.LogInformation("Excluindo usuário: {UserId}", id);
            await _userService.DeleteAsync(id);
            return NoContent();
        }
    }
}