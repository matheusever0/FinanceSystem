namespace FinanceSystem.Domain.Interfaces.Repositories
{
    public interface IPaymentRepository : IRepositoryBase<Payment>
    {
        Task<IEnumerable<Payment?>> GetPaymentsByUserIdAsync(Guid userId);
        Task<IEnumerable<Payment?>> GetPaymentsByUserIdAndMonthAsync(Guid userId, int month, int year);
        Task<IEnumerable<Payment?>> GetPendingPaymentsByUserIdAsync(Guid userId);
        Task<IEnumerable<Payment?>> GetOverduePaymentsByUserIdAsync(Guid userId);
        Task<IEnumerable<Payment?>> GetPaymentsByTypeAsync(Guid userId, Guid paymentTypeId);
        Task<IEnumerable<Payment?>> GetPaymentsByMethodAsync(Guid userId, Guid paymentMethodId);
        Task<Payment?> GetPaymentWithDetailsAsync(Guid paymentId);
        Task<IEnumerable<Payment>> GetRecurringPaymentsWithDetailsAsync();
        Task<IEnumerable<Payment>> GetPaymentsByPeriodAndDetailsAsync(Guid userId, Guid paymentTypeId, string description, DateTime startDate, DateTime endDate);
    }
}
