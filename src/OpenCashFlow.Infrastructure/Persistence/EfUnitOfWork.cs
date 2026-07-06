using Microsoft.EntityFrameworkCore.Storage;
using OpenCashFlow.Application.Abstractions;
using global::Shared.Data;

namespace OpenCashFlow.Infrastructure.Persistence;

public sealed class EfUnitOfWork(ApplicationDbContext db) : IUnitOfWork
{
    public async Task<T> ExecuteInTransactionAsync<T>(
        Func<CancellationToken, Task<T>> operation,
        CancellationToken cancellationToken = default)
    {
        if (db.Database.CurrentTransaction != null)
        {
            return await operation(cancellationToken);
        }

        await using IDbContextTransaction transaction = await db.Database.BeginTransactionAsync(cancellationToken);
        try
        {
            var result = await operation(cancellationToken);
            await transaction.CommitAsync(cancellationToken);
            return result;
        }
        catch
        {
            await transaction.RollbackAsync(cancellationToken);
            throw;
        }
    }
}
