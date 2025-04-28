using FinanceSystem.Web.Models.Financing;

namespace FinanceSystem.Web.Interfaces
{
    public interface IFinancingService
    {
        Task<IEnumerable<FinancingModel>> GetAllFinancingsAsync(string token);
        Task<IEnumerable<FinancingModel>> GetActiveFinancingsAsync(string token);
        Task<FinancingModel> GetFinancingByIdAsync(string id, string token);
        Task<FinancingDetailDto> GetFinancingDetailsAsync(string id, string token);
        Task<IEnumerable<FinancingInstallmentModel>> GetFinancingInstallmentsAsync(string financingId, string token);
        Task<FinancingModel> CreateFinancingAsync(CreateFinancingDto model, string token);
        Task<FinancingModel> UpdateFinancingAsync(string id, UpdateFinancingDto model, string token);
        Task<bool> CancelFinancingAsync(string id, string token);
        Task<bool> CompleteFinancingAsync(string id, string token);
        Task<FinancingSimulationDto> SimulateFinancingAsync(FinancingSimulationRequestDto model, string token);
        Task<FinancingForecastDto> ForecastFinancingAsync(string financingId, int forecastMonths, string token);
        Task<bool> ApplyMonetaryCorrectionAsync(ApplyCorrectionDto model, string token);
    }
}
