using System.ComponentModel.DataAnnotations;

namespace Equilibrium.Web.Models.CreditCard;

public class InvoicePaymentModel
{
    public required string Id { get; set; }

    [Display(Name = "Valor")]
    [DisplayFormat(DataFormatString = "{0:C2}")]
    public decimal Amount { get; set; }

    [Display(Name = "Data de Pagamento")]
    [DisplayFormat(DataFormatString = "{0:dd/MM/yyyy}")]
    public DateTime PaymentDate { get; set; }

    [Display(Name = "Observações")]
    public required string Notes { get; set; }

    [Display(Name = "Data de Criação")]
    [DisplayFormat(DataFormatString = "{0:dd/MM/yyyy HH:mm}")]
    public DateTime CreatedAt { get; set; }

    public string GetFormattedAmount() => string.Format("{0:C2}", Amount);
    public string GetFormattedPaymentDate() => PaymentDate.ToString("dd/MM/yyyy");
    public string GetFormattedCreatedAt() => CreatedAt.ToString("dd/MM/yyyy HH:mm");
}