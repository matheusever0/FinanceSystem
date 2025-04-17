using FinanceSystem.Domain.Entities;

namespace FinanceSystem.Domain.Entities
{
    public class IncomeType
    {
        public Guid Id { get; protected set; }
        public string Name { get; protected set; }
        public string Description { get; protected set; }
        public bool IsSystem { get; protected set; }
        public Guid? UserId { get; protected set; }
        public User User { get; protected set; }
        public DateTime CreatedAt { get; protected set; }
        public ICollection<Income> Incomes { get; protected set; }

        protected IncomeType()
        {
            Incomes = new List<Income>();
        }

        // Construtor para tipos de sistema
        public IncomeType(string name, string description)
        {
            Id = Guid.NewGuid();
            Name = name;
            Description = description;
            IsSystem = true;
            CreatedAt = DateTime.UtcNow;
            Incomes = new List<Income>();
        }

        // Construtor para tipos personalizados do usuário
        public IncomeType(string name, string description, User user)
        {
            Id = Guid.NewGuid();
            Name = name;
            Description = description;
            IsSystem = false;
            UserId = user.Id;
            User = user;
            CreatedAt = DateTime.UtcNow;
            Incomes = new List<Income>();
        }

        public void UpdateName(string name)
        {
            if (IsSystem)
                throw new InvalidOperationException("Cannot update system income type");

            Name = name;
        }

        public void UpdateDescription(string description)
        {
            if (IsSystem)
                throw new InvalidOperationException("Cannot update system income type");

            Description = description;
        }
    }
}