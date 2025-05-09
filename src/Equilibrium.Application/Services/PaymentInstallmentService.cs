using AutoMapper;
using Equilibrium.Application.DTOs.PaymentInstallment;
using Equilibrium.Application.Interfaces;
using Equilibrium.Domain.Interfaces.Services;
using Equilibrium.Resources;
using Equilibrium.Domain.Specifications;
using Equilibrium.Application.DTOs.Common;
using Equilibrium.Domain.DTOs.Filters;

namespace Equilibrium.Application.Services
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
            return paymentInstallment == null
                ? throw new KeyNotFoundException(ResourceFinanceApi.PaymentInstallment_NotFound)
                : _mapper.Map<PaymentInstallmentDto>(paymentInstallment);
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
            var paymentInstallment = await _unitOfWork.PaymentInstallments.GetByIdAsync(id) ?? throw new KeyNotFoundException(ResourceFinanceApi.PaymentInstallment_NotFound);
            paymentInstallment.MarkAsPaid(paymentDate);
            await _unitOfWork.PaymentInstallments.UpdateAsync(paymentInstallment);
            await _unitOfWork.CompleteAsync();

            return _mapper.Map<PaymentInstallmentDto>(paymentInstallment);
        }

        public async Task<PaymentInstallmentDto> MarkAsOverdueAsync(Guid id)
        {
            var paymentInstallment = await _unitOfWork.PaymentInstallments.GetByIdAsync(id) ?? throw new KeyNotFoundException(ResourceFinanceApi.PaymentInstallment_NotFound);
            paymentInstallment.MarkAsOverdue();
            await _unitOfWork.PaymentInstallments.UpdateAsync(paymentInstallment);
            await _unitOfWork.CompleteAsync();

            return _mapper.Map<PaymentInstallmentDto>(paymentInstallment);
        }

        public async Task<PaymentInstallmentDto> CancelAsync(Guid id)
        {
            var paymentInstallment = await _unitOfWork.PaymentInstallments.GetByIdAsync(id) ?? throw new KeyNotFoundException(ResourceFinanceApi.PaymentInstallment_NotFound);
            paymentInstallment.Cancel();
            await _unitOfWork.PaymentInstallments.UpdateAsync(paymentInstallment);
            await _unitOfWork.CompleteAsync();

            return _mapper.Map<PaymentInstallmentDto>(paymentInstallment);
        }
        public async Task<PagedResult<PaymentInstallmentDto>> GetFilteredAsync(PaymentInstallmentFilter filter, Guid userId)
        {
            var specification = new PaymentInstallmentSpecification(filter);

            // Apply custom criteria for user ID based on linked entities
            specification.UserId = userId;

            var (items, totalCount) = await _unitOfWork.PaymentInstallments.FindWithSpecificationAsync(specification);

            return new PagedResult<PaymentInstallmentDto>
            {
                Items = _mapper.Map<IEnumerable<PaymentInstallmentDto>>(items),
                TotalCount = totalCount,
                PageNumber = filter.PageNumber,
                PageSize = filter.PageSize
            };
        }
    }
}

