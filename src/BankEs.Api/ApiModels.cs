namespace BankEs.Api;

public record CreateAccountRequest(Guid AccountId, string Currency);
public record DepositMoneyRequest(Guid AccountId, decimal Amount, string Currency);
public record WithdrawMoneyRequest(Guid AccountId, decimal Amount, string Currency);
public record GetAccountResponse(string Balance);