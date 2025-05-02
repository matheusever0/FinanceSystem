using Equilibrium.Domain.Enums;

namespace Equilibrium.Application.DTOs.Financing
{
    public class FinancingSimulationDto
    {
        public decimal TotalAmount { get; set; }
        public decimal InterestRate { get; set; }
        public int TermMonths { get; set; }
        public FinancingType Type { get; set; }
        public string TypeDescription => Type.ToString();
        public decimal FirstInstallmentAmount { get; set; }
        public decimal LastInstallmentAmount { get; set; }
        public decimal TotalInterest { get; set; }
        public decimal TotalCost { get; set; }
        public decimal MonthlyDecreaseAmount { get; set; } // Para SAC
        public List<FinancingInstallmentSimulationDto> Installments { get; set; } = [];
    }
}
