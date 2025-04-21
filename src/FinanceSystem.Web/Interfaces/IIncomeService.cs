using FinanceSystem.Web.Models.Income;

namespace FinanceSystem.Web.Services
{
    public interface IIncomeService
    {
        Task<IEnumerable<IncomeModel>> GetAllIncomesAsync(string token);
        Task<IncomeModel> GetIncomeByIdAsync(string id, string token);
        Task<IEnumerable<IncomeModel>> GetIncomesByMonthAsync(int month, int year, string token);
        Task<IEnumerable<IncomeModel>> GetPendingIncomesAsync(string token);
        Task<IEnumerable<IncomeModel>> GetReceivedIncomesAsync(string token);
        Task<IEnumerable<IncomeModel>> GetOverdueIncomesAsync(string token);
        Task<IEnumerable<IncomeModel>> GetIncomesByTypeAsync(string typeId, string token);
        Task<IncomeModel> CreateIncomeAsync(CreateIncomeModel model, string token);
        Task<IncomeModel> UpdateIncomeAsync(string id, UpdateIncomeModel model, string token);
        Task DeleteIncomeAsync(string id, string token);
        Task<IncomeModel> MarkAsReceivedAsync(string id, DateTime? receivedDate, string token);
        Task<IncomeModel> CancelIncomeAsync(string id, string token);
        Task<string> GetInstallmentParentIncomeAsync(string installmentId, string token);
        Task<bool> MarkInstallmentAsReceivedAsync(string installmentId, DateTime receivedDate, string token);
        Task<bool> CancelInstallmentAsync(string installmentId, string token);
    }
}