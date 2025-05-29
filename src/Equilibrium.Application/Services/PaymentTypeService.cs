using AutoMapper;
using Equilibrium.Application.DTOs.PaymentType;
using Equilibrium.Application.Interfaces;
using Equilibrium.Domain.Interfaces.Services;
using Equilibrium.Resources;

namespace Equilibrium.Application.Services
{
    public class PaymentTypeService : IPaymentTypeService
    {
        private readonly IUnitOfWork _unitOfWork;
        private readonly IMapper _mapper;

        public PaymentTypeService(IUnitOfWork unitOfWork, IMapper mapper)
        {
            _unitOfWork = unitOfWork;
            _mapper = mapper;
        }

        public async Task<PaymentTypeDto> GetByIdAsync(Guid id)
        {
            var paymentType = await _unitOfWork.PaymentTypes.GetByIdAsync(id);
            return paymentType == null
                ? throw new KeyNotFoundException(ResourceFinanceApi.PaymentType_NotFound)
                : _mapper.Map<PaymentTypeDto>(paymentType);
        }

        public async Task<IEnumerable<PaymentTypeDto>> GetAllSystemTypesAsync()
        {
            var paymentTypes = await _unitOfWork.PaymentTypes.GetAllSystemTypesAsync();
            return _mapper.Map<IEnumerable<PaymentTypeDto>>(paymentTypes);
        }

        public async Task<IEnumerable<PaymentTypeDto>> GetUserTypesAsync(Guid userId)
        {
            var paymentTypes = await _unitOfWork.PaymentTypes.GetUserTypesAsync(userId);
            return _mapper.Map<IEnumerable<PaymentTypeDto>>(paymentTypes);
        }

        public async Task<IEnumerable<PaymentTypeDto>> GetAllAvailableForUserAsync(Guid userId)
        {
            var paymentTypes = await _unitOfWork.PaymentTypes.GetAllAvailableForUserAsync(userId);
            return _mapper.Map<IEnumerable<PaymentTypeDto>>(paymentTypes);
        }

        public async Task<PaymentTypeDto> CreateAsync(CreatePaymentTypeDto createPaymentTypeDto, Guid userId)
        {
            var user = await _unitOfWork.Users.GetByIdAsync(userId) ?? throw new KeyNotFoundException(ResourceFinanceApi.User_NotFound);
            var existingType = await _unitOfWork.PaymentTypes.GetByNameAsync(createPaymentTypeDto.Name);
            if (existingType != null && (existingType.IsSystem || existingType.UserId == userId))
                throw new InvalidOperationException(ResourceFinanceApi.PaymentType_NameExists);

            var paymentType = new PaymentType(
                createPaymentTypeDto.Name,
                createPaymentTypeDto.Description,
                user
            );

            await _unitOfWork.PaymentTypes.AddAsync(paymentType);
            await _unitOfWork.CompleteAsync();

            return _mapper.Map<PaymentTypeDto>(paymentType);
        }

        public async Task<PaymentTypeDto> UpdateAsync(Guid id, UpdatePaymentTypeDto updatePaymentTypeDto)
        {
            var paymentType = await _unitOfWork.PaymentTypes.GetByIdAsync(id) ?? throw new KeyNotFoundException(ResourceFinanceApi.PaymentType_NotFound);
            if (paymentType.IsSystem)
                throw new InvalidOperationException(ResourceFinanceApi.PaymentType_SystemCannotUpdate);

            if (!string.IsNullOrEmpty(updatePaymentTypeDto.Name) && updatePaymentTypeDto.Name != paymentType.Name)
            {
                var existingType = await _unitOfWork.PaymentTypes.GetByNameAsync(updatePaymentTypeDto.Name);
                if (existingType != null && existingType.Id != id)
                    throw new InvalidOperationException(ResourceFinanceApi.PaymentType_NameExists);

                paymentType.UpdateName(updatePaymentTypeDto.Name);
            }

            if (updatePaymentTypeDto.Description != null)
                paymentType.UpdateDescription(updatePaymentTypeDto.Description);

            await _unitOfWork.PaymentTypes.UpdateAsync(paymentType);
            await _unitOfWork.CompleteAsync();

            return _mapper.Map<PaymentTypeDto>(paymentType);
        }

        public async Task DeleteAsync(Guid id)
        {
            var paymentType = await _unitOfWork.PaymentTypes.GetByIdAsync(id) ?? throw new KeyNotFoundException(ResourceFinanceApi.PaymentType_NotFound);
            if (paymentType.IsSystem)
                throw new InvalidOperationException(ResourceFinanceApi.PaymentType_SystemCannotDelete);

            var payments = await _unitOfWork.Payments.GetPaymentsByTypeAsync(paymentType.UserId!.Value, id);
            if (payments.Any())
                throw new InvalidOperationException(ResourceFinanceApi.PaymentType_InUse);

            await _unitOfWork.PaymentTypes.DeleteAsync(paymentType);
            await _unitOfWork.CompleteAsync();
        }
    }
}

