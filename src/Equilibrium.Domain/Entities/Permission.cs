namespace Equilibrium.Domain.Entities
{
    public class Permission
    {
        public Guid Id { get; protected set; }
        public string Name { get; protected set; }
        public string Description { get; protected set; }
        public string SystemName { get; protected set; }
        public DateTime CreatedAt { get; protected set; }
        public ICollection<RolePermission> RolePermissions { get; protected set; }

        protected Permission()
        {
            RolePermissions = [];
        }

        public Permission(string name, string systemName, string description)
        {
            Id = Guid.NewGuid();
            Name = name;
            SystemName = systemName;
            Description = description;
            CreatedAt = DateTime.Now;
            RolePermissions = [];
        }

        public void UpdateName(string name)
        {
            Name = name;
        }

        public void UpdateDescription(string description)
        {
            Description = description;
        }
    }
}
