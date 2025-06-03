using System.ComponentModel.DataAnnotations;

namespace Equilibrium.Application.DTOs.CreditCard
{
    public class PayInvoiceDto
    {
        [Required]
        [Range(0.01, double.MaxValue, ErrorMessage = "O valor deve ser maior que zero")]
        public decimal Amount { get; set; }

        [Required]
        public DateTime PaymentDate { get; set; } = DateTime.Now;

        [StringLength(500)]
        public string Notes { get; set; } = string.Empty;

        public bool PayFullAmount { get; set; } = true;
    }
}
