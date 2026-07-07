using OpenCashFlow.Application.Payments.Lookups;
using OpenCashFlow.Application.Payments.Ports;
using OpenCashFlow.Domain.Common;

namespace OpenCashFlow.Application.Payments.DocumentTypes;

public sealed record GetDocumentTypesQuery(Guid TenantId);
public sealed record GetDocumentTypeDetailQuery(Guid DocumentTypeId, Guid TenantId);
public sealed record DeleteDocumentTypeCommand(Guid DocumentTypeId, Guid TenantId);

public interface IGetDocumentTypesUseCase
{
    Task<IReadOnlyList<DocumentTypeListItem>> ExecuteAsync(GetDocumentTypesQuery query, CancellationToken cancellationToken = default);
}

public interface IGetDocumentTypeDetailUseCase
{
    Task<DocumentTypeResult?> ExecuteAsync(GetDocumentTypeDetailQuery query, CancellationToken cancellationToken = default);
}

public interface ICreateDocumentTypeUseCase
{
    Task<DocumentTypeResult?> ExecuteAsync(DocumentTypeCreateCommand command, CancellationToken cancellationToken = default);
}

public interface IUpdateDocumentTypeUseCase
{
    Task<DocumentTypeResult?> ExecuteAsync(DocumentTypeUpdateCommand command, CancellationToken cancellationToken = default);
}

public interface IDeleteDocumentTypeUseCase
{
    Task<bool> ExecuteAsync(DeleteDocumentTypeCommand command, CancellationToken cancellationToken = default);
}

public sealed class GetDocumentTypesUseCase(IDocumentTypeReader reader) : IGetDocumentTypesUseCase
{
    public Task<IReadOnlyList<DocumentTypeListItem>> ExecuteAsync(GetDocumentTypesQuery query, CancellationToken cancellationToken = default)
    {
        var tenantId = TenantId.From(query.TenantId);
        return reader.GetAllAsync(tenantId.Value, cancellationToken);
    }
}

public sealed class GetDocumentTypeDetailUseCase(IDocumentTypeReader reader) : IGetDocumentTypeDetailUseCase
{
    public Task<DocumentTypeResult?> ExecuteAsync(GetDocumentTypeDetailQuery query, CancellationToken cancellationToken = default)
    {
        var tenantId = TenantId.From(query.TenantId);
        var documentTypeId = DocumentTypeUseCaseValidation.RequireGuid(query.DocumentTypeId, nameof(query.DocumentTypeId));
        return reader.GetByIdAsync(documentTypeId, tenantId.Value, cancellationToken);
    }
}

public sealed class CreateDocumentTypeUseCase(IDocumentTypeWriter writer) : ICreateDocumentTypeUseCase
{
    public Task<DocumentTypeResult?> ExecuteAsync(DocumentTypeCreateCommand command, CancellationToken cancellationToken = default)
    {
        var normalized = DocumentTypeUseCaseValidation.Normalize(command);
        return writer.CreateAsync(normalized, cancellationToken);
    }
}

public sealed class UpdateDocumentTypeUseCase(IDocumentTypeWriter writer) : IUpdateDocumentTypeUseCase
{
    public Task<DocumentTypeResult?> ExecuteAsync(DocumentTypeUpdateCommand command, CancellationToken cancellationToken = default)
    {
        var normalized = DocumentTypeUseCaseValidation.Normalize(command);
        return writer.UpdateAsync(normalized, cancellationToken);
    }
}

public sealed class DeleteDocumentTypeUseCase(IDocumentTypeWriter writer) : IDeleteDocumentTypeUseCase
{
    public Task<bool> ExecuteAsync(DeleteDocumentTypeCommand command, CancellationToken cancellationToken = default)
    {
        var tenantId = TenantId.From(command.TenantId);
        var documentTypeId = DocumentTypeUseCaseValidation.RequireGuid(command.DocumentTypeId, nameof(command.DocumentTypeId));
        return writer.DeleteAsync(documentTypeId, tenantId.Value, cancellationToken);
    }
}

file static class DocumentTypeUseCaseValidation
{
    public static DocumentTypeCreateCommand Normalize(DocumentTypeCreateCommand command)
    {
        var tenantId = TenantId.From(command.TenantId);
        var userId = UserId.From(command.UserId);
        var documentTypeId = command.DocumentTypeId == Guid.Empty ? Guid.NewGuid() : command.DocumentTypeId;
        var name = RequireName(command.Name);

        return command with
        {
            DocumentTypeId = documentTypeId,
            TenantId = tenantId.Value,
            UserId = userId.Value,
            Name = name,
            Description = command.Description?.Trim(),
            Icon = command.Icon?.Trim()
        };
    }

    public static DocumentTypeUpdateCommand Normalize(DocumentTypeUpdateCommand command)
    {
        var tenantId = TenantId.From(command.TenantId);
        var userId = UserId.From(command.UserId);
        var documentTypeId = RequireGuid(command.DocumentTypeId, nameof(command.DocumentTypeId));
        var name = RequireName(command.Name);

        return command with
        {
            DocumentTypeId = documentTypeId,
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
