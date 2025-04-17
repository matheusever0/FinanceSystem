using FinanceSystem.Web.Models.CreditCard;

namespace FinanceSystem.Web.Services
{
    public interface ICreditCardService
    {
        Task<IEnumerable<CreditCardModel>> GetAllCreditCardsAsync(string token);
        Task<CreditCardModel> GetCreditCardByIdAsync(string id, string token);
        Task<CreditCardModel> CreateCreditCardAsync(CreateCreditCardModel model, string token);
        Task<CreditCardModel> UpdateCreditCardAsync(string id, UpdateCreditCardModel model, string token);
        Task DeleteCreditCardAsync(string id, string token);
    }
}