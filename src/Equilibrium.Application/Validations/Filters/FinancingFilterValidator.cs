using FluentValidation;
using Equilibrium.Domain.DTOs.Filters;

namespace Equilibrium.Application.Validations.Filters
{
    public class FinancingFilterValidator : AbstractValidator<FinancingFilter>
    {
        public FinancingFilterValidator()
        {
            RuleFor(x => x.PageNumber).GreaterThan(0);
            RuleFor(x => x.PageSize).InclusiveBetween(1, 100);
            
            RuleFor(x => x.MinTotalAmount)
                .LessThanOrEqualTo(x => x.MaxTotalAmount)
                .When(x => x.MinTotalAmount.HasValue && x.MaxTotalAmount.HasValue);
                
            RuleFor(x => x.MinInterestRate)
                .LessThanOrEqualTo(x => x.MaxInterestRate)
                .When(x => x.MinInterestRate.HasValue && x.MaxInterestRate.HasValue);
                
            RuleFor(x => x.MinTermMonths)
                .LessThanOrEqualTo(x => x.MaxTermMonths)
                .When(x => x.MinTermMonths.HasValue && x.MaxTermMonths.HasValue);
                
            RuleFor(x => x.StartDateFrom)
                .LessThanOrEqualTo(x => x.StartDateTo)
                .When(x => x.StartDateFrom.HasValue && x.StartDateTo.HasValue);
                
            RuleFor(x => x.MinRemainingDebt)
                .LessThanOrEqualTo(x => x.MaxRemainingDebt)
                .When(x => x.MinRemainingDebt.HasValue && x.MaxRemainingDebt.HasValue);
        }
    }
}
