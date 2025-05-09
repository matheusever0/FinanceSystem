using FluentValidation;
using Equilibrium.Domain.DTOs.Filters;

namespace Equilibrium.Application.Validations.Filters
{
    public class CreditCardFilterValidator : AbstractValidator<CreditCardFilter>
    {
        public CreditCardFilterValidator()
        {
            RuleFor(x => x.PageNumber).GreaterThan(0);
            RuleFor(x => x.PageSize).InclusiveBetween(1, 100);
            
            RuleFor(x => x.MinClosingDay)
                .LessThanOrEqualTo(x => x.MaxClosingDay)
                .When(x => x.MinClosingDay.HasValue && x.MaxClosingDay.HasValue);
                
            RuleFor(x => x.MinDueDay)
                .LessThanOrEqualTo(x => x.MaxDueDay)
                .When(x => x.MinDueDay.HasValue && x.MaxDueDay.HasValue);
                
            RuleFor(x => x.MinLimit)
                .LessThanOrEqualTo(x => x.MaxLimit)
                .When(x => x.MinLimit.HasValue && x.MaxLimit.HasValue);
                
            RuleFor(x => x.MinAvailableLimit)
                .LessThanOrEqualTo(x => x.MaxAvailableLimit)
                .When(x => x.MinAvailableLimit.HasValue && x.MaxAvailableLimit.HasValue);
        }
    }
}
