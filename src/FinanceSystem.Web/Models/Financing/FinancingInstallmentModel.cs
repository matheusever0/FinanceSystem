using System.ComponentModel.DataAnnotations;

namespace FinanceSystem.Web.Models.Financing
{
    public class FinancingInstallmentModel
    {
        public string Id { get; set; }

        [Display(Name = "Número da Parcela")]
        public int InstallmentNumber { get; set; }

        [Display(Name = "Valor Total")]
        [DisplayFormat(DataFormatString = "{0:C2}")]
        public decimal TotalAmount { get; set; }

        [Display(Name = "Valor Correção")]
        [DisplayFormat(DataFormatString = "{0:C2}")]
        public decimal? TotalCorrection { get; set; }

        [Display(Name = "Juros")]
        [DisplayFormat(DataFormatString = "{0:C2}")]
        public decimal InterestAmount { get; set; }

        [Display(Name = "Amortização")]
        [DisplayFormat(DataFormatString = "{0:C2}")]
        public decimal AmortizationAmount { get; set; }

        [Display(Name = "Data de Vencimento")]
        [DisplayFormat(DataFormatString = "{0:dd/MM/yyyy}")]
        public DateTime DueDate { get; set; }

        [Display(Name = "Data de Pagamento")]
        [DisplayFormat(DataFormatString = "{0:dd/MM/yyyy}")]
        public DateTime? PaymentDate { get; set; }

        [Display(Name = "Status")]
        public int Status { get; set; }

        [Display(Name = "Status")]
        public string StatusDescription { get; set; }

        [Display(Name = "Valor Pago")]
        [DisplayFormat(DataFormatString = "{0:C2}")]
        public decimal PaidAmount { get; set; }

        [Display(Name = "Valor Restante")]
        [DisplayFormat(DataFormatString = "{0:C2}")]
        public decimal RemainingAmount { get; set; }

        public string FinancingId { get; set; }

        public string GetFormattedTotalAmount()
        {
            return string.Format("{0:C2}", TotalAmount);
        }

        public string GetFormattedTotalCorrection()
        {
            return string.Format("{0:C2}", TotalCorrection);
        }

        public string GetFormattedDueDate()
        {
            return DueDate.ToString("dd/MM/yyyy");
        }

        public string GetFormattedPaymentDate()
        {
            return PaymentDate?.ToString("dd/MM/yyyy") ?? "-";
        }

        [Display(Name = "Diferença Percentual")]
        [DisplayFormat(DataFormatString = "{0:P2}")]
        public decimal PercentageDifference
        {
            get
            {
                if (PaidAmount == 0 || TotalAmount == 0)
                    return 0;

                return (PaidAmount - TotalAmount) / TotalAmount;
            }
        }
        [Display(Name = "Tipo de Diferença")]
        public string DifferenceType
        {
            get
            {
                if (PercentageDifference == 0)
                    return "Sem alteração";
                else if (PercentageDifference < 0)
                    return "Economia";
                else
                    return "Correção";
            }
        }
        public string GetFormattedDifference()
        {
            if (PercentageDifference == 0)
                return "Sem alteração no valor";
            else if (PercentageDifference < 0)
                return $"Economia de {Math.Abs(PercentageDifference):P2}";
            else
                return $"Correção de {PercentageDifference:P2}";
        }
    }
}
