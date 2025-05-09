using System.Linq.Expressions;
using Equilibrium.Domain.DTOs.Filters;

namespace Equilibrium.Domain.Specifications
{
    public class PaymentMethodSpecification : BaseSpecification<PaymentMethod>
    {
        public PaymentMethodSpecification(PaymentMethodFilter filter)
        {
            AddInclude(pm => pm.CreditCards);
            
            if (!string.IsNullOrEmpty(filter.Name))
                ApplyCriteria(pm => pm.Name.Contains(filter.Name));
                
            if (!string.IsNullOrEmpty(filter.Description))
                ApplyCriteria(pm => pm.Description.Contains(filter.Description));
                
            if (filter.IsSystem.HasValue)
                ApplyCriteria(pm => pm.IsSystem == filter.IsSystem.Value);
                
            if (filter.Type.HasValue)
                ApplyCriteria(pm => pm.Type == filter.Type.Value);
                
            int skip = (filter.PageNumber - 1) * filter.PageSize;
            ApplyPaging(skip, filter.PageSize);
            
            if (filter.Ascending)
                ApplyOrderBy(GetSortProperty(filter.OrderBy));
            else
                ApplyOrderByDescending(GetSortProperty(filter.OrderBy));
        }
        
        private Expression<Func<PaymentMethod, object>> GetSortProperty(string propertyName)
        {
            return propertyName?.ToLower() switch
            {
                "name" => pm => pm.Name,
                "description" => pm => pm.Description,
                "issystem" => pm => pm.IsSystem,
                "type" => pm => pm.Type,
                _ => pm => pm.CreatedAt
            };
        }
    }
}
