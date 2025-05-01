using FinanceSystem.Web.Models.Payment;
using System.ComponentModel.DataAnnotations;

namespace FinanceSystem.Web.Models.Financing
{
    public class FinancingDetailModel : FinancingModel
    {
        [Display(Name = "Parcelas")]
        public List<FinancingInstallmentModel> Installments { get; set; } = [];

        [Display(Name = "Pagamentos")]
        public List<PaymentModel> Payments { get; set; } = [];

        [Display(Name = "Total de Juros Pago")]
        [DisplayFormat(DataFormatString = "{0:C2}")]
        public decimal TotalInterestPaid { get; set; }

        [Display(Name = "Total de Juros Restante")]
        [DisplayFormat(DataFormatString = "{0:C2}")]
        public decimal TotalInterestRemaining { get; set; }

        [Display(Name = "Total de Amortização Pago")]
        [DisplayFormat(DataFormatString = "{0:C2}")]
        public decimal TotalAmortizationPaid { get; set; }

        [Display(Name = "Valor Médio das Parcelas")]
        [DisplayFormat(DataFormatString = "{0:C2}")]
        public decimal AverageInstallmentAmount { get; set; }

        [Display(Name = "Pagamento Médio Mensal")]
        [DisplayFormat(DataFormatString = "{0:C2}")]
        public decimal MonthlyAveragePayment { get; set; }

        [Display(Name = "Custo Total Estimado")]
        [DisplayFormat(DataFormatString = "{0:C2}")]
        public decimal EstimatedTotalCost { get; set; }
    }
}
