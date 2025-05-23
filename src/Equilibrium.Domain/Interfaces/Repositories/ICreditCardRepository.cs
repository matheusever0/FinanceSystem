﻿using Equilibrium.Domain.Entities;

namespace Equilibrium.Domain.Interfaces.Repositories
{
    public interface ICreditCardRepository : IRepositoryBase<CreditCard>
    {
        Task<IEnumerable<CreditCard?>> GetCreditCardsByUserIdAsync(Guid userId);
        Task<CreditCard?> GetCreditCardWithDetailsAsync(Guid creditCardId);
    }
}
