﻿using FinanceSystem.Domain.Entities;
using FinanceSystem.Domain.Interfaces.Repositories;
using FinanceSystem.Infrastructure.Data;
using Microsoft.EntityFrameworkCore;

namespace FinanceSystem.Infrastructure.Repositories
{
    public class UserRepository : RepositoryBase<User>, IUserRepository
    {
        public UserRepository(ApplicationDbContext context) : base(context)
        {
        }

        public async Task<User?> GetByUsernameAsync(string username)
        {
            var user = await _dbSet
                .Include(e => e.UserRoles)
                .ThenInclude(e => e.Role)
                .Where(e => e.Username == username)
                .FirstOrDefaultAsync();


            return user;
        }

        public async Task<User?> GetByEmailAsync(string email)
        {
            return await _dbSet
                .FirstOrDefaultAsync(u => u.Email.ToLower() == email.ToLower());
        }

        public async Task<User?> GetUserWithRolesAsync(Guid id)
        {
            return await _dbSet
                .Include(u => u.UserRoles)
                .ThenInclude(ur => ur.Role)
                .FirstOrDefaultAsync(u => u.Id == id);
        }

        public async Task<IEnumerable<User?>> GetUsersWithRolesAsync()
        {
            return await _dbSet
                .Include(u => u.UserRoles)
                .ThenInclude(ur => ur.Role)
                .ToListAsync();
        }
    }
}