using System.Linq.Expressions;
using Equilibrium.Domain.DTOs.Filters;
using Equilibrium.Domain.Entities;

namespace Equilibrium.Domain.Specifications
{
    public class PaymentSpecification : BaseSpecification<Payment>
    {
        public PaymentSpecification(PaymentFilter filter)
        {
            AddInclude(p => p.PaymentType);
            AddInclude(p => p.PaymentMethod);
            AddInclude(p => p.Installments);
            
            if (!string.IsNullOrEmpty(filter.Description))
                ApplyCriteria(p => p.Description.Contains(filter.Description));
                
            if (filter.MinAmount.HasValue)
                ApplyCriteria(p => p.Amount >= filter.MinAmount.Value);
                
            if (filter.MaxAmount.HasValue)
                ApplyCriteria(p => p.Amount <= filter.MaxAmount.Value);
                
            if (filter.StartDate.HasValue)
                ApplyCriteria(p => p.DueDate >= filter.StartDate.Value);
                
            if (filter.EndDate.HasValue)
                ApplyCriteria(p => p.DueDate <= filter.EndDate.Value);
                
            if (filter.Status.HasValue)
                ApplyCriteria(p => p.Status == filter.Status.Value);
                
            if (filter.PaymentTypeId.HasValue)
                ApplyCriteria(p => p.PaymentTypeId == filter.PaymentTypeId.Value);
                
            if (filter.PaymentMethodId.HasValue)
                ApplyCriteria(p => p.PaymentMethodId == filter.PaymentMethodId.Value);
                
            if (filter.IsRecurring.HasValue)
                ApplyCriteria(p => p.IsRecurring == filter.IsRecurring.Value);
                
            int skip = (filter.PageNumber - 1) * filter.PageSize;
            ApplyPaging(skip, filter.PageSize);
            
            if (filter.Ascending)
                ApplyOrderBy(GetSortProperty(filter.OrderBy));
            else
                ApplyOrderByDescending(GetSortProperty(filter.OrderBy));
        }
        
        private Expression<Func<Payment, object>> GetSortProperty(string propertyName)
        {
            return propertyName?.ToLower() switch
            {
                "amount" => p => p.Amount,
                "duedate" => p => p.DueDate,
                "description" => p => p.Description,
                "status" => p => p.Status,
                "paymenttype" => p => p.PaymentType.Name,
                "paymentmethod" => p => p.PaymentMethod.Name,
                _ => p => p.CreatedAt
            };
        }
    }
}
