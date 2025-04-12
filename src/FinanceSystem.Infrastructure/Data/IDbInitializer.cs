namespace FinanceSystem.Infrastructure.Data
{
    public interface IDbInitializer
    {
        Task Initialize();
        Task SeedData();
    }
}