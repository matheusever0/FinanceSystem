namespace Equilibrium.Domain.Interfaces.Services
{
    public interface IRecurrenceService
    {
        Task ProcessRecurringPaymentsAsync();
        Task ProcessRecurringIncomesAsync();
    }
}