using Equilibrium.Domain.Entities;

namespace Equilibrium.Domain.Interfaces.Services
{
    public interface IAuthService
    {
        Task<string> GenerateJwtToken(User user);
        string HashPassword(string password);
        bool VerifyPassword(string password, string passwordHash);
    }
}