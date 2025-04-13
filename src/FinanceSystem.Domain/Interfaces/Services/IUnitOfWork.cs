using FinanceSystem.Domain.Interfaces.Repositories;

namespace FinanceSystem.Domain.Interfaces.Services
{
    public interface IUnitOfWork : IDisposable
    {
        IUserRepository Users { get; }
        IRoleRepository Roles { get; }
        IPermissionRepository Permissions { get; }
        Task<int> CompleteAsync();
    }
}