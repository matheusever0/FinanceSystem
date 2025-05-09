using Equilibrium.Web.Models.Payment;

using Equilibrium.Web.Models.Filters;
using Equilibrium.Web.Models.Generics;

namespace Equilibrium.Web.Services
{
    public interface IPaymentService
    {
        Task<IEnumerable<PaymentModel>> GetAllPaymentsAsync(string token);
        Task<PaymentModel> GetPaymentByIdAsync(string id, string token);
        Task<IEnumerable<PaymentModel>> GetPaymentsByMonthAsync(int month, int year, string token);
        Task<IEnumerable<PaymentModel>> GetPendingPaymentsAsync(string token);
        Task<IEnumerable<PaymentModel>> GetOverduePaymentsAsync(string token);
        Task<IEnumerable<PaymentModel>> GetPaymentsByTypeAsync(string typeId, string token);
        Task<IEnumerable<PaymentModel>> GetPaymentsByMethodAsync(string methodId, string token);
        Task<PaymentModel> CreatePaymentAsync(CreatePaymentModel model, string token);
        Task<PaymentModel> UpdatePaymentAsync(string id, UpdatePaymentModel model, string token);
        Task DeletePaymentAsync(string id, string token);
        Task<PaymentModel> MarkAsPaidAsync(string id, DateTime? paymentDate, string token);
        Task<PaymentModel> MarkAsOverdueAsync(string id, string token);
        Task<PaymentModel> CancelPaymentAsync(string id, string token);
        Task<string> GetInstallmentParentPaymentAsync(string installmentId, string token);
        Task<bool> MarkInstallmentAsPaidAsync(string installmentId, DateTime paymentDate, string token);
        Task<bool> MarkInstallmentAsOverdueAsync(string installmentId, string token);
        Task<bool> CancelInstallmentAsync(string installmentId, string token);
        Task<PagedResult<PaymentModel>> GetFilteredAsync(PaymentFilter filter, string token);
    }
}

