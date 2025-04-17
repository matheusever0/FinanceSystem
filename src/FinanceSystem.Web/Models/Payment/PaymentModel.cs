using FinanceSystem.Web.Models.PaymentInstallment;
using System.ComponentModel.DataAnnotations;

namespace FinanceSystem.Web.Models.Payment
{
    public class PaymentModel
    {
        public string Id { get; set; }

        [Display(Name = "Descrição")]
        public string Description { get; set; }

        [Display(Name = "Valor")]
        [DisplayFormat(DataFormatString = "{0:C2}")]
        public decimal Amount { get; set; }

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

        [Display(Name = "Recorrente")]
        public bool IsRecurring { get; set; }

        [Display(Name = "Observações")]
        public string Notes { get; set; }

        [Display(Name = "Data de Criação")]
        [DisplayFormat(DataFormatString = "{0:dd/MM/yyyy HH:mm}")]
        public DateTime CreatedAt { get; set; }

        [Display(Name = "Última Atualização")]
        [DisplayFormat(DataFormatString = "{0:dd/MM/yyyy HH:mm}")]
        public DateTime? UpdatedAt { get; set; }

        public string UserId { get; set; }

        [Display(Name = "Tipo de Pagamento")]
        public string PaymentTypeId { get; set; }

        [Display(Name = "Tipo de Pagamento")]
        public string PaymentTypeName { get; set; }

        [Display(Name = "Método de Pagamento")]
        public string PaymentMethodId { get; set; }

        [Display(Name = "Método de Pagamento")]
        public string PaymentMethodName { get; set; }

        [Display(Name = "Parcelas")]
        public List<PaymentInstallmentModel> Installments { get; set; } = new List<PaymentInstallmentModel>();

        [Display(Name = "Status")]
        public string StatusBadgeClass => GetStatusBadgeClass();

        private string GetStatusBadgeClass()
        {
            return Status switch
            {
                1 => "bg-warning",
                2 => "bg-success",
                3 => "bg-danger",
                4 => "bg-secondary",
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

        public string GetFormattedPaymentDate()
        {
            return PaymentDate?.ToString("dd/MM/yyyy") ?? "-";
        }
    }
}