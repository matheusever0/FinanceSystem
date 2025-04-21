using FinanceSystem.API.Extensions;
using FinanceSystem.Application.DTOs.User;
using FinanceSystem.Application.Interfaces;
using FinanceSystem.Resources;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace FinanceSystem.API.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class UsersController : ControllerBase
    {
        private readonly IUserService _userService;

        public UsersController(IUserService userService, ILogger<UsersController> logger)
        {
            _userService = userService;
        }

        [HttpGet]
        [Authorize]
        public async Task<ActionResult> GetAll()
        {
            var users = await _userService.GetAllAsync();
            return Ok(users);
        }

        [HttpGet("{id}")]
        [Authorize]
        public async Task<ActionResult> GetById(Guid id)
        {
            try
            {
                var user = await _userService.GetByIdAsync(id);
                return Ok(user);
            }
            catch (KeyNotFoundException)
            {
                return this.ApiNotFound<UserDto>(ResourceFinanceApi.User_NotFound);
            }
        }

        [HttpPost]
        [Authorize]
        public async Task<ActionResult> Create(CreateUserDto createUserDto)
        {
            try
            {
                var user = await _userService.CreateAsync(createUserDto);
                return CreatedAtAction(nameof(GetById), new { id = user.Id }, user);
            }
            catch (InvalidOperationException ex) when (ex.Message == ResourceFinanceApi.User_UsernameExists
                                                     || ex.Message == ResourceFinanceApi.User_EmailExists)
            {
                return this.ApiBadRequest<UserDto>(ex.Message);
            }
        }

        [HttpPut("{id}")]
        [Authorize]
        public async Task<ActionResult> Update(Guid id, UpdateUserDto updateUserDto)
        {
            try
            {
                var user = await _userService.UpdateAsync(id, updateUserDto);
                return Ok(user);
            }
            catch (KeyNotFoundException)
            {
                return this.ApiNotFound<UserDto>(ResourceFinanceApi.User_NotFound);
            }
            catch (InvalidOperationException ex) when (ex.Message == ResourceFinanceApi.User_UsernameExists
                                                     || ex.Message == ResourceFinanceApi.User_EmailExists)
            {
                return this.ApiBadRequest<UserDto>(ex.Message);
            }
        }

        [HttpDelete("{id}")]
        [Authorize]
        public async Task<ActionResult> Delete(Guid id)
        {
            try
            {
                await _userService.DeleteAsync(id);
                return NoContent();
            }
            catch (KeyNotFoundException)
            {
                return this.ApiNotFound<UserDto>(ResourceFinanceApi.User_NotFound);
            }
        }
    }
}