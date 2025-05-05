using AutoMapper;
using Equilibrium.Application.DTOs.Financing;
using Equilibrium.Application.DTOs.Payment;
using Equilibrium.Application.Interfaces;
using Equilibrium.Domain.Interfaces.Services;

namespace Equilibrium.Application.Services
{
    public class FinancingInstallmentService : IFinancingInstallmentService
    {
        private readonly IUnitOfWork _unitOfWork;
        private readonly IMapper _mapper;

        public FinancingInstallmentService(
            IUnitOfWork unitOfWork,
            IMapper mapper)
        {
            _unitOfWork = unitOfWork;
            _mapper = mapper;
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
    }
}