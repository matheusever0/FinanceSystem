using FluentValidation;
using Equilibrium.Domain.DTOs.Filters;

namespace Equilibrium.Application.Validations.Filters
{
    public class TypeFilterBaseValidator<T> : AbstractValidator<T> where T : TypeFilterBase
    {
        public TypeFilterBaseValidator()
        {
            RuleFor(x => x.PageNumber).GreaterThan(0);
            RuleFor(x => x.PageSize).InclusiveBetween(1, 100);
        }
    }
    
    public class PaymentTypeFilterValidator : TypeFilterBaseValidator<PaymentTypeFilter> { }
    
    public class IncomeTypeFilterValidator : TypeFilterBaseValidator<IncomeTypeFilter> { }
    
    public class PaymentMethodFilterValidator : TypeFilterBaseValidator<PaymentMethodFilter> { }
}
