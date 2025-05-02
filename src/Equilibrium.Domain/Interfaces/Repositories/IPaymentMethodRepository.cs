using Equilibrium.Domain.Enums;

namespace Equilibrium.Domain.Interfaces.Repositories
{
    public interface IPaymentMethodRepository : IRepositoryBase<PaymentMethod>
    {
        Task<IEnumerable<PaymentMethod?>> GetAllSystemMethodsAsync();
        Task<IEnumerable<PaymentMethod?>> GetUserMethodsAsync(Guid userId);
        Task<IEnumerable<PaymentMethod?>> GetAllAvailableForUserAsync(Guid userId);
        Task<IEnumerable<PaymentMethod?>> GetByTypeAsync(PaymentMethodType type);
        Task<PaymentMethod?> GetByNameAsync(string name);
    }
}
