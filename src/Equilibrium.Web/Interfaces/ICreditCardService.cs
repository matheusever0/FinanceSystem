using Equilibrium.Web.Models.CreditCard;

using Equilibrium.Web.Models.Filters;
using Equilibrium.Web.Models.Generics;

namespace Equilibrium.Web.Services
{
    public interface ICreditCardService
    {
        Task<IEnumerable<CreditCardModel>> GetAllCreditCardsAsync(string token);
        Task<CreditCardModel> GetCreditCardByIdAsync(string id, string token);
        Task<CreditCardModel> CreateCreditCardAsync(CreateCreditCardModel model, string token);
        Task<CreditCardModel> UpdateCreditCardAsync(string id, UpdateCreditCardModel model, string token);
        Task DeleteCreditCardAsync(string id, string token);
        Task<PagedResult<CreditCardModel>> GetFilteredAsync(CreditCardFilter filter, string token);
    }
}

