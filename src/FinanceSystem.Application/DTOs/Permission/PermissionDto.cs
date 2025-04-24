namespace FinanceSystem.Application.DTOs.Permission
{
    public class PermissionDto
    {
        public Guid Id { get; set; }
        public required string Name { get; set; }
        public required string SystemName { get; set; }
        public required string Description { get; set; }
        public DateTime CreatedAt { get; set; }
    }
}