using Equilibrium.Web.Models.IncomeType;

namespace Equilibrium.Web.Services
{
    public interface IIncomeTypeService
    {
        Task<IEnumerable<IncomeTypeModel>> GetAllIncomeTypesAsync(string token);
        Task<IEnumerable<IncomeTypeModel>> GetSystemIncomeTypesAsync(string token);
        Task<IEnumerable<IncomeTypeModel>> GetUserIncomeTypesAsync(string token);
        Task<IncomeTypeModel> GetIncomeTypeByIdAsync(string id, string token);
        Task<IncomeTypeModel> CreateIncomeTypeAsync(CreateIncomeTypeModel model, string token);
        Task<IncomeTypeModel> UpdateIncomeTypeAsync(string id, UpdateIncomeTypeModel model, string token);
        Task DeleteIncomeTypeAsync(string id, string token);
    }
}