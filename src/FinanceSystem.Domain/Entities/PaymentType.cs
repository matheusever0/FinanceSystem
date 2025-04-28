using FinanceSystem.Domain.Entities;

public class PaymentType
{
    public Guid Id { get; protected set; }
    public string Name { get; protected set; }
    public string Description { get; protected set; }
    public bool IsSystem { get; protected set; }
    public Guid? UserId { get; protected set; }
    public User User { get; protected set; }
    public DateTime CreatedAt { get; protected set; }
    public ICollection<Payment> Payments { get; protected set; }
    public bool IsFinancingType { get; protected set; }

    protected PaymentType()
    {
        Payments = [];
    }

    public PaymentType(string name, string description)
    {
        Id = Guid.NewGuid();
        Name = name;
        Description = description;
        IsSystem = true;
        CreatedAt = DateTime.Now;
        IsFinancingType = false;
        Payments = [];
    }

    public PaymentType(string name, string description, bool isFinancingType)
    {
        Id = Guid.NewGuid();
        Name = name;
        Description = description;
        IsSystem = false;
        CreatedAt = DateTime.Now;
        IsFinancingType = isFinancingType;
        Payments = [];
    }

    public PaymentType(string name, string description, User user, bool isFinancingType)
    {
        Id = Guid.NewGuid();
        Name = name;
        Description = description;
        IsSystem = false;
        UserId = user.Id;
        User = user;
        CreatedAt = DateTime.Now;
        IsFinancingType = isFinancingType;
        Payments = [];
    }

    public PaymentType(string name, string description, User user)
    {
        Id = Guid.NewGuid();
        Name = name;
        Description = description;
        IsSystem = false;
        UserId = user.Id;
        User = user;
        CreatedAt = DateTime.Now;
        IsFinancingType = false;
        Payments = [];
    }

    public void UpdateName(string name)
    {
        if (IsSystem)
            throw new InvalidOperationException("Cannot update system payment type");

        Name = name;
    }

    public void UpdateDescription(string description)
    {
        if (IsSystem)
            throw new InvalidOperationException("Cannot update system payment type");

        Description = description;
    }
}