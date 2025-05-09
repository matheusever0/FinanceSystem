using AutoMapper;
using Equilibrium.Application.DTOs.Login;
using Equilibrium.Application.DTOs.User;
using Equilibrium.Application.Interfaces;
using Equilibrium.Domain.Entities;
using Equilibrium.Domain.Interfaces.Services;
using Equilibrium.Resources;

namespace Equilibrium.Application.Services
{
    public class UserService : IUserService
    {
        private readonly IUnitOfWork _unitOfWork;
        private readonly IAuthService _authService;
        private readonly IMapper _mapper;

        public UserService(IUnitOfWork unitOfWork, IAuthService authService, IMapper mapper)
        {
            _unitOfWork = unitOfWork;
            _authService = authService;
            _mapper = mapper;
        }

        public async Task<UserDto> GetByIdAsync(Guid id)
        {
            var user = await _unitOfWork.Users.GetUserWithRolesAsync(id);
            return user == null ? throw new KeyNotFoundException(ResourceFinanceApi.User_NotFound) : _mapper.Map<UserDto>(user);
        }

        public async Task<IEnumerable<UserDto>> GetAllAsync()
        {
            var users = await _unitOfWork.Users.GetUsersWithRolesAsync();
            return _mapper.Map<IEnumerable<UserDto>>(users);
        }

        public async Task<UserDto> CreateAsync(CreateUserDto createUserDto)
        {
            var existingUserName = await _unitOfWork.Users.GetByUsernameAsync(createUserDto.Username);
            if (existingUserName != null)
                throw new InvalidOperationException(ResourceFinanceApi.User_UsernameExists);

            var existingEmail = await _unitOfWork.Users.GetByEmailAsync(createUserDto.Email);
            if (existingEmail != null)
                throw new InvalidOperationException(ResourceFinanceApi.User_EmailExists);

            var passwordHash = _authService.HashPassword(createUserDto.Password);

            var user = new User(createUserDto.Username.ToLower(), createUserDto.Email.ToLower(), passwordHash);

            if (createUserDto.Roles != null && createUserDto.Roles.Count != 0)
            {
                foreach (var roleName in createUserDto.Roles)
                {
                    var role = await _unitOfWork.Roles.GetByNameAsync(roleName);
                    if (role != null)
                    {
                        user.AddRole(role);
                    }
                }
            }

            await _unitOfWork.Users.AddAsync(user);
            await _unitOfWork.CompleteAsync();

            return _mapper.Map<UserDto>(user);
        }

        public async Task<UserDto> UpdateAsync(Guid id, UpdateUserDto updateUserDto)
        {
            var user = await _unitOfWork.Users.GetUserWithRolesAsync(id) ?? throw new KeyNotFoundException(ResourceFinanceApi.User_NotFound);
            if (!string.IsNullOrEmpty(updateUserDto.Username))
            {
                var existingUserName = await _unitOfWork.Users.GetByUsernameAsync(updateUserDto.Username);
                if (existingUserName != null && existingUserName.Id != id)
                    throw new InvalidOperationException(ResourceFinanceApi.User_UsernameExists);

                user.UpdateUsername(updateUserDto.Username);
            }

            if (!string.IsNullOrEmpty(updateUserDto.Email))
            {
                var existingEmail = await _unitOfWork.Users.GetByEmailAsync(updateUserDto.Email);
                if (existingEmail != null && existingEmail.Id != id)
                    throw new InvalidOperationException(ResourceFinanceApi.User_EmailExists);

                user.UpdateEmail(updateUserDto.Email);
            }

            if (!string.IsNullOrEmpty(updateUserDto.Password))
            {
                var passwordHash = _authService.HashPassword(updateUserDto.Password);
                user.UpdatePassword(passwordHash);
            }

            if (updateUserDto.IsActive.HasValue)
            {
                if (updateUserDto.IsActive.Value)
                    user.Activate();
                else
                    user.Deactivate();
            }

            if (updateUserDto.Roles != null)
            {
                var currentRoles = user.UserRoles.Select(ur => ur.Role).ToList();

                foreach (var role in currentRoles)
                {
                    if (!updateUserDto.Roles.Contains(role.Name))
                    {
                        user.RemoveRole(role);
                    }
                }

                foreach (var roleName in updateUserDto.Roles)
                {
                    var role = await _unitOfWork.Roles.GetByNameAsync(roleName);
                    if (role != null && !currentRoles.Any(r => r.Name == roleName))
                    {
                        user.AddRole(role);
                    }
                }
            }

            await _unitOfWork.Users.UpdateAsync(user);
            await _unitOfWork.CompleteAsync();

            return _mapper.Map<UserDto>(user);
        }

        public async Task DeleteAsync(Guid id)
        {
            var user = await _unitOfWork.Users.GetByIdAsync(id) ?? throw new KeyNotFoundException(ResourceFinanceApi.User_NotFound);
            user.Delete();

            await _unitOfWork.Users.UpdateAsync(user);
            await _unitOfWork.CompleteAsync();
        }

        public async Task<LoginResponseDto> LoginAsync(LoginDto loginDto)
        {
            var user = await _unitOfWork.Users.GetByUsernameAsync(loginDto.Username) ?? throw new InvalidOperationException(ResourceFinanceApi.Auth_InvalidCredentials);

            if (user.IsActive)
            {
                if (!_authService.VerifyPassword(loginDto.Password, user.PasswordHash))
                {
                    throw new InvalidOperationException(ResourceFinanceApi.Auth_InvalidCredentials);
                }

                user.SetLastLogin();
                await _unitOfWork.Users.UpdateAsync(user);
                await _unitOfWork.CompleteAsync();

                var token = await _authService.GenerateJwtToken(user);

                return new LoginResponseDto
                {
                    Token = token,
                    Expiration = DateTime.UtcNow.AddDays(double.Parse("1")),
                    User = _mapper.Map<UserDto>(user)
                };
            }

            throw new InvalidOperationException(ResourceFinanceApi.Auth_UserDeactivated);
        }
    }
}