namespace BankEs.Tests.E2ETests;

public class BankFixture : IDisposable
{
    private readonly BankApplication _application;
    public readonly HttpClient HttpClient;

    public BankFixture()
    {
        _application = new BankApplication(); 
        
        HttpClient = _application.CreateClient();
    }

    public void Dispose()
    {
        HttpClient.Dispose();

        _application.Dispose();
    }
}