using Microsoft.EntityFrameworkCore;
using OpenCashFlow.Application.Health.Ports;
using OpenCashFlow.Infrastructure.Persistence;

namespace OpenCashFlow.Infrastructure.Health;

public sealed class DatabaseHealthReader(ApplicationDbContext db) : IDatabaseHealthReader
{
    public Task<bool> CanConnectAsync(CancellationToken cancellationToken = default)
        => db.Database.CanConnectAsync(cancellationToken);
}

