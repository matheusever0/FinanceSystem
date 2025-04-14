using System.ComponentModel.DataAnnotations;

namespace FinanceSystem.Web.Models
{
    public class CreateCreditCardModel
    {
        [Required(ErrorMessage = "O nome é obrigatório")]
        [StringLength(50, MinimumLength = 3, ErrorMessage = "O nome deve ter entre 3 e 50 caracteres")]
        [Display(Name = "Nome")]
        public string Name { get; set; }

        [Required(ErrorMessage = "Os últimos 4 dígitos são obrigatórios")]
        [StringLength(4, MinimumLength = 4, ErrorMessage = "Os últimos 4 dígitos devem ter exatamente 4 caracteres")]
        [RegularExpression("^[0-9]*$", ErrorMessage = "Apenas números são permitidos")]
        [Display(Name = "Últimos 4 Dígitos")]
        public string LastFourDigits { get; set; }

        [Required(ErrorMessage = "A bandeira é obrigatória")]
        [StringLength(30, MinimumLength = 2, ErrorMessage = "A bandeira deve ter entre 2 e 30 caracteres")]
        [Display(Name = "Bandeira")]
        public string CardBrand { get; set; }

        [Required(ErrorMessage = "O dia de fechamento é obrigatório")]
        [Range(1, 31, ErrorMessage = "O dia de fechamento deve estar entre 1 e 31")]
        [Display(Name = "Dia de Fechamento")]
        public int ClosingDay { get; set; }

        [Required(ErrorMessage = "O dia de vencimento é obrigatório")]
        [Range(1, 31, ErrorMessage = "O dia de vencimento deve estar entre 1 e 31")]
        [Display(Name = "Dia de Vencimento")]
        public int DueDay { get; set; }

        [Required(ErrorMessage = "O limite é obrigatório")]
        [Range(0.01, double.MaxValue, ErrorMessage = "O limite deve ser maior que zero")]
        [Display(Name = "Limite")]
        public decimal Limit { get; set; }

        [Required(ErrorMessage = "O método de pagamento é obrigatório")]
        [Display(Name = "Método de Pagamento")]
        public string PaymentMethodId { get; set; }
    }
}