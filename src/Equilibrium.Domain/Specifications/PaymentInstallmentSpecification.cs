using System.Linq.Expressions;
using Equilibrium.Domain.DTOs.Filters;

namespace Equilibrium.Domain.Specifications
{
    public class PaymentInstallmentSpecification : BaseSpecification<PaymentInstallment>
    {
        public PaymentInstallmentSpecification(PaymentInstallmentFilter filter)
        {
            AddInclude(pi => pi.Payment);
            
            if (filter.InstallmentNumber.HasValue)
                ApplyCriteria(pi => pi.InstallmentNumber == filter.InstallmentNumber.Value);
                
            if (filter.MinAmount.HasValue)
                ApplyCriteria(pi => pi.Amount >= filter.MinAmount.Value);
                
            if (filter.MaxAmount.HasValue)
                ApplyCriteria(pi => pi.Amount <= filter.MaxAmount.Value);
                
            if (filter.DueDateFrom.HasValue)
                ApplyCriteria(pi => pi.DueDate >= filter.DueDateFrom.Value);
                
            if (filter.DueDateTo.HasValue)
                ApplyCriteria(pi => pi.DueDate <= filter.DueDateTo.Value);
                
            if (filter.PaymentDateFrom.HasValue)
                ApplyCriteria(pi => pi.PaymentDate.HasValue && pi.PaymentDate >= filter.PaymentDateFrom.Value);
                
            if (filter.PaymentDateTo.HasValue)
                ApplyCriteria(pi => pi.PaymentDate.HasValue && pi.PaymentDate <= filter.PaymentDateTo.Value);
                
            if (filter.Status.HasValue)
                ApplyCriteria(pi => pi.Status == filter.Status.Value);
                
            if (filter.PaymentId.HasValue)
                ApplyCriteria(pi => pi.PaymentId == filter.PaymentId.Value);
                
            int skip = (filter.PageNumber - 1) * filter.PageSize;
            ApplyPaging(skip, filter.PageSize);
            
            if (filter.Ascending)
                ApplyOrderBy(GetSortProperty(filter.OrderBy));
            else
                ApplyOrderByDescending(GetSortProperty(filter.OrderBy));
        }
        
        private Expression<Func<PaymentInstallment, object>> GetSortProperty(string propertyName)
        {
            return propertyName?.ToLower() switch
            {
                "installmentnumber" => pi => pi.InstallmentNumber,
                "amount" => pi => pi.Amount,
                "duedate" => pi => pi.DueDate,
                "paymentdate" => pi => pi.PaymentDate,
                "status" => pi => pi.Status,
                _ => pi => pi.DueDate
            };
        }
    }
}
