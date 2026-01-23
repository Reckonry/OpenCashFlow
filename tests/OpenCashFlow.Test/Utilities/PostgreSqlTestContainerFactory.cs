using Testcontainers.PostgreSql;

public class PostgreSqlTestContainerFactory
{
    private readonly PostgreSqlContainer _container;

    public string ConnectionString => _container.GetConnectionString();

    public PostgreSqlTestContainerFactory()
    {
        _container = new PostgreSqlBuilder()
            .WithImage("postgres:16")
            .WithDatabase("testdb")
            .WithUsername("testuser")
            .WithPassword("testpassword")
            .WithCleanUp(true)
            .Build();
    }

    public async Task StartAsync()
    {
        await _container.StartAsync();
    }

    public async Task StopAsync()
    {
        await _container.DisposeAsync();
    }
}
