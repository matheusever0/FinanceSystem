using System.ComponentModel.DataAnnotations;

namespace FinanceSystem.Application.DTOs.Payment
{
    public class CreatePaymentDto
    {
        [Required]
        [StringLength(100, MinimumLength = 3)]
        public string Description { get; set; }

        [Required]
        [Range(0.01, double.MaxValue, ErrorMessage = "O valor deve ser maior que zero")]
        public decimal Amount { get; set; }

        [Required]
        public DateTime DueDate { get; set; }

        public DateTime? PaymentDate { get; set; }

        public bool IsRecurring { get; set; }

        [StringLength(500)]
        public string Notes { get; set; }

        [Required]
        public Guid PaymentTypeId { get; set; }

        [Required]
        public Guid PaymentMethodId { get; set; }

        [Range(1, 48, ErrorMessage = "O número de parcelas deve estar entre 1 e 48")]
        public int NumberOfInstallments { get; set; } = 1;

        public Guid? CreditCardId { get; set; }
    }
}
