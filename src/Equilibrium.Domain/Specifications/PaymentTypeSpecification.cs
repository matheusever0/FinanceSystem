using System.Linq.Expressions;
using Equilibrium.Domain.DTOs.Filters;

namespace Equilibrium.Domain.Specifications
{
    public class PaymentTypeSpecification : BaseSpecification<PaymentType>
    {
        public PaymentTypeSpecification(PaymentTypeFilter filter)
        {
            if (!string.IsNullOrEmpty(filter.Name))
                ApplyCriteria(pt => pt.Name.Contains(filter.Name));
                
            if (!string.IsNullOrEmpty(filter.Description))
                ApplyCriteria(pt => pt.Description.Contains(filter.Description));
                
            if (filter.IsSystem.HasValue)
                ApplyCriteria(pt => pt.IsSystem == filter.IsSystem.Value);
                
            if (filter.IsFinancingType.HasValue)
                ApplyCriteria(pt => pt.IsFinancingType == filter.IsFinancingType.Value);
                
            int skip = (filter.PageNumber - 1) * filter.PageSize;
            ApplyPaging(skip, filter.PageSize);
            
            if (filter.Ascending)
                ApplyOrderBy(GetSortProperty(filter.OrderBy));
            else
                ApplyOrderByDescending(GetSortProperty(filter.OrderBy));
        }
        
        private Expression<Func<PaymentType, object>> GetSortProperty(string propertyName)
        {
            return propertyName?.ToLower() switch
            {
                "name" => pt => pt.Name,
                "description" => pt => pt.Description,
                "issystem" => pt => pt.IsSystem,
                "isfinancingtype" => pt => pt.IsFinancingType,
                _ => pt => pt.CreatedAt
            };
        }
    }
}
