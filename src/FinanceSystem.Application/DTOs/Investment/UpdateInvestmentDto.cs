using System.ComponentModel.DataAnnotations;

namespace FinanceSystem.Application.DTOs.Investment
{
    public class UpdateInvestmentDto
    {
        [StringLength(100)]
        public string Name { get; set; }
    }
}
