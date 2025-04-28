using FinanceSystem.Domain.Enums;

namespace FinanceSystem.Application.DTOs.Financing
{
    public class FinancingForecastDto
    {
        public Guid FinancingId { get; set; }
        public string FinancingDescription { get; set; }
        public decimal CurrentDebt { get; set; }
        public decimal BaseInterestRate { get; set; }
        public CorrectionIndexType CorrectionIndex { get; set; }
        public string CorrectionIndexDescription => CorrectionIndex.ToString();
        public List<FinancingForecastScenarioDto> Scenarios { get; set; } = [];
    }
}
