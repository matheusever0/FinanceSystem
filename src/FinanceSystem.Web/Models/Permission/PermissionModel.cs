namespace FinanceSystem.Web.Models.Permission
{
    public class PermissionModel
    {
        public string Id { get; set; }
        public string Name { get; set; }
        public string SystemName { get; set; }
        public string Description { get; set; }
        public DateTime CreatedAt { get; set; }
    }
}