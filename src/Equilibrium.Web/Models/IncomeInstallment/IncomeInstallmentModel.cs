using System.ComponentModel.DataAnnotations;

namespace Equilibrium.Web.Models.IncomeInstallment
{
    public class IncomeInstallmentModel
    {
        public required string Id { get; set; }

        [Display(Name = "Número da Parcela")]
        public int InstallmentNumber { get; set; }

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

        [Display(Name = "Data de Criação")]
        [DisplayFormat(DataFormatString = "{0:dd/MM/yyyy HH:mm}")]
        public DateTime CreatedAt { get; set; }

        [Display(Name = "Última Atualização")]
        [DisplayFormat(DataFormatString = "{0:dd/MM/yyyy HH:mm}")]
        public DateTime? UpdatedAt { get; set; }

        public required string IncomeId { get; set; }

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