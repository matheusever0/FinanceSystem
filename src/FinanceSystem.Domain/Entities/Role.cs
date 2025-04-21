namespace FinanceSystem.Domain.Entities
{
    public class Role
    {
        public Guid Id { get; protected set; }
        public string Name { get; protected set; }
        public string Description { get; protected set; }
        public DateTime CreatedAt { get; protected set; }
        public ICollection<UserRole> UserRoles { get; protected set; }
        public ICollection<RolePermission> RolePermissions { get; protected set; }

        protected Role()
        {
            UserRoles = new List<UserRole>();
            RolePermissions = new List<RolePermission>();
        }

        public Role(string name, string description)
        {
            Id = Guid.NewGuid();
            Name = name;
            Description = description;
            CreatedAt = DateTime.Now;
            UserRoles = new List<UserRole>();
            RolePermissions = new List<RolePermission>();
        }

        public void UpdateName(string name)
        {
            Name = name;
        }

        public void UpdateDescription(string description)
        {
            Description = description;
        }

        public void AddPermission(Permission permission)
        {
            if (!RolePermissions.Any(rp => rp.PermissionId == permission.Id))
            {
                RolePermissions.Add(new RolePermission(this, permission));
            }
        }

        public void RemovePermission(Permission permission)
        {
            var rolePermission = RolePermissions.FirstOrDefault(rp => rp.PermissionId == permission.Id);
            if (rolePermission != null)
            {
                RolePermissions.Remove(rolePermission);
            }
        }

        public bool HasPermission(string permissionSystemName)
        {
            return RolePermissions.Any(rp => rp.Permission.SystemName == permissionSystemName);
        }
    }
}