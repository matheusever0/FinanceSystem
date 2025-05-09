using FluentValidation;
using Equilibrium.Domain.DTOs.Filters;

namespace Equilibrium.Application.Validations.Filters
{
    public class IncomeInstallmentFilterValidator : AbstractValidator<IncomeInstallmentFilter>
    {
        public IncomeInstallmentFilterValidator()
        {
            RuleFor(x => x.PageNumber).GreaterThan(0);
            RuleFor(x => x.PageSize).InclusiveBetween(1, 100);
            
            RuleFor(x => x.MinAmount)
                .LessThanOrEqualTo(x => x.MaxAmount)
                .When(x => x.MinAmount.HasValue && x.MaxAmount.HasValue);
                
            RuleFor(x => x.DueDateFrom)
                .LessThanOrEqualTo(x => x.DueDateTo)
                .When(x => x.DueDateFrom.HasValue && x.DueDateTo.HasValue);
                
            RuleFor(x => x.ReceivedDateFrom)
                .LessThanOrEqualTo(x => x.ReceivedDateTo)
                .When(x => x.ReceivedDateFrom.HasValue && x.ReceivedDateTo.HasValue);
        }
    }
}
