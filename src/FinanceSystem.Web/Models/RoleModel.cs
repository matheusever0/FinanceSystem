namespace FinanceSystem.Web.Models
{
    public class RoleModel
    {
        public string Id { get; set; }
        public string Name { get; set; }
        public string Description { get; set; }
        public DateTime CreatedAt { get; set; }
        public List<string> Permissions { get; set; } = new List<string>();
    }
}