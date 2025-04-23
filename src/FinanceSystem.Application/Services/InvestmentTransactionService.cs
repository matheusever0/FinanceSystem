using AutoMapper;
using FinanceSystem.Application.DTOs.InvestmentTransaction;
using FinanceSystem.Application.Interfaces;
using FinanceSystem.Domain.Entities;
using FinanceSystem.Domain.Interfaces.Services;

namespace FinanceSystem.Application.Services
{
    public class InvestmentTransactionService : IInvestmentTransactionService
    {
        private readonly IUnitOfWork _unitOfWork;
        private readonly IMapper _mapper;

        public InvestmentTransactionService(IUnitOfWork unitOfWork, IMapper mapper)
        {
            _unitOfWork = unitOfWork;
            _mapper = mapper;
        }

        public async Task<InvestmentTransactionDto> GetByIdAsync(Guid id)
        {
            var transaction = await _unitOfWork.InvestmentTransactions.GetByIdAsync(id);
            if (transaction == null)
                throw new KeyNotFoundException("Transação não encontrada");

            return _mapper.Map<InvestmentTransactionDto>(transaction);
        }

        public async Task<IEnumerable<InvestmentTransactionDto>> GetByInvestmentIdAsync(Guid investmentId)
        {
            var transactions = await _unitOfWork.InvestmentTransactions.GetTransactionsByInvestmentIdAsync(investmentId);
            return _mapper.Map<IEnumerable<InvestmentTransactionDto>>(transactions);
        }

        public async Task<InvestmentTransactionDto> CreateAsync(Guid investmentId, CreateInvestmentTransactionDto createTransactionDto)
        {
            var investment = await _unitOfWork.Investments.GetInvestmentWithTransactionsAsync(investmentId);
            if (investment == null)
                throw new KeyNotFoundException("Investimento não encontrado");

            // Calcular o valor total
            decimal totalValue = createTransactionDto.Quantity * createTransactionDto.Price;

            // Criar e adicionar transação
            var transaction = new InvestmentTransaction(
                createTransactionDto.Date,
                createTransactionDto.Type,
                createTransactionDto.Quantity,
                createTransactionDto.Price,
                totalValue,
                createTransactionDto.Taxes,
                createTransactionDto.Broker,
                createTransactionDto.Notes,
                investment
            );

            await _unitOfWork.InvestmentTransactions.AddAsync(transaction);

            // Atualizar preço médio e quantidade do investimento
            investment.RecalculateAfterTransaction(transaction);
            await _unitOfWork.Investments.UpdateAsync(investment);

            await _unitOfWork.CompleteAsync();

            return _mapper.Map<InvestmentTransactionDto>(transaction);
        }

        public async Task DeleteAsync(Guid id)
        {
            var transaction = await _unitOfWork.InvestmentTransactions.GetByIdAsync(id);
            if (transaction == null)
                throw new KeyNotFoundException("Transação não encontrada");

            // Recuperar o investimento para recalcular após exclusão
            var investment = await _unitOfWork.Investments.GetInvestmentWithTransactionsAsync(transaction.InvestmentId);

            await _unitOfWork.InvestmentTransactions.DeleteAsync(transaction);

            // Recalcular o investimento excluindo esta transação
            investment.RecalculateWithoutTransaction(transaction);
            await _unitOfWork.Investments.UpdateAsync(investment);

            await _unitOfWork.CompleteAsync();
        }
    }
}
