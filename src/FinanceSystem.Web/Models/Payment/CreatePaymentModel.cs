using System.ComponentModel.DataAnnotations;

namespace FinanceSystem.Web.Models.Payment
{
    public class CreatePaymentModel
    {
        [Required(ErrorMessage = "A descrição é obrigatória")]
        [StringLength(100, MinimumLength = 3, ErrorMessage = "A descrição deve ter entre 3 e 100 caracteres")]
        [Display(Name = "Descrição")]
        public string Description { get; set; }

        [Required(ErrorMessage = "O valor é obrigatório")]
        [Range(0.01, double.MaxValue, ErrorMessage = "O valor deve ser maior que zero")]
        [Display(Name = "Valor")]
        public decimal Amount { get; set; }

        [Required(ErrorMessage = "A data de vencimento é obrigatória")]
        [Display(Name = "Data de Vencimento")]
        [DataType(DataType.Date)]
        public DateTime DueDate { get; set; } = DateTime.Today;

        [Display(Name = "Data de Pagamento")]
        [DataType(DataType.Date)]
        public DateTime? PaymentDate { get; set; }

        [Display(Name = "Pagamento Recorrente")]
        public bool IsRecurring { get; set; }

        [Display(Name = "Observações")]
        [StringLength(500, ErrorMessage = "As observações devem ter no máximo 500 caracteres")]
        public string Notes { get; set; }

        [Required(ErrorMessage = "O tipo de pagamento é obrigatório")]
        [Display(Name = "Tipo de Pagamento")]
        public string PaymentTypeId { get; set; }

        [Required(ErrorMessage = "O método de pagamento é obrigatório")]
        [Display(Name = "Método de Pagamento")]
        public string PaymentMethodId { get; set; }

        [Range(1, 48, ErrorMessage = "O número de parcelas deve estar entre 1 e 48")]
        [Display(Name = "Número de Parcelas")]
        public int NumberOfInstallments { get; set; } = 1;

        [Display(Name = "Cartão de Crédito")]
        public string CreditCardId { get; set; }
    }
}