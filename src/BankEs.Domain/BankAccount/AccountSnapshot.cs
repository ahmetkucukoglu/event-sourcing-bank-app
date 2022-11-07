namespace BankEs.Domain.BankAccount;

public record AccountSnapshot(AccountState State, long Version);