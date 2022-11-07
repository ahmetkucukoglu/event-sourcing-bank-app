using BankEs.Api;
using BankEs.Domain.BankAccount;
using BankEs.Domain.BankCustomer;
using BankEs.Domain.BankMoney;
using BankEs.Infrastructure;
using Microsoft.AspNetCore.Mvc;

var builder = WebApplication.CreateBuilder(args);
builder.Services.AddHostedService<AccountAggregateSubscriber>();
builder.Services.AddEventStoreClient("esdb://localhost:2113?tls=false");
builder.Services.AddTransient<AccountAggregateRepository>();

var app = builder.Build();

app.MapGet("/", () => "Hello!");

app.MapPost("/create",
    async ([FromBody] CreateAccountRequest request, AccountAggregateRepository aggregateRepository) =>
    {
        var account = await aggregateRepository.GetAsync(new AccountId(request.AccountId));
        account.CreateAccount(new CustomerId(Guid.NewGuid()), Currency.GetByName(request.Currency));

        await aggregateRepository.SaveAsync(account);
    });

app.MapPost("/deposit",
    async ([FromBody] DepositMoneyRequest request, AccountAggregateRepository aggregateRepository) =>
    {
        var account = await aggregateRepository.GetAsync(new AccountId(request.AccountId));
        account.DepositMoney(new Money(request.Amount, Currency.GetByName(request.Currency)));

        await aggregateRepository.SaveAsync(account);
    });

app.MapPost("/withdraw",
    async ([FromBody] WithdrawMoneyRequest request, AccountAggregateRepository aggregateRepository) =>
    {
        var account = await aggregateRepository.GetAsync(new AccountId(request.AccountId));
        account.WithdrawMoney(new Money(request.Amount, Currency.GetByName(request.Currency)));

        await aggregateRepository.SaveAsync(account);
    });

app.MapGet("/get/{id}", async (Guid id, AccountAggregateRepository aggregateRepository) =>
{
    var account = await aggregateRepository.GetAsync(new AccountId(id));

    return new GetAccountResponse(account.State.Balance.ToString());
});

app.Run();