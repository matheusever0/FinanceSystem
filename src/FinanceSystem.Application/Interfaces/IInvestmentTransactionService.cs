using FinanceSystem.Application.DTOs.InvestmentTransaction;

namespace FinanceSystem.Application.Interfaces
{
    public interface IInvestmentTransactionService
    {
        Task<InvestmentTransactionDto> GetByIdAsync(Guid id);
        Task<IEnumerable<InvestmentTransactionDto>> GetByInvestmentIdAsync(Guid investmentId);
        Task<InvestmentTransactionDto> CreateAsync(Guid investmentId, CreateInvestmentTransactionDto createInvestmentTransactionDto);
        Task DeleteAsync(Guid id);
    }
}
