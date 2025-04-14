using System.ComponentModel.DataAnnotations;

namespace FinanceSystem.Web.Models
{
    public class PaymentMethodModel
    {
        public string Id { get; set; }

        [Display(Name = "Nome")]
        public string Name { get; set; }

        [Display(Name = "Descrição")]
        public string Description { get; set; }

        [Display(Name = "Método do Sistema")]
        public bool IsSystem { get; set; }

        [Display(Name = "Tipo")]
        public int Type { get; set; }

        [Display(Name = "Tipo")]
        public string TypeDescription { get; set; }

        public string UserId { get; set; }

        [Display(Name = "Data de Criação")]
        [DisplayFormat(DataFormatString = "{0:dd/MM/yyyy HH:mm}")]
        public DateTime CreatedAt { get; set; }

        public List<CreditCardModel> CreditCards { get; set; } = new List<CreditCardModel>();
    }
}