using Equilibrium.Domain.Enums;

namespace Equilibrium.Domain.Entities
{
    public class CreditCard
    {
        public Guid Id { get; protected set; }
        public string Name { get; protected set; }
        public string LastFourDigits { get; protected set; }
        public string CardBrand { get; protected set; }
        public int ClosingDay { get; protected set; }
        public int DueDay { get; protected set; }
        public decimal Limit { get; protected set; }
        public decimal AvailableLimit { get; protected set; }
        public DateTime CreatedAt { get; protected set; }
        public DateTime? UpdatedAt { get; protected set; }
        public Guid PaymentMethodId { get; protected set; }
        public PaymentMethod PaymentMethod { get; protected set; }
        public Guid UserId { get; protected set; }
        public User User { get; protected set; }

        protected CreditCard() { }

        public CreditCard(
            string name,
            string lastFourDigits,
            string cardBrand,
            int closingDay,
            int dueDay,
            decimal limit,
            User user,
            PaymentMethod paymentMethod)
        {
            if (closingDay < 1 || closingDay > 31)
                throw new ArgumentException("Closing day must be between 1 and 31");

            if (dueDay < 1 || dueDay > 31)
                throw new ArgumentException("Due day must be between 1 and 31");

            if (paymentMethod.Type != PaymentMethodType.CreditCard)
                throw new ArgumentException("Payment method must be of type Credit Card");

            Id = Guid.NewGuid();
            Name = name;
            LastFourDigits = lastFourDigits;
            CardBrand = cardBrand;
            ClosingDay = closingDay;
            DueDay = dueDay;
            Limit = limit;
            AvailableLimit = limit;
            CreatedAt = DateTime.Now;

            UserId = user.Id;
            User = user;

            PaymentMethodId = paymentMethod.Id;
            PaymentMethod = paymentMethod;
        }

        public void UpdateName(string name)
        {
            Name = name;
            UpdatedAt = DateTime.Now;
        }

        public void UpdateDays(int closingDay, int dueDay)
        {
            if (closingDay < 1 || closingDay > 31)
                throw new ArgumentException("Closing day must be between 1 and 31");

            if (dueDay < 1 || dueDay > 31)
                throw new ArgumentException("Due day must be between 1 and 31");

            ClosingDay = closingDay;
            DueDay = dueDay;
            UpdatedAt = DateTime.Now;
        }

        public void UpdateLimit(decimal limit)
        {
            var difference = limit - Limit;
            Limit = limit;
            AvailableLimit += difference;
            UpdatedAt = DateTime.Now;
        }

        public void DecrementAvailableLimit(decimal amount)
        {
            if (amount > AvailableLimit)
                throw new InvalidOperationException("Insufficient available limit");

            AvailableLimit -= amount;
            UpdatedAt = DateTime.Now;
        }

        public void IncrementAvailableLimit(decimal amount)
        {
            if (AvailableLimit + amount > Limit)
                AvailableLimit = Limit;
            else
                AvailableLimit += amount;

            UpdatedAt = DateTime.Now;
        }
    }
}