using AutoMapper;
using Equilibrium.Application.DTOs.CreditCard;
using Equilibrium.Application.Interfaces;
using Equilibrium.Domain.Entities;
using Equilibrium.Domain.Enums;
using Equilibrium.Domain.Specifications;
using Equilibrium.Application.DTOs.Common;
using Equilibrium.Domain.DTOs.Filters;
using Equilibrium.Domain.Interfaces.Services;
using Equilibrium.Resources;

namespace Equilibrium.Application.Services
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
            return creditCard == null
                ? throw new KeyNotFoundException(ResourceFinanceApi.CreditCard_NotFound)
                : _mapper.Map<CreditCardDto>(creditCard);
        }

        public async Task<IEnumerable<CreditCardDto>> GetByUserIdAsync(Guid userId)
        {
            var creditCards = await _unitOfWork.CreditCards.GetCreditCardsByUserIdAsync(userId);
            return _mapper.Map<IEnumerable<CreditCardDto>>(creditCards);
        }

        public async Task<CreditCardDto> CreateAsync(CreateCreditCardDto createCreditCardDto, Guid userId)
        {
            var user = await _unitOfWork.Users.GetByIdAsync(userId) ?? throw new KeyNotFoundException(ResourceFinanceApi.User_NotFound);
            var paymentMethod = await _unitOfWork.PaymentMethods.GetByIdAsync(createCreditCardDto.PaymentMethodId) ?? throw new KeyNotFoundException(ResourceFinanceApi.PaymentMethod_NotFound);
            if (paymentMethod.Type != PaymentMethodType.CreditCard)
                throw new InvalidOperationException(ResourceFinanceApi.CreditCard_InvalidMethodType);

            if (!paymentMethod.IsSystem && paymentMethod.UserId != userId)
                throw new UnauthorizedAccessException(ResourceFinanceApi.Error_Unauthorized);

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
            var creditCard = await _unitOfWork.CreditCards.GetByIdAsync(id) ?? throw new KeyNotFoundException(ResourceFinanceApi.CreditCard_NotFound);
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
            var creditCard = await _unitOfWork.CreditCards.GetByIdAsync(id) ?? throw new KeyNotFoundException(ResourceFinanceApi.CreditCard_NotFound);
            await _unitOfWork.CreditCards.DeleteAsync(creditCard);
            await _unitOfWork.CompleteAsync();
        }

        public async Task<PagedResult<CreditCardDto>> GetFilteredAsync(CreditCardFilter filter, Guid userId)
        {
            var specification = new CreditCardSpecification(filter)
            {
                UserId = userId
            };

            var (creditCards, totalCount) = await _unitOfWork.CreditCards.FindWithSpecificationAsync(specification);

            return new PagedResult<CreditCardDto>
            {
                Items = _mapper.Map<IEnumerable<CreditCardDto>>(creditCards),
                TotalCount = totalCount,
                PageNumber = filter.PageNumber,
                PageSize = filter.PageSize
            };
        }
    }
}

