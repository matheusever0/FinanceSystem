using FinanceSystem.Domain.Enums;
using System.ComponentModel.DataAnnotations;

namespace FinanceSystem.Application.DTOs.Investment
{
    public class CreateInvestmentDto
    {
        [Required]
        [StringLength(20)]
        public string Symbol { get; set; }

        [Required]
        [StringLength(100)]
        public string Name { get; set; }

        [Required]
        public InvestmentType Type { get; set; }

        [Required]
        [Range(0.01, double.MaxValue)]
        public decimal InitialQuantity { get; set; }

        [Required]
        [Range(0.01, double.MaxValue)]
        public decimal InitialPrice { get; set; }

        [StringLength(100)]
        public string Broker { get; set; }

        [DataType(DataType.Date)]
        public DateTime TransactionDate { get; set; } = DateTime.Now;
    }
}
