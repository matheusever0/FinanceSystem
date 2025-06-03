// src/Equilibrium.Application/Services/PaymentService.cs
using AutoMapper;
using Equilibrium.Application.DTOs.Payment;
using Equilibrium.Application.Interfaces;
using Equilibrium.Domain.Entities;
using Equilibrium.Domain.Enums;
using Equilibrium.Domain.Interfaces.Services;
using Equilibrium.Resources;

namespace Equilibrium.Application.Services
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
            return payment == null
                ? throw new KeyNotFoundException(ResourceFinanceApi.Payment_NotFound)
                : _mapper.Map<PaymentDto>(payment);
        }

        public async Task<IEnumerable<PaymentDto>> GetFilteredAsync(Guid userId, PaymentFilter filter)
        {
            var payments = await _unitOfWork.Payments.GetPaymentsByUserIdAsync(userId);
            var filteredPayments = ApplyFilters(payments, filter);
            return _mapper.Map<IEnumerable<PaymentDto>>(filteredPayments);
        }

        public async Task<PaymentDto> CreateAsync(CreatePaymentDto createPaymentDto, Guid userId)
        {
            var user = await _unitOfWork.Users.GetByIdAsync(userId)
                ?? throw new KeyNotFoundException(ResourceFinanceApi.User_NotFound);

            var paymentType = await _unitOfWork.PaymentTypes.GetByIdAsync(createPaymentDto.PaymentTypeId)
                ?? throw new KeyNotFoundException(ResourceFinanceApi.PaymentType_NotFound);

            var paymentMethod = await _unitOfWork.PaymentMethods.GetByIdAsync(createPaymentDto.PaymentMethodId)
                ?? throw new KeyNotFoundException(ResourceFinanceApi.PaymentMethod_NotFound);

            // Verificar se é pagamento com cartão de crédito
            CreditCard? creditCard = null;
            if (paymentMethod.Type == PaymentMethodType.CreditCard)
            {
                if (!createPaymentDto.CreditCardId.HasValue)
                    throw new InvalidOperationException(ResourceFinanceApi.Payment_CreditCardRequired);

                creditCard = await _unitOfWork.CreditCards.GetByIdAsync(createPaymentDto.CreditCardId.Value)
                    ?? throw new KeyNotFoundException(ResourceFinanceApi.CreditCard_NotFound);

                // Verificar limite disponível
                if (creditCard.AvailableLimit < createPaymentDto.Amount)
                    throw new InvalidOperationException(ResourceFinanceApi.Payment_InsufficientCreditLimit);

                // Decrementar limite disponível
                creditCard.DecrementAvailableLimit(createPaymentDto.Amount);
                await _unitOfWork.CreditCards.UpdateAsync(creditCard);
            }

            // Criar pagamento
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

            // Se tem data de pagamento, marcar como pago
            if (createPaymentDto.PaymentDate.HasValue)
            {
                payment.MarkAsPaid(createPaymentDto.PaymentDate.Value);
            }

            // Adicionar parcelas se necessário
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
            var payment = await _unitOfWork.Payments.GetPaymentWithDetailsAsync(id)
                ?? throw new KeyNotFoundException(ResourceFinanceApi.Payment_NotFound);

            // Não permitir alterar pagamentos já pagos
            if (payment.Status == PaymentStatus.Paid)
                throw new InvalidOperationException("Cannot update paid payment");

            // Verificar mudança de valor em cartão de crédito
            var originalAmount = payment.Amount;
            var newAmount = updatePaymentDto.Amount;

            if (payment.PaymentMethod.Type == PaymentMethodType.CreditCard && originalAmount != newAmount)
            {
                var creditCard = await _unitOfWork.CreditCards.GetByIdAsync(payment.CreditCardId ?? Guid.Empty);
                if (creditCard != null)
                {
                    // Liberar o valor antigo e consumir o novo
                    creditCard.IncrementAvailableLimit(originalAmount);

                    if (creditCard.AvailableLimit < newAmount)
                        throw new InvalidOperationException(ResourceFinanceApi.Payment_InsufficientCreditLimit);

                    creditCard.DecrementAvailableLimit(newAmount);
                    await _unitOfWork.CreditCards.UpdateAsync(creditCard);
                }
            }

            // Atualizar campos
            if (!string.IsNullOrEmpty(updatePaymentDto.Description))
                payment.UpdateDescription(updatePaymentDto.Description);

            if (updatePaymentDto.Amount > 0)
                payment.UpdateAmount(updatePaymentDto.Amount);

            if (updatePaymentDto.DueDate.HasValue)
                payment.UpdateDueDate(updatePaymentDto.DueDate.Value);

            if (!string.IsNullOrEmpty(updatePaymentDto.Notes))
                payment.UpdateNotes(updatePaymentDto.Notes);

            await _unitOfWork.Payments.UpdateAsync(payment);
            await _unitOfWork.CompleteAsync();

            return _mapper.Map<PaymentDto>(payment);
        }

        public async Task DeleteAsync(Guid id)
        {
            var payment = await _unitOfWork.Payments.GetPaymentWithDetailsAsync(id)
                ?? throw new KeyNotFoundException(ResourceFinanceApi.Payment_NotFound);

            if (payment.Status == PaymentStatus.Paid)
                throw new InvalidOperationException(ResourceFinanceApi.Payment_AlreadyPaid);

            // Se é pagamento de cartão, liberar o limite
            if (payment.PaymentMethod.Type == PaymentMethodType.CreditCard && payment.CreditCardId.HasValue)
            {
                var creditCard = await _unitOfWork.CreditCards.GetByIdAsync(payment.CreditCardId.Value);
                if (creditCard != null)
                {
                    creditCard.IncrementAvailableLimit(payment.Amount);
                    await _unitOfWork.CreditCards.UpdateAsync(creditCard);
                }
            }

            await _unitOfWork.Payments.DeleteAsync(payment);
            await _unitOfWork.CompleteAsync();
        }

        public async Task<PaymentDto> MarkAsPaidAsync(Guid id, DateTime paymentDate)
        {
            var payment = await _unitOfWork.Payments.GetByIdAsync(id)
                ?? throw new KeyNotFoundException(ResourceFinanceApi.Payment_NotFound);

            payment.MarkAsPaid(paymentDate);
            await _unitOfWork.Payments.UpdateAsync(payment);
            await _unitOfWork.CompleteAsync();

            return _mapper.Map<PaymentDto>(payment);
        }

        public async Task<PaymentDto> MarkAsOverdueAsync(Guid id)
        {
            var payment = await _unitOfWork.Payments.GetByIdAsync(id)
                ?? throw new KeyNotFoundException(ResourceFinanceApi.Payment_NotFound);

            payment.MarkAsOverdue();
            await _unitOfWork.Payments.UpdateAsync(payment);
            await _unitOfWork.CompleteAsync();

            return _mapper.Map<PaymentDto>(payment);
        }

        public async Task<PaymentDto> CancelAsync(Guid id)
        {
            var payment = await _unitOfWork.Payments.GetPaymentWithDetailsAsync(id)
                ?? throw new KeyNotFoundException(ResourceFinanceApi.Payment_NotFound);

            // Se é pagamento de cartão, liberar o limite
            if (payment.PaymentMethod.Type == PaymentMethodType.CreditCard && payment.CreditCardId.HasValue)
            {
                var creditCard = await _unitOfWork.CreditCards.GetByIdAsync(payment.CreditCardId.Value);
                if (creditCard != null)
                {
                    creditCard.IncrementAvailableLimit(payment.Amount);
                    await _unitOfWork.CreditCards.UpdateAsync(creditCard);
                }
            }

            payment.Cancel();
            await _unitOfWork.Payments.UpdateAsync(payment);
            await _unitOfWork.CompleteAsync();

            return _mapper.Map<PaymentDto>(payment);
        }

        private IEnumerable<Payment?> ApplyFilters(IEnumerable<Payment?> payments, PaymentFilter filter)
        {
            var query = payments.AsQueryable();

            if (!string.IsNullOrEmpty(filter.Description))
                query = query.Where(p => p.Description.Contains(filter.Description, StringComparison.OrdinalIgnoreCase));

            if (filter.MinAmount.HasValue)
                query = query.Where(p => p.Amount >= filter.MinAmount.Value);

            if (filter.MaxAmount.HasValue)
                query = query.Where(p => p.Amount <= filter.MaxAmount.Value);

            if (filter.Status.HasValue)
                query = query.Where(p => p.Status == filter.Status.Value);

            if (filter.PaymentTypeId.HasValue)
                query = query.Where(p => p.PaymentTypeId == filter.PaymentTypeId.Value);

            if (filter.PaymentMethodId.HasValue)
                query = query.Where(p => p.PaymentMethodId == filter.PaymentMethodId.Value);

            if (filter.CreditCardId.HasValue)
                query = query.Where(p => p.CreditCardId == filter.CreditCardId.Value);

            // Aplicar ordenação
            switch (filter.OrderBy?.ToLower())
            {
                case "amount":
                    query = filter.Ascending ? query.OrderBy(p => p.Amount) : query.OrderByDescending(p => p.Amount);
                    break;
                case "description":
                    query = filter.Ascending ? query.OrderBy(p => p.Description) : query.OrderByDescending(p => p.Description);
                    break;
                case "paymentdate":
                    query = filter.Ascending ? query.OrderBy(p => p.PaymentDate) : query.OrderByDescending(p => p.PaymentDate);
                    break;
                default:
                    query = filter.Ascending ? query.OrderBy(p => p.DueDate) : query.OrderByDescending(p => p.DueDate);
                    break;
            }

            return query.ToList();
        }
    }
}