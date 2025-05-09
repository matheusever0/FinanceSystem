using FluentValidation;
using Equilibrium.Domain.DTOs.Filters;

namespace Equilibrium.Application.Validations.Filters
{
    public class PaymentFilterValidator : AbstractValidator<PaymentFilter>
    {
        public PaymentFilterValidator()
        {
            RuleFor(x => x.PageNumber).GreaterThan(0);
            RuleFor(x => x.PageSize).InclusiveBetween(1, 100);
            
            RuleFor(x => x.MinAmount)
                .LessThanOrEqualTo(x => x.MaxAmount)
                .When(x => x.MinAmount.HasValue && x.MaxAmount.HasValue);
                
            RuleFor(x => x.StartDate)
                .LessThanOrEqualTo(x => x.EndDate)
                .When(x => x.StartDate.HasValue && x.EndDate.HasValue);
        }
    }
}
