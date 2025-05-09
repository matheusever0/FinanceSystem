using System.Linq.Expressions;

namespace Equilibrium.Domain.Interfaces.Repositories
{
    public interface IRepositoryBase<T> where T : class
    {
        Task<T?> GetByIdAsync(Guid id);
        Task<IEnumerable<T?>> GetAllAsync();
        Task<IEnumerable<T?>> FindAsync(Expression<Func<T, bool>> predicate);
        Task AddAsync(T entity);
        Task UpdateAsync(T entity);
        Task DeleteAsync(T entity);
        Task<(IEnumerable<T> Items, int TotalCount)> FindWithSpecificationAsync(Specifications.BaseSpecification<T> specification);
    }
}