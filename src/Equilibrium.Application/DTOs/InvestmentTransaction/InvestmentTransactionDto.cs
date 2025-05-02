using Equilibrium.Domain.Enums;

namespace Equilibrium.Application.DTOs.InvestmentTransaction
{
    public class InvestmentTransactionDto
    {
        public Guid Id { get; set; }
        public DateTime Date { get; set; }
        public TransactionType Type { get; set; }
        public string TypeDescription => Type.ToString();
        public decimal Quantity { get; set; }
        public decimal Price { get; set; }
        public decimal TotalValue { get; set; }
        public decimal Taxes { get; set; }
        public required string Broker { get; set; }
        public required string Notes { get; set; }
        public Guid InvestmentId { get; set; }
        public DateTime CreatedAt { get; set; }
    }
}
