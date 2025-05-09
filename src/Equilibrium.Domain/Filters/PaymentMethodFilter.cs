using Equilibrium.Domain.Enums;

namespace Equilibrium.Domain.DTOs.Filters
{
    public class PaymentMethodFilter : TypeFilterBase
    {
        public PaymentMethodType? Type { get; set; }
    }
}
