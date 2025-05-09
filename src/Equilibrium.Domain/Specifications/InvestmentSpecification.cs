using Equilibrium.Domain.DTOs.Filters;
using Equilibrium.Domain.Entities;
using System.Linq.Expressions;

namespace Equilibrium.Domain.Specifications
{
    public class InvestmentSpecification : BaseSpecification<Investment>
    {
        public InvestmentSpecification(InvestmentFilter filter)
        {
            AddInclude(i => i.Transactions);
            
            if (!string.IsNullOrEmpty(filter.Symbol))
                ApplyCriteria(i => i.Symbol.Contains(filter.Symbol));
                
            if (!string.IsNullOrEmpty(filter.Name))
                ApplyCriteria(i => i.Name.Contains(filter.Name));
                
            if (filter.Type.HasValue)
                ApplyCriteria(i => i.Type == filter.Type.Value);
                
            if (filter.MinQuantity.HasValue)
                ApplyCriteria(i => i.TotalQuantity >= filter.MinQuantity.Value);
                
            if (filter.MaxQuantity.HasValue)
                ApplyCriteria(i => i.TotalQuantity <= filter.MaxQuantity.Value);
                
            if (filter.MinPrice.HasValue)
                ApplyCriteria(i => i.CurrentPrice >= filter.MinPrice.Value);
                
            if (filter.MaxPrice.HasValue)
                ApplyCriteria(i => i.CurrentPrice <= filter.MaxPrice.Value);
                
            if (filter.MinGainLossPercentage.HasValue)
                ApplyCriteria(i => i.GainLossPercentage >= filter.MinGainLossPercentage.Value);
                
            if (filter.MaxGainLossPercentage.HasValue)
                ApplyCriteria(i => i.GainLossPercentage <= filter.MaxGainLossPercentage.Value);
                
            if (!string.IsNullOrEmpty(filter.Currency))
                ApplyCriteria(i => i.Currency == filter.Currency);
                
            int skip = (filter.PageNumber - 1) * filter.PageSize;
            ApplyPaging(skip, filter.PageSize);
            
            if (filter.Ascending)
                ApplyOrderBy(GetSortProperty(filter.OrderBy));
            else
                ApplyOrderByDescending(GetSortProperty(filter.OrderBy));
        }
        
        private Expression<Func<Investment, object>> GetSortProperty(string propertyName)
        {
            return propertyName?.ToLower() switch
            {
                "symbol" => i => i.Symbol,
                "name" => i => i.Name,
                "type" => i => i.Type,
                "quantity" => i => i.TotalQuantity,
                "price" => i => i.CurrentPrice,
                "averageprice" => i => i.AveragePrice,
                "gainloss" => i => i.GainLossPercentage,
                "totalinvested" => i => i.TotalInvested,
                "currenttotal" => i => i.CurrentTotal,
                "lastupdate" => i => i.LastUpdate,
                _ => i => i.Symbol
            };
        }
    }
}
