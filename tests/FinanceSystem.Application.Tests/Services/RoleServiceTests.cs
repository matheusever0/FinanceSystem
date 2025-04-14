﻿using AutoMapper;
using FinanceSystem.Application.DTOs;
using FinanceSystem.Application.Mappings;
using FinanceSystem.Application.Services;
using FinanceSystem.Domain.Entities;
using FinanceSystem.Domain.Interfaces.Repositories;
using FinanceSystem.Domain.Interfaces.Services;
using Moq;
using System.Reflection;

namespace FinanceSystem.Application.Tests.Services
{
    public class RoleServiceTests
    {
        private readonly Mock<IUnitOfWork> _mockUnitOfWork;
        private readonly IMapper _mapper;
        private readonly RoleService _roleService;

        public RoleServiceTests()
        {
            var mapperConfig = new MapperConfiguration(cfg =>
{
    cfg.AddProfile(new MappingProfile());
});
            _mapper = mapperConfig.CreateMapper();

            _mockUnitOfWork = new Mock<IUnitOfWork>();

            _roleService = new RoleService(_mockUnitOfWork.Object, _mapper);
        }

        [Fact]
        public async Task GetAllAsync_ShouldReturnAllRoles()
        {
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

            var result = await _roleService.GetAllAsync();

            Assert.NotNull(result);
            Assert.Equal(roles.Count, result.Count());
        }

        [Fact]
        public async Task CreateAsync_WithValidData_ShouldCreateAndReturnRole()
        {
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

            var result = await _roleService.CreateAsync(createRoleDto);

            Assert.NotNull(result);
            Assert.Equal(createRoleDto.Name, result.Name);
            Assert.Equal(createRoleDto.Description, result.Description);

            mockRoleRepository.Verify(repo => repo.AddAsync(It.IsAny<Role>()), Times.Once);
            _mockUnitOfWork.Verify(uow => uow.CompleteAsync(), Times.Once);
        }

        [Fact]
        public async Task CreateAsync_WithExistingRoleName_ShouldThrowException()
        {
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

            var exception = await Assert.ThrowsAsync<InvalidOperationException>(
    () => _roleService.CreateAsync(createRoleDto));
            Assert.Contains("already exists", exception.Message);
        }

        [Fact]
        public async Task UpdateAsync_WithValidData_ShouldUpdateRole()
        {
            var roleId = Guid.NewGuid();
            var updateRoleDto = new UpdateRoleDto
            {
                Name = "UpdatedRole",
                Description = "Updated description"
            };

            var role = new Role("OldRole", "Old description");

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

            var result = await _roleService.UpdateAsync(roleId, updateRoleDto);

            Assert.NotNull(result);
            Assert.Equal(updateRoleDto.Name, result.Name);
            Assert.Equal(updateRoleDto.Description, result.Description);

            mockRoleRepository.Verify(repo => repo.UpdateAsync(It.IsAny<Role>()), Times.Once);
            _mockUnitOfWork.Verify(uow => uow.CompleteAsync(), Times.Once);
        }

        [Fact]
        public async Task DeleteAsync_WithRoleHavingNoUsers_ShouldDeleteRole()
        {
            var roleId = Guid.NewGuid();
            var role = new Role("DeleteRole", "Role to delete");

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

            await _roleService.DeleteAsync(roleId);

            mockRoleRepository.Verify(repo => repo.DeleteAsync(It.IsAny<Role>()), Times.Once);
            _mockUnitOfWork.Verify(uow => uow.CompleteAsync(), Times.Once);
        }

        [Fact]
        public async Task DeleteAsync_WithRoleAssignedToUsers_ShouldThrowException()
        {
            var roleId = Guid.NewGuid();
            var role = new Role("RoleWithUsers", "Role assigned to users");

            typeof(Role)
    .GetProperty("Id")
    .SetValue(role, roleId);

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

            var exception = await Assert.ThrowsAsync<InvalidOperationException>(
    () => _roleService.DeleteAsync(roleId));
            Assert.Contains("assigned to users", exception.Message);
        }
    }
}