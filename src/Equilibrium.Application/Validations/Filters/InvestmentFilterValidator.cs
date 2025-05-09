using FluentValidation;
using Equilibrium.Domain.DTOs.Filters;

namespace Equilibrium.Application.Validations.Filters
{
    public class InvestmentFilterValidator : AbstractValidator<InvestmentFilter>
    {
        public InvestmentFilterValidator()
        {
            RuleFor(x => x.PageNumber).GreaterThan(0);
            RuleFor(x => x.PageSize).InclusiveBetween(1, 100);
            
            RuleFor(x => x.MinQuantity)
                .LessThanOrEqualTo(x => x.MaxQuantity)
                .When(x => x.MinQuantity.HasValue && x.MaxQuantity.HasValue);
                
            RuleFor(x => x.MinPrice)
                .LessThanOrEqualTo(x => x.MaxPrice)
                .When(x => x.MinPrice.HasValue && x.MaxPrice.HasValue);
                
            RuleFor(x => x.MinGainLossPercentage)
                .LessThanOrEqualTo(x => x.MaxGainLossPercentage)
                .When(x => x.MinGainLossPercentage.HasValue && x.MaxGainLossPercentage.HasValue);
        }
    }
}
