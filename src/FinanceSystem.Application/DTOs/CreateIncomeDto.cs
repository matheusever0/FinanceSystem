using System.ComponentModel.DataAnnotations;

namespace FinanceSystem.Application.DTOs
{
    public class CreateIncomeDto
    {
        [Required]
        [StringLength(100, MinimumLength = 3)]
        public string Description { get; set; }

        [Required]
        [Range(0.01, double.MaxValue, ErrorMessage = "O valor deve ser maior que zero")]
        public decimal Amount { get; set; }

        [Required]
        public DateTime DueDate { get; set; }

        public DateTime? ReceivedDate { get; set; }

        public bool IsRecurring { get; set; }

        [StringLength(500)]
        public string Notes { get; set; }

        [Required]
        public Guid IncomeTypeId { get; set; }

        [Range(1, 48, ErrorMessage = "O número de parcelas deve estar entre 1 e 48")]
        public int NumberOfInstallments { get; set; } = 1;
    }
}