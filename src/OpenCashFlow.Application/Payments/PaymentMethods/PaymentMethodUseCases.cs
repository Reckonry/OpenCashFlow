using OpenCashFlow.Application.Payments.Lookups;
using OpenCashFlow.Application.Payments.Ports;
using OpenCashFlow.Domain.Common;

namespace OpenCashFlow.Application.Payments.PaymentMethods;

public sealed record GetPaymentMethodsQuery(Guid TenantId);
public sealed record GetPaymentMethodDetailQuery(Guid PaymentMethodId, Guid TenantId);
public sealed record DeletePaymentMethodCommand(Guid PaymentMethodId, Guid TenantId);

public interface IGetPaymentMethodsUseCase
{
    Task<IReadOnlyList<PaymentMethodListItem>> ExecuteAsync(GetPaymentMethodsQuery query, CancellationToken cancellationToken = default);
}

public interface IGetPaymentMethodDetailUseCase
{
    Task<PaymentMethodResult?> ExecuteAsync(GetPaymentMethodDetailQuery query, CancellationToken cancellationToken = default);
}

public interface ICreatePaymentMethodUseCase
{
    Task<PaymentMethodResult?> ExecuteAsync(PaymentMethodCreateCommand command, CancellationToken cancellationToken = default);
}

public interface IUpdatePaymentMethodUseCase
{
    Task<PaymentMethodResult?> ExecuteAsync(PaymentMethodUpdateCommand command, CancellationToken cancellationToken = default);
}

public interface IDeletePaymentMethodUseCase
{
    Task<bool> ExecuteAsync(DeletePaymentMethodCommand command, CancellationToken cancellationToken = default);
}

public sealed class GetPaymentMethodsUseCase(IPaymentMethodReader reader) : IGetPaymentMethodsUseCase
{
    public Task<IReadOnlyList<PaymentMethodListItem>> ExecuteAsync(GetPaymentMethodsQuery query, CancellationToken cancellationToken = default)
    {
        var tenantId = TenantId.From(query.TenantId);
        return reader.GetAllAsync(tenantId.Value, cancellationToken);
    }
}

public sealed class GetPaymentMethodDetailUseCase(IPaymentMethodReader reader) : IGetPaymentMethodDetailUseCase
{
    public Task<PaymentMethodResult?> ExecuteAsync(GetPaymentMethodDetailQuery query, CancellationToken cancellationToken = default)
    {
        var tenantId = TenantId.From(query.TenantId);
        var paymentMethodId = PaymentMethodUseCaseValidation.RequireGuid(query.PaymentMethodId, nameof(query.PaymentMethodId));
        return reader.GetByIdAsync(paymentMethodId, tenantId.Value, cancellationToken);
    }
}

public sealed class CreatePaymentMethodUseCase(IPaymentMethodWriter writer) : ICreatePaymentMethodUseCase
{
    public Task<PaymentMethodResult?> ExecuteAsync(PaymentMethodCreateCommand command, CancellationToken cancellationToken = default)
    {
        var normalized = PaymentMethodUseCaseValidation.Normalize(command);
        return writer.CreateAsync(normalized, cancellationToken);
    }
}

public sealed class UpdatePaymentMethodUseCase(IPaymentMethodWriter writer) : IUpdatePaymentMethodUseCase
{
    public Task<PaymentMethodResult?> ExecuteAsync(PaymentMethodUpdateCommand command, CancellationToken cancellationToken = default)
    {
        var normalized = PaymentMethodUseCaseValidation.Normalize(command);
        return writer.UpdateAsync(normalized, cancellationToken);
    }
}

public sealed class DeletePaymentMethodUseCase(IPaymentMethodWriter writer) : IDeletePaymentMethodUseCase
{
    public Task<bool> ExecuteAsync(DeletePaymentMethodCommand command, CancellationToken cancellationToken = default)
    {
        var tenantId = TenantId.From(command.TenantId);
        var paymentMethodId = PaymentMethodUseCaseValidation.RequireGuid(command.PaymentMethodId, nameof(command.PaymentMethodId));
        return writer.DeleteAsync(paymentMethodId, tenantId.Value, cancellationToken);
    }
}

file static class PaymentMethodUseCaseValidation
{
    public static PaymentMethodCreateCommand Normalize(PaymentMethodCreateCommand command)
    {
        var tenantId = TenantId.From(command.TenantId);
        var userId = UserId.From(command.UserId);
        var paymentMethodId = command.PaymentMethodId == Guid.Empty ? Guid.NewGuid() : command.PaymentMethodId;
        var name = RequireName(command.Name);

        return command with
        {
            PaymentMethodId = paymentMethodId,
            TenantId = tenantId.Value,
            UserId = userId.Value,
            Name = name,
            Description = command.Description?.Trim(),
            Icon = command.Icon?.Trim()
        };
    }

    public static PaymentMethodUpdateCommand Normalize(PaymentMethodUpdateCommand command)
    {
        var tenantId = TenantId.From(command.TenantId);
        var userId = UserId.From(command.UserId);
        var paymentMethodId = RequireGuid(command.PaymentMethodId, nameof(command.PaymentMethodId));
        var name = RequireName(command.Name);

        return command with
        {
            PaymentMethodId = paymentMethodId,
            TenantId = tenantId.Value,
            UserId = userId.Value,
            Name = name,
            Description = command.Description?.Trim(),
            Icon = command.Icon?.Trim()
        };
    }

    public static Guid RequireGuid(Guid value, string parameterName)
    {
        if (value == Guid.Empty)
        {
            throw new ArgumentException("Identifier is required.", parameterName);
        }

        return value;
    }

    private static string RequireName(string? name)
    {
        if (string.IsNullOrWhiteSpace(name))
        {
            throw new ArgumentException("Name is required.", nameof(name));
        }

        return name.Trim();
    }
}
