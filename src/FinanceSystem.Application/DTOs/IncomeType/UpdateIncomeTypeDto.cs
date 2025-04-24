using System.ComponentModel.DataAnnotations;

namespace FinanceSystem.Application.DTOs.IncomeType
{
    public class UpdateIncomeTypeDto
    {
        [StringLength(50, MinimumLength = 3)]
        public required string Name { get; set; }

        [StringLength(200)]
        public required string Description { get; set; }
    }
}