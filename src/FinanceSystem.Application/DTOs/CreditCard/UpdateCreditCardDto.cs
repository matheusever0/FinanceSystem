using System.ComponentModel.DataAnnotations;

namespace FinanceSystem.Application.DTOs.CreditCard
{
    public class UpdateCreditCardDto
    {
        [StringLength(50, MinimumLength = 3)]
        public required string Name { get; set; }

        [Range(1, 31, ErrorMessage = "O dia de fechamento deve estar entre 1 e 31")]
        public int? ClosingDay { get; set; }

        [Range(1, 31, ErrorMessage = "O dia de vencimento deve estar entre 1 e 31")]
        public int? DueDay { get; set; }

        [Range(0.01, double.MaxValue, ErrorMessage = "O limite deve ser maior que zero")]
        public decimal? Limit { get; set; }
    }
}
