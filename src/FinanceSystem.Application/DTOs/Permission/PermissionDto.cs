namespace FinanceSystem.Application.DTOs
{
    public class PermissionDto
    {
        public Guid Id { get; set; }
        public string Name { get; set; }
        public string SystemName { get; set; }
        public string Description { get; set; }
        public DateTime CreatedAt { get; set; }
    }
}