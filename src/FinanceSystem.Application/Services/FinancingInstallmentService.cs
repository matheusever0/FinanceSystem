using AutoMapper;
using FinanceSystem.Application.DTOs.Financing;
using FinanceSystem.Application.DTOs.Payment;
using FinanceSystem.Application.Interfaces;
using FinanceSystem.Domain.Entities;
using FinanceSystem.Domain.Enums;
using FinanceSystem.Domain.Interfaces.Services;
using Microsoft.Extensions.DependencyInjection;

namespace FinanceSystem.Application.Services
{
    public class FinancingInstallmentService : IFinancingInstallmentService
    {
        private readonly IUnitOfWork _unitOfWork;
        private readonly IMapper _mapper;
        private readonly IServiceProvider _serviceProvider;

        public FinancingInstallmentService(
            IUnitOfWork unitOfWork,
            IMapper mapper,
            IServiceProvider serviceProvider)
        {
            _unitOfWork = unitOfWork;
            _mapper = mapper;
            _serviceProvider = serviceProvider;
        }

        public async Task<FinancingInstallmentDto> GetByIdAsync(Guid id)
        {
            var installment = await _unitOfWork.FinancingInstallments.GetByIdAsync(id);
            return installment == null
                ? throw new KeyNotFoundException("Parcela não encontrada")
                : _mapper.Map<FinancingInstallmentDto>(installment);
        }

        public async Task<FinancingInstallmentDetailDto> GetDetailsByIdAsync(Guid id)
        {
            var installment = await _unitOfWork.FinancingInstallments.GetByIdAsync(id);
            if (installment == null)
                throw new KeyNotFoundException("Parcela não encontrada");

            var financing = await _unitOfWork.Financings.GetByIdAsync(installment.FinancingId);
            if (financing == null)
                throw new KeyNotFoundException("Financiamento não encontrado");

            var dto = _mapper.Map<FinancingInstallmentDetailDto>(installment);
            dto.FinancingDescription = financing.Description;

            // Obter pagamentos relacionados a esta parcela
            var payments = await _unitOfWork.Payments
                .FindAsync(p => p.FinancingInstallmentId == id);

            dto.Payments = _mapper.Map<List<PaymentDto>>(payments);

            return dto;
        }

        public async Task<IEnumerable<FinancingInstallmentDto>> GetByFinancingIdAsync(Guid financingId)
        {
            var installments = await _unitOfWork.FinancingInstallments.GetInstallmentsByFinancingIdAsync(financingId);
            return _mapper.Map<IEnumerable<FinancingInstallmentDto>>(installments);
        }

        public async Task<IEnumerable<FinancingInstallmentDto>> GetByDueDateAsync(Guid userId, DateTime startDate, DateTime endDate)
        {
            var installments = await _unitOfWork.FinancingInstallments.GetInstallmentsByDueDateAsync(userId, startDate, endDate);
            return _mapper.Map<IEnumerable<FinancingInstallmentDto>>(installments);
        }

        public async Task<IEnumerable<FinancingInstallmentDto>> GetPendingAsync(Guid userId)
        {
            var installments = await _unitOfWork.FinancingInstallments.GetPendingInstallmentsByUserIdAsync(userId);
            return _mapper.Map<IEnumerable<FinancingInstallmentDto>>(installments);
        }

        public async Task<IEnumerable<FinancingInstallmentDto>> GetOverdueAsync(Guid userId)
        {
            var installments = await _unitOfWork.FinancingInstallments.GetOverdueInstallmentsByUserIdAsync(userId);
            return _mapper.Map<IEnumerable<FinancingInstallmentDto>>(installments);
        }

        public async Task<FinancingInstallmentDto> ProcessPaymentAsync(FinancingInstallmentPaymentDto paymentDto)
        {
            var installment = await _unitOfWork.FinancingInstallments.GetByIdAsync(paymentDto.InstallmentId);
            if (installment == null)
                throw new KeyNotFoundException("Parcela não encontrada");

            var financing = await _unitOfWork.Financings.GetByIdAsync(installment.FinancingId);
            if (financing == null)
                throw new KeyNotFoundException("Financiamento não encontrado");

            var paymentMethod = await _unitOfWork.PaymentMethods.GetByIdAsync(paymentDto.PaymentMethodId);
            if (paymentMethod == null)
                throw new KeyNotFoundException("Método de pagamento não encontrado");

            // Find appropriate payment type for financing
            var paymentType = (await _unitOfWork.PaymentTypes.GetAllSystemTypesAsync())
                .FirstOrDefault(pt => pt.IsFinancingType);

            if (paymentType == null)
                throw new InvalidOperationException("Nenhum tipo de pagamento para financiamentos encontrado");

            // Verify payment amount is valid
            if (paymentDto.Amount <= 0 || paymentDto.Amount > installment.RemainingAmount)
                throw new InvalidOperationException($"Valor de pagamento inválido. O valor deve estar entre 0 e {installment.RemainingAmount}");

            // Create the payment
            var payment = new Payment(
                $"Parcela {installment.InstallmentNumber} - {financing.Description}",
                paymentDto.Amount,
                installment.DueDate,
                paymentType,
                paymentMethod,
                financing.User,
                financing,
                installment,
                false,
                paymentDto.Notes ?? $"Pagamento de parcela do financiamento {financing.Description}"
            );

            // Save the payment
            await _unitOfWork.Payments.AddAsync(payment);

            // Determine if this is an amortization payment
            bool isAmortization = paymentDto.IsAmortization;

            // Process payment on the installment
            installment.AddPayment(payment, isAmortization);

            // Recalculate future installments if needed
            if (isAmortization || paymentDto.RecalculateInstallments)
            {
                financing.RecalculateAfterPayment(paymentDto.Amount, isAmortization);
            }

            // Update the records
            await _unitOfWork.FinancingInstallments.UpdateAsync(installment);
            await _unitOfWork.Financings.UpdateAsync(financing);
            await _unitOfWork.CompleteAsync();

            return _mapper.Map<FinancingInstallmentDto>(installment);
        }

        public async Task<FinancingInstallmentDto> MarkAsOverdueAsync(Guid id)
        {
            var installment = await _unitOfWork.FinancingInstallments.GetByIdAsync(id);
            if (installment == null)
                throw new KeyNotFoundException("Parcela não encontrada");

            if (installment.Status != FinancingInstallmentStatus.Pending &&
                installment.Status != FinancingInstallmentStatus.PartiallyPaid)
                throw new InvalidOperationException("Apenas parcelas pendentes ou parcialmente pagas podem ser marcadas como vencidas");

            installment.MarkAsOverdue();

            await _unitOfWork.FinancingInstallments.UpdateAsync(installment);
            await _unitOfWork.CompleteAsync();

            return _mapper.Map<FinancingInstallmentDto>(installment);
        }
    }
}