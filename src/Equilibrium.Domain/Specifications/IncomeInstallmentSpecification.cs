using Equilibrium.Domain.DTOs.Filters;
using Equilibrium.Domain.Entities;
using System.Linq.Expressions;

namespace Equilibrium.Domain.Specifications
{
    public class IncomeInstallmentSpecification : BaseSpecification<IncomeInstallment>
    {
        public IncomeInstallmentSpecification(IncomeInstallmentFilter filter)
        {
            AddInclude(ii => ii.Income);
            
            if (filter.InstallmentNumber.HasValue)
                ApplyCriteria(ii => ii.InstallmentNumber == filter.InstallmentNumber.Value);
                
            if (filter.MinAmount.HasValue)
                ApplyCriteria(ii => ii.Amount >= filter.MinAmount.Value);
                
            if (filter.MaxAmount.HasValue)
                ApplyCriteria(ii => ii.Amount <= filter.MaxAmount.Value);
                
            if (filter.DueDateFrom.HasValue)
                ApplyCriteria(ii => ii.DueDate >= filter.DueDateFrom.Value);
                
            if (filter.DueDateTo.HasValue)
                ApplyCriteria(ii => ii.DueDate <= filter.DueDateTo.Value);
                
            if (filter.ReceivedDateFrom.HasValue)
                ApplyCriteria(ii => ii.ReceivedDate.HasValue && ii.ReceivedDate >= filter.ReceivedDateFrom.Value);
                
            if (filter.ReceivedDateTo.HasValue)
                ApplyCriteria(ii => ii.ReceivedDate.HasValue && ii.ReceivedDate <= filter.ReceivedDateTo.Value);
                
            if (filter.Status.HasValue)
                ApplyCriteria(ii => ii.Status == filter.Status.Value);
                
            if (filter.IncomeId.HasValue)
                ApplyCriteria(ii => ii.IncomeId == filter.IncomeId.Value);
                
            int skip = (filter.PageNumber - 1) * filter.PageSize;
            ApplyPaging(skip, filter.PageSize);
            
            if (filter.Ascending)
                ApplyOrderBy(GetSortProperty(filter.OrderBy));
            else
                ApplyOrderByDescending(GetSortProperty(filter.OrderBy));
        }
        
        private Expression<Func<IncomeInstallment, object>> GetSortProperty(string propertyName)
        {
            return propertyName?.ToLower() switch
            {
                "installmentnumber" => ii => ii.InstallmentNumber,
                "amount" => ii => ii.Amount,
                "duedate" => ii => ii.DueDate,
                "receiveddate" => ii => ii.ReceivedDate,
                "status" => ii => ii.Status,
                _ => ii => ii.DueDate
            };
        }
    }
}
