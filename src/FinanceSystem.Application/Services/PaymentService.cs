using AutoMapper;
using FinanceSystem.Application.DTOs.Payment;
using FinanceSystem.Application.Interfaces;
using FinanceSystem.Domain.Enums;
using FinanceSystem.Domain.Interfaces.Services;

namespace FinanceSystem.Application.Services
{
    public class PaymentService : IPaymentService
    {
        private readonly IUnitOfWork _unitOfWork;
        private readonly IMapper _mapper;

        public PaymentService(IUnitOfWork unitOfWork, IMapper mapper)
        {
            _unitOfWork = unitOfWork;
            _mapper = mapper;
        }

        public async Task<PaymentDto> GetByIdAsync(Guid id)
        {
            var payment = await _unitOfWork.Payments.GetPaymentWithDetailsAsync(id);
            if (payment == null)
                throw new KeyNotFoundException($"Payment with ID {id} not found");

            return _mapper.Map<PaymentDto>(payment);
        }

        public async Task<IEnumerable<PaymentDto>> GetAllByUserIdAsync(Guid userId)
        {
            var payments = await _unitOfWork.Payments.GetPaymentsByUserIdAsync(userId);
            return _mapper.Map<IEnumerable<PaymentDto>>(payments);
        }

        public async Task<IEnumerable<PaymentDto>> GetByMonthAsync(Guid userId, int month, int year)
        {
            var payments = await _unitOfWork.Payments.GetPaymentsByUserIdAndMonthAsync(userId, month, year);
            return _mapper.Map<IEnumerable<PaymentDto>>(payments);
        }

        public async Task<IEnumerable<PaymentDto>> GetPendingAsync(Guid userId)
        {
            var payments = await _unitOfWork.Payments.GetPendingPaymentsByUserIdAsync(userId);
            return _mapper.Map<IEnumerable<PaymentDto>>(payments);
        }

        public async Task<IEnumerable<PaymentDto>> GetOverdueAsync(Guid userId)
        {
            var payments = await _unitOfWork.Payments.GetOverduePaymentsByUserIdAsync(userId);
            return _mapper.Map<IEnumerable<PaymentDto>>(payments);
        }

        public async Task<IEnumerable<PaymentDto>> GetByTypeAsync(Guid userId, Guid paymentTypeId)
        {
            var payments = await _unitOfWork.Payments.GetPaymentsByTypeAsync(userId, paymentTypeId);
            return _mapper.Map<IEnumerable<PaymentDto>>(payments);
        }

        public async Task<IEnumerable<PaymentDto>> GetByMethodAsync(Guid userId, Guid paymentMethodId)
        {
            var payments = await _unitOfWork.Payments.GetPaymentsByMethodAsync(userId, paymentMethodId);
            return _mapper.Map<IEnumerable<PaymentDto>>(payments);
        }

        public async Task<PaymentDto> CreateAsync(CreatePaymentDto createPaymentDto, Guid userId)
        {
            var user = await _unitOfWork.Users.GetByIdAsync(userId);
            if (user == null)
                throw new KeyNotFoundException($"User with ID {userId} not found");

            var paymentType = await _unitOfWork.PaymentTypes.GetByIdAsync(createPaymentDto.PaymentTypeId);
            if (paymentType == null)
                throw new KeyNotFoundException($"Payment type with ID {createPaymentDto.PaymentTypeId} not found");

            if (!paymentType.IsSystem && paymentType.UserId != userId)
                throw new UnauthorizedAccessException("User does not have access to this payment type");

            var paymentMethod = await _unitOfWork.PaymentMethods.GetByIdAsync(createPaymentDto.PaymentMethodId);
            if (paymentMethod == null)
                throw new KeyNotFoundException($"Payment method with ID {createPaymentDto.PaymentMethodId} not found");

            if (!paymentMethod.IsSystem && paymentMethod.UserId != userId)
                throw new UnauthorizedAccessException("User does not have access to this payment method");

            if (paymentMethod.Type == PaymentMethodType.CreditCard && !createPaymentDto.CreditCardId.HasValue)
                throw new InvalidOperationException("Credit card ID is required for credit card payments");

            if (createPaymentDto.CreditCardId.HasValue)
            {
                var creditCard = await _unitOfWork.CreditCards.GetByIdAsync(createPaymentDto.CreditCardId.Value);
                if (creditCard == null)
                    throw new KeyNotFoundException($"Credit card with ID {createPaymentDto.CreditCardId.Value} not found");

                if (creditCard.UserId != userId)
                    throw new UnauthorizedAccessException("User does not have access to this credit card");

                if (creditCard.AvailableLimit < createPaymentDto.Amount)
                    throw new InvalidOperationException("Insufficient credit card limit");

                creditCard.DecrementAvailableLimit(createPaymentDto.Amount);
                await _unitOfWork.CreditCards.UpdateAsync(creditCard);
            }

            var payment = new Payment(
                createPaymentDto.Description,
                createPaymentDto.Amount,
                createPaymentDto.DueDate,
                paymentType,
                paymentMethod,
                user,
                createPaymentDto.IsRecurring,
                createPaymentDto.Notes
            );

            if (createPaymentDto.PaymentDate.HasValue)
            {
                payment.MarkAsPaid(createPaymentDto.PaymentDate.Value);
            }

            if (createPaymentDto.NumberOfInstallments > 1)
            {
                payment.AddInstallments(createPaymentDto.NumberOfInstallments);
            }

            await _unitOfWork.Payments.AddAsync(payment);
            await _unitOfWork.CompleteAsync();

            return _mapper.Map<PaymentDto>(payment);
        }

        public async Task<PaymentDto> UpdateAsync(Guid id, UpdatePaymentDto updatePaymentDto)
        {
            var payment = await _unitOfWork.Payments.GetPaymentWithDetailsAsync(id);
            if (payment == null)
                throw new KeyNotFoundException($"Payment with ID {id} not found");

            if (!string.IsNullOrEmpty(updatePaymentDto.Description))
                payment.UpdateDescription(updatePaymentDto.Description);

            if (updatePaymentDto.Amount > 0)
                payment.UpdateAmount(updatePaymentDto.Amount);

            if (updatePaymentDto.DueDate.HasValue)
                payment.UpdateDueDate(updatePaymentDto.DueDate.Value);

            if (updatePaymentDto.Notes != null)
                payment.UpdateNotes(updatePaymentDto.Notes);

            if (updatePaymentDto.Status.HasValue)
            {
                switch (updatePaymentDto.Status.Value)
                {
                    case PaymentStatus.Paid:
                        if (updatePaymentDto.PaymentDate.HasValue)
                            payment.MarkAsPaid(updatePaymentDto.PaymentDate.Value);
                        else
                            payment.MarkAsPaid(DateTime.UtcNow);
                        break;
                    case PaymentStatus.Overdue:
                        payment.MarkAsOverdue();
                        break;
                    case PaymentStatus.Cancelled:
                        payment.Cancel();
                        break;
                }
            }

            if (updatePaymentDto.PaymentTypeId.HasValue)
            {
                var paymentType = await _unitOfWork.PaymentTypes.GetByIdAsync(updatePaymentDto.PaymentTypeId.Value);
                if (paymentType == null)
                    throw new KeyNotFoundException($"Payment type with ID {updatePaymentDto.PaymentTypeId.Value} not found");

                if (!paymentType.IsSystem && paymentType.UserId != payment.UserId)
                    throw new UnauthorizedAccessException("User does not have access to this payment type");

                payment.UpdateType(paymentType);

            }

            if (updatePaymentDto.PaymentMethodId.HasValue)
            {
                var paymentMethod = await _unitOfWork.PaymentMethods.GetByIdAsync(updatePaymentDto.PaymentMethodId.Value);
                if (paymentMethod == null)
                    throw new KeyNotFoundException($"Payment method with ID {updatePaymentDto.PaymentMethodId.Value} not found");

                if (!paymentMethod.IsSystem && paymentMethod.UserId != payment.UserId)
                    throw new UnauthorizedAccessException("User does not have access to this payment method");

                payment.UpdateMethod(paymentMethod);

            }

            if (updatePaymentDto.IsRecurring.HasValue)
            {
                payment.UpdateRecurring(updatePaymentDto.IsRecurring.Value);
            }

            await _unitOfWork.Payments.UpdateAsync(payment);
            await _unitOfWork.CompleteAsync();

            return _mapper.Map<PaymentDto>(payment);
        }

        public async Task DeleteAsync(Guid id)
        {
            var payment = await _unitOfWork.Payments.GetByIdAsync(id);
            if (payment == null)
                throw new KeyNotFoundException($"Payment with ID {id} not found");

            if (payment.Status == PaymentStatus.Paid)
                throw new InvalidOperationException("Cannot delete a paid payment");

            await _unitOfWork.Payments.DeleteAsync(payment);
            await _unitOfWork.CompleteAsync();
        }

        public async Task<PaymentDto> MarkAsPaidAsync(Guid id, DateTime paymentDate)
        {
            var payment = await _unitOfWork.Payments.GetPaymentWithDetailsAsync(id);
            if (payment == null)
                throw new KeyNotFoundException($"Payment with ID {id} not found");

            payment.MarkAsPaid(paymentDate);
            await _unitOfWork.Payments.UpdateAsync(payment);
            await _unitOfWork.CompleteAsync();

            return _mapper.Map<PaymentDto>(payment);
        }

        public async Task<PaymentDto> MarkAsOverdueAsync(Guid id)
        {
            var payment = await _unitOfWork.Payments.GetPaymentWithDetailsAsync(id);
            if (payment == null)
                throw new KeyNotFoundException($"Payment with ID {id} not found");

            payment.MarkAsOverdue();
            await _unitOfWork.Payments.UpdateAsync(payment);
            await _unitOfWork.CompleteAsync();

            return _mapper.Map<PaymentDto>(payment);
        }

        public async Task<PaymentDto> CancelAsync(Guid id)
        {
            var payment = await _unitOfWork.Payments.GetPaymentWithDetailsAsync(id);
            if (payment == null)
                throw new KeyNotFoundException($"Payment with ID {id} not found");

            payment.Cancel();
            await _unitOfWork.Payments.UpdateAsync(payment);
            await _unitOfWork.CompleteAsync();

            return _mapper.Map<PaymentDto>(payment);
        }
    }
}
