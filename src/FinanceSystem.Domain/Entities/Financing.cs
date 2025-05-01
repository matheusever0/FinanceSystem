using FinanceSystem.Domain.Enums;

namespace FinanceSystem.Domain.Entities
{
    public class Financing
    {
        public Guid Id { get; protected set; }
        public string Description { get; protected set; }
        public decimal TotalAmount { get; protected set; }  // Valor total financiado
        public decimal InterestRate { get; protected set; } // Taxa de juros anual
        public int TermMonths { get; protected set; }       // Prazo em meses
        public DateTime StartDate { get; protected set; }   // Data de início
        public DateTime? EndDate { get; protected set; }    // Data de término (calculada ou efetiva)
        public FinancingType Type { get; protected set; }   // Tipo de sistema de amortização
        public FinancingStatus Status { get; protected set; }
        public CorrectionIndexType CorrectionIndex { get; protected set; } // Índice de correção
        public string Notes { get; protected set; }
        public DateTime CreatedAt { get; protected set; }
        public DateTime? UpdatedAt { get; protected set; }

        public Guid UserId { get; protected set; }
        public User User { get; protected set; }

        public decimal RemainingDebt { get; protected set; } // Saldo devedor atual
        public DateTime LastCorrectionDate { get; protected set; }

        public ICollection<FinancingInstallment> Installments { get; protected set; }
        public ICollection<Payment> Payments { get; protected set; }

        protected Financing()
        {
            Installments = new List<FinancingInstallment>();
            Payments = new List<Payment>();
        }

        public Financing(
            string description,
            decimal totalAmount,
            decimal interestRate,
            int termMonths,
            DateTime startDate,
            FinancingType type,
            CorrectionIndexType correctionIndex,
            User user,
            string notes = "")
        {
            Id = Guid.NewGuid();
            Description = description;
            TotalAmount = totalAmount;
            InterestRate = interestRate;
            TermMonths = termMonths;
            StartDate = startDate;
            Type = type;
            Status = FinancingStatus.Active;
            CorrectionIndex = correctionIndex;
            Notes = notes;
            CreatedAt = DateTime.Now;

            UserId = user.Id;
            User = user;

            RemainingDebt = totalAmount;
            LastCorrectionDate = startDate;

            Installments = new List<FinancingInstallment>();
            Payments = new List<Payment>();

            CalculateInstallments();
        }

        public void UpdateDescription(string description)
        {
            Description = description;
            UpdatedAt = DateTime.Now;
        }

        public void UpdateNotes(string notes)
        {
            Notes = notes;
            UpdatedAt = DateTime.Now;
        }

        public void Complete()
        {
            Status = FinancingStatus.Completed;
            EndDate = DateTime.Now;
            RemainingDebt = 0;
            UpdatedAt = DateTime.Now;
        }

        public void Cancel()
        {
            Status = FinancingStatus.Cancelled;
            UpdatedAt = DateTime.Now;
        }

        public void ApplyCorrection(decimal indexValue, DateTime correctionDate)
        {
            if (Status != FinancingStatus.Active)
                throw new InvalidOperationException("Cannot apply correction to a non-active financing");

            if (correctionDate <= LastCorrectionDate)
                throw new InvalidOperationException("Correction date must be after the last correction date");

            // Aplicar correção ao saldo devedor
            RemainingDebt *= (1 + indexValue);

            // Ajustar parcelas futuras
            RecalculateRemainingInstallments(correctionDate);

            LastCorrectionDate = correctionDate;
            UpdatedAt = DateTime.Now;
        }

        public void RecalculateRemainingInstallments(DateTime referenceDate)
        {
            // Identificar parcelas futuras não pagas
            var futureInstallments = Installments
                .Where(i => i.DueDate > referenceDate && i.Status == FinancingInstallmentStatus.Pending)
                .OrderBy(i => i.InstallmentNumber)
                .ToList();

            if (!futureInstallments.Any())
                return;

            if (Type == FinancingType.PRICE)
            {
                RecalculatePRICEInstallments(futureInstallments);
            }
            else // SAC
            {
                RecalculateSACInstallments(futureInstallments);
            }
        }

        private void RecalculatePRICEInstallments(List<FinancingInstallment> installments)
        {
            int remainingMonths = installments.Count;
            decimal monthlyRate = InterestRate / 12 / 100; // Taxa mensal em formato decimal

            // Fórmula PRICE: PMT = VP × [i × (1 + i)^n] / [(1 + i)^n - 1]
            decimal factor = monthlyRate * (decimal)Math.Pow((double)(1 + monthlyRate), remainingMonths) /
                             ((decimal)Math.Pow((double)(1 + monthlyRate), remainingMonths) - 1);

            decimal installmentAmount = RemainingDebt * factor;

            decimal currentDebt = RemainingDebt;

            foreach (var installment in installments)
            {
                decimal interest = currentDebt * monthlyRate;
                decimal amortization = installmentAmount - interest;

                installment.UpdateValues(installmentAmount, interest, amortization);
                installment.MarkAsAdjusted();

                currentDebt -= amortization;
            }
        }

        private void RecalculateSACInstallments(List<FinancingInstallment> installments)
        {
            int remainingMonths = installments.Count;
            decimal monthlyRate = InterestRate / 12 / 100; // Taxa mensal em formato decimal

            // No SAC, a amortização é constante
            decimal constantAmortization = RemainingDebt / remainingMonths;

            decimal currentDebt = RemainingDebt;

            foreach (var installment in installments)
            {
                decimal interest = currentDebt * monthlyRate;
                decimal totalAmount = constantAmortization + interest;

                installment.UpdateValues(totalAmount, interest, constantAmortization);
                installment.MarkAsAdjusted();

                currentDebt -= constantAmortization;
            }
        }

        private void CalculateInstallments()
        {
            decimal monthlyRate = InterestRate / 12 / 100; // Taxa mensal em formato decimal

            if (Type == FinancingType.PRICE)
            {
                // Fórmula PRICE: PMT = VP × [i × (1 + i)^n] / [(1 + i)^n - 1]
                decimal factor = monthlyRate * (decimal)Math.Pow((double)(1 + monthlyRate), TermMonths) /
                                 ((decimal)Math.Pow((double)(1 + monthlyRate), TermMonths) - 1);

                decimal installmentAmount = TotalAmount * factor;
                decimal currentDebt = TotalAmount;

                for (int i = 1; i <= TermMonths; i++)
                {
                    decimal interest = currentDebt * monthlyRate;
                    decimal amortization = installmentAmount - interest;

                    var dueDate = StartDate.AddMonths(i);

                    var installment = new FinancingInstallment(
                        this,
                        i,
                        installmentAmount,
                        interest,
                        amortization,
                        dueDate
                    );

                    Installments.Add(installment);

                    currentDebt -= amortization;
                }
            }
            else // SAC
            {
                decimal constantAmortization = TotalAmount / TermMonths;
                decimal currentDebt = TotalAmount;

                for (int i = 1; i <= TermMonths; i++)
                {
                    decimal interest = currentDebt * monthlyRate;
                    decimal totalAmount = constantAmortization + interest;

                    var dueDate = StartDate.AddMonths(i);

                    var installment = new FinancingInstallment(
                        this,
                        i,
                        totalAmount,
                        interest,
                        constantAmortization,
                        dueDate
                    );

                    Installments.Add(installment);

                    currentDebt -= constantAmortization;
                }
            }

            // Calcular data de término
            EndDate = StartDate.AddMonths(TermMonths);
        }

        public void UpdateRemainingDebt(decimal payment)
        {
            RemainingDebt -= payment;
            if (RemainingDebt <= 0)
            {
                RemainingDebt = 0;
                Complete();
            }

            UpdatedAt = DateTime.Now;
        }

        public void RestoreRemainingDebt(decimal amount)
        {
            RemainingDebt += amount;

            // If financing was completed but now has debt again, reactivate it
            if (Status == FinancingStatus.Completed && RemainingDebt > 0)
            {
                Status = FinancingStatus.Active;
                EndDate = null;
            }

            UpdatedAt = DateTime.Now;
        }
    }
}