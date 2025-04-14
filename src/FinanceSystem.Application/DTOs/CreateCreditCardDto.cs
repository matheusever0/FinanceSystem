using System.ComponentModel.DataAnnotations;

namespace FinanceSystem.Application.DTOs
{
    public class CreateCreditCardDto
    {
        [Required]
        [StringLength(50, MinimumLength = 3)]
        public string Name { get; set; }

        [Required]
        [StringLength(4, MinimumLength = 4)]
        [RegularExpression("^[0-9]*$", ErrorMessage = "Apenas números são permitidos")]
        public string LastFourDigits { get; set; }

        [Required]
        [StringLength(30, MinimumLength = 2)]
        public string CardBrand { get; set; }

        [Required]
        [Range(1, 31, ErrorMessage = "O dia de fechamento deve estar entre 1 e 31")]
        public int ClosingDay { get; set; }

        [Required]
        [Range(1, 31, ErrorMessage = "O dia de vencimento deve estar entre 1 e 31")]
        public int DueDay { get; set; }

        [Required]
        [Range(0.01, double.MaxValue, ErrorMessage = "O limite deve ser maior que zero")]
        public decimal Limit { get; set; }

        [Required]
        public Guid PaymentMethodId { get; set; }
    }
}
