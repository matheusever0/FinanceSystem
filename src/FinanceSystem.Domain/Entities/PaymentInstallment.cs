using FinanceSystem.Domain.Enums;

public class PaymentInstallment
{
    public Guid Id { get; protected set; }
    public int InstallmentNumber { get; protected set; }
    public decimal Amount { get; protected set; }
    public DateTime DueDate { get; protected set; }
    public DateTime? PaymentDate { get; protected set; }
    public PaymentStatus Status { get; protected set; }
    public DateTime CreatedAt { get; protected set; }
    public DateTime? UpdatedAt { get; protected set; }
    public Guid PaymentId { get; protected set; }
    public Payment Payment { get; protected set; }

    protected PaymentInstallment() { }

    public PaymentInstallment(
        Payment payment,
        int installmentNumber,
        decimal amount,
        DateTime dueDate)
    {
        Id = Guid.NewGuid();
        InstallmentNumber = installmentNumber;
        Amount = amount;
        DueDate = dueDate;
        Status = PaymentStatus.Pending;
        CreatedAt = DateTime.UtcNow;

        PaymentId = payment.Id;
        Payment = payment;
    }

    public void MarkAsPaid(DateTime paymentDate)
    {
        PaymentDate = paymentDate;
        Status = PaymentStatus.Paid;
        UpdatedAt = DateTime.UtcNow;
    }

    public void MarkAsOverdue()
    {
        Status = PaymentStatus.Overdue;
        UpdatedAt = DateTime.UtcNow;
    }

    public void Cancel()
    {
        Status = PaymentStatus.Cancelled;
        UpdatedAt = DateTime.UtcNow;
    }
}