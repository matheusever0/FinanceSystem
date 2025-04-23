using FinanceSystem.Domain.Enums;

namespace FinanceSystem.Domain.Entities
{
    public class InvestmentTransaction
    {
        public Guid Id { get; protected set; }
        public DateTime Date { get; protected set; }  // Data da transação
        public TransactionType Type { get; protected set; }  // Compra, Venda, etc.
        public decimal Quantity { get; protected set; }  // Quantidade
        public decimal Price { get; protected set; }  // Preço unitário
        public decimal TotalValue { get; protected set; }  // Valor total
        public decimal Taxes { get; protected set; }  // Taxas e impostos
        public string Broker { get; protected set; }  // Corretora
        public string Notes { get; protected set; }   // Observações
        public DateTime CreatedAt { get; protected set; }

        public Guid InvestmentId { get; protected set; }
        public Investment Investment { get; protected set; }
    }
}
