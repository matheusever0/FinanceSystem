namespace FinanceSystem.Domain.Entities
{
    public class Role
    {
        public Guid Id { get; protected set; }
        public string Name { get; protected set; }
        public string Description { get; protected set; }
        public DateTime CreatedAt { get; protected set; }
        public ICollection<UserRole> UserRoles { get; protected set; }

        // Construtor protegido para o EF Core
        protected Role()
        {
            // Inicializar a coleção para evitar NullReferenceException
            UserRoles = new List<UserRole>();
        }

        public Role(string name, string description = null)
        {
            Id = Guid.NewGuid();
            Name = name;
            Description = description;
            CreatedAt = DateTime.UtcNow;
            UserRoles = new List<UserRole>();
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
