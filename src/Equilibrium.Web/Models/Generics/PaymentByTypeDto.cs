namespace Equilibrium.Web.Models.Generics
{
    public class PaymentByTypeDto
    {
        public required string TypeId { get; set; }
        public required string TypeName { get; set; }
        public decimal TotalAmount { get; set; }
    }
}
