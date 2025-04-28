using System.ComponentModel.DataAnnotations;

namespace FinanceSystem.Application.DTOs.Financing
{
    public class FinancingForecastRequestDto
    {
        [Required]
        public Guid FinancingId { get; set; }

        [Required]
        [Range(1, 120, ErrorMessage = "O período de previsão deve estar entre 1 e 120 meses")]
        public int ForecastMonths { get; set; } = 12;

        public decimal OptimisticScenarioIndex { get; set; } = 0.003m; // 0.3% mensal
        public decimal RealisticScenarioIndex { get; set; } = 0.005m;  // 0.5% mensal
        public decimal PessimisticScenarioIndex { get; set; } = 0.008m; // 0.8% mensal
    }
}
