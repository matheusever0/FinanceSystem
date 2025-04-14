using FinanceSystem.Domain.Enums;
using System.ComponentModel.DataAnnotations;

namespace FinanceSystem.Application.DTOs
{
    public class UpdatePaymentDto
    {
        [StringLength(100, MinimumLength = 3)]
        public string Description { get; set; }

        [Range(0.01, double.MaxValue, ErrorMessage = "O valor deve ser maior que zero")]
        public decimal Amount { get; set; }

        public DateTime? DueDate { get; set; }

        public DateTime? PaymentDate { get; set; }

        public PaymentStatus? Status { get; set; }

        public bool? IsRecurring { get; set; }

        [StringLength(500)]
        public string Notes { get; set; }

        public Guid? PaymentTypeId { get; set; }

        public Guid? PaymentMethodId { get; set; }
    }
}
