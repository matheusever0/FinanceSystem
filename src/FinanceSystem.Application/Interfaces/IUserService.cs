using FinanceSystem.Application.DTOs.Login;
using FinanceSystem.Application.DTOs.User;

namespace FinanceSystem.Application.Interfaces
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
