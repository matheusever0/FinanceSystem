using Equilibrium.Web.Models.CreditCard;
using System.ComponentModel.DataAnnotations;

namespace Equilibrium.Web.Models.PaymentMethod
{
    public class PaymentMethodModel
    {
        public required string Id { get; set; }

        [Display(Name = "Nome")]
        public required string Name { get; set; }

        [Display(Name = "Descrição")]
        public required string Description { get; set; }

        [Display(Name = "Método do Sistema")]
        public bool IsSystem { get; set; }

        [Display(Name = "Tipo")]
        public int Type { get; set; }

        [Display(Name = "Tipo")]
        public required string TypeDescription { get; set; }

        public required string UserId { get; set; }

        [Display(Name = "Data de Criação")]
        [DisplayFormat(DataFormatString = "{0:dd/MM/yyyy HH:mm}")]
        public DateTime CreatedAt { get; set; }

        public List<CreditCardModel> CreditCards { get; set; } = [];
    }
}