using Equilibrium.Application.DTOs.CreditCard;
using Equilibrium.Domain.Enums;

namespace Equilibrium.Application.DTOs.PaymentMethod
{
    public class PaymentMethodDto
    {
        public Guid Id { get; set; }
        public required string Name { get; set; }
        public required string Description { get; set; }
        public bool IsSystem { get; set; }
        public PaymentMethodType Type { get; set; }
        public string TypeDescription => Type.ToString();
        public Guid? UserId { get; set; }
        public DateTime CreatedAt { get; set; }
        public List<CreditCardDto> CreditCards { get; set; } = [];
    }
}
