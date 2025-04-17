using System.ComponentModel.DataAnnotations;

namespace FinanceSystem.Application.DTOs.IncomeType
{
    public class CreateIncomeTypeDto
    {
        [Required]
        [StringLength(50, MinimumLength = 3)]
        public string Name { get; set; }

        [StringLength(200)]
        public string Description { get; set; }
    }
}