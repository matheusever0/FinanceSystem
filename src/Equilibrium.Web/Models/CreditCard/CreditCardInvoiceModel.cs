using System.ComponentModel.DataAnnotations;

namespace Equilibrium.Web.Models.CreditCard
{
    public class CreditCardInvoiceModel
    {
        public required string CreditCardId { get; set; }

        [Display(Name = "Cartão de Crédito")]
        public required string CreditCardName { get; set; }

        [Display(Name = "Data de Referência")]
        [DisplayFormat(DataFormatString = "{0:dd/MM/yyyy}")]
        public DateTime ReferenceDate { get; set; }

        [Display(Name = "Data de Vencimento")]
        [DisplayFormat(DataFormatString = "{0:dd/MM/yyyy}")]
        public DateTime DueDate { get; set; }

        [Display(Name = "Data de Fechamento")]
        [DisplayFormat(DataFormatString = "{0:dd/MM/yyyy}")]
        public DateTime ClosingDate { get; set; }

        [Display(Name = "Valor Total")]
        [DisplayFormat(DataFormatString = "{0:C2}")]
        public decimal TotalAmount { get; set; }

        [Display(Name = "Valor Pago")]
        [DisplayFormat(DataFormatString = "{0:C2}")]
        public decimal PaidAmount { get; set; }

        [Display(Name = "Valor Pendente")]
        [DisplayFormat(DataFormatString = "{0:C2}")]
        public decimal RemainingAmount { get; set; }

        [Display(Name = "Paga")]
        public bool IsPaid { get; set; }

        [Display(Name = "Vencida")]
        public bool IsOverdue { get; set; }

        [Display(Name = "Quantidade de Transações")]
        public int TransactionCount { get; set; }

        [Display(Name = "Limite Disponível")]
        [DisplayFormat(DataFormatString = "{0:C2}")]
        public decimal AvailableLimit { get; set; }

        [Display(Name = "Limite Usado")]
        [DisplayFormat(DataFormatString = "{0:C2}")]
        public decimal UsedLimit { get; set; }

        [Display(Name = "Limite Total")]
        [DisplayFormat(DataFormatString = "{0:C2}")]
        public decimal TotalLimit { get; set; }

        // Propriedades calculadas (seguindo padrão do projeto)
        [Display(Name = "Percentual de Uso")]
        [DisplayFormat(DataFormatString = "{0:P0}")]
        public decimal UsagePercentage => TotalLimit > 0 ? (UsedLimit / TotalLimit) * 100 : 0;

        [Display(Name = "Status")]
        public string StatusDescription => IsPaid ? "Paga" : IsOverdue ? "Vencida" : "Pendente";

        [Display(Name = "Status")]
        public string StatusBadgeClass => IsPaid ? "bg-success" : IsOverdue ? "bg-danger" : "bg-warning";

        // Métodos de formatação (seguindo padrão do projeto)
        public string GetFormattedTotalAmount() => string.Format("{0:C2}", TotalAmount);
        public string GetFormattedPaidAmount() => string.Format("{0:C2}", PaidAmount);
        public string GetFormattedRemainingAmount() => string.Format("{0:C2}", RemainingAmount);
        public string GetFormattedDueDate() => DueDate.ToString("dd/MM/yyyy");
        public string GetFormattedClosingDate() => ClosingDate.ToString("dd/MM/yyyy");
        public string GetFormattedReferenceDate() => ReferenceDate.ToString("dd/MM/yyyy");
        public string GetFormattedUsagePercentage() => string.Format("{0:P0}", UsagePercentage / 100);
        public string GetFormattedAvailableLimit() => string.Format("{0:C2}", AvailableLimit);
        public string GetFormattedUsedLimit() => string.Format("{0:C2}", UsedLimit);
        public string GetFormattedTotalLimit() => string.Format("{0:C2}", TotalLimit);
    }
}