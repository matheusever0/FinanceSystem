namespace FinanceSystem.Domain.Interfaces.Services
{
    public interface IRecurrenceService
    {
        Task ProcessRecurringPaymentsAsync();
        Task ProcessRecurringIncomesAsync();
    }
}