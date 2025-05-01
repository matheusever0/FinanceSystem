using System.ComponentModel.DataAnnotations;

namespace FinanceSystem.Web.Models.Financing
{
    public class FinancingModel
    {
        public string Id { get; set; }

        [Display(Name = "Descrição")]
        public string Description { get; set; }

        [Display(Name = "Valor Total")]
        [DisplayFormat(DataFormatString = "{0:C2}")]
        public decimal TotalAmount { get; set; }

        [Display(Name = "Taxa de Juros")]
        [DisplayFormat(DataFormatString = "{0:F2}%")]
        public decimal InterestRate { get; set; }

        [Display(Name = "Prazo (meses)")]
        public int TermMonths { get; set; }

        [Display(Name = "Data de Início")]
        [DisplayFormat(DataFormatString = "{0:dd/MM/yyyy}")]
        public DateTime StartDate { get; set; }

        [Display(Name = "Data de Término")]
        [DisplayFormat(DataFormatString = "{0:dd/MM/yyyy}")]
        public DateTime? EndDate { get; set; }

        [Display(Name = "Tipo")]
        public int Type { get; set; }

        [Display(Name = "Tipo")]
        public string TypeDescription { get; set; }

        [Display(Name = "Status")]
        public int Status { get; set; }

        [Display(Name = "Status")]
        public string StatusDescription { get; set; }

        [Display(Name = "Índice de Correção")]
        public int CorrectionIndex { get; set; }

        [Display(Name = "Índice de Correção")]
        public string CorrectionIndexDescription { get; set; }

        [Display(Name = "Observações")]
        public string Notes { get; set; }

        [Display(Name = "Data de Criação")]
        [DisplayFormat(DataFormatString = "{0:dd/MM/yyyy HH:mm}")]
        public DateTime CreatedAt { get; set; }

        [Display(Name = "Dívida Restante")]
        [DisplayFormat(DataFormatString = "{0:C2}")]
        public decimal RemainingDebt { get; set; }

        [Display(Name = "Última Correção")]
        [DisplayFormat(DataFormatString = "{0:dd/MM/yyyy}")]
        public DateTime? LastCorrectionDate { get; set; }

        [Display(Name = "Total Pago")]
        [DisplayFormat(DataFormatString = "{0:C2}")]
        public decimal TotalPaid { get; set; }

        [Display(Name = "Total Restante")]
        [DisplayFormat(DataFormatString = "{0:C2}")]
        public decimal TotalRemaining { get; set; }

        [Display(Name = "Progresso")]
        [DisplayFormat(DataFormatString = "{0:P2}")]
        public decimal ProgressPercentage { get; set; }

        [Display(Name = "Parcelas Pagas")]
        public int InstallmentsPaid { get; set; }

        [Display(Name = "Parcelas Restantes")]
        public int InstallmentsRemaining { get; set; }

        public string GetFormattedTotalAmount()
        {
            return string.Format("{0:C2}", TotalAmount);
        }

        public string GetFormattedRemainingDebt()
        {
            return string.Format("{0:C2}", RemainingDebt);
        }

        public string GetFormattedTotalPaid()
        {
            return string.Format("{0:C2}", TotalPaid);
        }

        public string GetFormattedProgress()
        {
            return string.Format("{0:P0}", ProgressPercentage);
        }
    }
}