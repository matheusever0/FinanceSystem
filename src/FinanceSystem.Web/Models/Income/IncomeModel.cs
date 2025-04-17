using FinanceSystem.Web.Models.IncomeInstallment;
using System.ComponentModel.DataAnnotations;

namespace FinanceSystem.Web.Models.Income
{
    // Modelo principal para exibição de receitas
    public class IncomeModel
    {
        public string Id { get; set; }

        [Display(Name = "Descrição")]
        public string Description { get; set; }

        [Display(Name = "Valor")]
        [DisplayFormat(DataFormatString = "{0:C2}")]
        public decimal Amount { get; set; }

        [Display(Name = "Data de Vencimento")]
        [DisplayFormat(DataFormatString = "{0:dd/MM/yyyy}")]
        public DateTime DueDate { get; set; }

        [Display(Name = "Data de Recebimento")]
        [DisplayFormat(DataFormatString = "{0:dd/MM/yyyy}")]
        public DateTime? ReceivedDate { get; set; }

        [Display(Name = "Status")]
        public int Status { get; set; }

        [Display(Name = "Status")]
        public string StatusDescription { get; set; }

        [Display(Name = "Recorrente")]
        public bool IsRecurring { get; set; }

        [Display(Name = "Observações")]
        public string Notes { get; set; }

        [Display(Name = "Data de Criação")]
        [DisplayFormat(DataFormatString = "{0:dd/MM/yyyy HH:mm}")]
        public DateTime CreatedAt { get; set; }

        [Display(Name = "Última Atualização")]
        [DisplayFormat(DataFormatString = "{0:dd/MM/yyyy HH:mm}")]
        public DateTime? UpdatedAt { get; set; }

        public string UserId { get; set; }

        [Display(Name = "Tipo de Receita")]
        public string IncomeTypeId { get; set; }

        [Display(Name = "Tipo de Receita")]
        public string IncomeTypeName { get; set; }

        [Display(Name = "Parcelas")]
        public List<IncomeInstallmentModel> Installments { get; set; } = new List<IncomeInstallmentModel>();

        [Display(Name = "Status")]
        public string StatusBadgeClass => GetStatusBadgeClass();

        private string GetStatusBadgeClass()
        {
            return Status switch
            {
                1 => "bg-warning", // Pendente
                2 => "bg-success", // Recebido
                3 => "bg-secondary", // Cancelado
                _ => "bg-primary",
            };
        }

        public string GetFormattedAmount()
        {
            return string.Format("{0:C2}", Amount);
        }

        public string GetFormattedDueDate()
        {
            return DueDate.ToString("dd/MM/yyyy");
        }

        public string GetFormattedReceivedDate()
        {
            return ReceivedDate?.ToString("dd/MM/yyyy") ?? "-";
        }
    }

    // Modelo para criação de novas receitas
    public class CreateIncomeModel
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

        [Display(Name = "Data de Recebimento")]
        [DataType(DataType.Date)]
        public DateTime? ReceivedDate { get; set; }

        [Display(Name = "Receita Recorrente")]
        public bool IsRecurring { get; set; }

        [Display(Name = "Observações")]
        [StringLength(500, ErrorMessage = "As observações devem ter no máximo 500 caracteres")]
        public string Notes { get; set; }

        [Required(ErrorMessage = "O tipo de receita é obrigatório")]
        [Display(Name = "Tipo de Receita")]
        public string IncomeTypeId { get; set; }

        [Range(1, 48, ErrorMessage = "O número de parcelas deve estar entre 1 e 48")]
        [Display(Name = "Número de Parcelas")]
        public int NumberOfInstallments { get; set; } = 1;
    }

    // Modelo para atualização de receitas existentes
    public class UpdateIncomeModel
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

        [Display(Name = "Data de Recebimento")]
        [DataType(DataType.Date)]
        public DateTime? ReceivedDate { get; set; }

        [Display(Name = "Status")]
        public int? Status { get; set; }

        [Display(Name = "Receita Recorrente")]
        public bool? IsRecurring { get; set; }

        [Display(Name = "Observações")]
        [StringLength(500, ErrorMessage = "As observações devem ter no máximo 500 caracteres")]
        public string Notes { get; set; }

        [Display(Name = "Tipo de Receita")]
        public string IncomeTypeId { get; set; }
    }
}