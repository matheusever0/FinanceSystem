using AutoMapper;
using FinanceSystem.Application.DTOs.PaymentInstallment;
using FinanceSystem.Application.Interfaces;
using FinanceSystem.Domain.Interfaces.Services;

namespace FinanceSystem.Application.Services
{
    public class PaymentInstallmentService : IPaymentInstallmentService
    {
        private readonly IUnitOfWork _unitOfWork;
        private readonly IMapper _mapper;

        public PaymentInstallmentService(IUnitOfWork unitOfWork, IMapper mapper)
        {
            _unitOfWork = unitOfWork;
            _mapper = mapper;
        }

        public async Task<PaymentInstallmentDto> GetByIdAsync(Guid id)
        {
            var paymentInstallment = await _unitOfWork.PaymentInstallments.GetByIdAsync(id);
            if (paymentInstallment == null)
                throw new KeyNotFoundException($"Payment installment with ID {id} not found");

            return _mapper.Map<PaymentInstallmentDto>(paymentInstallment);
        }

        public async Task<IEnumerable<PaymentInstallmentDto>> GetByPaymentIdAsync(Guid paymentId)
        {
            var paymentInstallments = await _unitOfWork.PaymentInstallments.GetInstallmentsByPaymentIdAsync(paymentId);
            return _mapper.Map<IEnumerable<PaymentInstallmentDto>>(paymentInstallments);
        }

        public async Task<IEnumerable<PaymentInstallmentDto>> GetByDueDateAsync(Guid userId, DateTime startDate, DateTime endDate)
        {
            var paymentInstallments = await _unitOfWork.PaymentInstallments.GetInstallmentsByDueDateAsync(userId, startDate, endDate);
            return _mapper.Map<IEnumerable<PaymentInstallmentDto>>(paymentInstallments);
        }

        public async Task<IEnumerable<PaymentInstallmentDto>> GetPendingAsync(Guid userId)
        {
            var paymentInstallments = await _unitOfWork.PaymentInstallments.GetPendingInstallmentsByUserIdAsync(userId);
            return _mapper.Map<IEnumerable<PaymentInstallmentDto>>(paymentInstallments);
        }

        public async Task<IEnumerable<PaymentInstallmentDto>> GetOverdueAsync(Guid userId)
        {
            var paymentInstallments = await _unitOfWork.PaymentInstallments.GetOverdueInstallmentsByUserIdAsync(userId);
            return _mapper.Map<IEnumerable<PaymentInstallmentDto>>(paymentInstallments);
        }

        public async Task<PaymentInstallmentDto> MarkAsPaidAsync(Guid id, DateTime paymentDate)
        {
            var paymentInstallment = await _unitOfWork.PaymentInstallments.GetByIdAsync(id);
            if (paymentInstallment == null)
                throw new KeyNotFoundException($"Payment installment with ID {id} not found");

            paymentInstallment.MarkAsPaid(paymentDate);
            await _unitOfWork.PaymentInstallments.UpdateAsync(paymentInstallment);
            await _unitOfWork.CompleteAsync();

            return _mapper.Map<PaymentInstallmentDto>(paymentInstallment);
        }

        public async Task<PaymentInstallmentDto> MarkAsOverdueAsync(Guid id)
        {
            var paymentInstallment = await _unitOfWork.PaymentInstallments.GetByIdAsync(id);
            if (paymentInstallment == null)
                throw new KeyNotFoundException($"Payment installment with ID {id} not found");

            paymentInstallment.MarkAsOverdue();
            await _unitOfWork.PaymentInstallments.UpdateAsync(paymentInstallment);
            await _unitOfWork.CompleteAsync();

            return _mapper.Map<PaymentInstallmentDto>(paymentInstallment);
        }

        public async Task<PaymentInstallmentDto> CancelAsync(Guid id)
        {
            var paymentInstallment = await _unitOfWork.PaymentInstallments.GetByIdAsync(id);
            if (paymentInstallment == null)
                throw new KeyNotFoundException($"Payment installment with ID {id} not found");

            paymentInstallment.Cancel();
            await _unitOfWork.PaymentInstallments.UpdateAsync(paymentInstallment);
            await _unitOfWork.CompleteAsync();

            return _mapper.Map<PaymentInstallmentDto>(paymentInstallment);
        }
    }
}
