using System.ComponentModel.DataAnnotations;

public class PayInvoiceModel
{
    [Required(ErrorMessage = "O valor é obrigatório")]
    [Range(0.01, double.MaxValue, ErrorMessage = "O valor deve ser maior que zero")]
    [Display(Name = "Valor")]
    public decimal Amount { get; set; }

    [Required(ErrorMessage = "A data de pagamento é obrigatória")]
    [Display(Name = "Data de Pagamento")]
    [DataType(DataType.Date)]
    public DateTime PaymentDate { get; set; } = DateTime.Now;

    [Display(Name = "Observações")]
    [StringLength(500, ErrorMessage = "As observações devem ter no máximo 500 caracteres")]
    public string Notes { get; set; } = "";

    [Display(Name = "Pagar Valor Total")]
    public bool PayFullAmount { get; set; } = true;

    public string GetFormattedAmount() => string.Format("{0:C2}", Amount);
    public string GetFormattedPaymentDate() => PaymentDate.ToString("dd/MM/yyyy");
}