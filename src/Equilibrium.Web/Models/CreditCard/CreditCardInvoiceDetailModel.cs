using Equilibrium.Web.Models.Payment;
using System.ComponentModel.DataAnnotations;

namespace Equilibrium.Web.Models.CreditCard;

public class CreditCardInvoiceDetailModel : CreditCardInvoiceModel
{
    [Display(Name = "Transações")]
    public List<PaymentModel> Transactions { get; set; } = new();

    [Display(Name = "Pagamentos da Fatura")]
    public List<InvoicePaymentModel> Payments { get; set; } = new();

    [Display(Name = "Juros")]
    [DisplayFormat(DataFormatString = "{0:C2}")]
    public decimal InterestCharges { get; set; }

    [Display(Name = "Taxas")]
    [DisplayFormat(DataFormatString = "{0:C2}")]
    public decimal Fees { get; set; }

    [Display(Name = "Créditos")]
    [DisplayFormat(DataFormatString = "{0:C2}")]
    public decimal Credits { get; set; }

    [Display(Name = "Saldo Anterior")]
    [DisplayFormat(DataFormatString = "{0:dd/MM/yyyy}")]
    public DateTime PreviousBalance { get; set; }

    [Display(Name = "Valor do Saldo Anterior")]
    [DisplayFormat(DataFormatString = "{0:C2}")]
    public decimal PreviousBalanceAmount { get; set; }

    public string GetFormattedInterestCharges() => string.Format("{0:C2}", InterestCharges);
    public string GetFormattedFees() => string.Format("{0:C2}", Fees);
    public string GetFormattedCredits() => string.Format("{0:C2}", Credits);
    public string GetFormattedPreviousBalanceAmount() => string.Format("{0:C2}", PreviousBalanceAmount);
}