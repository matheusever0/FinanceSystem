namespace Equilibrium.Web.Interfaces
{
    public interface IInvestmentTransactionService
    {
        Task<IEnumerable<Models.Investment.InvestmentTransactionModel>> GetTransactionsByInvestmentIdAsync(string investmentId, string token);
        Task<Models.Investment.InvestmentTransactionModel> GetTransactionByIdAsync(string id, string token);
        Task<Models.Investment.InvestmentTransactionModel> CreateTransactionAsync(string investmentId, Models.Investment.CreateInvestmentTransactionModel model, string token);
        Task DeleteTransactionAsync(string id, string token);
    }
}
