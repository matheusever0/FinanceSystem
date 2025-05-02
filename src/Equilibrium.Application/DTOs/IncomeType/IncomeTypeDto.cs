namespace Equilibrium.Application.DTOs.IncomeType
{
    public class IncomeTypeDto
    {
        public Guid Id { get; set; }
        public required string Name { get; set; }
        public required string Description { get; set; }
        public bool IsSystem { get; set; }
        public Guid? UserId { get; set; }
        public DateTime CreatedAt { get; set; }
    }
}