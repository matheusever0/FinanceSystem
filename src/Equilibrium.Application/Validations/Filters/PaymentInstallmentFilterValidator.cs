using FluentValidation;
using Equilibrium.Domain.DTOs.Filters;

namespace Equilibrium.Application.Validations.Filters
{
    public class PaymentInstallmentFilterValidator : AbstractValidator<PaymentInstallmentFilter>
    {
        public PaymentInstallmentFilterValidator()
        {
            RuleFor(x => x.PageNumber).GreaterThan(0);
            RuleFor(x => x.PageSize).InclusiveBetween(1, 100);
            
            RuleFor(x => x.MinAmount)
                .LessThanOrEqualTo(x => x.MaxAmount)
                .When(x => x.MinAmount.HasValue && x.MaxAmount.HasValue);
                
            RuleFor(x => x.DueDateFrom)
                .LessThanOrEqualTo(x => x.DueDateTo)
                .When(x => x.DueDateFrom.HasValue && x.DueDateTo.HasValue);
                
            RuleFor(x => x.PaymentDateFrom)
                .LessThanOrEqualTo(x => x.PaymentDateTo)
                .When(x => x.PaymentDateFrom.HasValue && x.PaymentDateTo.HasValue);
        }
    }
}
