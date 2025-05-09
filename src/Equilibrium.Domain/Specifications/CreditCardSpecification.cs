using Equilibrium.Domain.DTOs.Filters;
using Equilibrium.Domain.Entities;
using System.Linq.Expressions;

namespace Equilibrium.Domain.Specifications
{
    public class CreditCardSpecification : BaseSpecification<CreditCard>
    {
        public CreditCardSpecification(CreditCardFilter filter)
        {
            AddInclude(c => c.PaymentMethod);
            
            if (!string.IsNullOrEmpty(filter.Name))
                ApplyCriteria(c => c.Name.Contains(filter.Name));
                
            if (!string.IsNullOrEmpty(filter.CardBrand))
                ApplyCriteria(c => c.CardBrand.Contains(filter.CardBrand));
                
            if (!string.IsNullOrEmpty(filter.LastFourDigits))
                ApplyCriteria(c => c.LastFourDigits == filter.LastFourDigits);
                
            if (filter.MinClosingDay.HasValue)
                ApplyCriteria(c => c.ClosingDay >= filter.MinClosingDay.Value);
                
            if (filter.MaxClosingDay.HasValue)
                ApplyCriteria(c => c.ClosingDay <= filter.MaxClosingDay.Value);
                
            if (filter.MinDueDay.HasValue)
                ApplyCriteria(c => c.DueDay >= filter.MinDueDay.Value);
                
            if (filter.MaxDueDay.HasValue)
                ApplyCriteria(c => c.DueDay <= filter.MaxDueDay.Value);
                
            if (filter.MinLimit.HasValue)
                ApplyCriteria(c => c.Limit >= filter.MinLimit.Value);
                
            if (filter.MaxLimit.HasValue)
                ApplyCriteria(c => c.Limit <= filter.MaxLimit.Value);
                
            if (filter.MinAvailableLimit.HasValue)
                ApplyCriteria(c => c.AvailableLimit >= filter.MinAvailableLimit.Value);
                
            if (filter.MaxAvailableLimit.HasValue)
                ApplyCriteria(c => c.AvailableLimit <= filter.MaxAvailableLimit.Value);
                
            if (filter.PaymentMethodId.HasValue)
                ApplyCriteria(c => c.PaymentMethodId == filter.PaymentMethodId.Value);
                
            int skip = (filter.PageNumber - 1) * filter.PageSize;
            ApplyPaging(skip, filter.PageSize);
            
            if (filter.Ascending)
                ApplyOrderBy(GetSortProperty(filter.OrderBy));
            else
                ApplyOrderByDescending(GetSortProperty(filter.OrderBy));
        }
        
        private Expression<Func<CreditCard, object>> GetSortProperty(string propertyName)
        {
            return propertyName?.ToLower() switch
            {
                "name" => c => c.Name,
                "cardbrand" => c => c.CardBrand,
                "closingday" => c => c.ClosingDay,
                "dueday" => c => c.DueDay,
                "limit" => c => c.Limit,
                "availablelimit" => c => c.AvailableLimit,
                "paymentmethod" => c => c.PaymentMethod.Name,
                _ => c => c.CreatedAt
            };
        }
    }
}
