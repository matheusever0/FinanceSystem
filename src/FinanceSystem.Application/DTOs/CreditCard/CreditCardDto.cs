namespace FinanceSystem.Application.DTOs
{
    public class CreditCardDto
    {
        public Guid Id { get; set; }
        public string Name { get; set; }
        public string LastFourDigits { get; set; }
        public string CardBrand { get; set; }
        public int ClosingDay { get; set; }
        public int DueDay { get; set; }
        public decimal Limit { get; set; }
        public decimal AvailableLimit { get; set; }
        public DateTime CreatedAt { get; set; }
        public DateTime? UpdatedAt { get; set; }
        public Guid UserId { get; set; }
        public Guid PaymentMethodId { get; set; }
        public string PaymentMethodName { get; set; }
    }
}
