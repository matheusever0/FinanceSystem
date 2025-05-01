using FinanceSystem.Web.Models.Payment;
using System.ComponentModel.DataAnnotations;

namespace FinanceSystem.Web.Models.Financing
{
    /// <summary>
    /// Model de financiamento
    /// </summary>
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

    /// <summary>
    /// Model para criar financiamento
    /// </summary>
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

    /// <summary>
    /// Model para atualizar financiamento
    /// </summary>
    public class UpdateFinancingModel
    {
        [StringLength(100, MinimumLength = 3, ErrorMessage = "A descrição deve ter entre 3 e 100 caracteres")]
        [Display(Name = "Descrição")]
        public string Description { get; set; }

        [StringLength(500, ErrorMessage = "As observações devem ter no máximo 500 caracteres")]
        [Display(Name = "Observações")]
        public string Notes { get; set; }
    }

    /// <summary>
    /// Model de parcela de financiamento
    /// </summary>
    public class FinancingInstallmentModel
    {
        public string Id { get; set; }

        [Display(Name = "Número da Parcela")]
        public int InstallmentNumber { get; set; }

        [Display(Name = "Valor Total")]
        [DisplayFormat(DataFormatString = "{0:C2}")]
        public decimal TotalAmount { get; set; }

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

        public string GetFormattedDueDate()
        {
            return DueDate.ToString("dd/MM/yyyy");
        }

        public string GetFormattedPaymentDate()
        {
            return PaymentDate?.ToString("dd/MM/yyyy") ?? "-";
        }
    }

    /// <summary>
    /// Model para simular financiamento
    /// </summary>
    public class FinancingSimulationRequestModel
    {
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

        [Required(ErrorMessage = "O tipo de financiamento é obrigatório")]
        [Display(Name = "Tipo")]
        public int Type { get; set; }

        [Required(ErrorMessage = "A data de início é obrigatória")]
        [Display(Name = "Data de Início")]
        [DataType(DataType.Date)]
        public DateTime StartDate { get; set; } = DateTime.Today;
    }

    /// <summary>
    /// Model do resultado da simulação de financiamento
    /// </summary>
    public class FinancingSimulationModel
    {
        public decimal TotalAmount { get; set; }
        public decimal InterestRate { get; set; }
        public int TermMonths { get; set; }
        public int Type { get; set; }
        public string TypeDescription { get; set; }
        public decimal FirstInstallmentAmount { get; set; }
        public decimal LastInstallmentAmount { get; set; }
        public decimal TotalInterest { get; set; }
        public decimal TotalCost { get; set; }
        public decimal MonthlyDecreaseAmount { get; set; }
        public List<FinancingInstallmentSimulationModel> Installments { get; set; } = new List<FinancingInstallmentSimulationModel>();

        public string GetFormattedTotalAmount()
        {
            return string.Format("{0:C2}", TotalAmount);
        }

        public string GetFormattedInterestRate()
        {
            return string.Format("{0:F2}%", InterestRate);
        }

        public string GetFormattedTotalInterest()
        {
            return string.Format("{0:C2}", TotalInterest);
        }

        public string GetFormattedTotalCost()
        {
            return string.Format("{0:C2}", TotalCost);
        }
    }

    /// <summary>
    /// Model de parcela na simulação de financiamento
    /// </summary>
    public class FinancingInstallmentSimulationModel
    {
        public int Number { get; set; }
        public decimal TotalAmount { get; set; }
        public decimal InterestAmount { get; set; }
        public decimal AmortizationAmount { get; set; }
        public decimal RemainingDebt { get; set; }
        public DateTime DueDate { get; set; }

        public string GetFormattedTotalAmount()
        {
            return string.Format("{0:C2}", TotalAmount);
        }

        public string GetFormattedInterestAmount()
        {
            return string.Format("{0:C2}", InterestAmount);
        }

        public string GetFormattedAmortizationAmount()
        {
            return string.Format("{0:C2}", AmortizationAmount);
        }

        public string GetFormattedRemainingDebt()
        {
            return string.Format("{0:C2}", RemainingDebt);
        }

        public string GetFormattedDueDate()
        {
            return DueDate.ToString("dd/MM/yyyy");
        }
    }

    /// <summary>
    /// Model para requisição de previsão
    /// </summary>
    public class FinancingForecastRequestModel
    {
        [Required]
        public string FinancingId { get; set; }

        [Required]
        [Range(1, 120, ErrorMessage = "O número de meses deve estar entre 1 e 120")]
        [Display(Name = "Meses de Previsão")]
        public int ForecastMonths { get; set; } = 12;

        [Range(0, 100, ErrorMessage = "O índice otimista deve estar entre 0 e 100")]
        [Display(Name = "Índice Otimista (%)")]
        public decimal OptimisticScenarioIndex { get; set; }

        [Range(0, 100, ErrorMessage = "O índice realista deve estar entre 0 e 100")]
        [Display(Name = "Índice Realista (%)")]
        public decimal RealisticScenarioIndex { get; set; }

        [Range(0, 100, ErrorMessage = "O índice pessimista deve estar entre 0 e 100")]
        [Display(Name = "Índice Pessimista (%)")]
        public decimal PessimisticScenarioIndex { get; set; }
    }

    /// <summary>
    /// Model de resultado da previsão
    /// </summary>
    public class FinancingForecastModel
    {
        public string FinancingId { get; set; }
        public string FinancingDescription { get; set; }
        public decimal CurrentDebt { get; set; }
        public decimal BaseInterestRate { get; set; }
        public int CorrectionIndex { get; set; }
        public string CorrectionIndexDescription { get; set; }
        public List<FinancingForecastScenarioModel> Scenarios { get; set; } = new List<FinancingForecastScenarioModel>();

        public string GetFormattedCurrentDebt()
        {
            return string.Format("{0:C2}", CurrentDebt);
        }

        public string GetFormattedBaseInterestRate()
        {
            return string.Format("{0:F2}%", BaseInterestRate);
        }
    }

    /// <summary>
    /// Model de cenário de previsão
    /// </summary>
    public class FinancingForecastScenarioModel
    {
        public string Name { get; set; }
        public decimal ProjectedIndexValue { get; set; }
        public decimal ProjectedFinalDebt { get; set; }
        public decimal ProjectedTotalCost { get; set; }
        public decimal DifferenceFromCurrent { get; set; }
        public decimal DifferencePercentage { get; set; }
        public List<FinancingForecastInstallmentModel> Installments { get; set; } = new List<FinancingForecastInstallmentModel>();

        public string GetFormattedProjectedIndexValue()
        {
            return string.Format("{0:F2}%", ProjectedIndexValue);
        }

        public string GetFormattedProjectedFinalDebt()
        {
            return string.Format("{0:C2}", ProjectedFinalDebt);
        }

        public string GetFormattedProjectedTotalCost()
        {
            return string.Format("{0:C2}", ProjectedTotalCost);
        }

        public string GetFormattedDifferenceFromCurrent()
        {
            return string.Format("{0:C2}", DifferenceFromCurrent);
        }

        public string GetFormattedDifferencePercentage()
        {
            return string.Format("{0:P2}", DifferencePercentage);
        }
    }

    /// <summary>
    /// Model de parcela na previsão
    /// </summary>
    public class FinancingForecastInstallmentModel
    {
        public int Number { get; set; }
        public DateTime DueDate { get; set; }
        public decimal OriginalAmount { get; set; }
        public decimal ProjectedAmount { get; set; }
        public decimal Difference { get; set; }
        public decimal DifferencePercentage { get; set; }

        public string GetFormattedOriginalAmount()
        {
            return string.Format("{0:C2}", OriginalAmount);
        }

        public string GetFormattedProjectedAmount()
        {
            return string.Format("{0:C2}", ProjectedAmount);
        }

        public string GetFormattedDifference()
        {
            return string.Format("{0:C2}", Difference);
        }

        public string GetFormattedDifferencePercentage()
        {
            return string.Format("{0:P2}", DifferencePercentage);
        }

        public string GetFormattedDueDate()
        {
            return DueDate.ToString("dd/MM/yyyy");
        }
    }

    /// <summary>
    /// Model para pagamento de financiamento
    /// </summary>
    public class FinancingPaymentModel
    {
        [Required]
        public string FinancingId { get; set; }

        [Required]
        public string InstallmentId { get; set; }

        [Required]
        [Range(0.01, double.MaxValue, ErrorMessage = "O valor deve ser maior que zero")]
        [Display(Name = "Valor")]
        public decimal Amount { get; set; }

        [Required]
        [Display(Name = "Data de Pagamento")]
        [DataType(DataType.Date)]
        public DateTime PaymentDate { get; set; } = DateTime.Today;

        [Required]
        [Display(Name = "Método de Pagamento")]
        public string PaymentMethodId { get; set; }

        [Display(Name = "Observações")]
        [StringLength(200, ErrorMessage = "As observações devem ter no máximo 200 caracteres")]
        public string Notes { get; set; }
    }

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