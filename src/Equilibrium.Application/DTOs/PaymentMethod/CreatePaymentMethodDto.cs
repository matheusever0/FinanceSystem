using Equilibrium.Domain.Enums;
using System.ComponentModel.DataAnnotations;

namespace Equilibrium.Application.DTOs.PaymentMethod
{
    public class CreatePaymentMethodDto
    {
        [Required]
        [StringLength(50, MinimumLength = 3)]
        public required string Name { get; set; }

        [StringLength(200)]
        public required string Description { get; set; }

        [Required]
        public PaymentMethodType Type { get; set; }
    }
}
