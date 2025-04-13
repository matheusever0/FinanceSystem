using AutoMapper;
using FinanceSystem.Application.DTOs;
using FinanceSystem.Application.Mappings;
using FinanceSystem.Application.Services;
using FinanceSystem.Domain.Entities;
using FinanceSystem.Domain.Interfaces.Repositories;
using FinanceSystem.Domain.Interfaces.Services;
using Moq;

namespace FinanceSystem.Application.Tests.Services
{
    public class UserServiceTests
    {
        private readonly Mock<IUnitOfWork> _mockUnitOfWork;
        private readonly Mock<IAuthService> _mockAuthService;
        private readonly IMapper _mapper;
        private readonly UserService _userService;

        public UserServiceTests()
        {
            // Configurar o AutoMapper
            var mapperConfig = new MapperConfiguration(cfg =>
            {
                cfg.AddProfile(new MappingProfile());
            });
            _mapper = mapperConfig.CreateMapper();

            // Configurar mocks
            _mockUnitOfWork = new Mock<IUnitOfWork>();
            _mockAuthService = new Mock<IAuthService>();

            // Criar o serviço a ser testado
            _userService = new UserService(_mockUnitOfWork.Object, _mockAuthService.Object, _mapper);
        }

        [Fact]
        public async Task GetByIdAsync_WithExistingUser_ShouldReturnUserDto()
        {
            // Arrange
            var userId = Guid.NewGuid();
            var user = new User("testuser", "test@example.com", "hashedpassword");

            // Usar Reflection para definir o Id (propriedade protegida)
            typeof(User)
                .GetProperty("Id")
                .SetValue(user, userId);

            var mockUserRepository = new Mock<IUserRepository>();
            mockUserRepository
                .Setup(repo => repo.GetUserWithRolesAsync(userId))
                .ReturnsAsync(user);

            _mockUnitOfWork
                .Setup(uow => uow.Users)
                .Returns(mockUserRepository.Object);

            // Act
            var result = await _userService.GetByIdAsync(userId);

            // Assert
            Assert.NotNull(result);
            Assert.Equal(userId, result.Id);
            Assert.Equal(user.Username, result.Username);
            Assert.Equal(user.Email, result.Email);
        }

        [Fact]
        public async Task GetAllAsync_ShouldReturnAllUsers()
        {
            // Arrange
            var users = new List<User>
            {
                new User("user1", "user1@example.com", "hash1"),
                new User("user2", "user2@example.com", "hash2")
            };

            var mockUserRepository = new Mock<IUserRepository>();
            mockUserRepository
                .Setup(repo => repo.GetUsersWithRolesAsync())
                .ReturnsAsync(users);

            _mockUnitOfWork
                .Setup(uow => uow.Users)
                .Returns(mockUserRepository.Object);

            // Act
            var result = await _userService.GetAllAsync();

            // Assert
            Assert.NotNull(result);
            Assert.Equal(users.Count, result.Count());
        }

        [Fact]
        public async Task CreateAsync_WithValidData_ShouldCreateAndReturnUser()
        {
            // Arrange
            var createUserDto = new CreateUserDto
            {
                Username = "newuser",
                Email = "newuser@example.com",
                Password = "password123",
                Roles = new List<string> { "User" }
            };

            var role = new Role("User", "Standard user role");

            var mockUserRepository = new Mock<IUserRepository>();
            mockUserRepository
                .Setup(repo => repo.GetByUsernameAsync(It.IsAny<string>()))
                .ReturnsAsync((User)null);

            mockUserRepository
                .Setup(repo => repo.GetByEmailAsync(It.IsAny<string>()))
                .ReturnsAsync((User)null);

            var mockRoleRepository = new Mock<IRoleRepository>();
            mockRoleRepository
                .Setup(repo => repo.GetByNameAsync("User"))
                .ReturnsAsync(role);

            _mockUnitOfWork
                .Setup(uow => uow.Users)
                .Returns(mockUserRepository.Object);

            _mockUnitOfWork
                .Setup(uow => uow.Roles)
                .Returns(mockRoleRepository.Object);

            _mockAuthService
                .Setup(auth => auth.HashPassword(createUserDto.Password))
                .Returns("hashedpassword");

            // Act
            var result = await _userService.CreateAsync(createUserDto);

            // Assert
            Assert.NotNull(result);
            Assert.Equal(createUserDto.Username, result.Username);
            Assert.Equal(createUserDto.Email, result.Email);

            mockUserRepository.Verify(repo => repo.AddAsync(It.IsAny<User>()), Times.Once);
            _mockUnitOfWork.Verify(uow => uow.CompleteAsync(), Times.Once);
        }

        [Fact]
        public async Task CreateAsync_WithExistingUsername_ShouldThrowException()
        {
            // Arrange
            var createUserDto = new CreateUserDto
            {
                Username = "existinguser",
                Email = "user@example.com",
                Password = "password123"
            };

            var existingUser = new User(createUserDto.Username, "existing@example.com", "hash");

            var mockUserRepository = new Mock<IUserRepository>();
            mockUserRepository
                .Setup(repo => repo.GetByUsernameAsync(createUserDto.Username))
                .ReturnsAsync(existingUser);

            _mockUnitOfWork
                .Setup(uow => uow.Users)
                .Returns(mockUserRepository.Object);

            // Act & Assert
            var exception = await Assert.ThrowsAsync<InvalidOperationException>(
                () => _userService.CreateAsync(createUserDto));
            Assert.Contains("already exists", exception.Message);
        }

        [Fact]
        public async Task UpdateAsync_WithValidData_ShouldUpdateUser()
        {
            // Arrange
            var userId = Guid.NewGuid();
            var updateUserDto = new UpdateUserDto
            {
                Username = "updateduser",
                Email = "updated@example.com",
                Password = "newpassword",
                IsActive = true
            };

            var user = new User("olduser", "old@example.com", "oldhash");

            // Usar Reflection para definir o Id (propriedade protegida)
            typeof(User)
                .GetProperty("Id")
                .SetValue(user, userId);

            var mockUserRepository = new Mock<IUserRepository>();
            mockUserRepository
                .Setup(repo => repo.GetUserWithRolesAsync(userId))
                .ReturnsAsync(user);

            mockUserRepository
                .Setup(repo => repo.GetByUsernameAsync(updateUserDto.Username))
                .ReturnsAsync((User)null);

            mockUserRepository
                .Setup(repo => repo.GetByEmailAsync(updateUserDto.Email))
                .ReturnsAsync((User)null);

            _mockUnitOfWork
                .Setup(uow => uow.Users)
                .Returns(mockUserRepository.Object);

            _mockAuthService
                .Setup(auth => auth.HashPassword(updateUserDto.Password))
                .Returns("newhashedpassword");

            // Act
            var result = await _userService.UpdateAsync(userId, updateUserDto);

            // Assert
            Assert.NotNull(result);
            Assert.Equal(updateUserDto.Username, result.Username);
            Assert.Equal(updateUserDto.Email, result.Email);

            mockUserRepository.Verify(repo => repo.UpdateAsync(It.IsAny<User>()), Times.Once);
            _mockUnitOfWork.Verify(uow => uow.CompleteAsync(), Times.Once);
        }

        [Fact]
        public async Task LoginAsync_WithValidCredentials_ShouldReturnLoginResponse()
        {
            // Arrange
            var loginDto = new LoginDto
            {
                Username = "testuser",
                Password = "password123"
            };

            var user = new User(loginDto.Username, "test@example.com", "hashedpassword");
            user.Activate();

            var mockUserRepository = new Mock<IUserRepository>();
            mockUserRepository
                .Setup(repo => repo.GetByUsernameAsync(loginDto.Username))
                .ReturnsAsync(user);

            _mockUnitOfWork
                .Setup(uow => uow.Users)
                .Returns(mockUserRepository.Object);

            _mockAuthService
                .Setup(auth => auth.VerifyPassword(loginDto.Password, user.PasswordHash))
                .Returns(true);

            _mockAuthService
                .Setup(auth => auth.GenerateJwtToken(user))
                .ReturnsAsync("jwt-token");

            // Act
            var result = await _userService.LoginAsync(loginDto);

            // Assert
            Assert.NotNull(result);
            Assert.Equal("jwt-token", result.Token);
            Assert.NotNull(result.User);
            Assert.Equal(user.Username, result.User.Username);

            mockUserRepository.Verify(repo => repo.UpdateAsync(It.IsAny<User>()), Times.Once);
            _mockUnitOfWork.Verify(uow => uow.CompleteAsync(), Times.Once);
        }

        [Fact]
        public async Task LoginAsync_WithInvalidCredentials_ShouldThrowException()
        {
            // Arrange
            var loginDto = new LoginDto
            {
                Username = "testuser",
                Password = "wrongpassword"
            };

            var user = new User(loginDto.Username, "test@example.com", "hashedpassword");
            user.Activate();

            var mockUserRepository = new Mock<IUserRepository>();
            mockUserRepository
                .Setup(repo => repo.GetByUsernameAsync(loginDto.Username))
                .ReturnsAsync(user);

            _mockUnitOfWork
                .Setup(uow => uow.Users)
                .Returns(mockUserRepository.Object);

            _mockAuthService
                .Setup(auth => auth.VerifyPassword(loginDto.Password, user.PasswordHash))
                .Returns(false);

            // Act & Assert
            var exception = await Assert.ThrowsAsync<InvalidOperationException>(
                () => _userService.LoginAsync(loginDto));
            Assert.Contains("Invalid username or password", exception.Message);
        }

        [Fact]
        public async Task LoginAsync_WithInactiveUser_ShouldThrowException()
        {
            // Arrange
            var loginDto = new LoginDto
            {
                Username = "testuser",
                Password = "password123"
            };

            var user = new User(loginDto.Username, "test@example.com", "hashedpassword");
            user.Deactivate();

            var mockUserRepository = new Mock<IUserRepository>();
            mockUserRepository
                .Setup(repo => repo.GetByUsernameAsync(loginDto.Username))
                .ReturnsAsync(user);

            _mockUnitOfWork
                .Setup(uow => uow.Users)
                .Returns(mockUserRepository.Object);

            // Act & Assert
            var exception = await Assert.ThrowsAsync<InvalidOperationException>(
                () => _userService.LoginAsync(loginDto));
            Assert.Contains("User account is deactivated", exception.Message);
        }
    }
}