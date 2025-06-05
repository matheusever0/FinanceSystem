using Equilibrium.Web.Models.Generics;
using Equilibrium.Web.Models.PaymentMethod;

namespace Equilibrium.Web.Interfaces
{
    public interface IPaymentMethodService
    {
        Task<IEnumerable<PaymentMethodModel>> GetAllPaymentMethodsAsync(string token);
        Task<IEnumerable<PaymentMethodModel>> GetSystemPaymentMethodsAsync(string token);
        Task<IEnumerable<PaymentMethodModel>> GetUserPaymentMethodsAsync(string token);
        Task<IEnumerable<PaymentMethodModel>> GetByTypeAsync(int type, string token);
        Task<PaymentMethodModel> GetPaymentMethodByIdAsync(string id, string token);
        Task<PaymentMethodModel> CreatePaymentMethodAsync(CreatePaymentMethodModel model, string token);
        Task<PaymentMethodModel> UpdatePaymentMethodAsync(string id, UpdatePaymentMethodModel model, string token);
        Task DeletePaymentMethodAsync(string id, string token);
    }
}


