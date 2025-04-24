namespace FinanceSystem.Web.Interfaces
{
    public interface IInvestmentService
    {
        Task<IEnumerable<Models.Investment.InvestmentModel>> GetAllInvestmentsAsync(string token);
        Task<Models.Investment.InvestmentModel> GetInvestmentByIdAsync(string id, string token);
        Task<IEnumerable<Models.Investment.InvestmentModel>> GetInvestmentsByTypeAsync(int type, string token);
        Task<Models.Investment.InvestmentModel> CreateInvestmentAsync(Models.Investment.CreateInvestmentModel model, string token);
        Task<Models.Investment.InvestmentModel> UpdateInvestmentAsync(string id, Models.Investment.UpdateInvestmentModel model, string token);
        Task DeleteInvestmentAsync(string id, string token);
        Task<Models.Investment.InvestmentModel> RefreshPriceAsync(string id, string token);
        Task<IEnumerable<Models.Investment.InvestmentModel>> RefreshAllPricesAsync(string token);
    }
}
