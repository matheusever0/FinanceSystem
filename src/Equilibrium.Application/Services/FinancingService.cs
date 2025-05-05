using AutoMapper;
using Equilibrium.Application.DTOs.Financing;
using Equilibrium.Application.Interfaces;
using Equilibrium.Domain.Entities;
using Equilibrium.Domain.Enums;
using Equilibrium.Domain.Interfaces.Services;

namespace Equilibrium.Application.Services
{
    public class FinancingService : IFinancingService
    {
        private readonly IUnitOfWork _unitOfWork;
        private readonly IMapper _mapper;

        public FinancingService(
            IUnitOfWork unitOfWork,
            IMapper mapper)
        {
            _unitOfWork = unitOfWork;
            _mapper = mapper;
        }

        public async Task<FinancingDto> GetByIdAsync(Guid id)
        {
            var financing = await _unitOfWork.Financings.GetByIdAsync(id);
            return financing == null
                ? throw new KeyNotFoundException("Financiamento não encontrado")
                : _mapper.Map<FinancingDto>(financing);
        }

        public async Task<FinancingDetailDto> GetDetailsByIdAsync(Guid id)
        {
            var financing = await _unitOfWork.Financings.GetFinancingWithDetailsAsync(id);
            if (financing == null)
                throw new KeyNotFoundException("Financiamento não encontrado");

            var dto = _mapper.Map<FinancingDetailDto>(financing);

            // Calcular métricas adicionais
            var installments = financing.Installments.ToList();
            var paidInstallments = installments.Where(i => i.Status == FinancingInstallmentStatus.Paid
                || i.Status == FinancingInstallmentStatus.PartiallyPaid).ToList();
            var pendingInstallments = installments.Where(i => i.Status == FinancingInstallmentStatus.Pending).ToList();

            dto.InstallmentsPaid = paidInstallments.Count;
            dto.InstallmentsRemaining = pendingInstallments.Count;

            dto.TotalPaid = paidInstallments.Sum(i => i.PaidAmount);
            dto.TotalRemaining = pendingInstallments.Sum(i => i.RemainingAmount);

            dto.TotalInterestPaid = paidInstallments.Sum(i => i.InterestAmount);
            dto.TotalInterestRemaining = pendingInstallments.Sum(i => i.InterestAmount);

            dto.TotalAmortizationPaid = paidInstallments.Sum(i => i.AmortizationAmount);

            dto.ProgressPercentage = financing.TotalAmount > 0
                ? (dto.TotalAmortizationPaid / financing.TotalAmount) * 100
                : 0;

            dto.AverageInstallmentAmount = installments.Count > 0
                ? installments.Average(i => i.TotalAmount)
                : 0;

            dto.MonthlyAveragePayment = paidInstallments.Count > 0
                ? paidInstallments.Sum(i => i.PaidAmount) / paidInstallments.Count
                : 0;

            dto.EstimatedTotalCost = dto.TotalPaid + dto.TotalRemaining;

            return dto;
        }

        public async Task<IEnumerable<FinancingDto>> GetAllByUserIdAsync(Guid userId)
        {
            var financings = await _unitOfWork.Financings.GetFinancingsByUserIdAsync(userId);
            return _mapper.Map<IEnumerable<FinancingDto>>(financings); ;
        }

        public async Task<IEnumerable<FinancingDto>> GetActiveFinancingsByUserIdAsync(Guid userId)
        {
            var financings = await _unitOfWork.Financings.GetActiveFinancingsByUserIdAsync(userId);
            return _mapper.Map<IEnumerable<FinancingDto>>(financings);
        }

        public async Task<IEnumerable<FinancingDto>> GetFinancingsByStatusAsync(Guid userId, FinancingStatus status)
        {
            var financings = await _unitOfWork.Financings.GetFinancingsByStatusAsync(userId, status);
            return _mapper.Map<IEnumerable<FinancingDto>>(financings);
        }

        public async Task<decimal> GetTotalRemainingDebtByUserIdAsync(Guid userId)
        {
            return await _unitOfWork.Financings.GetTotalRemainingDebtByUserIdAsync(userId);
        }

        public async Task<FinancingDto> CreateAsync(CreateFinancingDto createFinancingDto, Guid userId)
        {
            var user = await _unitOfWork.Users.GetByIdAsync(userId) ??
                throw new KeyNotFoundException("Usuário não encontrado");

            var paymentType = await _unitOfWork.PaymentTypes.GetByIdAsync(createFinancingDto.PaymentTypeId) ??
                throw new KeyNotFoundException("Tipo de pagamento não encontrado");

            if (!paymentType.IsFinancingType)
                throw new InvalidOperationException("O tipo de pagamento não é válido para financiamentos");

            var financing = new Financing(
                createFinancingDto.Description,
                createFinancingDto.TotalAmount,
                createFinancingDto.InterestRate,
                createFinancingDto.TermMonths,
                createFinancingDto.StartDate,
                createFinancingDto.Type,
                createFinancingDto.CorrectionIndex,
                user,
                createFinancingDto.Notes);

            await _unitOfWork.Financings.AddAsync(financing);
            await _unitOfWork.CompleteAsync();

            return _mapper.Map<FinancingDto>(financing);
        }

        public async Task<FinancingDto> UpdateAsync(Guid id, UpdateFinancingDto updateFinancingDto)
        {
            var financing = await _unitOfWork.Financings.GetByIdAsync(id) ??
                throw new KeyNotFoundException("Financiamento não encontrado");

            if (financing.Status != FinancingStatus.Active)
                throw new InvalidOperationException("Não é possível atualizar um financiamento que não está ativo");

            if (!string.IsNullOrEmpty(updateFinancingDto.Description))
                financing.UpdateDescription(updateFinancingDto.Description);

            if (!string.IsNullOrEmpty(updateFinancingDto.Notes))
                financing.UpdateNotes(updateFinancingDto.Notes);

            await _unitOfWork.Financings.UpdateAsync(financing);
            await _unitOfWork.CompleteAsync();

            return _mapper.Map<FinancingDto>(financing);
        }

        public async Task CompleteAsync(Guid id)
        {
            var financing = await _unitOfWork.Financings.GetByIdAsync(id) ??
                throw new KeyNotFoundException("Financiamento não encontrado");

            if (financing.Status != FinancingStatus.Active)
                throw new InvalidOperationException("Não é possível completar um financiamento que não está ativo");

            financing.Complete();

            await _unitOfWork.Financings.UpdateAsync(financing);
            await _unitOfWork.CompleteAsync();
        }

        public async Task CancelAsync(Guid id)
        {
            var financing = await _unitOfWork.Financings.GetByIdAsync(id) ??
                throw new KeyNotFoundException("Financiamento não encontrado");

            if (financing.Status != FinancingStatus.Active)
                throw new InvalidOperationException("Não é possível cancelar um financiamento que não está ativo");

            financing.Cancel();

            await _unitOfWork.Financings.UpdateAsync(financing);
            await _unitOfWork.CompleteAsync();
        }

        public async Task<FinancingSimulationDto> SimulateAsync(FinancingSimulationRequestDto simulationRequest)
        {
            var result = new FinancingSimulationDto
            {
                TotalAmount = simulationRequest.TotalAmount,
                InterestRate = simulationRequest.InterestRate,
                TermMonths = simulationRequest.TermMonths,
                Type = simulationRequest.Type
            };

            decimal monthlyRate = simulationRequest.InterestRate / 12 / 100;

            if (simulationRequest.Type == FinancingType.PRICE)
            {
                // Fórmula PRICE
                decimal factor = monthlyRate * (decimal)Math.Pow((double)(1 + monthlyRate), simulationRequest.TermMonths) /
                                ((decimal)Math.Pow((double)(1 + monthlyRate), simulationRequest.TermMonths) - 1);

                decimal installmentAmount = simulationRequest.TotalAmount * factor;
                result.FirstInstallmentAmount = installmentAmount;
                result.LastInstallmentAmount = installmentAmount;

                decimal currentDebt = simulationRequest.TotalAmount;
                decimal totalInterest = 0;

                for (int i = 1; i <= simulationRequest.TermMonths; i++)
                {
                    decimal interest = currentDebt * monthlyRate;
                    decimal amortization = installmentAmount - interest;

                    totalInterest += interest;

                    var installment = new FinancingInstallmentSimulationDto
                    {
                        Number = i,
                        TotalAmount = installmentAmount,
                        InterestAmount = interest,
                        AmortizationAmount = amortization,
                        RemainingDebt = currentDebt - amortization,
                        DueDate = simulationRequest.StartDate.AddMonths(i)
                    };

                    result.Installments.Add(installment);

                    currentDebt -= amortization;
                }

                result.TotalInterest = totalInterest;
                result.TotalCost = simulationRequest.TotalAmount + totalInterest;
            }
            else // SAC
            {
                decimal constantAmortization = simulationRequest.TotalAmount / simulationRequest.TermMonths;
                decimal currentDebt = simulationRequest.TotalAmount;
                decimal totalInterest = 0;

                for (int i = 1; i <= simulationRequest.TermMonths; i++)
                {
                    decimal interest = currentDebt * monthlyRate;
                    decimal installmentAmount = constantAmortization + interest;

                    if (i == 1)
                        result.FirstInstallmentAmount = installmentAmount;
                    if (i == simulationRequest.TermMonths)
                        result.LastInstallmentAmount = installmentAmount;

                    totalInterest += interest;

                    var installment = new FinancingInstallmentSimulationDto
                    {
                        Number = i,
                        TotalAmount = installmentAmount,
                        InterestAmount = interest,
                        AmortizationAmount = constantAmortization,
                        RemainingDebt = currentDebt - constantAmortization,
                        DueDate = simulationRequest.StartDate.AddMonths(i)
                    };

                    result.Installments.Add(installment);

                    currentDebt -= constantAmortization;
                }

                result.TotalInterest = totalInterest;
                result.TotalCost = simulationRequest.TotalAmount + totalInterest;
                result.MonthlyDecreaseAmount = monthlyRate * constantAmortization;
            }

            // Limitar a no máximo 12 parcelas para visualização
            if (result.Installments.Count > 12)
            {
                var firstInstallments = result.Installments.Take(3).ToList();
                var middleInstallments = result.Installments
                    .Skip(simulationRequest.TermMonths / 2 - 2)
                    .Take(4)
                    .ToList();
                var lastInstallments = result.Installments
                    .Skip(simulationRequest.TermMonths - 5)
                    .Take(5)
                    .ToList();

                result.Installments = firstInstallments.Concat(middleInstallments).Concat(lastInstallments).ToList();
            }

            await Task.CompletedTask; // Para manter o método assíncrono
            return result;
        }

        public async Task RecalculateRemainingInstallmentsAsync(Guid financingId, DateTime dueDate, bool cancelling = false)
        {
            var financing = await _unitOfWork.Financings.GetFinancingWithDetailsAsync(financingId);
            if (financing == null)
                throw new KeyNotFoundException("Financiamento não encontrado");

            if (financing.Status != FinancingStatus.Active)
                throw new InvalidOperationException("Apenas financiamentos ativos podem ser recalculados");

            // Recalculate installments based on remaining debt
            financing.RecalculateRemainingInstallments(dueDate, cancelling);

            await _unitOfWork.Financings.UpdateAsync(financing);
            await _unitOfWork.CompleteAsync();
        }
    }
}
