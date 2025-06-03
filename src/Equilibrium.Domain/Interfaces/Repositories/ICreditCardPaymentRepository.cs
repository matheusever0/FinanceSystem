using Equilibrium.Domain.Entities;

namespace Equilibrium.Domain.Interfaces.Repositories
{
    public interface ICreditCardPaymentRepository : IRepositoryBase<CreditCardPayment>
    {
        Task<IEnumerable<CreditCardPayment>> GetPaymentsByCreditCardIdAsync(Guid creditCardId);
        Task<IEnumerable<CreditCardPayment>> GetPaymentsByUserIdAsync(Guid userId);
        Task<IEnumerable<CreditCardPayment>> GetPaymentsByPeriodAsync(Guid creditCardId, DateTime startDate, DateTime endDate);
        Task<decimal> GetTotalPaidInPeriodAsync(Guid creditCardId, DateTime startDate, DateTime endDate);
    }
}