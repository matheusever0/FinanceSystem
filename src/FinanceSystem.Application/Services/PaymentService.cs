using AutoMapper;
using FinanceSystem.Application.DTOs.Payment;
using FinanceSystem.Application.Interfaces;
using FinanceSystem.Domain.Entities;
using FinanceSystem.Domain.Enums;
using FinanceSystem.Domain.Interfaces.Services;
using FinanceSystem.Resources;
using Microsoft.Extensions.DependencyInjection;
using System;

namespace FinanceSystem.Application.Services
{
    public class PaymentService : IPaymentService
    {
        private readonly IUnitOfWork _unitOfWork;
        private readonly IMapper _mapper;
        private readonly IServiceProvider _serviceProvider;

        public PaymentService(
            IUnitOfWork unitOfWork,
            IMapper mapper,
            IServiceProvider serviceProvider)
        {
            _unitOfWork = unitOfWork;
            _mapper = mapper;
            _serviceProvider = serviceProvider;
        }

        public async Task<PaymentDto> GetByIdAsync(Guid id)
        {
            var payment = await _unitOfWork.Payments.GetPaymentWithDetailsAsync(id);
            return payment == null ? throw new KeyNotFoundException(ResourceFinanceApi.Payment_NotFound) : _mapper.Map<PaymentDto>(payment);
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
            var user = await _unitOfWork.Users.GetByIdAsync(userId) ?? throw new KeyNotFoundException(ResourceFinanceApi.User_NotFound);
            var paymentType = await _unitOfWork.PaymentTypes.GetByIdAsync(createPaymentDto.PaymentTypeId) ?? throw new KeyNotFoundException(ResourceFinanceApi.PaymentType_NotFound);
            if (!paymentType.IsSystem && paymentType.UserId != userId)
                throw new UnauthorizedAccessException(ResourceFinanceApi.Error_Unauthorized);

            var paymentMethod = await _unitOfWork.PaymentMethods.GetByIdAsync(createPaymentDto.PaymentMethodId) ?? throw new KeyNotFoundException(ResourceFinanceApi.PaymentMethod_NotFound);
            if (!paymentMethod.IsSystem && paymentMethod.UserId != userId)
                throw new UnauthorizedAccessException(ResourceFinanceApi.Error_Unauthorized);

            if (paymentMethod.Type == PaymentMethodType.CreditCard && !createPaymentDto.CreditCardId.HasValue)
                throw new InvalidOperationException(ResourceFinanceApi.Payment_CreditCardRequired);

            if (createPaymentDto.CreditCardId.HasValue)
            {
                var creditCard = await _unitOfWork.CreditCards.GetByIdAsync(createPaymentDto.CreditCardId.Value) ?? throw new KeyNotFoundException(ResourceFinanceApi.CreditCard_NotFound);
                if (creditCard.UserId != userId)
                    throw new UnauthorizedAccessException(ResourceFinanceApi.Error_Unauthorized);

                if (creditCard.AvailableLimit < createPaymentDto.Amount)
                    throw new InvalidOperationException(ResourceFinanceApi.Payment_InsufficientCreditLimit);

                creditCard.DecrementAvailableLimit(createPaymentDto.Amount);
                await _unitOfWork.CreditCards.UpdateAsync(creditCard);
            }

            Payment? payment = null;

            if (paymentType.IsFinancingType)
            {
                // Verificar se foi fornecido um financiamento
                if (!createPaymentDto.FinancingId.HasValue)
                    throw new InvalidOperationException("É necessário informar um financiamento para este tipo de pagamento");

                // Buscar o financiamento
                var financing = await _unitOfWork.Financings.GetByIdAsync(createPaymentDto.FinancingId.Value);
                if (financing == null)
                    throw new KeyNotFoundException("Financiamento não encontrado");

                if (financing.UserId != userId)
                    throw new UnauthorizedAccessException(ResourceFinanceApi.Error_Unauthorized);

                // Se informou uma parcela específica
                if (createPaymentDto.FinancingInstallmentId.HasValue)
                {
                    var installment = await _unitOfWork.FinancingInstallments.GetByIdAsync(createPaymentDto.FinancingInstallmentId.Value);
                    if (installment == null)
                        throw new KeyNotFoundException("Parcela de financiamento não encontrada");

                    if (installment.FinancingId != financing.Id)
                        throw new InvalidOperationException("A parcela não pertence ao financiamento informado");

                    // Criar um pagamento vinculado ao financiamento e parcela
                    payment = new Payment(
                        createPaymentDto.Description,
                        createPaymentDto.Amount,
                        createPaymentDto.DueDate,
                        paymentType,
                        paymentMethod,
                        user,
                        financing,
                        installment,
                        createPaymentDto.IsRecurring,
                        createPaymentDto.Notes
                    );
                }
                else
                {
                    // Criar um pagamento vinculado apenas ao financiamento
                    payment = new Payment(
                        createPaymentDto.Description,
                        createPaymentDto.Amount,
                        createPaymentDto.DueDate,
                        paymentType,
                        paymentMethod,
                        user,
                        financing,
                        null,  // Sem parcela específica
                        createPaymentDto.IsRecurring,
                        createPaymentDto.Notes
                    );
                }
            }
            else
            {
                payment = new Payment(
                        createPaymentDto.Description,
                        createPaymentDto.Amount,
                        createPaymentDto.DueDate,
                        paymentType,
                        paymentMethod,
                        user,
                        createPaymentDto.IsRecurring,
                        createPaymentDto.Notes
                    );
             }

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
            var payment = await _unitOfWork.Payments.GetPaymentWithDetailsAsync(id) ?? throw new KeyNotFoundException(ResourceFinanceApi.Payment_NotFound);
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
                            payment.MarkAsPaid(DateTime.Now);
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
                var paymentType = await _unitOfWork.PaymentTypes.GetByIdAsync(updatePaymentDto.PaymentTypeId.Value) ?? throw new KeyNotFoundException(ResourceFinanceApi.PaymentType_NotFound);
                if (!paymentType.IsSystem && paymentType.UserId != payment.UserId)
                    throw new UnauthorizedAccessException(ResourceFinanceApi.Error_Unauthorized);

                payment.UpdateType(paymentType);

            }

            if (updatePaymentDto.PaymentMethodId.HasValue)
            {
                var paymentMethod = await _unitOfWork.PaymentMethods.GetByIdAsync(updatePaymentDto.PaymentMethodId.Value) ?? throw new KeyNotFoundException(ResourceFinanceApi.PaymentMethod_NotFound);
                if (!paymentMethod.IsSystem && paymentMethod.UserId != payment.UserId)
                    throw new UnauthorizedAccessException(ResourceFinanceApi.Error_Unauthorized);

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
            var payment = await _unitOfWork.Payments.GetByIdAsync(id) ?? throw new KeyNotFoundException(ResourceFinanceApi.Payment_NotFound);
            if (payment.Status == PaymentStatus.Paid)
                throw new InvalidOperationException(ResourceFinanceApi.Payment_AlreadyPaid);

            await _unitOfWork.Payments.DeleteAsync(payment);
            await _unitOfWork.CompleteAsync();
        }

        public async Task<PaymentDto> MarkAsPaidAsync(Guid id, DateTime paymentDate)
        {
            var payment = await _unitOfWork.Payments.GetPaymentWithDetailsAsync(id) ?? throw new KeyNotFoundException(ResourceFinanceApi.Payment_NotFound);
            payment.MarkAsPaid(paymentDate);
            await _unitOfWork.Payments.UpdateAsync(payment);
            await _unitOfWork.CompleteAsync();

            return _mapper.Map<PaymentDto>(payment);
        }

        public async Task<PaymentDto> MarkAsOverdueAsync(Guid id)
        {
            var payment = await _unitOfWork.Payments.GetPaymentWithDetailsAsync(id) ?? throw new KeyNotFoundException(ResourceFinanceApi.Payment_NotFound);
            payment.MarkAsOverdue();
            await _unitOfWork.Payments.UpdateAsync(payment);
            await _unitOfWork.CompleteAsync();

            return _mapper.Map<PaymentDto>(payment);
        }

        public async Task<PaymentDto> CancelAsync(Guid id)
        {
            var payment = await _unitOfWork.Payments.GetPaymentWithDetailsAsync(id) ?? throw new KeyNotFoundException(ResourceFinanceApi.Payment_NotFound);

            // Check if payment is linked to a financing
            if (payment.FinancingId.HasValue)
            {
                var financing = await _unitOfWork.Financings.GetByIdAsync(payment.FinancingId.Value);
                if (financing != null && financing.Status == Domain.Enums.FinancingStatus.Active)
                {
                    decimal amortizationAmount = 0;

                    // If payment is linked to a specific installment
                    if (payment.FinancingInstallmentId.HasValue)
                    {
                        var installment = await _unitOfWork.FinancingInstallments.GetByIdAsync(payment.FinancingInstallmentId.Value);
                        if (installment != null)
                        {
                            // Calculate proportion of amortization to restore
                            decimal proportionPaid = payment.Amount / installment.TotalAmount;
                            amortizationAmount = proportionPaid * installment.AmortizationAmount;

                            // Update installment status and amounts
                            await RevertPaymentFromInstallment(installment, payment);
                        }
                    }
                    else
                    {
                        // For payments not linked to specific installments (extra payments)
                        // Consider the full amount as amortization
                        amortizationAmount = payment.Amount;
                    }

                    // Increase the remaining debt by the amortization amount
                    financing.RestoreRemainingDebt(amortizationAmount);
                    await _unitOfWork.Financings.UpdateAsync(financing);

                    // Recalculate future installments
                    var financingService = _serviceProvider.GetRequiredService<IFinancingService>();
                    await financingService.RecalculateRemainingInstallmentsAsync(financing.Id);
                }
            }

            payment.Cancel();
            await _unitOfWork.Payments.UpdateAsync(payment);
            await _unitOfWork.CompleteAsync();

            return _mapper.Map<PaymentDto>(payment);
        }

        private async Task RevertPaymentFromInstallment(FinancingInstallment installment, Payment payment)
        {
            // Remove the payment amount from paid amount
            decimal newPaidAmount = installment.PaidAmount - payment.Amount;

            // Recalculate remaining amount
            decimal newRemainingAmount = installment.TotalAmount - newPaidAmount;

            // Determine new status based on payment amounts
            FinancingInstallmentStatus newStatus;
            if (newPaidAmount <= 0)
            {
                newPaidAmount = 0;
                newRemainingAmount = installment.TotalAmount;
                newStatus = FinancingInstallmentStatus.Pending;

                if (installment.DueDate < DateTime.Now)
                {
                    newStatus = FinancingInstallmentStatus.Overdue;
                }
            }
            else if (newRemainingAmount > 0)
            {
                newStatus = FinancingInstallmentStatus.PartiallyPaid;
            }
            else
            {
                newStatus = FinancingInstallmentStatus.Paid;
            }

            // Update installment
            installment.RevertPayment(newPaidAmount, newRemainingAmount, newStatus);
            await _unitOfWork.FinancingInstallments.UpdateAsync(installment);
        }
    }
}
