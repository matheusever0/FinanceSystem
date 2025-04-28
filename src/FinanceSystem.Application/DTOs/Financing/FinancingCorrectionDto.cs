namespace FinanceSystem.Application.DTOs.Financing
{
    public class FinancingCorrectionDto
    {
        public Guid Id { get; set; }
        public decimal IndexValue { get; set; }
        public DateTime CorrectionDate { get; set; }
        public decimal PreviousDebt { get; set; }
        public decimal NewDebt { get; set; }
        public decimal DifferenceAmount => NewDebt - PreviousDebt;
        public decimal DifferencePercentage => PreviousDebt > 0 ? (DifferenceAmount / PreviousDebt) * 100 : 0;
        public DateTime CreatedAt { get; set; }
        public Guid FinancingId { get; set; }
    }
}
