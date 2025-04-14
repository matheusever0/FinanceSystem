using System.ComponentModel.DataAnnotations;

namespace FinanceSystem.Web.Models
{
    public class UpdateCreditCardModel
    {
        [StringLength(50, MinimumLength = 3, ErrorMessage = "O nome deve ter entre 3 e 50 caracteres")]
        [Display(Name = "Nome")]
        public string Name { get; set; }

        [Range(1, 31, ErrorMessage = "O dia de fechamento deve estar entre 1 e 31")]
        [Display(Name = "Dia de Fechamento")]
        public int? ClosingDay { get; set; }

        [Range(1, 31, ErrorMessage = "O dia de vencimento deve estar entre 1 e 31")]
        [Display(Name = "Dia de Vencimento")]
        public int? DueDay { get; set; }

        [Range(0.01, double.MaxValue, ErrorMessage = "O limite deve ser maior que zero")]
        [Display(Name = "Limite")]
        public decimal? Limit { get; set; }
    }
}