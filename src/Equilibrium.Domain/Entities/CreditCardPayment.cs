namespace Equilibrium.Domain.Entities
{
    public class CreditCardPayment
    {
        public Guid Id { get; protected set; }
        public decimal Amount { get; protected set; }
        public DateTime PaymentDate { get; protected set; }
        public string Notes { get; protected set; }
        public DateTime CreatedAt { get; protected set; }

        public Guid CreditCardId { get; protected set; }
        public CreditCard CreditCard { get; protected set; }

        public Guid UserId { get; protected set; }
        public User User { get; protected set; }

        protected CreditCardPayment()
        {
            Notes = string.Empty;
        }

        public CreditCardPayment(
            decimal amount,
            DateTime paymentDate,
            CreditCard creditCard,
            User user,
            string notes = "")
        {
            if (amount <= 0)
                throw new ArgumentException("Payment amount must be greater than zero");

            Id = Guid.NewGuid();
            Amount = amount;
            PaymentDate = paymentDate;
            Notes = notes ?? string.Empty;
            CreatedAt = DateTime.Now;

            CreditCardId = creditCard.Id;
            CreditCard = creditCard;

            UserId = user.Id;
            User = user;
        }

        public void UpdateNotes(string notes)
        {
            Notes = notes ?? string.Empty;
        }
    }
}