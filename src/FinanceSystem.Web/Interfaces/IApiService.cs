using System.Security.Claims;

namespace FinanceSystem.Web.Interfaces
{
    public interface IApiService
    {
        Task<bool> VerifyTokenAsync(string token);
        Task<T> GetAsync<T>(string endpoint, string token = null);
        Task<T> PostAsync<T>(string endpoint, object data, string token = null);
        Task<T> PutAsync<T>(string endpoint, object data, string token = null);
        Task DeleteAsync(string endpoint, string token = null);
        Task<ClaimsPrincipal> GetClaimsPrincipalFromToken(string token);
    }
}