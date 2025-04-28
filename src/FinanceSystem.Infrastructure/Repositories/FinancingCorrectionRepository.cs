using FinanceSystem.Domain.Entities;
using FinanceSystem.Domain.Interfaces.Repositories;
using FinanceSystem.Infrastructure.Data;
using Microsoft.EntityFrameworkCore;

namespace FinanceSystem.Infrastructure.Repositories
{
    public class FinancingCorrectionRepository : RepositoryBase<FinancingCorrection>, IFinancingCorrectionRepository
    {
        public FinancingCorrectionRepository(ApplicationDbContext context) : base(context)
        {
        }

        public async Task<IEnumerable<FinancingCorrection>> GetCorrectionsByFinancingIdAsync(Guid financingId)
        {
            return await _dbSet
                .Where(c => c.FinancingId == financingId)
                .OrderByDescending(c => c.CorrectionDate)
                .ToListAsync();
        }

        public async Task<IEnumerable<FinancingCorrection>> GetCorrectionsByDateRangeAsync(Guid financingId, DateTime startDate, DateTime endDate)
        {
            return await _dbSet
                .Where(c => c.FinancingId == financingId &&
                            c.CorrectionDate >= startDate &&
                            c.CorrectionDate <= endDate)
                .OrderByDescending(c => c.CorrectionDate)
                .ToListAsync();
        }

        public async Task<FinancingCorrection?> GetLatestCorrectionByFinancingIdAsync(Guid financingId)
        {
            return await _dbSet
                .Where(c => c.FinancingId == financingId)
                .OrderByDescending(c => c.CorrectionDate)
                .FirstOrDefaultAsync();
        }

        public async Task<decimal> GetTotalCorrectionImpactAsync(Guid financingId)
        {
            var corrections = await _dbSet
                .Where(c => c.FinancingId == financingId)
                .ToListAsync();

            if (!corrections.Any())
                return 0;

            decimal totalImpact = corrections.Sum(c => c.NewDebt - c.PreviousDebt);
            return totalImpact;
        }

        public async Task<Dictionary<int, List<FinancingCorrection>>> GetCorrectionsByYearAsync(Guid financingId)
        {
            var corrections = await _dbSet
                .Where(c => c.FinancingId == financingId)
                .OrderByDescending(c => c.CorrectionDate)
                .ToListAsync();

            return corrections
                .GroupBy(c => c.CorrectionDate.Year)
                .ToDictionary(g => g.Key, g => g.ToList());
        }
    }
}