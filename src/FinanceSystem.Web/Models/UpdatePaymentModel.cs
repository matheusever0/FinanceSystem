using System.ComponentModel.DataAnnotations;

namespace FinanceSystem.Web.Models
{
    public class UpdatePaymentModel
    {
        [StringLength(100, MinimumLength = 3, ErrorMessage = "A descrição deve ter entre 3 e 100 caracteres")]
        [Display(Name = "Descrição")]
        public string Description { get; set; }

        [Range(0.01, double.MaxValue, ErrorMessage = "O valor deve ser maior que zero")]
        [Display(Name = "Valor")]
        public decimal Amount { get; set; }

        [Display(Name = "Data de Vencimento")]
        [DataType(DataType.Date)]
        public DateTime? DueDate { get; set; }

        [Display(Name = "Data de Pagamento")]
        [DataType(DataType.Date)]
        public DateTime? PaymentDate { get; set; }

        [Display(Name = "Status")]
        public int? Status { get; set; }

        [Display(Name = "Pagamento Recorrente")]
        public bool? IsRecurring { get; set; }

        [Display(Name = "Observações")]
        [StringLength(500, ErrorMessage = "As observações devem ter no máximo 500 caracteres")]
        public string Notes { get; set; }

        [Display(Name = "Tipo de Pagamento")]
        public string PaymentTypeId { get; set; }

        [Display(Name = "Método de Pagamento")]
        public string PaymentMethodId { get; set; }
    }
}