using FinanceSystem.Web.Models.Financing;

namespace FinanceSystem.Web.Interfaces
{
    public interface IFinancingService
    {
        Task<IEnumerable<FinancingModel>> GetAllFinancingsAsync(string token);
        Task<IEnumerable<FinancingModel>> GetActiveFinancingsAsync(string token);
        Task<FinancingModel> GetFinancingByIdAsync(string id, string token);
        Task<FinancingDetailModel> GetFinancingDetailsAsync(string id, string token);
        Task<IEnumerable<FinancingInstallmentModel>> GetFinancingInstallmentsAsync(string financingId, string token);
        Task<FinancingModel> CreateFinancingAsync(CreateFinancingModel model, string token);
        Task<FinancingModel> UpdateFinancingAsync(string id, UpdateFinancingModel model, string token);
        Task<bool> CancelFinancingAsync(string id, string token);
        Task<bool> CompleteFinancingAsync(string id, string token);
        Task<FinancingSimulationModel> SimulateFinancingAsync(FinancingSimulationRequestModel model, string token);
        Task<FinancingForecastModel> ForecastFinancingAsync(string financingId, int forecastMonths, string token);
        Task<bool> ApplyMonetaryCorrectionAsync(ApplyCorrectionModel model, string token);
    }
}
