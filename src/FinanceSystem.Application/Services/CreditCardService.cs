using AutoMapper;
using FinanceSystem.Application.DTOs.CreditCard;
using FinanceSystem.Application.Interfaces;
using FinanceSystem.Domain.Enums;
using FinanceSystem.Domain.Interfaces.Services;

namespace FinanceSystem.Application.Services
{
    public class CreditCardService : ICreditCardService
    {
        private readonly IUnitOfWork _unitOfWork;
        private readonly IMapper _mapper;

        public CreditCardService(IUnitOfWork unitOfWork, IMapper mapper)
        {
            _unitOfWork = unitOfWork;
            _mapper = mapper;
        }

        public async Task<CreditCardDto> GetByIdAsync(Guid id)
        {
            var creditCard = await _unitOfWork.CreditCards.GetCreditCardWithDetailsAsync(id);
            if (creditCard == null)
                throw new KeyNotFoundException($"Credit card with ID {id} not found");

            return _mapper.Map<CreditCardDto>(creditCard);
        }

        public async Task<IEnumerable<CreditCardDto>> GetByUserIdAsync(Guid userId)
        {
            var creditCards = await _unitOfWork.CreditCards.GetCreditCardsByUserIdAsync(userId);
            return _mapper.Map<IEnumerable<CreditCardDto>>(creditCards);
        }

        public async Task<CreditCardDto> CreateAsync(CreateCreditCardDto createCreditCardDto, Guid userId)
        {
            var user = await _unitOfWork.Users.GetByIdAsync(userId);
            if (user == null)
                throw new KeyNotFoundException($"User with ID {userId} not found");

            var paymentMethod = await _unitOfWork.PaymentMethods.GetByIdAsync(createCreditCardDto.PaymentMethodId);
            if (paymentMethod == null)
                throw new KeyNotFoundException($"Payment method with ID {createCreditCardDto.PaymentMethodId} not found");

            if (paymentMethod.Type != PaymentMethodType.CreditCard)
                throw new InvalidOperationException("Payment method must be of type Credit Card");

            if (!paymentMethod.IsSystem && paymentMethod.UserId != userId)
                throw new UnauthorizedAccessException("User does not have access to this payment method");

            var creditCard = new CreditCard(
                createCreditCardDto.Name,
                createCreditCardDto.LastFourDigits,
                createCreditCardDto.CardBrand,
                createCreditCardDto.ClosingDay,
                createCreditCardDto.DueDay,
                createCreditCardDto.Limit,
                user,
                paymentMethod
            );

            await _unitOfWork.CreditCards.AddAsync(creditCard);
            await _unitOfWork.CompleteAsync();

            return _mapper.Map<CreditCardDto>(creditCard);
        }

        public async Task<CreditCardDto> UpdateAsync(Guid id, UpdateCreditCardDto updateCreditCardDto)
        {
            var creditCard = await _unitOfWork.CreditCards.GetByIdAsync(id);
            if (creditCard == null)
                throw new KeyNotFoundException($"Credit card with ID {id} not found");

            if (!string.IsNullOrEmpty(updateCreditCardDto.Name))
                creditCard.UpdateName(updateCreditCardDto.Name);

            if (updateCreditCardDto.ClosingDay.HasValue && updateCreditCardDto.DueDay.HasValue)
                creditCard.UpdateDays(updateCreditCardDto.ClosingDay.Value, updateCreditCardDto.DueDay.Value);

            if (updateCreditCardDto.Limit.HasValue)
                creditCard.UpdateLimit(updateCreditCardDto.Limit.Value);

            await _unitOfWork.CreditCards.UpdateAsync(creditCard);
            await _unitOfWork.CompleteAsync();

            return _mapper.Map<CreditCardDto>(creditCard);
        }

        public async Task DeleteAsync(Guid id)
        {
            var creditCard = await _unitOfWork.CreditCards.GetByIdAsync(id);
            if (creditCard == null)
                throw new KeyNotFoundException($"Credit card with ID {id} not found");

            await _unitOfWork.CreditCards.DeleteAsync(creditCard);
            await _unitOfWork.CompleteAsync();
        }
    }
}
