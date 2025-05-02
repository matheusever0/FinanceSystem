namespace Equilibrium.Application.DTOs.CreditCard
{
    public class CreditCardDto
    {
        public Guid Id { get; set; }
        public required string Name { get; set; }
        public required string LastFourDigits { get; set; }
        public required string CardBrand { get; set; }
        public int ClosingDay { get; set; }
        public int DueDay { get; set; }
        public decimal Limit { get; set; }
        public decimal AvailableLimit { get; set; }
        public DateTime CreatedAt { get; set; }
        public DateTime? UpdatedAt { get; set; }
        public Guid UserId { get; set; }
        public Guid PaymentMethodId { get; set; }
        public required string PaymentMethodName { get; set; }
    }
}
