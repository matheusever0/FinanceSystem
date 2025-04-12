using FinanceSystem.Web.Interfaces;
using FinanceSystem.Web.Models;

namespace FinanceSystem.Web.Services
{
    public class UserService : IUserService
    {
        private readonly IApiService _apiService;
        private readonly ILogger<UserService> _logger;

        public UserService(IApiService apiService, ILogger<UserService> logger)
        {
            _apiService = apiService;
            _logger = logger;
        }

        public async Task<LoginResponseModel> LoginAsync(LoginModel model)
        {
            try
            {
                return await _apiService.PostAsync<LoginResponseModel>("/api/auth/login", model);
            }
            catch (HttpRequestException ex)
            {
                // Registre informações detalhadas sobre a exceção
                _logger.LogError(ex, "Erro de comunicação com a API durante o login");
                throw new Exception("Não foi possível conectar ao servidor. Verifique sua conexão.", ex);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Erro geral durante o login");
                throw;
            }
        }

        public async Task<IEnumerable<UserModel>> GetAllUsersAsync(string token)
        {
            return await _apiService.GetAsync<IEnumerable<UserModel>>("/api/users", token);
        }

        public async Task<UserModel> GetUserByIdAsync(string id, string token)
        {
            return await _apiService.GetAsync<UserModel>($"/api/users/{id}", token);
        }

        public async Task<UserModel> CreateUserAsync(CreateUserModel model, string token)
        {
            return await _apiService.PostAsync<UserModel>("/api/users", model, token);
        }

        public async Task<UserModel> UpdateUserAsync(string id, UpdateUserModel model, string token)
        {
            return await _apiService.PutAsync<UserModel>($"/api/users/{id}", model, token);
        }

        public async Task DeleteUserAsync(string id, string token)
        {
            await _apiService.DeleteAsync($"/api/users/{id}", token);
        }
    }
}