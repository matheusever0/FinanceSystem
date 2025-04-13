using FinanceSystem.Domain.Entities;

namespace FinanceSystem.Domain.Tests.Entities
{
    public class UserTests
    {
        [Fact]
        public void Constructor_WithValidParameters_ShouldCreateUserCorrectly()
        {
            // Arrange
            string username = "testuser";
            string email = "test@example.com";
            string passwordHash = "hashedpassword";

            // Act
            var user = new User(username, email, passwordHash);

            // Assert
            Assert.Equal(username, user.Username);
            Assert.Equal(email, user.Email);
            Assert.Equal(passwordHash, user.PasswordHash);
            Assert.True(user.IsActive);
            Assert.NotEqual(Guid.Empty, user.Id);
            Assert.NotNull(user.UserRoles);
            Assert.Empty(user.UserRoles);
            Assert.Null(user.LastLogin);
        }

        [Fact]
        public void UpdateUsername_WithValidUsername_ShouldUpdateUsername()
        {
            // Arrange
            var user = new User("oldusername", "test@example.com", "hashedpassword");
            string newUsername = "newusername";

            // Act
            user.UpdateUsername(newUsername);

            // Assert
            Assert.Equal(newUsername, user.Username);
        }

        [Fact]
        public void UpdateEmail_WithValidEmail_ShouldUpdateEmail()
        {
            // Arrange
            var user = new User("testuser", "old@example.com", "hashedpassword");
            string newEmail = "new@example.com";

            // Act
            user.UpdateEmail(newEmail);

            // Assert
            Assert.Equal(newEmail, user.Email);
        }

        [Fact]
        public void UpdatePassword_WithValidPasswordHash_ShouldUpdatePasswordHash()
        {
            // Arrange
            var user = new User("testuser", "test@example.com", "oldhashedpassword");
            string newPasswordHash = "newhashedpassword";

            // Act
            user.UpdatePassword(newPasswordHash);

            // Assert
            Assert.Equal(newPasswordHash, user.PasswordHash);
        }

        [Fact]
        public void Deactivate_ShouldSetIsActiveToFalse()
        {
            // Arrange
            var user = new User("testuser", "test@example.com", "hashedpassword");

            // Act
            user.Deactivate();

            // Assert
            Assert.False(user.IsActive);
        }

        [Fact]
        public void Activate_ShouldSetIsActiveToTrue()
        {
            // Arrange
            var user = new User("testuser", "test@example.com", "hashedpassword");
            user.Deactivate();

            // Act
            user.Activate();

            // Assert
            Assert.True(user.IsActive);
        }

        [Fact]
        public void SetLastLogin_ShouldUpdateLastLoginToCurrentDateTime()
        {
            // Arrange
            var user = new User("testuser", "test@example.com", "hashedpassword");
            DateTime beforeLogin = DateTime.UtcNow;

            // Act
            user.SetLastLogin();

            // Assert
            Assert.NotNull(user.LastLogin);
            Assert.True(user.LastLogin >= beforeLogin);
            Assert.True(user.LastLogin <= DateTime.UtcNow);
        }

        [Fact]
        public void AddRole_WithValidRole_ShouldAddRoleToUserRoles()
        {
            // Arrange
            var user = new User("testuser", "test@example.com", "hashedpassword");
            var role = new Role("Admin", "Administrator role");

            // Act
            user.AddRole(role);

            // Assert
            Assert.Single(user.UserRoles);
            Assert.Equal(role.Id, user.UserRoles.First().RoleId);
        }

        [Fact]
        public void RemoveRole_WithExistingRole_ShouldRemoveRoleFromUserRoles()
        {
            // Arrange
            var user = new User("testuser", "test@example.com", "hashedpassword");
            var role = new Role("Admin", "Administrator role");
            user.AddRole(role);

            // Act
            user.RemoveRole(role);

            // Assert
            Assert.Empty(user.UserRoles);
        }
    }
}