namespace FinanceSystem.Domain.Entities
{
    public class UserRole
    {
        public Guid UserId { get; protected set; }
        public User User { get; protected set; }
        public Guid RoleId { get; protected set; }
        public Role Role { get; protected set; }
        public DateTime CreatedAt { get; protected set; }

        // Construtor protegido para o EF Core
        protected UserRole() { }

        public UserRole(User user, Role role)
        {
            UserId = user.Id;
            User = user;
            RoleId = role.Id;
            Role = role;
            CreatedAt = DateTime.UtcNow;
        }
    }
}
