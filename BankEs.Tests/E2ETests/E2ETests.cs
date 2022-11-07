using System.Net.Http.Json;
using BankEs.Api;
using BankEs.Domain.BankMoney;

namespace BankEs.Tests.E2ETests;

public class E2ETests : IClassFixture<BankFixture>
{
    private readonly BankFixture _fixture;

    public E2ETests(BankFixture fixture)
    {
        _fixture = fixture;
    }

    [Fact]
    public async Task Should_Be_True_When_AddEvents()
    {
        var accountId = Guid.NewGuid();

        await CreateAccount(accountId, Currency.TL);

        await DepositMoney(accountId, 100M, Currency.TL);
        await WithdrawMoney(accountId, 50M, Currency.TL);

        await DepositMoney(accountId, 200M, Currency.TL);
        await WithdrawMoney(accountId, 100M, Currency.TL);

        await DepositMoney(accountId, 300M, Currency.TL);
        await WithdrawMoney(accountId, 150M, Currency.TL);

        await DepositMoney(accountId, 400M, Currency.TL);
        await WithdrawMoney(accountId, 200M, Currency.TL);

        await DepositMoney(accountId, 500M, Currency.TL);
        await WithdrawMoney(accountId, 250M, Currency.TL);

        await WithdrawMoney(accountId, 50M, Currency.TL);

        var account = await GetAccount(accountId);

        Assert.Equal("700₺", account?.Balance);
    }

    private async Task CreateAccount(Guid accountId, string currency)
    {
        await _fixture.HttpClient.PostAsJsonAsync("create",
            new CreateAccountRequest(accountId, currency));
    }

    private async Task DepositMoney(Guid accountId, decimal amount, string currency)
    {
        await _fixture.HttpClient.PostAsJsonAsync("deposit",
            new DepositMoneyRequest(accountId, amount, currency));
    }

    private async Task WithdrawMoney(Guid accountId, decimal amount, string currency)
    {
        await _fixture.HttpClient.PostAsJsonAsync("withdraw",
            new DepositMoneyRequest(accountId, amount, currency));
    }
    
    private async Task<GetAccountResponse?> GetAccount(Guid accountId)
    {
        return await _fixture.HttpClient.GetFromJsonAsync<GetAccountResponse>($"get/{accountId}");
    }
}