using FinanceSystem.Domain.Entities;

namespace FinanceSystem.Domain.Tests.Entities
{
    public class RoleTests
    {
        [Fact]
        public void Constructor_WithValidParameters_ShouldCreateRoleCorrectly()
        {
            string name = "Admin";
            string description = "Administrator role";

            var role = new Role(name, description);

            Assert.Equal(name, role.Name);
            Assert.Equal(description, role.Description);
            Assert.NotEqual(Guid.Empty, role.Id);
            Assert.NotNull(role.UserRoles);
            Assert.Empty(role.UserRoles);
        }

        [Fact]
        public void Constructor_WithNameOnly_ShouldCreateRoleWithNullDescription()
        {
            string name = "User";

            var role = new Role(name, null);

            Assert.Equal(name, role.Name);
            Assert.Null(role.Description);
        }

        [Fact]
        public void UpdateName_WithValidName_ShouldUpdateName()
        {
            var role = new Role("OldRole", "");
            string newName = "NewRole";

            role.UpdateName(newName);

            Assert.Equal(newName, role.Name);
        }

        [Fact]
        public void UpdateDescription_WithValidDescription_ShouldUpdateDescription()
        {
            var role = new Role("Admin", "Old description");
            string newDescription = "New description";

            role.UpdateDescription(newDescription);

            Assert.Equal(newDescription, role.Description);
        }
    }
}