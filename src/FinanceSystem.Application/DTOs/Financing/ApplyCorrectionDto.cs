using System.ComponentModel.DataAnnotations;

namespace FinanceSystem.Application.DTOs.Financing
{
    public class ApplyCorrectionDto
    {
        [Required]
        public Guid FinancingId { get; set; }

        [Required]
        [Range(0.000001, 100, ErrorMessage = "O valor do índice deve estar entre 0,0001% e 100%")]
        public decimal IndexValue { get; set; }

        [Required]
        public DateTime CorrectionDate { get; set; } = DateTime.Now;
    }
}
