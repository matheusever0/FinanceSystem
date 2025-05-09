using Equilibrium.Domain.Interfaces.Repositories;
using Equilibrium.Infrastructure.Data;
using Microsoft.EntityFrameworkCore;
using System.Linq.Expressions;

namespace Equilibrium.Infrastructure.Repositories
{
    public class RepositoryBase<T> : IRepositoryBase<T> where T : class
    {
        protected readonly ApplicationDbContext _context;
        protected readonly DbSet<T> _dbSet;

        public RepositoryBase(ApplicationDbContext context)
        {
            _context = context;
            _dbSet = context.Set<T>();
        }

        public async Task<T?> GetByIdAsync(Guid id)
        {
            return await _dbSet.FindAsync(id);
        }

        public async Task<IEnumerable<T?>> GetAllAsync()
        {
            return await _dbSet.ToListAsync();
        }

        public async Task<IEnumerable<T?>> FindAsync(Expression<Func<T, bool>> predicate)
        {
            return await _dbSet.Where(predicate).ToListAsync();
        }

        public async Task AddAsync(T entity)
        {
            await _dbSet.AddAsync(entity);
        }

        public Task UpdateAsync(T entity)
        {
            _context.Entry(entity).State = EntityState.Modified;
            return Task.CompletedTask;
        }

        public Task DeleteAsync(T entity)
        {
            _dbSet.Remove(entity);
            return Task.CompletedTask;
        }
        public async Task<(IEnumerable<T> Items, int TotalCount)> FindWithSpecificationAsync(Domain.Specifications.BaseSpecification<T> specification)
        {
            var query = ApplySpecification(specification);
            var totalCount = await query.CountAsync();

            if (specification.IsPagingEnabled)
            {
                query = query.Skip(specification.Skip).Take(specification.Take);
            }

            var items = await query.ToListAsync();
            return (items, totalCount);
        }

        private IQueryable<T> ApplySpecification(Domain.Specifications.BaseSpecification<T> spec)
        {
            var query = _dbSet.AsQueryable();

            if (spec.Criteria != null)
                query = query.Where(spec.Criteria);

            // Apply user filter if provided and entity has UserId property
            if (spec.UserId.HasValue)
            {
                var userIdProperty = typeof(T).GetProperty("UserId");
                if (userIdProperty != null)
                {
                    var parameter = Expression.Parameter(typeof(T), "x");
                    var property = Expression.Property(parameter, userIdProperty);
                    var value = Expression.Constant(spec.UserId.Value);
                    var equals = Expression.Equal(property, value);
                    var lambda = Expression.Lambda<Func<T, bool>>(equals, parameter);

                    query = query.Where(lambda);
                }
            }

            foreach (var include in spec.Includes)
            {
                query = query.Include(include);
            }

            foreach (var include in spec.IncludeStrings)
            {
                query = query.Include(include);
            }

            if (spec.OrderBy != null)
                query = query.OrderBy(spec.OrderBy);
            else if (spec.OrderByDescending != null)
                query = query.OrderByDescending(spec.OrderByDescending);

            return query;
        }
    }
}
