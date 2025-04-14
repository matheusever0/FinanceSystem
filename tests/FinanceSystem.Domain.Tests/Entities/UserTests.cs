using FinanceSystem.Domain.Entities;

namespace FinanceSystem.Domain.Tests.Entities
{
    public class UserTests
    {
        [Fact]
        public void Constructor_WithValidParameters_ShouldCreateUserCorrectly()
        {
            string username = "testuser";
            string email = "test@example.com";
            string passwordHash = "hashedpassword";

            var user = new User(username, email, passwordHash);

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
            var user = new User("oldusername", "test@example.com", "hashedpassword");
            string newUsername = "newusername";

            user.UpdateUsername(newUsername);

            Assert.Equal(newUsername, user.Username);
        }

        [Fact]
        public void UpdateEmail_WithValidEmail_ShouldUpdateEmail()
        {
            var user = new User("testuser", "old@example.com", "hashedpassword");
            string newEmail = "new@example.com";

            user.UpdateEmail(newEmail);

            Assert.Equal(newEmail, user.Email);
        }

        [Fact]
        public void UpdatePassword_WithValidPasswordHash_ShouldUpdatePasswordHash()
        {
            var user = new User("testuser", "test@example.com", "oldhashedpassword");
            string newPasswordHash = "newhashedpassword";

            user.UpdatePassword(newPasswordHash);

            Assert.Equal(newPasswordHash, user.PasswordHash);
        }

        [Fact]
        public void Deactivate_ShouldSetIsActiveToFalse()
        {
            var user = new User("testuser", "test@example.com", "hashedpassword");

            user.Deactivate();

            Assert.False(user.IsActive);
        }

        [Fact]
        public void Activate_ShouldSetIsActiveToTrue()
        {
            var user = new User("testuser", "test@example.com", "hashedpassword");
            user.Deactivate();

            user.Activate();

            Assert.True(user.IsActive);
        }

        [Fact]
        public void SetLastLogin_ShouldUpdateLastLoginToCurrentDateTime()
        {
            var user = new User("testuser", "test@example.com", "hashedpassword");
            DateTime beforeLogin = DateTime.UtcNow;

            user.SetLastLogin();

            Assert.NotNull(user.LastLogin);
            Assert.True(user.LastLogin >= beforeLogin);
            Assert.True(user.LastLogin <= DateTime.UtcNow);
        }

        [Fact]
        public void AddRole_WithValidRole_ShouldAddRoleToUserRoles()
        {
            var user = new User("testuser", "test@example.com", "hashedpassword");
            var role = new Role("Admin", "Administrator role");

            user.AddRole(role);

            Assert.Single(user.UserRoles);
            Assert.Equal(role.Id, user.UserRoles.First().RoleId);
        }

        [Fact]
        public void RemoveRole_WithExistingRole_ShouldRemoveRoleFromUserRoles()
        {
            var user = new User("testuser", "test@example.com", "hashedpassword");
            var role = new Role("Admin", "Administrator role");
            user.AddRole(role);

            user.RemoveRole(role);

            Assert.Empty(user.UserRoles);
        }
    }
}