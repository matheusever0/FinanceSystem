using FinanceSystem.Domain.Enums;

namespace FinanceSystem.Domain.Entities
{
    public class Investment
    {
        public Guid Id { get; protected set; }
        public string Symbol { get; protected set; }  // Código da ação, FII, ETF
        public string Name { get; protected set; }    // Nome descritivo
        public InvestmentType Type { get; protected set; }  // Tipo do investimento
        public decimal TotalQuantity { get; protected set; }  // Quantidade total
        public decimal AveragePrice { get; protected set; }  // Preço médio
        public decimal CurrentPrice { get; protected set; }  // Preço atual
        public decimal TotalInvested { get; protected set; }  // Valor total investido
        public decimal CurrentTotal { get; protected set; }  // Valor total atual
        public decimal GainLossPercentage { get; protected set; }  // Percentual de ganho/perda
        public decimal GainLossValue { get; protected set; }  // Valor absoluto de ganho/perda
        public DateTime LastUpdate { get; protected set; }  // Última atualização do preço
        public Guid UserId { get; protected set; }
        public User User { get; protected set; }

        public ICollection<InvestmentTransaction> Transactions { get; protected set; }
    }
}
