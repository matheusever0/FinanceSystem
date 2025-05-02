using System.ComponentModel.DataAnnotations;

namespace Equilibrium.Web.Models.Financing
{
    public class CreateFinancingModel
    {
        [Required(ErrorMessage = "A descrição é obrigatória")]
        [StringLength(100, MinimumLength = 3, ErrorMessage = "A descrição deve ter entre 3 e 100 caracteres")]
        [Display(Name = "Descrição")]
        public string Description { get; set; }

        [Required(ErrorMessage = "O valor total é obrigatório")]
        [Range(0.01, double.MaxValue, ErrorMessage = "O valor deve ser maior que zero")]
        [Display(Name = "Valor Total")]
        public decimal TotalAmount { get; set; }

        [Required(ErrorMessage = "A taxa de juros é obrigatória")]
        [Range(0, 100, ErrorMessage = "A taxa de juros deve estar entre 0 e 100")]
        [Display(Name = "Taxa de Juros (%)")]
        public decimal InterestRate { get; set; }

        [Required(ErrorMessage = "O prazo é obrigatório")]
        [Range(1, 600, ErrorMessage = "O prazo deve estar entre 1 e 600 meses")]
        [Display(Name = "Prazo (meses)")]
        public int TermMonths { get; set; }

        [Required(ErrorMessage = "A data de início é obrigatória")]
        [Display(Name = "Data de Início")]
        [DataType(DataType.Date)]
        public DateTime StartDate { get; set; } = DateTime.Today;

        [Required(ErrorMessage = "O tipo de financiamento é obrigatório")]
        [Display(Name = "Tipo")]
        public int Type { get; set; }

        [Required(ErrorMessage = "O índice de correção é obrigatório")]
        [Display(Name = "Índice de Correção")]
        public int CorrectionIndex { get; set; }

        [StringLength(500, ErrorMessage = "As observações devem ter no máximo 500 caracteres")]
        [Display(Name = "Observações")]
        public string Notes { get; set; } = "";

        [Display(Name = "Tipo de Pagamento")]
        public string PaymentTypeId { get; set; }
    }
}
