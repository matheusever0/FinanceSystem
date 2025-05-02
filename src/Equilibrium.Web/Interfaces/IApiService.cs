using System.Security.Claims;

namespace Equilibrium.Web.Interfaces
{
    public interface IApiService
    {
        Task<bool> VerifyTokenAsync(string token);
        Task<T> GetAsync<T>(string endpoint, string token);
        Task<T> PostAsync<T>(string endpoint, object? data = null, string token = "");
        Task<T> PutAsync<T>(string endpoint, object? data = null, string token = "");
        Task DeleteAsync(string endpoint, string token);
        Task<T> DeleteAsync<T>(string endpoint, string token);
        Task<ClaimsPrincipal> GetClaimsPrincipalFromToken(string token);
    }
}