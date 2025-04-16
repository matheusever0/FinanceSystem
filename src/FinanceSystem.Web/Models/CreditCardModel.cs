using System.ComponentModel.DataAnnotations;

namespace FinanceSystem.Web.Models
{
    public class CreditCardModel
    {
        public string Id { get; set; }

        [Display(Name = "Nome")]
        public string Name { get; set; }

        [Display(Name = "Últimos 4 Dígitos")]
        public string LastFourDigits { get; set; }

        [Display(Name = "Bandeira")]
        public string CardBrand { get; set; }

        [Display(Name = "Dia de Fechamento")]
        public int ClosingDay { get; set; }

        [Display(Name = "Dia de Vencimento")]
        public int DueDay { get; set; }

        [Display(Name = "Limite")]
        [DisplayFormat(DataFormatString = "{0:C2}")]
        public decimal Limit { get; set; }

        [Display(Name = "Limite Disponível")]
        [DisplayFormat(DataFormatString = "{0:C2}")]
        public decimal AvailableLimit { get; set; }

        [Display(Name = "Data de Criação")]
        [DisplayFormat(DataFormatString = "{0:dd/MM/yyyy HH:mm}")]
        public DateTime CreatedAt { get; set; }

        [Display(Name = "Última Atualização")]
        [DisplayFormat(DataFormatString = "{0:dd/MM/yyyy HH:mm}")]
        public DateTime? UpdatedAt { get; set; }

        public string UserId { get; set; }

        [Display(Name = "Método de Pagamento")]
        public string PaymentMethodId { get; set; }

        [Display(Name = "Método de Pagamento")]
        public string PaymentMethodName { get; set; }

        [Display(Name = "Limite Utilizado")]
        [DisplayFormat(DataFormatString = "{0:C2}")]
        public decimal UsedLimit => Limit - AvailableLimit;

        [Display(Name = "Percentual Utilizado")]
        [DisplayFormat(DataFormatString = "{0:P0}")]
        public decimal UsedPercentage => (UsedLimit / Limit) * 100;

        public string GetFormattedLimit()
        {
            return string.Format("{0:C2}", Limit);
        }

        public string GetFormattedAvailableLimit()
        {
            return string.Format("{0:C2}", AvailableLimit);
        }

        public string GetFormattedUsedLimit()
        {
            return string.Format("{0:C2}", UsedLimit);
        }

        public string GetFormattedUsedPercentage()
        {
            return string.Format("{0:P0}", UsedPercentage / 100);
        }
    }
}