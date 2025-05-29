using System.ComponentModel.DataAnnotations;

namespace Equilibrium.Web.Models.Filters
{
    // Filtro base para paginação e ordenação
    public abstract class BaseFilter
    {
        [Display(Name = "Ordenar por")]
        public string? OrderBy { get; set; }

        [Display(Name = "Ordem crescente")]
        public bool Ascending { get; set; } = true;

        [Display(Name = "Página")]
        public int Page { get; set; } = 1;

        [Display(Name = "Itens por página")]
        public int PageSize { get; set; } = 10;
    }

    // Filtro avançado para pagamentos
    public class PaymentFilter : BaseFilter
    {
        [Display(Name = "Descrição")]
        public string? Description { get; set; }

        [Display(Name = "Observações")]
        public string? Notes { get; set; }

        [Display(Name = "Valor mínimo")]
        [DataType(DataType.Currency)]
        public decimal? MinAmount { get; set; }

        [Display(Name = "Valor máximo")]
        [DataType(DataType.Currency)]
        public decimal? MaxAmount { get; set; }

        [Display(Name = "Data inicial")]
        [DataType(DataType.Date)]
        public DateTime? StartDate { get; set; }

        [Display(Name = "Data final")]
        [DataType(DataType.Date)]
        public DateTime? EndDate { get; set; }

        [Display(Name = "Mês")]
        [Range(1, 12, ErrorMessage = "Mês deve estar entre 1 e 12")]
        public int? Month { get; set; }

        [Display(Name = "Ano")]
        [Range(2020, 2050, ErrorMessage = "Ano deve estar entre 2020 e 2050")]
        public int? Year { get; set; }

        [Display(Name = "Data de pagamento inicial")]
        [DataType(DataType.Date)]
        public DateTime? PaymentStartDate { get; set; }

        [Display(Name = "Data de pagamento final")]
        [DataType(DataType.Date)]
        public DateTime? PaymentEndDate { get; set; }

        [Display(Name = "Status")]
        public string? Status { get; set; }

        [Display(Name = "Tipo de pagamento")]
        public string? PaymentTypeId { get; set; }

        [Display(Name = "Método de pagamento")]
        public string? PaymentMethodId { get; set; }

        [Display(Name = "Financiamento")]
        public string? FinancingId { get; set; }

        [Display(Name = "Parcela de financiamento")]
        public string? FinancingInstallmentId { get; set; }

        [Display(Name = "Cartão de crédito")]
        public string? CreditCardId { get; set; }

        [Display(Name = "É recorrente")]
        public bool? IsRecurring { get; set; }

        [Display(Name = "Tem parcelas")]
        public bool? HasInstallments { get; set; }

        [Display(Name = "É pagamento de financiamento")]
        public bool? IsFinancingPayment { get; set; }

        // Propriedades computadas para facilitar uso
        public bool HasDateRange => StartDate.HasValue || EndDate.HasValue;
        public bool HasPaymentDateRange => PaymentStartDate.HasValue || PaymentEndDate.HasValue;
        public bool HasAmountRange => MinAmount.HasValue || MaxAmount.HasValue;
        public bool HasMonthYear => Month.HasValue || Year.HasValue;

        // Método para verificar se tem filtros aplicados
        public bool HasFilters()
        {
            return !string.IsNullOrWhiteSpace(Description) ||
                   !string.IsNullOrWhiteSpace(Notes) ||
                   MinAmount.HasValue ||
                   MaxAmount.HasValue ||
                   StartDate.HasValue ||
                   EndDate.HasValue ||
                   Month.HasValue ||
                   Year.HasValue ||
                   PaymentStartDate.HasValue ||
                   PaymentEndDate.HasValue ||
                   !string.IsNullOrWhiteSpace(Status) ||
                   !string.IsNullOrWhiteSpace(PaymentTypeId) ||
                   !string.IsNullOrWhiteSpace(PaymentMethodId) ||
                   !string.IsNullOrWhiteSpace(FinancingId) ||
                   !string.IsNullOrWhiteSpace(FinancingInstallmentId) ||
                   !string.IsNullOrWhiteSpace(CreditCardId) ||
                   IsRecurring.HasValue ||
                   HasInstallments.HasValue ||
                   IsFinancingPayment.HasValue;
        }

        // Método para limpar filtros
        public void Clear()
        {
            Description = null;
            Notes = null;
            MinAmount = null;
            MaxAmount = null;
            StartDate = null;
            EndDate = null;
            Month = null;
            Year = null;
            PaymentStartDate = null;
            PaymentEndDate = null;
            Status = null;
            PaymentTypeId = null;
            PaymentMethodId = null;
            FinancingId = null;
            FinancingInstallmentId = null;
            CreditCardId = null;
            IsRecurring = null;
            HasInstallments = null;
            IsFinancingPayment = null;
        }
    }

    // Filtro avançado para receitas
    public class IncomeFilter : BaseFilter
    {
        [Display(Name = "Descrição")]
        public string? Description { get; set; }

        [Display(Name = "Valor mínimo")]
        [DataType(DataType.Currency)]
        public decimal? MinAmount { get; set; }

        [Display(Name = "Valor máximo")]
        [DataType(DataType.Currency)]
        public decimal? MaxAmount { get; set; }

        [Display(Name = "Data inicial")]
        [DataType(DataType.Date)]
        public DateTime? StartDate { get; set; }

        [Display(Name = "Data final")]
        [DataType(DataType.Date)]
        public DateTime? EndDate { get; set; }

        [Display(Name = "Mês")]
        [Range(1, 12, ErrorMessage = "Mês deve estar entre 1 e 12")]
        public int? Month { get; set; }

        [Display(Name = "Ano")]
        [Range(2020, 2050, ErrorMessage = "Ano deve estar entre 2020 e 2050")]
        public int? Year { get; set; }

        [Display(Name = "Data de recebimento inicial")]
        [DataType(DataType.Date)]
        public DateTime? ReceivedStartDate { get; set; }

        [Display(Name = "Data de recebimento final")]
        [DataType(DataType.Date)]
        public DateTime? ReceivedEndDate { get; set; }

        [Display(Name = "Status")]
        public string? Status { get; set; }

        [Display(Name = "Tipo de receita")]
        public string? IncomeTypeId { get; set; }

        [Display(Name = "É recorrente")]
        public bool? IsRecurring { get; set; }

        [Display(Name = "Tem parcelas")]
        public bool? HasInstallments { get; set; }

        // Propriedades computadas
        public bool HasDateRange => StartDate.HasValue || EndDate.HasValue;
        public bool HasReceivedDateRange => ReceivedStartDate.HasValue || ReceivedEndDate.HasValue;
        public bool HasAmountRange => MinAmount.HasValue || MaxAmount.HasValue;
        public bool HasMonthYear => Month.HasValue || Year.HasValue;

        public bool HasFilters()
        {
            return !string.IsNullOrWhiteSpace(Description) ||
                   MinAmount.HasValue ||
                   MaxAmount.HasValue ||
                   StartDate.HasValue ||
                   EndDate.HasValue ||
                   Month.HasValue ||
                   Year.HasValue ||
                   ReceivedStartDate.HasValue ||
                   ReceivedEndDate.HasValue ||
                   !string.IsNullOrWhiteSpace(Status) ||
                   !string.IsNullOrWhiteSpace(IncomeTypeId) ||
                   IsRecurring.HasValue ||
                   HasInstallments.HasValue;
        }

        public void Clear()
        {
            Description = null;
            MinAmount = null;
            MaxAmount = null;
            StartDate = null;
            EndDate = null;
            Month = null;
            Year = null;
            ReceivedStartDate = null;
            ReceivedEndDate = null;
            Status = null;
            IncomeTypeId = null;
            IsRecurring = null;
            HasInstallments = null;
        }
    }

    // Filtro para financiamentos
    public class FinancingFilter : BaseFilter
    {
        [Display(Name = "Descrição")]
        public string? Description { get; set; }

        [Display(Name = "Valor mínimo")]
        [DataType(DataType.Currency)]
        public decimal? MinAmount { get; set; }

        [Display(Name = "Valor máximo")]
        [DataType(DataType.Currency)]
        public decimal? MaxAmount { get; set; }

        [Display(Name = "Status")]
        public string? Status { get; set; }

        [Display(Name = "Tipo")]
        public int? Type { get; set; }

        [Display(Name = "Data de início")]
        [DataType(DataType.Date)]
        public DateTime? StartDate { get; set; }

        [Display(Name = "Data de término")]
        [DataType(DataType.Date)]
        public DateTime? EndDate { get; set; }

        [Display(Name = "Taxa mínima")]
        public decimal? MinInterestRate { get; set; }

        [Display(Name = "Taxa máxima")]
        public decimal? MaxInterestRate { get; set; }

        public bool HasFilters()
        {
            return !string.IsNullOrWhiteSpace(Description) ||
                   MinAmount.HasValue ||
                   MaxAmount.HasValue ||
                   !string.IsNullOrWhiteSpace(Status) ||
                   Type.HasValue ||
                   StartDate.HasValue ||
                   EndDate.HasValue ||
                   MinInterestRate.HasValue ||
                   MaxInterestRate.HasValue;
        }
    }

    // Enum para opções de ordenação específicas
    public enum PaymentOrderBy
    {
        [Display(Name = "Descrição")]
        Description,
        [Display(Name = "Valor")]
        Amount,
        [Display(Name = "Data de vencimento")]
        DueDate,
        [Display(Name = "Data de pagamento")]
        PaymentDate,
        [Display(Name = "Status")]
        Status,
        [Display(Name = "Data de criação")]
        CreatedAt
    }

    public enum IncomeOrderBy
    {
        [Display(Name = "Descrição")]
        Description,
        [Display(Name = "Valor")]
        Amount,
        [Display(Name = "Data de vencimento")]
        DueDate,
        [Display(Name = "Data de recebimento")]
        ReceivedDate,
        [Display(Name = "Status")]
        Status,
        [Display(Name = "Data de criação")]
        CreatedAt
    }
}