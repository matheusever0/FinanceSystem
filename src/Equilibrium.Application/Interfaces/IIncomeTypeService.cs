using Equilibrium.Application.DTOs.IncomeType;

namespace Equilibrium.Application.Interfaces
{
    public interface IIncomeTypeService
    {
        Task<IncomeTypeDto> GetByIdAsync(Guid id);
        Task<IEnumerable<IncomeTypeDto>> GetAllSystemTypesAsync();
        Task<IEnumerable<IncomeTypeDto>> GetAllAvailableForUserAsync(Guid userId);
        Task<IEnumerable<IncomeTypeDto>> GetUserTypesAsync(Guid userId);
        Task<IncomeTypeDto> CreateAsync(CreateIncomeTypeDto createIncomeTypeDto, Guid userId);
        Task<IncomeTypeDto> UpdateAsync(Guid id, UpdateIncomeTypeDto updateIncomeTypeDto);
        Task DeleteAsync(Guid id);
    }
}
