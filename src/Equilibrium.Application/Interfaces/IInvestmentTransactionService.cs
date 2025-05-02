using Equilibrium.Application.DTOs.InvestmentTransaction;

namespace Equilibrium.Application.Interfaces
{
    public interface IInvestmentTransactionService
    {
        Task<InvestmentTransactionDto> GetByIdAsync(Guid id);
        Task<IEnumerable<InvestmentTransactionDto>> GetByInvestmentIdAsync(Guid investmentId);
        Task<InvestmentTransactionDto> CreateAsync(Guid investmentId, CreateInvestmentTransactionDto createInvestmentTransactionDto);
        Task DeleteAsync(Guid id);
    }
}
