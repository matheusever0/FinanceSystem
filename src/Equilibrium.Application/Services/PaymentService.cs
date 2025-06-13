using AutoMapper;
using Equilibrium.Application.DTOs.Payment;
using Equilibrium.Application.Interfaces;
using Equilibrium.Domain.Entities;
using Equilibrium.Domain.Enums;
using Equilibrium.Domain.Interfaces.Services;
using Equilibrium.Resources;
using Microsoft.Extensions.DependencyInjection;

namespace Equilibrium.Application.Services
{
    public class PaymentService : IPaymentService
    {
        private readonly IUnitOfWork _unitOfWork;
        private readonly IMapper _mapper;
        private readonly IServiceProvider _serviceProvider;
        private readonly ICreditCardService _creditCardService;

        public PaymentService(
            IUnitOfWork unitOfWork,
            IMapper mapper,
            IServiceProvider serviceProvider,
            ICreditCardService creditCardService)
        {
            _unitOfWork = unitOfWork;
            _mapper = mapper;
            _serviceProvider = serviceProvider;
            _creditCardService = creditCardService;
        }

        public async Task<PaymentDto> GetByIdAsync(Guid id)
        {
            var payment = await _unitOfWork.Payments.GetPaymentWithDetailsAsync(id);
            return payment == null ? throw new KeyNotFoundException(ResourceFinanceApi.Payment_NotFound) : _mapper.Map<PaymentDto>(payment);
        }
        public async Task<IEnumerable<PaymentDto>> GetFilteredAsync(Guid userId, PaymentFilter filter)
        {
            var query = await _unitOfWork.Payments.FindAsync(
                                    p => p.UserId == userId,
                                    p => p.User,
                                    p => p.PaymentMethod,
                                    p => p.PaymentType
                                );

            if (!string.IsNullOrEmpty(filter.Description))
            {
                query = query.Where(p => p.Description.Contains(filter.Description, StringComparison.OrdinalIgnoreCase));
            }

            if (!string.IsNullOrEmpty(filter.Notes))
            {
                query = query.Where(p => p.Notes.Contains(filter.Notes, StringComparison.OrdinalIgnoreCase));
            }

            if (filter.MinAmount.HasValue)
            {
                query = query.Where(p => p.Amount >= filter.MinAmount.Value);
            }

            if (filter.MaxAmount.HasValue)
            {
                query = query.Where(p => p.Amount <= filter.MaxAmount.Value);
            }

            if (filter.Status.HasValue)
            {
                query = query.Where(p => p.Status == filter.Status.Value);
            }

            if (filter.PaymentTypeId.HasValue)
            {
                query = query.Where(p => p.PaymentTypeId == filter.PaymentTypeId.Value);
            }

            if (filter.PaymentMethodId.HasValue)
            {
                query = query.Where(p => p.PaymentMethodId == filter.PaymentMethodId.Value);
            }

            if (filter.CreditCardId.HasValue)
            {
                query = query.Where(e => e.CreditCardId == filter.CreditCardId.Value);
            }

            if (filter.FinancingId.HasValue)
            {
                query = query.Where(p => p.FinancingId == filter.FinancingId.Value);
            }

            if (filter.FinancingInstallmentId.HasValue)
            {
                query = query.Where(p => p.FinancingInstallmentId == filter.FinancingInstallmentId.Value);
            }

            if (filter.IsRecurring.HasValue)
            {
                query = query.Where(p => p.IsRecurring == filter.IsRecurring.Value);
            }

            if (filter.IsFinancingPayment.HasValue)
            {
                if (filter.IsFinancingPayment.Value)
                {
                    query = query.Where(p => p.FinancingId.HasValue);
                }
                else
                {
                    query = query.Where(p => !p.FinancingId.HasValue);
                }
            }

            if (filter.Month.HasValue)
            {
                query = query.Where(p => p.DueDate.Month == filter.Month.Value);
            }

            if (filter.Year.HasValue)
            {
                query = query.Where(p => p.DueDate.Year == filter.Year.Value);
            }

            if (filter.StartDate.HasValue)
            {
                query = query.Where(p => p.DueDate >= filter.StartDate.Value);
            }

            if (filter.EndDate.HasValue)
            {
                query = query.Where(p => p.DueDate <= filter.EndDate.Value);
            }


            if (filter.PaymentStartDate.HasValue)
            {
                query = query.Where(p => p.PaymentDate >= filter.PaymentStartDate.Value);
            }

            if (filter.PaymentEndDate.HasValue)
            {
                query = query.Where(p => p.PaymentDate <= filter.PaymentEndDate.Value);
            }

            if (filter.HasInstallments.HasValue)
            {
                if (filter.HasInstallments.Value)
                {
                    query = query.Where(p => p.Installments.Any());
                }
                else
                {
                    query = query.Where(p => !p.Installments.Any());
                }
            }

            query = filter.OrderBy?.ToLower() switch
            {
                "description" => filter.Ascending ? query.OrderBy(p => p.Description) : query.OrderByDescending(p => p.Description),
                "amount" => filter.Ascending ? query.OrderBy(p => p.Amount) : query.OrderByDescending(p => p.Amount),
                "duedate" => filter.Ascending ? query.OrderBy(p => p.DueDate) : query.OrderByDescending(p => p.DueDate),
                "paymentdate" => filter.Ascending ? query.OrderBy(p => p.PaymentDate) : query.OrderByDescending(p => p.PaymentDate),
                "status" => filter.Ascending ? query.OrderBy(p => p.Status) : query.OrderByDescending(p => p.Status),
                "createdat" => filter.Ascending ? query.OrderBy(p => p.CreatedAt) : query.OrderByDescending(p => p.CreatedAt),
                _ => filter.Ascending ? query.OrderBy(p => p.DueDate) : query.OrderByDescending(p => p.DueDate)
            };

            var items = query.ToList();
            return _mapper.Map<IEnumerable<PaymentDto>>(items);
        }

        public async Task<PaymentDto> CreateAsync(CreatePaymentDto createPaymentDto, Guid userId)
        {
            await ValidateCreatePaymentRequest(createPaymentDto, userId);

            var user = await _unitOfWork.Users.GetByIdAsync(userId) ??
                throw new KeyNotFoundException(ResourceFinanceApi.User_NotFound);

            var paymentType = await _unitOfWork.PaymentTypes.GetByIdAsync(createPaymentDto.PaymentTypeId) ??
                throw new KeyNotFoundException(ResourceFinanceApi.PaymentType_NotFound);

            var paymentMethod = await _unitOfWork.PaymentMethods.GetByIdAsync(createPaymentDto.PaymentMethodId) ??
                throw new KeyNotFoundException(ResourceFinanceApi.PaymentMethod_NotFound);

            var payment = new Payment(
                createPaymentDto.Description,
                createPaymentDto.Amount,
                createPaymentDto.DueDate,
                paymentType,
                paymentMethod,
                user
            );

            if (createPaymentDto.IsRecurring)
                payment.SetRecurring(createPaymentDto.IsRecurring);

            if (!string.IsNullOrEmpty(createPaymentDto.Notes))
                payment.SetNotes(createPaymentDto.Notes);

            await ProcessCreditCardAssociation(payment, createPaymentDto, userId);

            await ProcessFinancingAssociation(payment, paymentType, createPaymentDto, userId);

            await _unitOfWork.Payments.AddAsync(payment);

            if (createPaymentDto.PaymentDate.HasValue)
            {
                payment.MarkAsPaid(createPaymentDto.PaymentDate.Value);
                await _unitOfWork.CompleteAsync();

                if (payment.FinancingId.HasValue)
                {
                    var financingService = _serviceProvider.GetRequiredService<IFinancingService>();
                    await financingService.RecalculateRemainingInstallmentsAsync(payment.FinancingId.Value, payment.DueDate);
                }
            }

            if (createPaymentDto.NumberOfInstallments > 1)
            {
                payment.AddInstallments(createPaymentDto.NumberOfInstallments);
            }

            await _unitOfWork.CompleteAsync();
            var savedPayment = await _unitOfWork.Payments.GetByIdAsync(payment.Id);
            return _mapper.Map<PaymentDto>(savedPayment);
        }

        public async Task<PaymentDto> UpdateAsync(Guid id, UpdatePaymentDto updatePaymentDto)
        {
            var payment = await _unitOfWork.Payments.GetPaymentWithDetailsAsync(id) ??
                throw new KeyNotFoundException(ResourceFinanceApi.Payment_NotFound);

            ValidateUpdatePaymentRequest(payment, updatePaymentDto);

            if (!string.IsNullOrEmpty(updatePaymentDto.Description))
                payment.UpdateDescription(updatePaymentDto.Description);

            if (updatePaymentDto.Amount > 0)
                payment.UpdateAmount(updatePaymentDto.Amount);

            if (updatePaymentDto.DueDate.HasValue)
                payment.UpdateDueDate(updatePaymentDto.DueDate.Value);

            if (updatePaymentDto.Notes != null)
                payment.UpdateNotes(updatePaymentDto.Notes);

            if (updatePaymentDto.IsRecurring.HasValue)
                payment.UpdateRecurring(updatePaymentDto.IsRecurring.Value);

            if (updatePaymentDto.Status.HasValue)
            {
                ProcessStatusChange(payment, updatePaymentDto.Status.Value, updatePaymentDto.PaymentDate);
            }

            if (updatePaymentDto.PaymentTypeId.HasValue)
            {
                await UpdatePaymentType(payment, updatePaymentDto.PaymentTypeId.Value);
            }

            if (updatePaymentDto.PaymentMethodId.HasValue)
            {
                await UpdatePaymentMethod(payment, updatePaymentDto.PaymentMethodId.Value);
            }

            await _unitOfWork.Payments.UpdateAsync(payment);
            await _unitOfWork.CompleteAsync();

            return _mapper.Map<PaymentDto>(payment);
        }

        public async Task DeleteAsync(Guid id)
        {
            var payment = await _unitOfWork.Payments.GetByIdAsync(id) ??
                throw new KeyNotFoundException(ResourceFinanceApi.Payment_NotFound);

            if (payment.Status == PaymentStatus.Paid)
                throw new InvalidOperationException(ResourceFinanceApi.Payment_AlreadyPaid);

            await _unitOfWork.Payments.DeleteAsync(payment);
            await _unitOfWork.CompleteAsync();
        }

        public async Task<PaymentDto> MarkAsPaidAsync(Guid id, DateTime paymentDate)
        {
            var payment = await _unitOfWork.Payments.GetPaymentWithDetailsAsync(id) ??
                throw new KeyNotFoundException(ResourceFinanceApi.Payment_NotFound);

            if (payment.Status == PaymentStatus.Paid)
                throw new InvalidOperationException("Payment is already marked as paid");

            if (payment.Status == PaymentStatus.Cancelled)
                throw new InvalidOperationException("Cannot mark cancelled payment as paid");

            payment.MarkAsPaid(paymentDate);
            await _unitOfWork.Payments.UpdateAsync(payment);

            if (payment.FinancingId.HasValue)
            {
                var financingService = _serviceProvider.GetRequiredService<IFinancingService>();
                await financingService.RecalculateRemainingInstallmentsAsync(payment.FinancingId.Value, payment.DueDate);
            }

            await _unitOfWork.CompleteAsync();
            return _mapper.Map<PaymentDto>(payment);
        }

        public async Task<PaymentDto> MarkAsOverdueAsync(Guid id)
        {
            var payment = await _unitOfWork.Payments.GetPaymentWithDetailsAsync(id) ??
                throw new KeyNotFoundException(ResourceFinanceApi.Payment_NotFound);

            if (payment.Status == PaymentStatus.Paid)
                throw new InvalidOperationException("Cannot mark paid payment as overdue");

            if (payment.Status == PaymentStatus.Cancelled)
                throw new InvalidOperationException("Cannot mark cancelled payment as overdue");

            payment.MarkAsOverdue();
            await _unitOfWork.Payments.UpdateAsync(payment);
            await _unitOfWork.CompleteAsync();

            return _mapper.Map<PaymentDto>(payment);
        }

        public async Task<PaymentDto> CancelAsync(Guid id)
        {
            var payment = await _unitOfWork.Payments.GetPaymentWithDetailsAsync(id) ??
                throw new KeyNotFoundException(ResourceFinanceApi.Payment_NotFound);

            if (payment.Status == PaymentStatus.Cancelled)
                throw new InvalidOperationException("Payment is already cancelled");

            if (payment.FinancingId.HasValue && payment.Status == PaymentStatus.Paid)
                await RevertPaymentFinancing(payment);


            if (payment.CreditCardId.HasValue && payment.Status == PaymentStatus.Paid)
                await _creditCardService.UpdateLimitAsync(payment.CreditCardId.Value, payment.Amount);


            payment.Cancel();
            await _unitOfWork.Payments.UpdateAsync(payment);
            await _unitOfWork.CompleteAsync();

            return _mapper.Map<PaymentDto>(payment);
        }

        #region Private Methods - Validations

        private async Task ValidateCreatePaymentRequest(CreatePaymentDto createPaymentDto, Guid userId)
        {
            if (string.IsNullOrWhiteSpace(createPaymentDto.Description))
                throw new ArgumentException("Description cannot be null or empty");

            if (createPaymentDto.Amount <= 0)
                throw new ArgumentException("Amount must be greater than zero");

            var paymentType = await _unitOfWork.PaymentTypes.GetByIdAsync(createPaymentDto.PaymentTypeId);
            if (paymentType != null && !paymentType.IsSystem && paymentType.UserId != userId)
                throw new UnauthorizedAccessException(ResourceFinanceApi.Error_Unauthorized);

            var paymentMethod = await _unitOfWork.PaymentMethods.GetByIdAsync(createPaymentDto.PaymentMethodId);
            if (paymentMethod != null && !paymentMethod.IsSystem && paymentMethod.UserId != userId)
                throw new UnauthorizedAccessException(ResourceFinanceApi.Error_Unauthorized);

            if (paymentMethod?.Type == PaymentMethodType.CreditCard && !createPaymentDto.CreditCardId.HasValue)
                throw new InvalidOperationException(ResourceFinanceApi.Payment_CreditCardRequired);
        }

        private static void ValidateUpdatePaymentRequest(Payment payment, UpdatePaymentDto updatePaymentDto)
        {
            if (updatePaymentDto.Amount <= 0)
                throw new ArgumentException("Amount must be greater than zero");

            if (payment.Status == PaymentStatus.Paid && updatePaymentDto.DueDate.HasValue)
                throw new InvalidOperationException("Cannot update amount or due date of paid payment");
        }

        #endregion

        #region Private Methods - Business Logic

        private async Task ProcessCreditCardAssociation(Payment payment, CreatePaymentDto createPaymentDto, Guid userId)
        {
            if (createPaymentDto.CreditCardId.HasValue)
            {
                var creditCard = await _unitOfWork.CreditCards.GetByIdAsync(createPaymentDto.CreditCardId.Value) ??
                    throw new KeyNotFoundException(ResourceFinanceApi.CreditCard_NotFound);

                if (creditCard.UserId != userId)
                    throw new UnauthorizedAccessException(ResourceFinanceApi.Error_Unauthorized);

                if (creditCard.AvailableLimit < createPaymentDto.Amount)
                    throw new InvalidOperationException(ResourceFinanceApi.Payment_InsufficientCreditLimit);

                creditCard.DecrementAvailableLimit(createPaymentDto.Amount);
                await _unitOfWork.CreditCards.UpdateAsync(creditCard);

                payment.SetCreditCard(creditCard);
            }
        }

        private async Task ProcessFinancingAssociation(Payment payment, PaymentType paymentType, CreatePaymentDto createPaymentDto, Guid userId)
        {
            if (paymentType.IsFinancingType)
            {
                if (!createPaymentDto.FinancingId.HasValue)
                    throw new InvalidOperationException("É necessário informar um financiamento para este tipo de pagamento");

                var financing = await _unitOfWork.Financings.GetByIdAsync(createPaymentDto.FinancingId.Value) ??
                    throw new KeyNotFoundException("Financiamento não encontrado");

                if (financing.UserId != userId)
                    throw new UnauthorizedAccessException(ResourceFinanceApi.Error_Unauthorized);

                payment.SetFinancing(financing);

                if (createPaymentDto.FinancingInstallmentId.HasValue)
                {
                    var installment = await _unitOfWork.FinancingInstallments.GetByIdAsync(createPaymentDto.FinancingInstallmentId.Value) ??
                        throw new KeyNotFoundException("Parcela de financiamento não encontrada");

                    if (installment.FinancingId != financing.Id)
                        throw new InvalidOperationException("A parcela não pertence ao financiamento informado");

                    payment.SetFinancingInstallment(installment);
                }
            }
        }

        private static void ProcessStatusChange(Payment payment, PaymentStatus newStatus, DateTime? paymentDate)
        {
            switch (newStatus)
            {
                case PaymentStatus.Paid:
                    if (payment.Status == PaymentStatus.Paid)
                        throw new InvalidOperationException("Payment is already marked as paid");

                    if (payment.Status == PaymentStatus.Cancelled)
                        throw new InvalidOperationException("Cannot mark cancelled payment as paid");

                    payment.MarkAsPaid(paymentDate ?? DateTime.Now);
                    break;

                case PaymentStatus.Overdue:
                    if (payment.Status == PaymentStatus.Paid)
                        throw new InvalidOperationException("Cannot mark paid payment as overdue");

                    if (payment.Status == PaymentStatus.Cancelled)
                        throw new InvalidOperationException("Cannot mark cancelled payment as overdue");

                    payment.MarkAsOverdue();
                    break;

                case PaymentStatus.Cancelled:
                    if (payment.Status == PaymentStatus.Cancelled)
                        throw new InvalidOperationException("Payment is already cancelled");

                    payment.Cancel();
                    break;
            }
        }

        private async Task UpdatePaymentType(Payment payment, Guid paymentTypeId)
        {
            var paymentType = await _unitOfWork.PaymentTypes.GetByIdAsync(paymentTypeId) ??
                throw new KeyNotFoundException(ResourceFinanceApi.PaymentType_NotFound);

            if (!paymentType.IsSystem && paymentType.UserId != payment.UserId)
                throw new UnauthorizedAccessException(ResourceFinanceApi.Error_Unauthorized);

            payment.UpdateType(paymentType);
        }

        private async Task UpdatePaymentMethod(Payment payment, Guid paymentMethodId)
        {
            var paymentMethod = await _unitOfWork.PaymentMethods.GetByIdAsync(paymentMethodId) ??
                throw new KeyNotFoundException(ResourceFinanceApi.PaymentMethod_NotFound);

            if (!paymentMethod.IsSystem && paymentMethod.UserId != payment.UserId)
                throw new UnauthorizedAccessException(ResourceFinanceApi.Error_Unauthorized);

            payment.UpdateMethod(paymentMethod);
        }

        private async Task RevertPaymentFinancing(Payment payment)
        {
            var financing = await _unitOfWork.Financings.GetByIdAsync(payment.FinancingId!.Value);
            if (financing != null && financing.Status == FinancingStatus.Active)
            {
                decimal amortizationAmount = 0;

                if (payment.FinancingInstallmentId.HasValue)
                {
                    var installment = await _unitOfWork.FinancingInstallments.GetByIdAsync(payment.FinancingInstallmentId.Value);
                    if (installment != null)
                    {
                        amortizationAmount = installment.AmortizationAmount;
                        await RevertPaymentFromInstallment(installment, payment);
                    }
                }
                else
                {
                    amortizationAmount = payment.Amount;
                }

                financing.RestoreRemainingDebt(amortizationAmount);
                await _unitOfWork.Financings.UpdateAsync(financing);

                var financingService = _serviceProvider.GetRequiredService<IFinancingService>();
                await financingService.RecalculateRemainingInstallmentsAsync(financing.Id, payment.DueDate, true);
            }
        }

        private async Task RevertPaymentFromInstallment(FinancingInstallment installment, Payment payment)
        {
            decimal newPaidAmount = installment.PaidAmount - payment.Amount;
            decimal newRemainingAmount = installment.TotalAmount - newPaidAmount;

            FinancingInstallmentStatus newStatus;
            if (newPaidAmount <= 0)
            {
                newPaidAmount = 0;
                newRemainingAmount = installment.TotalAmount;
                newStatus = FinancingInstallmentStatus.Pending;
            }
            else if (newRemainingAmount > 0)
            {
                newStatus = FinancingInstallmentStatus.PartiallyPaid;
            }
            else
            {
                newStatus = FinancingInstallmentStatus.Paid;
            }

            installment.RevertPayment(newPaidAmount, newRemainingAmount, newStatus);
            await _unitOfWork.FinancingInstallments.UpdateAsync(installment);
        }

        #endregion
    }
}

