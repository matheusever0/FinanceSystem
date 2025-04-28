using FinanceSystem.Application.DTOs.Financing;
using FinanceSystem.Domain.Enums;

namespace FinanceSystem.Application.Interfaces
{
    public interface IFinancingService
    {
        Task<FinancingDto> GetByIdAsync(Guid id);
        Task<FinancingDetailDto> GetDetailsByIdAsync(Guid id);
        Task<IEnumerable<FinancingDto>> GetAllByUserIdAsync(Guid userId);
        Task<IEnumerable<FinancingDto>> GetActiveFinancingsByUserIdAsync(Guid userId);
        Task<IEnumerable<FinancingDto>> GetFinancingsByStatusAsync(Guid userId, FinancingStatus status);
        Task<decimal> GetTotalRemainingDebtByUserIdAsync(Guid userId);
        Task<FinancingDto> CreateAsync(CreateFinancingDto createFinancingDto, Guid userId);
        Task<FinancingDto> UpdateAsync(Guid id, UpdateFinancingDto updateFinancingDto);
        Task CompleteAsync(Guid id);
        Task CancelAsync(Guid id);
        Task<FinancingSimulationDto> SimulateAsync(FinancingSimulationRequestDto simulationRequest);
        Task<FinancingForecastDto> ForecastAsync(FinancingForecastRequestDto forecastRequest);
    }
}
