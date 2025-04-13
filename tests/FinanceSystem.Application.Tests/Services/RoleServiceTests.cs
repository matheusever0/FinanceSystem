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
    public class RoleServiceTests
    {
        private readonly Mock<IUnitOfWork> _mockUnitOfWork;
        private readonly IMapper _mapper;
        private readonly RoleService _roleService;

        public RoleServiceTests()
        {
            // Configurar o AutoMapper
            var mapperConfig = new MapperConfiguration(cfg =>
            {
                cfg.AddProfile(new MappingProfile());
            });
            _mapper = mapperConfig.CreateMapper();

            // Configurar mocks
            _mockUnitOfWork = new Mock<IUnitOfWork>();

            // Criar o serviço a ser testado
            _roleService = new RoleService(_mockUnitOfWork.Object, _mapper);
        }

        [Fact]
        public async Task GetByIdAsync_WithExistingRole_ShouldReturnRoleDto()
        {
            // Arrange
            var roleId = Guid.NewGuid();
            var role = new Role("Admin", "Administrator role");

            // Usar Reflection para definir o Id (propriedade protegida)
            typeof(Role)
                .GetProperty("Id")
                .SetValue(role, roleId);

            var mockRoleRepository = new Mock<IRoleRepository>();
            mockRoleRepository
                .Setup(repo => repo.GetByIdAsync(roleId))
                .ReturnsAsync(role);

            _mockUnitOfWork
                .Setup(uow => uow.Roles)
                .Returns(mockRoleRepository.Object);

            // Act
            var result = await _roleService.GetByIdAsync(roleId);

            // Assert
            Assert.NotNull(result);
            Assert.Equal(roleId, result.Id);
            Assert.Equal(role.Name, result.Name);
            Assert.Equal(role.Description, result.Description);
        }

        [Fact]
        public async Task GetAllAsync_ShouldReturnAllRoles()
        {
            // Arrange
            var roles = new List<Role>
            {
                new Role("Admin", "Administrator role"),
                new Role("User", "Standard user role")
            };

            var mockRoleRepository = new Mock<IRoleRepository>();
            mockRoleRepository
                .Setup(repo => repo.GetAllAsync())
                .ReturnsAsync(roles);

            _mockUnitOfWork
                .Setup(uow => uow.Roles)
                .Returns(mockRoleRepository.Object);

            // Act
            var result = await _roleService.GetAllAsync();

            // Assert
            Assert.NotNull(result);
            Assert.Equal(roles.Count, result.Count());
        }

        [Fact]
        public async Task CreateAsync_WithValidData_ShouldCreateAndReturnRole()
        {
            // Arrange
            var createRoleDto = new CreateRoleDto
            {
                Name = "NewRole",
                Description = "New role description"
            };

            var mockRoleRepository = new Mock<IRoleRepository>();
            mockRoleRepository
                .Setup(repo => repo.GetByNameAsync(It.IsAny<string>()))
                .ReturnsAsync((Role)null);

            _mockUnitOfWork
                .Setup(uow => uow.Roles)
                .Returns(mockRoleRepository.Object);

            // Act
            var result = await _roleService.CreateAsync(createRoleDto);

            // Assert
            Assert.NotNull(result);
            Assert.Equal(createRoleDto.Name, result.Name);
            Assert.Equal(createRoleDto.Description, result.Description);

            mockRoleRepository.Verify(repo => repo.AddAsync(It.IsAny<Role>()), Times.Once);
            _mockUnitOfWork.Verify(uow => uow.CompleteAsync(), Times.Once);
        }

        [Fact]
        public async Task CreateAsync_WithExistingRoleName_ShouldThrowException()
        {
            // Arrange
            var createRoleDto = new CreateRoleDto
            {
                Name = "ExistingRole",
                Description = "Role description"
            };

            var existingRole = new Role(createRoleDto.Name, "Existing description");

            var mockRoleRepository = new Mock<IRoleRepository>();
            mockRoleRepository
                .Setup(repo => repo.GetByNameAsync(createRoleDto.Name))
                .ReturnsAsync(existingRole);

            _mockUnitOfWork
                .Setup(uow => uow.Roles)
                .Returns(mockRoleRepository.Object);

            // Act & Assert
            var exception = await Assert.ThrowsAsync<InvalidOperationException>(
                () => _roleService.CreateAsync(createRoleDto));
            Assert.Contains("already exists", exception.Message);
        }

        [Fact]
        public async Task UpdateAsync_WithValidData_ShouldUpdateRole()
        {
            // Arrange
            var roleId = Guid.NewGuid();
            var updateRoleDto = new UpdateRoleDto
            {
                Name = "UpdatedRole",
                Description = "Updated description"
            };

            var role = new Role("OldRole", "Old description");

            // Usar Reflection para definir o Id (propriedade protegida)
            typeof(Role)
                .GetProperty("Id")
                .SetValue(role, roleId);

            var mockRoleRepository = new Mock<IRoleRepository>();
            mockRoleRepository
                .Setup(repo => repo.GetByIdAsync(roleId))
                .ReturnsAsync(role);

            mockRoleRepository
                .Setup(repo => repo.GetByNameAsync(updateRoleDto.Name))
                .ReturnsAsync((Role)null);

            _mockUnitOfWork
                .Setup(uow => uow.Roles)
                .Returns(mockRoleRepository.Object);

            // Act
            var result = await _roleService.UpdateAsync(roleId, updateRoleDto);

            // Assert
            Assert.NotNull(result);
            Assert.Equal(updateRoleDto.Name, result.Name);
            Assert.Equal(updateRoleDto.Description, result.Description);

            mockRoleRepository.Verify(repo => repo.UpdateAsync(It.IsAny<Role>()), Times.Once);
            _mockUnitOfWork.Verify(uow => uow.CompleteAsync(), Times.Once);
        }

        [Fact]
        public async Task DeleteAsync_WithRoleHavingNoUsers_ShouldDeleteRole()
        {
            // Arrange
            var roleId = Guid.NewGuid();
            var role = new Role("DeleteRole", "Role to delete");

            // Usar Reflection para definir o Id (propriedade protegida)
            typeof(Role)
                .GetProperty("Id")
                .SetValue(role, roleId);

            var mockRoleRepository = new Mock<IRoleRepository>();
            mockRoleRepository
                .Setup(repo => repo.GetByIdAsync(roleId))
                .ReturnsAsync(role);

            var rolesWithUsers = new List<Role> { new Role("Other", "Other role") };
            mockRoleRepository
                .Setup(repo => repo.GetAllWithUsersAsync())
                .ReturnsAsync(rolesWithUsers);

            _mockUnitOfWork
                .Setup(uow => uow.Roles)
                .Returns(mockRoleRepository.Object);

            // Act
            await _roleService.DeleteAsync(roleId);

            // Assert
            mockRoleRepository.Verify(repo => repo.DeleteAsync(It.IsAny<Role>()), Times.Once);
            _mockUnitOfWork.Verify(uow => uow.CompleteAsync(), Times.Once);
        }

        [Fact]
        public async Task DeleteAsync_WithRoleAssignedToUsers_ShouldThrowException()
        {
            // Arrange
            var roleId = Guid.NewGuid();
            var role = new Role("RoleWithUsers", "Role assigned to users");

            // Usar Reflection para definir o Id (propriedade protegida)
            typeof(Role)
                .GetProperty("Id")
                .SetValue(role, roleId);

            // Adicionar um UserRole à role
            var user = new User("testuser", "test@example.com", "hashedpassword");
            var userRole = new UserRole(user, role);
            typeof(Role)
                .GetProperty("UserRoles")
                .SetValue(role, new List<UserRole> { userRole });

            var mockRoleRepository = new Mock<IRoleRepository>();
            mockRoleRepository
                .Setup(repo => repo.GetByIdAsync(roleId))
                .ReturnsAsync(role);

            mockRoleRepository
                .Setup(repo => repo.GetAllWithUsersAsync())
                .ReturnsAsync(new List<Role> { role });

            _mockUnitOfWork
                .Setup(uow => uow.Roles)
                .Returns(mockRoleRepository.Object);

            // Act & Assert
            var exception = await Assert.ThrowsAsync<InvalidOperationException>(
                () => _roleService.DeleteAsync(roleId));
            Assert.Contains("assigned to users", exception.Message);
        }
    }
}