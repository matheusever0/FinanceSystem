using Equilibrium.Domain.Entities;

namespace Equilibrium.Domain.Interfaces.Repositories
{
    public interface IIncomeTypeRepository : IRepositoryBase<IncomeType>
    {
        Task<IncomeType?> GetByNameAsync(string name);
        Task<IEnumerable<IncomeType>> GetAllSystemTypesAsync();
        Task<IEnumerable<IncomeType>> GetUserTypesAsync(Guid userId);
        Task<IEnumerable<IncomeType>> GetAllAvailableForUserAsync(Guid userId);
    }
}