using AutoMapper;
using FinanceSystem.Application.DTOs.Financing;
using FinanceSystem.Application.Interfaces;
using FinanceSystem.Domain.Entities;
using FinanceSystem.Domain.Interfaces.Services;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace FinanceSystem.Application.Services
{
    public class FinancingCorrectionService : IFinancingCorrectionService
    {
        private readonly IUnitOfWork _unitOfWork;
        private readonly IMapper _mapper;

        public FinancingCorrectionService(IUnitOfWork unitOfWork, IMapper mapper)
        {
            _unitOfWork = unitOfWork;
            _mapper = mapper;
        }

        public async Task<FinancingCorrectionDto> GetByIdAsync(Guid id)
        {
            var correction = await _unitOfWork.FinancingCorrections.GetByIdAsync(id);
            return correction == null
                ? throw new KeyNotFoundException("Correção não encontrada")
                : _mapper.Map<FinancingCorrectionDto>(correction);
        }

        public async Task<IEnumerable<FinancingCorrectionDto>> GetByFinancingIdAsync(Guid financingId)
        {
            var corrections = await _unitOfWork.FinancingCorrections.GetCorrectionsByFinancingIdAsync(financingId);
            return _mapper.Map<IEnumerable<FinancingCorrectionDto>>(corrections);
        }

        public async Task<IEnumerable<FinancingCorrectionDto>> GetByDateRangeAsync(Guid financingId, DateTime startDate, DateTime endDate)
        {
            var corrections = await _unitOfWork.FinancingCorrections.GetCorrectionsByDateRangeAsync(financingId, startDate, endDate);
            return _mapper.Map<IEnumerable<FinancingCorrectionDto>>(corrections);
        }

        public async Task<FinancingCorrectionDto> ApplyCorrectionAsync(ApplyCorrectionDto correctionDto)
        {
            var financing = await _unitOfWork.Financings.GetFinancingWithDetailsAsync(correctionDto.FinancingId);
            if (financing == null)
                throw new KeyNotFoundException("Financiamento não encontrado");

            if (financing.Status != Domain.Enums.FinancingStatus.Active)
                throw new InvalidOperationException("Correção só pode ser aplicada em financiamentos ativos");

            if (correctionDto.IndexValue <= 0)
                throw new InvalidOperationException("O valor do índice deve ser maior que zero");

            // Aplicar a correção
            financing.ApplyCorrection(correctionDto.IndexValue, correctionDto.CorrectionDate);

            // A correção foi adicionada à coleção de correções dentro do método ApplyCorrection
            var newCorrection = financing.Corrections
                .OrderByDescending(c => c.CorrectionDate)
                .FirstOrDefault();

            if (newCorrection == null)
                throw new InvalidOperationException("Erro ao criar correção");

            await _unitOfWork.Financings.UpdateAsync(financing);
            await _unitOfWork.CompleteAsync();

            return _mapper.Map<FinancingCorrectionDto>(newCorrection);
        }

        public async Task<decimal> GetTotalCorrectionImpactAsync(Guid financingId)
        {
            return await _unitOfWork.FinancingCorrections.GetTotalCorrectionImpactAsync(financingId);
        }
    }
}