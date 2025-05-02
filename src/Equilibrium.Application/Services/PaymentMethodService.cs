using AutoMapper;
using Equilibrium.Application.DTOs.PaymentMethod;
using Equilibrium.Application.Interfaces;
using Equilibrium.Domain.Enums;
using Equilibrium.Domain.Interfaces.Services;
using Equilibrium.Resources;

namespace Equilibrium.Application.Services
{
    public class PaymentMethodService : IPaymentMethodService
    {
        private readonly IUnitOfWork _unitOfWork;
        private readonly IMapper _mapper;

        public PaymentMethodService(IUnitOfWork unitOfWork, IMapper mapper)
        {
            _unitOfWork = unitOfWork;
            _mapper = mapper;
        }

        public async Task<PaymentMethodDto> GetByIdAsync(Guid id)
        {
            var paymentMethod = await _unitOfWork.PaymentMethods.GetByIdAsync(id);
            return paymentMethod == null
                ? throw new KeyNotFoundException(ResourceFinanceApi.PaymentMethod_NotFound)
                : _mapper.Map<PaymentMethodDto>(paymentMethod);
        }

        public async Task<IEnumerable<PaymentMethodDto>> GetAllSystemMethodsAsync()
        {
            var paymentMethods = await _unitOfWork.PaymentMethods.GetAllSystemMethodsAsync();
            return _mapper.Map<IEnumerable<PaymentMethodDto>>(paymentMethods);
        }

        public async Task<IEnumerable<PaymentMethodDto>> GetUserMethodsAsync(Guid userId)
        {
            var paymentMethods = await _unitOfWork.PaymentMethods.GetUserMethodsAsync(userId);
            return _mapper.Map<IEnumerable<PaymentMethodDto>>(paymentMethods);
        }

        public async Task<IEnumerable<PaymentMethodDto>> GetAllAvailableForUserAsync(Guid userId)
        {
            var paymentMethods = await _unitOfWork.PaymentMethods.GetAllAvailableForUserAsync(userId);
            return _mapper.Map<IEnumerable<PaymentMethodDto>>(paymentMethods);
        }

        public async Task<IEnumerable<PaymentMethodDto>> GetByTypeAsync(PaymentMethodType type)
        {
            var paymentMethods = await _unitOfWork.PaymentMethods.GetByTypeAsync(type);
            return _mapper.Map<IEnumerable<PaymentMethodDto>>(paymentMethods);
        }

        public async Task<PaymentMethodDto> CreateAsync(CreatePaymentMethodDto createPaymentMethodDto, Guid userId)
        {
            var user = await _unitOfWork.Users.GetByIdAsync(userId) ?? throw new KeyNotFoundException($"User with ID {userId} not found");
            var existingMethod = await _unitOfWork.PaymentMethods.GetByNameAsync(createPaymentMethodDto.Name);
            if (existingMethod != null && (existingMethod.IsSystem || existingMethod.UserId == userId))
                throw new InvalidOperationException(ResourceFinanceApi.PaymentMethod_NameExists);

            var paymentMethod = new PaymentMethod(
                createPaymentMethodDto.Name,
                createPaymentMethodDto.Description,
                createPaymentMethodDto.Type,
                user
            );

            await _unitOfWork.PaymentMethods.AddAsync(paymentMethod);
            await _unitOfWork.CompleteAsync();

            return _mapper.Map<PaymentMethodDto>(paymentMethod);
        }

        public async Task<PaymentMethodDto> UpdateAsync(Guid id, UpdatePaymentMethodDto updatePaymentMethodDto)
        {
            var paymentMethod = await _unitOfWork.PaymentMethods.GetByIdAsync(id) ?? throw new KeyNotFoundException(ResourceFinanceApi.PaymentMethod_NotFound);
            if (paymentMethod.IsSystem)
                throw new InvalidOperationException(ResourceFinanceApi.PaymentMethod_SystemCannotUpdate);

            if (!string.IsNullOrEmpty(updatePaymentMethodDto.Name) && updatePaymentMethodDto.Name != paymentMethod.Name)
            {
                var existingMethod = await _unitOfWork.PaymentMethods.GetByNameAsync(updatePaymentMethodDto.Name);
                if (existingMethod != null && existingMethod.Id != id)
                    throw new InvalidOperationException(ResourceFinanceApi.PaymentMethod_NameExists);

                paymentMethod.UpdateName(updatePaymentMethodDto.Name);
            }

            if (updatePaymentMethodDto.Description != null)
                paymentMethod.UpdateDescription(updatePaymentMethodDto.Description);

            await _unitOfWork.PaymentMethods.UpdateAsync(paymentMethod);
            await _unitOfWork.CompleteAsync();

            return _mapper.Map<PaymentMethodDto>(paymentMethod);
        }

        public async Task DeleteAsync(Guid id)
        {
            var paymentMethod = await _unitOfWork.PaymentMethods.GetByIdAsync(id) ?? throw new KeyNotFoundException(ResourceFinanceApi.PaymentMethod_NotFound);
            if (paymentMethod.IsSystem)
                throw new InvalidOperationException(ResourceFinanceApi.PaymentMethod_SystemCannotDelete);

            var payments = await _unitOfWork.Payments.GetPaymentsByMethodAsync(paymentMethod!.UserId!.Value, id);
            if (payments.Any())
                throw new InvalidOperationException(ResourceFinanceApi.PaymentMethod_InUse);

            if (paymentMethod.Type == PaymentMethodType.CreditCard)
            {
                var creditCards = await _unitOfWork.CreditCards.GetCreditCardsByUserIdAsync(paymentMethod.UserId.Value);
                if (creditCards.Any(cc => cc.PaymentMethodId == id))
                    throw new InvalidOperationException(ResourceFinanceApi.PaymentMethod_HasCreditCards);
            }

            await _unitOfWork.PaymentMethods.DeleteAsync(paymentMethod);
            await _unitOfWork.CompleteAsync();
        }
    }
}
