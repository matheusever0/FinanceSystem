namespace FinanceSystem.Web.Models.Role
{
    public class RoleModel
    {
        public required string Id { get; set; }
        public required string Name { get; set; }
        public required string Description { get; set; }
        public DateTime CreatedAt { get; set; }
        public List<string> Permissions { get; set; } = [];
    }
}