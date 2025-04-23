using AutoMapper;
using FinanceSystem.Application.DTOs.Investment;
using FinanceSystem.Application.Interfaces;
using FinanceSystem.Domain.Entities;
using FinanceSystem.Domain.Enums;
using FinanceSystem.Domain.Interfaces.Services;

namespace FinanceSystem.Application.Services
{
    public class InvestmentService : IInvestmentService
    {
        private readonly IUnitOfWork _unitOfWork;
        private readonly IMapper _mapper;
        private readonly IStockPriceService _stockPriceService;

        public InvestmentService(
            IUnitOfWork unitOfWork,
            IMapper mapper,
            IStockPriceService stockPriceService)
        {
            _unitOfWork = unitOfWork;
            _mapper = mapper;
            _stockPriceService = stockPriceService;
        }

        public async Task<InvestmentDto> GetByIdAsync(Guid id)
        {
            var investment = await _unitOfWork.Investments.GetInvestmentWithTransactionsAsync(id);
            if (investment == null)
                throw new KeyNotFoundException("Investimento não encontrado");

            return _mapper.Map<InvestmentDto>(investment);
        }

        public async Task<IEnumerable<InvestmentDto>> GetAllByUserIdAsync(Guid userId)
        {
            var investments = await _unitOfWork.Investments.GetInvestmentsByUserIdAsync(userId);
            return _mapper.Map<IEnumerable<InvestmentDto>>(investments);
        }

        public async Task<IEnumerable<InvestmentDto>> GetByTypeAsync(Guid userId, InvestmentType type)
        {
            var investments = await _unitOfWork.Investments.GetInvestmentsByTypeAsync(userId, type);
            return _mapper.Map<IEnumerable<InvestmentDto>>(investments);
        }

        public async Task<InvestmentDto> CreateAsync(CreateInvestmentDto createInvestmentDto, Guid userId)
        {
            var user = await _unitOfWork.Users.GetByIdAsync(userId);
            if (user == null)
                throw new KeyNotFoundException("Usuário não encontrado");

            // Verificar se já existe um investimento com o mesmo símbolo
            var existingInvestment = await _unitOfWork.Investments.GetInvestmentBySymbolAsync(userId, createInvestmentDto.Symbol);
            if (existingInvestment != null)
                throw new InvalidOperationException("Já existe um investimento com este símbolo");

            // Buscar o preço atual
            decimal currentPrice;
            try
            {
                currentPrice = await _stockPriceService.GetCurrentPriceAsync(createInvestmentDto.Symbol);
            }
            catch (Exception)
            {
                currentPrice = createInvestmentDto.InitialPrice;
            }

            // Calcular valores iniciais
            decimal totalInvested = createInvestmentDto.InitialQuantity * createInvestmentDto.InitialPrice;
            decimal currentTotal = createInvestmentDto.InitialQuantity * currentPrice;
            decimal gainLossValue = currentTotal - totalInvested;
            decimal gainLossPercentage = totalInvested > 0 ? (gainLossValue / totalInvested) * 100 : 0;

            // Criar o investimento
            var investment = new Investment(
                createInvestmentDto.Symbol,
                createInvestmentDto.Name,
                createInvestmentDto.Type,
                createInvestmentDto.InitialQuantity,
                createInvestmentDto.InitialPrice,
                currentPrice,
                totalInvested,
                currentTotal,
                gainLossPercentage,
                gainLossValue,
                user
            );

            // Adicionar a transação inicial
            investment.AddTransaction(
                createInvestmentDto.TransactionDate,
                TransactionType.Buy,
                createInvestmentDto.InitialQuantity,
                createInvestmentDto.InitialPrice,
                totalInvested,
                0,
                createInvestmentDto.Broker,
                "Transação inicial"
            );

            await _unitOfWork.Investments.AddAsync(investment);
            await _unitOfWork.CompleteAsync();

            return _mapper.Map<InvestmentDto>(investment);
        }

        public async Task<InvestmentDto> UpdateAsync(Guid id, UpdateInvestmentDto updateInvestmentDto)
        {
            var investment = await _unitOfWork.Investments.GetByIdAsync(id);
            if (investment == null)
                throw new KeyNotFoundException("Investimento não encontrado");

            if (!string.IsNullOrEmpty(updateInvestmentDto.Name))
                investment.UpdateName(updateInvestmentDto.Name);

            await _unitOfWork.Investments.UpdateAsync(investment);
            await _unitOfWork.CompleteAsync();

            return _mapper.Map<InvestmentDto>(investment);
        }

        public async Task DeleteAsync(Guid id)
        {
            var investment = await _unitOfWork.Investments.GetInvestmentWithTransactionsAsync(id);
            if (investment == null)
                throw new KeyNotFoundException("Investimento não encontrado");

            await _unitOfWork.Investments.DeleteAsync(investment);
            await _unitOfWork.CompleteAsync();
        }

        public async Task<InvestmentDto> RefreshPriceAsync(Guid id)
        {
            var investment = await _unitOfWork.Investments.GetInvestmentWithTransactionsAsync(id);
            if (investment == null)
                throw new KeyNotFoundException("Investimento não encontrado");

            // Buscar o preço atual
            try
            {
                decimal currentPrice = await _stockPriceService.GetCurrentPriceAsync(investment.Symbol);
                investment.UpdateCurrentPrice(currentPrice);

                await _unitOfWork.Investments.UpdateAsync(investment);
                await _unitOfWork.CompleteAsync();
            }
            catch (Exception ex)
            {
                throw new Exception($"Erro ao atualizar preço: {ex.Message}");
            }

            return _mapper.Map<InvestmentDto>(investment);
        }

        public async Task<IEnumerable<InvestmentDto>> RefreshAllPricesAsync(Guid userId)
        {
            var investments = await _unitOfWork.Investments.GetInvestmentsByUserIdAsync(userId);

            var symbols = investments.Select(i => i.Symbol).ToList();

            try
            {
                var quotes = await _stockPriceService.GetBatchQuotesAsync(symbols);

                foreach (var investment in investments)
                {
                    var quote = quotes.FirstOrDefault(q => q.Symbol.Equals(investment.Symbol, StringComparison.OrdinalIgnoreCase));
                    if (quote != null)
                    {
                        investment.UpdateCurrentPrice(quote.Price);
                        await _unitOfWork.Investments.UpdateAsync(investment);
                    }
                }

                await _unitOfWork.CompleteAsync();
            }
            catch (Exception ex)
            {
                throw new Exception($"Erro ao atualizar preços: {ex.Message}");
            }

            return _mapper.Map<IEnumerable<InvestmentDto>>(investments);
        }
    }
}
