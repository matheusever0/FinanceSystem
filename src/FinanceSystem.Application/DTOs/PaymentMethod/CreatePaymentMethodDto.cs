using FinanceSystem.Domain.Enums;
using System.ComponentModel.DataAnnotations;

namespace FinanceSystem.Application.DTOs.PaymentMethod
{
    public class CreatePaymentMethodDto
    {
        [Required]
        [StringLength(50, MinimumLength = 3)]
        public string Name { get; set; }

        [StringLength(200)]
        public string Description { get; set; }

        [Required]
        public PaymentMethodType Type { get; set; }
    }
}
