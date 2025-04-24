using FinanceSystem.Web.Models.IncomeInstallment;
using System.ComponentModel.DataAnnotations;

namespace FinanceSystem.Web.Models.Income
{
    // Modelo principal para exibição de receitas
    public class IncomeModel
    {
        public required string Id { get; set; }

        [Display(Name = "Descrição")]
        public required string Description { get; set; }

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
        public required string StatusDescription { get; set; }

        [Display(Name = "Recorrente")]
        public bool IsRecurring { get; set; }

        [Display(Name = "Observações")]
        public required string Notes { get; set; }

        [Display(Name = "Data de Criação")]
        [DisplayFormat(DataFormatString = "{0:dd/MM/yyyy HH:mm}")]
        public DateTime CreatedAt { get; set; }

        [Display(Name = "Última Atualização")]
        [DisplayFormat(DataFormatString = "{0:dd/MM/yyyy HH:mm}")]
        public DateTime? UpdatedAt { get; set; }

        public required string UserId { get; set; }

        [Display(Name = "Tipo de Receita")]
        public required string IncomeTypeId { get; set; }

        [Display(Name = "Tipo de Receita")]
        public required string IncomeTypeName { get; set; }

        [Display(Name = "Parcelas")]
        public List<IncomeInstallmentModel> Installments { get; set; } = [];

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
}