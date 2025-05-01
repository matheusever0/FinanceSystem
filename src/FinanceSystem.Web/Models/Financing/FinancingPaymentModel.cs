using System.ComponentModel.DataAnnotations;

namespace FinanceSystem.Web.Models.Financing
{
    public class FinancingPaymentModel
    {
        [Required]
        public string FinancingId { get; set; }

        [Required]
        public string InstallmentId { get; set; }

        [Required]
        [Range(0.01, double.MaxValue, ErrorMessage = "O valor deve ser maior que zero")]
        [Display(Name = "Valor")]
        public decimal Amount { get; set; }

        [Required]
        [Display(Name = "Data de Pagamento")]
        [DataType(DataType.Date)]
        public DateTime PaymentDate { get; set; } = DateTime.Today;

        [Required]
        [Display(Name = "Método de Pagamento")]
        public string PaymentMethodId { get; set; }

        [Display(Name = "Observações")]
        [StringLength(200, ErrorMessage = "As observações devem ter no máximo 200 caracteres")]
        public string Notes { get; set; }
    }
}
