namespace FinanceSystem.Domain.Interfaces.Repositories
{
    public interface IPaymentTypeRepository : IRepositoryBase<PaymentType>
    {
        Task<IEnumerable<PaymentType?>> GetAllSystemTypesAsync();
        Task<IEnumerable<PaymentType?>> GetUserTypesAsync(Guid userId);
        Task<IEnumerable<PaymentType?>> GetAllAvailableForUserAsync(Guid userId);
        Task<PaymentType?> GetByNameAsync(string name);
    }
}
