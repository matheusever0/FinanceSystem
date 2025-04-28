namespace FinanceSystem.Domain.Entities
{
    public class FinancingCorrection
    {
        public Guid Id { get; protected set; }
        public decimal IndexValue { get; protected set; }     // Valor do índice aplicado
        public DateTime CorrectionDate { get; protected set; } // Data da correção
        public decimal PreviousDebt { get; protected set; }    // Saldo devedor antes da correção
        public decimal NewDebt { get; protected set; }         // Saldo devedor após a correção
        public DateTime CreatedAt { get; protected set; }

        public Guid FinancingId { get; protected set; }
        public Financing Financing { get; protected set; }

        protected FinancingCorrection() { }

        public FinancingCorrection(
            Financing financing,
            decimal indexValue,
            DateTime correctionDate)
        {
            Id = Guid.NewGuid();
            IndexValue = indexValue;
            CorrectionDate = correctionDate;
            PreviousDebt = financing.RemainingDebt;
            NewDebt = financing.RemainingDebt * (1 + indexValue);
            CreatedAt = DateTime.Now;

            FinancingId = financing.Id;
            Financing = financing;
        }
    }
}