using FinanceSystem.Domain.Enums;

namespace FinanceSystem.Domain.Entities
{
    public class Investment
    {
        public Guid Id { get; protected set; }
        public string Symbol { get; protected set; }  
        public string Name { get; protected set; }  
        public InvestmentType Type { get; protected set; }  
        public decimal TotalQuantity { get; protected set; } 
        public decimal AveragePrice { get; protected set; }  
        public decimal CurrentPrice { get; protected set; }  
        public decimal TotalInvested { get; protected set; } 
        public decimal CurrentTotal { get; protected set; } 
        public decimal GainLossPercentage { get; protected set; } 
        public decimal GainLossValue { get; protected set; } 
        public DateTime LastUpdate { get; protected set; }  
        public Guid UserId { get; protected set; }
        public User User { get; protected set; }

        public ICollection<InvestmentTransaction> Transactions { get; protected set; }

        protected Investment()
        {
            Transactions = [];
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
            Transactions = [];
        }

        public void UpdateCurrentPrice(decimal newPrice)
        {
            CurrentPrice = newPrice;
            RecalculateValues();
            LastUpdate = DateTime.Now;
        }

        private void RecalculateValues()
        {
            CurrentTotal = TotalQuantity * CurrentPrice;
            GainLossValue = CurrentTotal - TotalInvested;
            GainLossPercentage = TotalInvested > 0 ? (GainLossValue / TotalInvested) * 100 : 0;
        }

        public void AddTransaction(
            DateTime date,
            TransactionType type,
            decimal quantity,
            decimal price,
            decimal totalValue,
            decimal taxes,
            string broker,
            string notes,
            bool isInitial = false)
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

            if (!isInitial)
            {
                RecalculateAfterTransaction(transaction);
            }
        }

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
                    break;
                case TransactionType.Split:
                    RecalculateAfterSplit(transaction.Quantity);
                    break;
                case TransactionType.Bonus:
                    RecalculateAfterBonus(transaction.Quantity);
                    break;
                case TransactionType.Yield:
                    break;
            }

            RecalculateValues();
        }

        private void RecalculateAfterBuy(decimal quantity, decimal price)
        {
            decimal buyValue = quantity * price;

            TotalQuantity += quantity;
            TotalInvested += buyValue;

            if (TotalQuantity > 0)
                AveragePrice = TotalInvested / TotalQuantity;
        }

        private void RecalculateAfterSell(decimal quantity)
        {
            TotalQuantity -= quantity;

            if (TotalQuantity >= 0)
                TotalInvested = TotalQuantity * AveragePrice;
        }

        private void RecalculateAfterSplit(decimal factor)
        {
            TotalQuantity *= factor;
            AveragePrice /= factor;
        }

        private void RecalculateAfterBonus(decimal additionalShares)
        {
            decimal totalBefore = TotalQuantity * AveragePrice;
            TotalQuantity += additionalShares;

            if (TotalQuantity > 0)
                AveragePrice = totalBefore / TotalQuantity;
        }

        public void RecalculateWithoutTransaction(InvestmentTransaction transaction)
        {
            TotalQuantity = 0;
            TotalInvested = 0;
            AveragePrice = 0;

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

        public void UpdateName(string name)
        {
            Name = name;
        }
    }
}
