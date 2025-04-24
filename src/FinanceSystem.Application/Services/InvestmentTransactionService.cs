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
            return transaction == null
                ? throw new KeyNotFoundException("Transação não encontrada")
                : _mapper.Map<InvestmentTransactionDto>(transaction);
        }

        public async Task<IEnumerable<InvestmentTransactionDto>> GetByInvestmentIdAsync(Guid investmentId)
        {
            var transactions = await _unitOfWork.InvestmentTransactions.GetTransactionsByInvestmentIdAsync(investmentId);
            return _mapper.Map<IEnumerable<InvestmentTransactionDto>>(transactions);
        }

        public async Task<InvestmentTransactionDto> CreateAsync(Guid investmentId, CreateInvestmentTransactionDto createInvestmentTransactionDto)
        {
            var investment = await _unitOfWork.Investments.GetInvestmentWithTransactionsAsync(investmentId) ?? throw new KeyNotFoundException("Investimento não encontrado");

            // Calcular o valor total
            decimal totalValue = createInvestmentTransactionDto.Quantity * createInvestmentTransactionDto.Price;

            // Criar e adicionar transação
            var transaction = new InvestmentTransaction(
                createInvestmentTransactionDto.Date,
                createInvestmentTransactionDto.Type,
                createInvestmentTransactionDto.Quantity,
                createInvestmentTransactionDto.Price,
                totalValue,
                createInvestmentTransactionDto.Taxes,
                createInvestmentTransactionDto.Broker,
                createInvestmentTransactionDto.Notes,
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
            var transaction = await _unitOfWork.InvestmentTransactions.GetByIdAsync(id) ?? throw new KeyNotFoundException("Transação não encontrada");
            var investment = await _unitOfWork.Investments.GetInvestmentWithTransactionsAsync(transaction.InvestmentId);

            await _unitOfWork.InvestmentTransactions.DeleteAsync(transaction);

            if(investment is not null)
            {
                investment.RecalculateWithoutTransaction(transaction);
                await _unitOfWork.Investments.UpdateAsync(investment);

                await _unitOfWork.CompleteAsync();
            }


        }
    }
}
