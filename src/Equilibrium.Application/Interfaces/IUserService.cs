using Equilibrium.Application.DTOs.Login;
using Equilibrium.Application.DTOs.User;

namespace Equilibrium.Application.Interfaces
{
    public interface IUserService
    {
        Task<UserDto> GetByIdAsync(Guid id);
        Task<IEnumerable<UserDto>> GetAllAsync();
        Task<UserDto> CreateAsync(CreateUserDto createUserDto);
        Task<UserDto> UpdateAsync(Guid id, UpdateUserDto updateUserDto);
        Task DeleteAsync(Guid id);
        Task<LoginResponseDto> LoginAsync(LoginDto loginDto);
    }
}
