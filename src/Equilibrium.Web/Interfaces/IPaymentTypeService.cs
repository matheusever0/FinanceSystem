using Equilibrium.Web.Models;

namespace Equilibrium.Web.Services
{
    public interface IPaymentTypeService
    {
        Task<IEnumerable<PaymentTypeModel>> GetAllPaymentTypesAsync(string token);
        Task<IEnumerable<PaymentTypeModel>> GetSystemPaymentTypesAsync(string token);
        Task<IEnumerable<PaymentTypeModel>> GetUserPaymentTypesAsync(string token);
        Task<PaymentTypeModel> GetPaymentTypeByIdAsync(string id, string token);
        Task<PaymentTypeModel> CreatePaymentTypeAsync(CreatePaymentTypeModel model, string token);
        Task<PaymentTypeModel> UpdatePaymentTypeAsync(string id, UpdatePaymentTypeModel model, string token);
        Task DeletePaymentTypeAsync(string id, string token);
    }
}

