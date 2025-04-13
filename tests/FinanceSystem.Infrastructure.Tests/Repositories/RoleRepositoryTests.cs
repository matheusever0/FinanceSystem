using FinanceSystem.Domain.Entities;
using FinanceSystem.Infrastructure.Data;
using FinanceSystem.Infrastructure.Repositories;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Moq;

namespace FinanceSystem.Infrastructure.Tests.Repositories
{
    public class RoleRepositoryTests
    {
        private DbContextOptions<ApplicationDbContext> GetInMemoryDbContextOptions()
        {
            return new DbContextOptionsBuilder<ApplicationDbContext>()
                .UseInMemoryDatabase(databaseName: Guid.NewGuid().ToString())
                .Options;
        }

        private ApplicationDbContext GetDbContext(DbContextOptions<ApplicationDbContext> options)
        {
            return new ApplicationDbContext(options);
        }

        [Fact]
        public async Task GetByNameAsync_ShouldReturnRole_WhenRoleExists()
        {
            // Arrange
            var options = GetInMemoryDbContextOptions();
            var roleName = "Admin";

            using (var context = GetDbContext(options))
            {
                var role = new Role(roleName, "Administrator role");
                await context.Roles.AddAsync(role);
                await context.SaveChangesAsync();
            }

            // Act
            using (var context = GetDbContext(options))
            {
                var repository = new RoleRepository(context);
                var result = await repository.GetByNameAsync(roleName);

                // Assert
                Assert.NotNull(result);
                Assert.Equal(roleName, result.Name);
            }
        }

        [Fact]
        public async Task GetByNameAsync_ShouldReturnNull_WhenRoleDoesNotExist()
        {
            // Arrange
            var options = GetInMemoryDbContextOptions();

            // Act
            using (var context = GetDbContext(options))
            {
                var repository = new RoleRepository(context);
                var result = await repository.GetByNameAsync("NonexistentRole");

                // Assert
                Assert.Null(result);
            }
        }

        [Fact]
        public async Task GetAllWithUsersAsync_ShouldReturnRolesWithUsers()
        {
            // Arrange
            var options = GetInMemoryDbContextOptions();

            using (var context = GetDbContext(options))
            {
                var role1 = new Role("Admin", "Administrator role");
                var role2 = new Role("User", "Standard user role");

                var user1 = new User("user1", "user1@example.com", "hashedpassword1");
                var user2 = new User("user2", "user2@example.com", "hashedpassword2");

                await context.Roles.AddRangeAsync(role1, role2);
                await context.Users.AddRangeAsync(user1, user2);
                await context.SaveChangesAsync();

                var userRole1 = new UserRole(user1, role1);
                var userRole2 = new UserRole(user2, role2);

                await context.UserRoles.AddRangeAsync(userRole1, userRole2);
                await context.SaveChangesAsync();
            }

            // Act
            using (var context = GetDbContext(options))
            {
                var repository = new RoleRepository(context);
                var result = await repository.GetAllWithUsersAsync();

                // Assert
                Assert.NotNull(result);
                Assert.Equal(2, result.Count());
                Assert.All(result, role => Assert.NotEmpty(role.UserRoles));
            }
        }
    }
}