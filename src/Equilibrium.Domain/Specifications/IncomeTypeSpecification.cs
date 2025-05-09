using Equilibrium.Domain.Entities;
using System.Linq.Expressions;
using Equilibrium.Domain.DTOs.Filters;

namespace Equilibrium.Domain.Specifications
{
    public class IncomeTypeSpecification : BaseSpecification<IncomeType>
    {
        public IncomeTypeSpecification(IncomeTypeFilter filter)
        {
            if (!string.IsNullOrEmpty(filter.Name))
                ApplyCriteria(it => it.Name.Contains(filter.Name));
                
            if (!string.IsNullOrEmpty(filter.Description))
                ApplyCriteria(it => it.Description.Contains(filter.Description));
                
            if (filter.IsSystem.HasValue)
                ApplyCriteria(it => it.IsSystem == filter.IsSystem.Value);
                
            int skip = (filter.PageNumber - 1) * filter.PageSize;
            ApplyPaging(skip, filter.PageSize);
            
            if (filter.Ascending)
                ApplyOrderBy(GetSortProperty(filter.OrderBy));
            else
                ApplyOrderByDescending(GetSortProperty(filter.OrderBy));
        }
        
        private Expression<Func<IncomeType, object>> GetSortProperty(string propertyName)
        {
            return propertyName?.ToLower() switch
            {
                "name" => it => it.Name,
                "description" => it => it.Description,
                "issystem" => it => it.IsSystem,
                _ => it => it.CreatedAt
            };
        }
    }
}
