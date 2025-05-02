namespace Equilibrium.Application.DTOs.Financing
{
    public class FinancingInstallmentSimulationDto
    {
        public int Number { get; set; }
        public decimal TotalAmount { get; set; }
        public decimal InterestAmount { get; set; }
        public decimal AmortizationAmount { get; set; }
        public decimal RemainingDebt { get; set; }
        public DateTime DueDate { get; set; }
    }
}
