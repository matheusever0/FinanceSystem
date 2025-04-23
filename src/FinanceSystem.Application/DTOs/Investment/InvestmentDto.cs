using FinanceSystem.Application.DTOs.InvestmentTransaction;
using FinanceSystem.Domain.Enums;

namespace FinanceSystem.Application.DTOs.Investment
{
    public class InvestmentDto
    {
        public Guid Id { get; set; }
        public string Symbol { get; set; }
        public string Name { get; set; }
        public InvestmentType Type { get; set; }
        public string TypeDescription => Type.ToString();
        public decimal TotalQuantity { get; set; }
        public decimal AveragePrice { get; set; }
        public decimal CurrentPrice { get; set; }
        public decimal TotalInvested { get; set; }
        public decimal CurrentTotal { get; set; }
        public decimal GainLossPercentage { get; set; }
        public decimal GainLossValue { get; set; }
        public DateTime LastUpdate { get; set; }
        public Guid UserId { get; set; }
        public List<InvestmentTransactionDto> Transactions { get; set; } = new List<InvestmentTransactionDto>();
    }
}
