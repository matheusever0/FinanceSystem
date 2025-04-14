using FinanceSystem.Domain.Interfaces.Repositories;

namespace FinanceSystem.Domain.Interfaces.Services
{
    public interface IUnitOfWork : IDisposable
    {
        IUserRepository Users { get; }
        IRoleRepository Roles { get; }
        IPermissionRepository Permissions { get; }
        IPaymentRepository Payments { get; }
        IPaymentTypeRepository PaymentTypes { get; }
        IPaymentMethodRepository PaymentMethods { get; }
        ICreditCardRepository CreditCards { get; }
        IPaymentInstallmentRepository PaymentInstallments { get; }
        Task<int> CompleteAsync();
    }
}