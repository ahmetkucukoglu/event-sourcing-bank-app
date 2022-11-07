namespace BankEs.Domain.BankCustomer;

public class CustomerId
{
    public Guid Id { get; }

    public CustomerId(Guid id)
    {
        ArgumentNullException.ThrowIfNull(id);

        Id = id;
    }
    
    public override string ToString() => Id.ToString();
}