using System.ComponentModel.DataAnnotations;

namespace FinanceSystem.Web.Models.PaymentMethod
{
    public class UpdatePaymentMethodModel
    {
        [StringLength(50, MinimumLength = 3, ErrorMessage = "O nome deve ter entre 3 e 50 caracteres")]
        [Display(Name = "Nome")]
        public required string Name { get; set; }

        [StringLength(200, ErrorMessage = "A descrição deve ter no máximo 200 caracteres")]
        [Display(Name = "Descrição")]
        public required string Description { get; set; }
    }
}