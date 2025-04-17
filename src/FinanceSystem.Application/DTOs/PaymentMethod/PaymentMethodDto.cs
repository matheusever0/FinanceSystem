using FinanceSystem.Application.DTOs.CreditCard;
using FinanceSystem.Domain.Enums;

namespace FinanceSystem.Application.DTOs.PaymentMethod
{
    public class PaymentMethodDto
    {
        public Guid Id { get; set; }
        public string Name { get; set; }
        public string Description { get; set; }
        public bool IsSystem { get; set; }
        public PaymentMethodType Type { get; set; }
        public string TypeDescription => Type.ToString();
        public Guid? UserId { get; set; }
        public DateTime CreatedAt { get; set; }
        public List<CreditCardDto> CreditCards { get; set; } = new List<CreditCardDto>();
    }
}
