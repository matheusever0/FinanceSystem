namespace FinanceSystem.Domain.Entities
{
    public class RolePermission
    {
        public Guid RoleId { get; protected set; }
        public Role Role { get; protected set; }
        public Guid PermissionId { get; protected set; }
        public Permission Permission { get; protected set; }
        public DateTime CreatedAt { get; protected set; }

        protected RolePermission() { }

        public RolePermission(Role role, Permission permission)
        {
            RoleId = role.Id;
            Role = role;
            PermissionId = permission.Id;
            Permission = permission;
            CreatedAt = DateTime.UtcNow;
        }
    }
}
