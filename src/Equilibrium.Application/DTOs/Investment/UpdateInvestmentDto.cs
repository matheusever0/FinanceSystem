using System.ComponentModel.DataAnnotations;

namespace Equilibrium.Application.DTOs.Investment
{
    public class UpdateInvestmentDto
    {
        [StringLength(100)]
        public required string Name { get; set; }
    }
}
