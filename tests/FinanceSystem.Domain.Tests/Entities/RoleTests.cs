using FinanceSystem.Domain.Entities;

namespace FinanceSystem.Domain.Tests.Entities
{
    public class RoleTests
    {
        [Fact]
        public void Constructor_WithValidParameters_ShouldCreateRoleCorrectly()
        {
            // Arrange
            string name = "Admin";
            string description = "Administrator role";

            // Act
            var role = new Role(name, description);

            // Assert
            Assert.Equal(name, role.Name);
            Assert.Equal(description, role.Description);
            Assert.NotEqual(Guid.Empty, role.Id);
            Assert.NotNull(role.UserRoles);
            Assert.Empty(role.UserRoles);
        }

        [Fact]
        public void Constructor_WithNameOnly_ShouldCreateRoleWithNullDescription()
        {
            // Arrange
            string name = "User";

            // Act
            var role = new Role(name);

            // Assert
            Assert.Equal(name, role.Name);
            Assert.Null(role.Description);
        }

        [Fact]
        public void UpdateName_WithValidName_ShouldUpdateName()
        {
            // Arrange
            var role = new Role("OldRole");
            string newName = "NewRole";

            // Act
            role.UpdateName(newName);

            // Assert
            Assert.Equal(newName, role.Name);
        }

        [Fact]
        public void UpdateDescription_WithValidDescription_ShouldUpdateDescription()
        {
            // Arrange
            var role = new Role("Admin", "Old description");
            string newDescription = "New description";

            // Act
            role.UpdateDescription(newDescription);

            // Assert
            Assert.Equal(newDescription, role.Description);
        }
    }
}