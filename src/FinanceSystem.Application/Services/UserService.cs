using AutoMapper;
using FinanceSystem.Application.DTOs;
using FinanceSystem.Application.Services.Interfaces;
using FinanceSystem.Domain.Entities;
using FinanceSystem.Domain.Interfaces.Services;

namespace FinanceSystem.Application.Services
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
            if (user == null)
                throw new KeyNotFoundException($"User with ID {id} not found");

            return _mapper.Map<UserDto>(user);
        }

        public async Task<IEnumerable<UserDto>> GetAllAsync()
        {
            var users = await _unitOfWork.Users.GetUsersWithRolesAsync();
            return _mapper.Map<IEnumerable<UserDto>>(users);
        }

        public async Task<UserDto> CreateAsync(CreateUserDto createUserDto)
        {
            // Verificar se o username já existe
            var existingUserName = await _unitOfWork.Users.GetByUsernameAsync(createUserDto.Username);
            if (existingUserName != null)
                throw new InvalidOperationException($"Username '{createUserDto.Username}' already exists");

            // Verificar se o email já existe
            var existingEmail = await _unitOfWork.Users.GetByEmailAsync(createUserDto.Email);
            if (existingEmail != null)
                throw new InvalidOperationException($"Email '{createUserDto.Email}' already exists");

            // Criar hash da senha
            var passwordHash = _authService.HashPassword(createUserDto.Password);

            // Criar usuário
            var user = new User(createUserDto.Username, createUserDto.Email, passwordHash);

            // Adicionar roles
            if (createUserDto.Roles != null && createUserDto.Roles.Any())
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

            // Persistir no banco de dados
            await _unitOfWork.Users.AddAsync(user);
            await _unitOfWork.CompleteAsync();

            return _mapper.Map<UserDto>(user);
        }

        public async Task<UserDto> UpdateAsync(Guid id, UpdateUserDto updateUserDto)
        {
            var user = await _unitOfWork.Users.GetUserWithRolesAsync(id);
            if (user == null)
                throw new KeyNotFoundException($"User with ID {id} not found");

            // Atualizar username se fornecido
            if (!string.IsNullOrEmpty(updateUserDto.Username))
            {
                var existingUserName = await _unitOfWork.Users.GetByUsernameAsync(updateUserDto.Username);
                if (existingUserName != null && existingUserName.Id != id)
                    throw new InvalidOperationException($"Username '{updateUserDto.Username}' already exists");

                user.UpdateUsername(updateUserDto.Username);
            }

            // Atualizar email se fornecido
            if (!string.IsNullOrEmpty(updateUserDto.Email))
            {
                var existingEmail = await _unitOfWork.Users.GetByEmailAsync(updateUserDto.Email);
                if (existingEmail != null && existingEmail.Id != id)
                    throw new InvalidOperationException($"Email '{updateUserDto.Email}' already exists");

                user.UpdateEmail(updateUserDto.Email);
            }

            // Atualizar senha se fornecida
            if (!string.IsNullOrEmpty(updateUserDto.Password))
            {
                var passwordHash = _authService.HashPassword(updateUserDto.Password);
                user.UpdatePassword(passwordHash);
            }

            // Atualizar status ativo/inativo
            if (updateUserDto.IsActive.HasValue)
            {
                if (updateUserDto.IsActive.Value)
                    user.Activate();
                else
                    user.Deactivate();
            }

            // Atualizar roles se fornecidas
            if (updateUserDto.Roles != null)
            {
                // Obter todas as roles atuais do usuário
                var currentRoles = user.UserRoles.Select(ur => ur.Role).ToList();

                // Remover roles que não estão na lista
                foreach (var role in currentRoles)
                {
                    if (!updateUserDto.Roles.Contains(role.Name))
                    {
                        user.RemoveRole(role);
                    }
                }

                // Adicionar novas roles
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
            var user = await _unitOfWork.Users.GetByIdAsync(id);
            if (user == null)
                throw new KeyNotFoundException($"User with ID {id} not found");

            await _unitOfWork.Users.DeleteAsync(user);
            await _unitOfWork.CompleteAsync();
        }

        public async Task<LoginResponseDto> LoginAsync(LoginDto loginDto)
        {
            // Buscar usuário pelo username
            var user = await _unitOfWork.Users.GetByUsernameAsync(loginDto.Username);
            if (user == null)
                throw new InvalidOperationException("Invalid username or password");

            // Verificar se o usuário está ativo
            if (!user.IsActive)
                throw new InvalidOperationException("User account is deactivated");

            // Verificar senha
            if (!_authService.VerifyPassword(loginDto.Password, user.PasswordHash))
                throw new InvalidOperationException("Invalid username or password");

            // Atualizar último login
            user.SetLastLogin();
            await _unitOfWork.Users.UpdateAsync(user);
            await _unitOfWork.CompleteAsync();

            // Gerar token JWT
            var token = await _authService.GenerateJwtToken(user);

            // Retornar resposta com token
            return new LoginResponseDto
            {
                Token = token,
                Expiration = DateTime.UtcNow.AddHours(1), // Token válido por 1 hora
                User = _mapper.Map<UserDto>(user)
            };
        }
    }
}
