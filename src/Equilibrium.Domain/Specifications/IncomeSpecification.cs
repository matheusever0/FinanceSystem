using System.Linq.Expressions;
using Equilibrium.Domain.Entities;
using Equilibrium.Domain.DTOs.Filters;

namespace Equilibrium.Domain.Specifications
{
    public class IncomeSpecification : BaseSpecification<Income>
    {
        public IncomeSpecification(IncomeFilter filter)
        {
            AddInclude(i => i.IncomeType);
            AddInclude(i => i.Installments);
            
            if (!string.IsNullOrEmpty(filter.Description))
                ApplyCriteria(i => i.Description.Contains(filter.Description));
                
            if (filter.MinAmount.HasValue)
                ApplyCriteria(i => i.Amount >= filter.MinAmount.Value);
                
            if (filter.MaxAmount.HasValue)
                ApplyCriteria(i => i.Amount <= filter.MaxAmount.Value);
                
            if (filter.StartDate.HasValue)
                ApplyCriteria(i => i.DueDate >= filter.StartDate.Value);
                
            if (filter.EndDate.HasValue)
                ApplyCriteria(i => i.DueDate <= filter.EndDate.Value);
                
            if (filter.Status.HasValue)
                ApplyCriteria(i => i.Status == filter.Status.Value);
                
            if (filter.IncomeTypeId.HasValue)
                ApplyCriteria(i => i.IncomeTypeId == filter.IncomeTypeId.Value);
                
            if (filter.IsRecurring.HasValue)
                ApplyCriteria(i => i.IsRecurring == filter.IsRecurring.Value);
                
            int skip = (filter.PageNumber - 1) * filter.PageSize;
            ApplyPaging(skip, filter.PageSize);
            
            if (filter.Ascending)
                ApplyOrderBy(GetSortProperty(filter.OrderBy));
            else
                ApplyOrderByDescending(GetSortProperty(filter.OrderBy));
        }
        
        private Expression<Func<Income, object>> GetSortProperty(string propertyName)
        {
            return propertyName?.ToLower() switch
            {
                "amount" => i => i.Amount,
                "duedate" => i => i.DueDate,
                "description" => i => i.Description,
                "status" => i => i.Status,
                "incometype" => i => i.IncomeType.Name,
                _ => i => i.CreatedAt
            };
        }
    }
}
