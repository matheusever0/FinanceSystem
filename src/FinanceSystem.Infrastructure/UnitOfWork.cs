using FinanceSystem.Domain.Interfaces.Repositories;
using FinanceSystem.Domain.Interfaces.Services;
using FinanceSystem.Infrastructure.Data;
using FinanceSystem.Infrastructure.Repositories;

namespace FinanceSystem.Infrastructure
{
    public class UnitOfWork : IUnitOfWork
    {
        private readonly ApplicationDbContext _context;
        private IUserRepository _userRepository;
        private IRoleRepository _roleRepository;
        private IPermissionRepository _permissionRepository;
        private IPaymentRepository _paymentRepository;
        private IPaymentTypeRepository _paymentTypeRepository;
        private IPaymentMethodRepository _paymentMethodRepository;
        private ICreditCardRepository _creditCardRepository;
        private IPaymentInstallmentRepository _paymentInstallmentRepository;
        private bool _disposed = false;

        public UnitOfWork(ApplicationDbContext context)
        {
            _context = context;
        }

        public IUserRepository Users => _userRepository ??= new UserRepository(_context);

        public IRoleRepository Roles => _roleRepository ??= new RoleRepository(_context);

        public IPermissionRepository Permissions => _permissionRepository ??= new PermissionRepository(_context);

        public IPaymentRepository Payments => _paymentRepository ??= new PaymentRepository(_context);

        public IPaymentTypeRepository PaymentTypes => _paymentTypeRepository ??= new PaymentTypeRepository(_context);

        public IPaymentMethodRepository PaymentMethods => _paymentMethodRepository ??= new PaymentMethodRepository(_context);

        public ICreditCardRepository CreditCards => _creditCardRepository ??= new CreditCardRepository(_context);

        public IPaymentInstallmentRepository PaymentInstallments => _paymentInstallmentRepository ??= new PaymentInstallmentRepository(_context);

        public async Task<int> CompleteAsync()
        {
            return await _context.SaveChangesAsync();
        }

        protected virtual void Dispose(bool disposing)
        {
            if (!_disposed)
            {
                if (disposing)
                {
                    _context.Dispose();
                }
                _disposed = true;
            }
        }

        public void Dispose()
        {
            Dispose(true);
            GC.SuppressFinalize(this);
        }
    }
}