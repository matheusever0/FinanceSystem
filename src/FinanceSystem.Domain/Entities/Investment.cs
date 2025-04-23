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

        protected Investment()
        {
            Transactions = new List<InvestmentTransaction>();
        }

        public Investment(
            string symbol,
            string name,
            InvestmentType type,
            decimal totalQuantity,
            decimal averagePrice,
            decimal currentPrice,
            decimal totalInvested,
            decimal currentTotal,
            decimal gainLossPercentage,
            decimal gainLossValue,
            User user)
        {
            Id = Guid.NewGuid();
            Symbol = symbol;
            Name = name;
            Type = type;
            TotalQuantity = totalQuantity;
            AveragePrice = averagePrice;
            CurrentPrice = currentPrice;
            TotalInvested = totalInvested;
            CurrentTotal = currentTotal;
            GainLossPercentage = gainLossPercentage;
            GainLossValue = gainLossValue;
            UserId = user.Id;
            User = user;
            LastUpdate = DateTime.Now;
            Transactions = new List<InvestmentTransaction>();
        }

        // Método para atualizar o preço atual
        public void UpdateCurrentPrice(decimal newPrice)
        {
            CurrentPrice = newPrice;
            RecalculateValues();
            LastUpdate = DateTime.Now;
        }

        // Método para recalcular valores após atualização de preço
        private void RecalculateValues()
        {
            CurrentTotal = TotalQuantity * CurrentPrice;
            GainLossValue = CurrentTotal - TotalInvested;
            GainLossPercentage = TotalInvested > 0 ? (GainLossValue / TotalInvested) * 100 : 0;
        }

        // Método para adicionar uma transação
        public void AddTransaction(
            DateTime date,
            TransactionType type,
            decimal quantity,
            decimal price,
            decimal totalValue,
            decimal taxes,
            string broker,
            string notes)
        {
            var transaction = new InvestmentTransaction(
                date,
                type,
                quantity,
                price,
                totalValue,
                taxes,
                broker,
                notes,
                this);

            Transactions.Add(transaction);
            RecalculateAfterTransaction(transaction);
        }

        // Método para recalcular após nova transação
        public void RecalculateAfterTransaction(InvestmentTransaction transaction)
        {
            switch (transaction.Type)
            {
                case TransactionType.Buy:
                    RecalculateAfterBuy(transaction.Quantity, transaction.Price);
                    break;
                case TransactionType.Sell:
                    RecalculateAfterSell(transaction.Quantity);
                    break;
                case TransactionType.Dividend:
                case TransactionType.JCP:
                    // Não altera quantidade nem preço médio
                    break;
                case TransactionType.Split:
                    RecalculateAfterSplit(transaction.Quantity);
                    break;
                case TransactionType.Bonus:
                    RecalculateAfterBonus(transaction.Quantity);
                    break;
                case TransactionType.Yield:
                    // Não altera quantidade nem preço médio para rendimentos
                    break;
            }

            RecalculateValues();
        }

        // Método para recalcular após compra
        private void RecalculateAfterBuy(decimal quantity, decimal price)
        {
            decimal totalBefore = TotalQuantity * AveragePrice;
            decimal buyValue = quantity * price;

            TotalQuantity += quantity;
            TotalInvested += buyValue;

            if (TotalQuantity > 0)
                AveragePrice = TotalInvested / TotalQuantity;
        }

        // Método para recalcular após venda
        private void RecalculateAfterSell(decimal quantity)
        {
            // Para venda, diminui a quantidade mas não altera o preço médio
            TotalQuantity -= quantity;

            // Atualiza o valor investido proporcionalmente
            if (TotalQuantity >= 0)
                TotalInvested = TotalQuantity * AveragePrice;
        }

        // Método para recalcular após desdobramento (split)
        private void RecalculateAfterSplit(decimal factor)
        {
            TotalQuantity *= factor;
            AveragePrice /= factor;
        }

        // Método para recalcular após bonificação
        private void RecalculateAfterBonus(decimal additionalShares)
        {
            decimal totalBefore = TotalQuantity * AveragePrice;
            TotalQuantity += additionalShares;

            // O preço médio diminui pois a quantidade aumentou sem novo investimento
            if (TotalQuantity > 0)
                AveragePrice = totalBefore / TotalQuantity;
        }

        // Método para recalcular sem uma transação (usado ao deletar uma transação)
        public void RecalculateWithoutTransaction(InvestmentTransaction transaction)
        {
            // Recalcular todos os valores a partir das transações restantes
            TotalQuantity = 0;
            TotalInvested = 0;
            AveragePrice = 0;

            // Ordenar as transações por data
            var orderedTransactions = Transactions
                .Where(t => t.Id != transaction.Id)
                .OrderBy(t => t.Date)
                .ToList();

            foreach (var tx in orderedTransactions)
            {
                RecalculateAfterTransaction(tx);
            }

            RecalculateValues();
        }

        // Método para atualizar o nome
        public void UpdateName(string name)
        {
            Name = name;
        }
    }
}
