namespace Equilibrium.Domain.Interfaces.Repositories
{
    public interface IPaymentInstallmentRepository : IRepositoryBase<PaymentInstallment>
    {
        Task<IEnumerable<PaymentInstallment>> GetInstallmentsByPaymentIdAsync(Guid paymentId);
        Task<IEnumerable<PaymentInstallment>> GetInstallmentsByDueDateAsync(Guid userId, DateTime startDate, DateTime endDate);
        Task<IEnumerable<PaymentInstallment>> GetPendingInstallmentsByUserIdAsync(Guid userId);
        Task<IEnumerable<PaymentInstallment>> GetOverdueInstallmentsByUserIdAsync(Guid userId);
    }
}
