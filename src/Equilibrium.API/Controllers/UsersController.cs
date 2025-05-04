using Equilibrium.Application.DTOs.User;
using Equilibrium.Application.Interfaces;
using Equilibrium.Domain.Interfaces.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace Equilibrium.API.Controllers
{
    public class UsersController : AuthenticatedController<IUserService>
    {
        public UsersController(IUnitOfWork unitOfWork, 
            IUserService service) : base(unitOfWork, service)
        {
        }

        [HttpGet]
        [Authorize]
        public async Task<ActionResult> GetAll()
        {
            var users = await _service.GetAllAsync();
            return Ok(users);
        }

        [HttpGet("{id}")]
        [Authorize]
        public async Task<ActionResult> GetById(Guid id)
        {
            try
            {
                var user = await _service.GetByIdAsync(id);
                return Ok(user);
            }
            catch (Exception ex)
            {
                return NotFound(ex.Message);
            }
        }

        [HttpPost]
        [Authorize]
        public async Task<ActionResult> Create(CreateUserDto createUserDto)
        {
            try
            {
                var user = await _service.CreateAsync(createUserDto);
                return CreatedAtAction(nameof(GetById), new { id = user.Id }, user);
            }
            catch (Exception ex)
            {
                return BadRequest(ex.Message);
            }
        }

        [HttpPut("{id}")]
        [Authorize]
        public async Task<ActionResult> Update(Guid id, UpdateUserDto updateUserDto)
        {
            try
            {
                var user = await _service.UpdateAsync(id, updateUserDto);
                return Ok(user);
            }
            catch (KeyNotFoundException ex)
            {
                return NotFound(ex.Message);
            }
            catch (Exception ex)
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
                await _service.DeleteAsync(id);
                return NoContent();
            }
            catch (Exception ex)
            {
                return NotFound(ex.Message);
            }
        }
    }
}