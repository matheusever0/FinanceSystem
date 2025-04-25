using FinanceSystem.Application.DTOs.Investment;
using FinanceSystem.Domain.Enums;

namespace FinanceSystem.Application.Interfaces
{
    public interface IInvestmentService
    {
        Task<InvestmentDto> GetByIdAsync(Guid id);
        Task<IEnumerable<InvestmentDto>> GetAllByUserIdAsync(Guid userId);
        Task<IEnumerable<InvestmentDto>> GetByTypeAsync(Guid userId, InvestmentType type);
        Task<InvestmentDto> CreateAsync(CreateInvestmentDto createInvestmentDto, Guid userId);
        Task DeleteAsync(Guid id);
        Task<InvestmentDto> RefreshPriceAsync(Guid id);
        Task<IEnumerable<InvestmentDto>> RefreshAllPricesAsync(Guid userId);
    }
}
