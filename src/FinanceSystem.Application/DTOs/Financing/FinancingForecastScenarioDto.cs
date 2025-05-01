namespace FinanceSystem.Application.DTOs.Financing
{
    public class FinancingForecastScenarioDto
    {
        public string Name { get; set; } 
        public decimal ProjectedIndexValue { get; set; }
        public decimal ProjectedFinalDebt { get; set; }
        public decimal ProjectedTotalCost { get; set; }
        public decimal DifferenceFromCurrent { get; set; }
        public decimal DifferencePercentage { get; set; }
        public List<FinancingForecastInstallmentDto> Installments { get; set; } = [];
    }
}
