namespace BankEs.Domain.BankAccount;

public record AccountId
{
    public Guid Id { get; }

    public AccountId(Guid id)
    {
        ArgumentNullException.ThrowIfNull(id);
        
        Id = id;
    }

    public override string ToString() => Id.ToString();
}