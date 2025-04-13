using FinanceSystem.Domain.Entities;

namespace FinanceSystem.Domain.Tests.Entities
{
    public class UserRoleTests
    {
        [Fact]
        public void Constructor_WithValidParameters_ShouldCreateUserRoleCorrectly()
        {
            // Arrange
            var user = new User("testuser", "test@example.com", "hashedpassword");
            var role = new Role("Admin", "Administrator role");

            // Act
            var userRole = new UserRole(user, role);

            // Assert
            Assert.Equal(user.Id, userRole.UserId);
            Assert.Equal(role.Id, userRole.RoleId);
            Assert.Same(user, userRole.User);
            Assert.Same(role, userRole.Role);
        }
    }
}