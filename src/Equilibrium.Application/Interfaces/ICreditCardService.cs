using Equilibrium.Application.DTOs.CreditCard;

namespace Equilibrium.Application.Interfaces
{
    public interface ICreditCardService
    {
        Task<CreditCardDto> GetByIdAsync(Guid id);
        Task<IEnumerable<CreditCardDto>> GetByUserIdAsync(Guid userId);
        Task<CreditCardDto> CreateAsync(CreateCreditCardDto createCreditCardDto, Guid userId);
        Task<CreditCardDto> UpdateAsync(Guid id, UpdateCreditCardDto updateCreditCardDto);
        Task DeleteAsync(Guid id);
    }
}

