using Equilibrium.Domain.Enums;
using System.ComponentModel.DataAnnotations;

namespace Equilibrium.Application.DTOs.Financing
{
    public class FinancingSimulationRequestDto
    {
        [Required]
        [Range(1000, 100000000, ErrorMessage = "O valor deve estar entre 1.000 e 100.000.000")]
        public decimal TotalAmount { get; set; }

        [Required]
        [Range(0.01, 100, ErrorMessage = "A taxa de juros deve estar entre 0,01% e 100%")]
        public decimal InterestRate { get; set; }

        [Required]
        [Range(1, 600, ErrorMessage = "O prazo deve estar entre 1 e 600 meses")]
        public int TermMonths { get; set; }

        [Required]
        public FinancingType Type { get; set; }

        [Required]
        public DateTime StartDate { get; set; } = DateTime.Now;
    }
}
