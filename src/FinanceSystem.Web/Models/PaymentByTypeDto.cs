namespace FinanceSystem.Web.Models
{
    public class PaymentByTypeDto
    {
        public string TypeId { get; set; }
        public string TypeName { get; set; }
        public decimal TotalAmount { get; set; }
    }
}
