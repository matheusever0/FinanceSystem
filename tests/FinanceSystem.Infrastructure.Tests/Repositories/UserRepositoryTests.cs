using FinanceSystem.Domain.Entities;
using FinanceSystem.Infrastructure.Data;
using FinanceSystem.Infrastructure.Repositories;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Moq;

namespace FinanceSystem.Infrastructure.Tests.Repositories
{
    public class UserRepositoryTests
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
        public async Task GetByUsernameAsync_ShouldReturnUser_WhenUserExists()
        {
            // Arrange
            var options = GetInMemoryDbContextOptions();
            var username = "testuser";

            using (var context = GetDbContext(options))
            {
                var user = new User(username, "test@example.com", "hashedpassword");
                await context.Users.AddAsync(user);
                await context.SaveChangesAsync();
            }

            // Act
            using (var context = GetDbContext(options))
            {
                var repository = new UserRepository(context);
                var result = await repository.GetByUsernameAsync(username);

                // Assert
                Assert.NotNull(result);
                Assert.Equal(username, result.Username);
            }
        }

        [Fact]
        public async Task GetByUsernameAsync_ShouldReturnNull_WhenUserDoesNotExist()
        {
            // Arrange
            var options = GetInMemoryDbContextOptions();

            // Act
            using (var context = GetDbContext(options))
            {
                var repository = new UserRepository(context);
                var result = await repository.GetByUsernameAsync("nonexistentuser");

                // Assert
                Assert.Null(result);
            }
        }

        [Fact]
        public async Task GetByEmailAsync_ShouldReturnUser_WhenEmailExists()
        {
            // Arrange
            var options = GetInMemoryDbContextOptions();
            var email = "test@example.com";

            using (var context = GetDbContext(options))
            {
                var user = new User("testuser", email, "hashedpassword");
                await context.Users.AddAsync(user);
                await context.SaveChangesAsync();
            }

            // Act
            using (var context = GetDbContext(options))
            {
                var repository = new UserRepository(context);
                var result = await repository.GetByEmailAsync(email);

                // Assert
                Assert.NotNull(result);
                Assert.Equal(email, result.Email);
            }
        }

        [Fact]
        public async Task GetUserWithRolesAsync_ShouldReturnUserWithRoles()
        {
            // Arrange
            var options = GetInMemoryDbContextOptions();
            var userId = Guid.NewGuid();

            using (var context = GetDbContext(options))
            {
                var user = new User("testuser", "test@example.com", "hashedpassword");
                // Set user ID using reflection since it's a protected property
                typeof(User).GetProperty("Id").SetValue(user, userId);

                var role = new Role("Admin", "Administrator role");
                await context.Users.AddAsync(user);
                await context.Roles.AddAsync(role);
                await context.SaveChangesAsync();

                var userRole = new UserRole(user, role);
                await context.UserRoles.AddAsync(userRole);
                await context.SaveChangesAsync();
            }

            // Act
            using (var context = GetDbContext(options))
            {
                var repository = new UserRepository(context);
                var result = await repository.GetUserWithRolesAsync(userId);

                // Assert
                Assert.NotNull(result);
                Assert.NotEmpty(result.UserRoles);
                Assert.Equal("Admin", result.UserRoles.First().Role.Name);
            }
        }

        [Fact]
        public async Task GetUsersWithRolesAsync_ShouldReturnAllUsersWithRoles()
        {
            // Arrange
            var options = GetInMemoryDbContextOptions();

            using (var context = GetDbContext(options))
            {
                var user1 = new User("user1", "user1@example.com", "hashedpassword1");
                var user2 = new User("user2", "user2@example.com", "hashedpassword2");

                var role1 = new Role("Admin", "Administrator role");
                var role2 = new Role("User", "Standard user role");

                await context.Users.AddRangeAsync(user1, user2);
                await context.Roles.AddRangeAsync(role1, role2);
                await context.SaveChangesAsync();

                var userRole1 = new UserRole(user1, role1);
                var userRole2 = new UserRole(user2, role2);

                await context.UserRoles.AddRangeAsync(userRole1, userRole2);
                await context.SaveChangesAsync();
            }

            // Act
            using (var context = GetDbContext(options))
            {
                var repository = new UserRepository(context);
                var result = await repository.GetUsersWithRolesAsync();

                // Assert
                Assert.NotNull(result);
                Assert.Equal(2, result.Count());
                Assert.All(result, user => Assert.NotEmpty(user.UserRoles));
            }
        }
    }
}