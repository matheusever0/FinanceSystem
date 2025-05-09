using Equilibrium.Domain.DTOs.Filters;
using Equilibrium.Domain.Entities;
using System.Linq.Expressions;

namespace Equilibrium.Domain.Specifications
{
    public class FinancingSpecification : BaseSpecification<Financing>
    {
        public FinancingSpecification(FinancingFilter filter)
        {
            AddInclude(f => f.Installments);
            AddInclude(f => f.Payments);
            
            if (!string.IsNullOrEmpty(filter.Description))
                ApplyCriteria(f => f.Description.Contains(filter.Description));
                
            if (filter.MinTotalAmount.HasValue)
                ApplyCriteria(f => f.TotalAmount >= filter.MinTotalAmount.Value);
                
            if (filter.MaxTotalAmount.HasValue)
                ApplyCriteria(f => f.TotalAmount <= filter.MaxTotalAmount.Value);
                
            if (filter.MinInterestRate.HasValue)
                ApplyCriteria(f => f.InterestRate >= filter.MinInterestRate.Value);
                
            if (filter.MaxInterestRate.HasValue)
                ApplyCriteria(f => f.InterestRate <= filter.MaxInterestRate.Value);
                
            if (filter.MinTermMonths.HasValue)
                ApplyCriteria(f => f.TermMonths >= filter.MinTermMonths.Value);
                
            if (filter.MaxTermMonths.HasValue)
                ApplyCriteria(f => f.TermMonths <= filter.MaxTermMonths.Value);
                
            if (filter.StartDateFrom.HasValue)
                ApplyCriteria(f => f.StartDate >= filter.StartDateFrom.Value);
                
            if (filter.StartDateTo.HasValue)
                ApplyCriteria(f => f.StartDate <= filter.StartDateTo.Value);
                
            if (filter.Type.HasValue)
                ApplyCriteria(f => f.Type == filter.Type.Value);
                
            if (filter.Status.HasValue)
                ApplyCriteria(f => f.Status == filter.Status.Value);
                
            if (filter.CorrectionIndex.HasValue)
                ApplyCriteria(f => f.CorrectionIndex == filter.CorrectionIndex.Value);
                
            if (filter.MinRemainingDebt.HasValue)
                ApplyCriteria(f => f.RemainingDebt >= filter.MinRemainingDebt.Value);
                
            if (filter.MaxRemainingDebt.HasValue)
                ApplyCriteria(f => f.RemainingDebt <= filter.MaxRemainingDebt.Value);
                
            int skip = (filter.PageNumber - 1) * filter.PageSize;
            ApplyPaging(skip, filter.PageSize);
            
            if (filter.Ascending)
                ApplyOrderBy(GetSortProperty(filter.OrderBy));
            else
                ApplyOrderByDescending(GetSortProperty(filter.OrderBy));
        }
        
        private Expression<Func<Financing, object>> GetSortProperty(string propertyName)
        {
            return propertyName?.ToLower() switch
            {
                "description" => f => f.Description,
                "totalamount" => f => f.TotalAmount,
                "interestrate" => f => f.InterestRate,
                "termmonths" => f => f.TermMonths,
                "startdate" => f => f.StartDate,
                "enddate" => f => f.EndDate,
                "type" => f => f.Type,
                "status" => f => f.Status,
                "remainingdebt" => f => f.RemainingDebt,
                "lastcorrectiondate" => f => f.LastCorrectionDate,
                _ => f => f.CreatedAt
            };
        }
    }
}
