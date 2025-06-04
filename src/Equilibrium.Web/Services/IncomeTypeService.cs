using Equilibrium.Web.Interfaces;
using Equilibrium.Web.Models.IncomeType;

namespace Equilibrium.Web.Services
{
    public class IncomeTypeService : IIncomeTypeService
    {
        private readonly IApiService _apiService;
        private readonly ILogger<IncomeTypeService> _logger;

        public IncomeTypeService(IApiService apiService, ILogger<IncomeTypeService> logger)
        {
            _apiService = apiService;
            _logger = logger;
        }

        public async Task<IEnumerable<IncomeTypeModel>> GetAllIncomeTypesAsync(string token)
        {
            try
            {
                return await _apiService.GetAsync<IEnumerable<IncomeTypeModel>>("/api/IncomeTypes", token);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Erro ao obter tipos de receita");
                throw;
            }
        }

        public async Task<IEnumerable<IncomeTypeModel>> GetSystemIncomeTypesAsync(string token)
        {
            try
            {
                return await _apiService.GetAsync<IEnumerable<IncomeTypeModel>>("/api/IncomeTypes/system", token);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Erro ao obter tipos de receita do sistema");
                throw;
            }
        }

        public async Task<IEnumerable<IncomeTypeModel>> GetUserIncomeTypesAsync(string token)
        {
            try
            {
                return await _apiService.GetAsync<IEnumerable<IncomeTypeModel>>("/api/IncomeTypes/user", token);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Erro ao obter tipos de receita do usuário");
                throw;
            }
        }

        public async Task<IncomeTypeModel> GetIncomeTypeByIdAsync(string id, string token)
        {
            try
            {
                return await _apiService.GetAsync<IncomeTypeModel>($"/api/IncomeTypes/{id}", token);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Erro ao obter tipo de receita com ID: {TypeId}", id);
                throw;
            }
        }

        public async Task<IncomeTypeModel> CreateIncomeTypeAsync(CreateIncomeTypeModel model, string token)
        {
            try
            {
                return await _apiService.PostAsync<IncomeTypeModel>("/api/IncomeTypes", model, token);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Erro ao criar tipo de receita: {Name}", model.Name);
                throw;
            }
        }

        public async Task<IncomeTypeModel> UpdateIncomeTypeAsync(string id, UpdateIncomeTypeModel model, string token)
        {
            try
            {
                return await _apiService.PutAsync<IncomeTypeModel>($"/api/IncomeTypes/{id}", model, token);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Erro ao atualizar tipo de receita com ID: {TypeId}", id);
                throw;
            }
        }

        public async Task DeleteIncomeTypeAsync(string id, string token)
        {
            try
            {
                await _apiService.DeleteAsync($"/api/IncomeTypes/{id}", token);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Erro ao excluir tipo de receita com ID: {TypeId}", id);
                throw;
            }
        }
    }
}
