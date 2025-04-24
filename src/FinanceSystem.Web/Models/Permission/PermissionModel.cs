namespace FinanceSystem.Web.Models.Permission
{
    public class PermissionModel
    {
        public required string Id { get; set; }
        public required string Name { get; set; }
        public required string SystemName { get; set; }
        public required string Description { get; set; }
        public DateTime CreatedAt { get; set; }
    }
}