using System.ComponentModel.DataAnnotations;

namespace FinanceSystem.Web.Models.Income
{
    public class UpdateIncomeModel
    {
        public string Id { get; set; }
        [StringLength(100, MinimumLength = 3, ErrorMessage = "A descrição deve ter entre 3 e 100 caracteres")]
        [Display(Name = "Descrição")]
        public string Description { get; set; }

        [Range(0.01, double.MaxValue, ErrorMessage = "O valor deve ser maior que zero")]
        [Display(Name = "Valor")]
        public decimal Amount { get; set; }

        [Display(Name = "Data de Vencimento")]
        [DataType(DataType.Date)]
        public DateTime? DueDate { get; set; }

        [Display(Name = "Data de Recebimento")]
        [DataType(DataType.Date)]
        public DateTime? ReceivedDate { get; set; }

        [Display(Name = "Status")]
        public int? Status { get; set; }

        [Display(Name = "Receita Recorrente")]
        public bool IsRecurring { get; set; }

        [Display(Name = "Observações")]
        [StringLength(500, ErrorMessage = "As observações devem ter no máximo 500 caracteres")]
        public string Notes { get; set; }

        [Display(Name = "Tipo de Receita")]
        public string IncomeTypeId { get; set; }
    }
}
