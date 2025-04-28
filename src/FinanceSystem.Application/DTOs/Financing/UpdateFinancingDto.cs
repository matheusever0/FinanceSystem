using System.ComponentModel.DataAnnotations;

namespace FinanceSystem.Application.DTOs.Financing
{
    public class UpdateFinancingDto
    {
        [StringLength(100, MinimumLength = 3)]
        public required string Description { get; set; }

        [StringLength(1000)]
        public required string Notes { get; set; }
    }
}
