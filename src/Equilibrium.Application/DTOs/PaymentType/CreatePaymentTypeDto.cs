using System.ComponentModel.DataAnnotations;

namespace Equilibrium.Application.DTOs.PaymentType
{
    public class CreatePaymentTypeDto
    {
        [Required]
        [StringLength(50, MinimumLength = 3)]
        public required string Name { get; set; }

        [StringLength(200)]
        public required string Description { get; set; }
    }
}
