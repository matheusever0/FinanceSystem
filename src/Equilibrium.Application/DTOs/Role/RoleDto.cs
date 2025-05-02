namespace Equilibrium.Application.DTOs.Role
{
    public class RoleDto
    {
        public Guid Id { get; set; }
        public required string Name { get; set; }
        public required string Description { get; set; }
        public DateTime CreatedAt { get; set; }
        public List<string> Permissions { get; set; } = [];
    }
}