using OpenCashFlow.Application.Payments.DocumentTypes;
using OpenCashFlow.Application.Payments.Lookups;
using OpenCashFlow.Application.Payments.Ports;

namespace OpenCashFlow.Application.Tests.Payments.DocumentTypes;

public sealed class DocumentTypeUseCaseTests
{
    [Fact]
    public async Task GetAll_WithEmptyTenant_Fails()
    {
        var useCase = new GetDocumentTypesUseCase(new FakeDocumentTypeReader());

        await Assert.ThrowsAsync<ArgumentException>(() =>
            useCase.ExecuteAsync(new GetDocumentTypesQuery(Guid.Empty)));
    }

    [Fact]
    public async Task Create_WithEmptyName_Fails()
    {
        var useCase = new CreateDocumentTypeUseCase(new FakeDocumentTypeWriter());

        await Assert.ThrowsAsync<ArgumentException>(() =>
            useCase.ExecuteAsync(ValidCreateCommand() with { Name = "" }));
    }

    [Fact]
    public async Task Create_WithValidCommand_NormalizesAndPassesToWriter()
    {
        var writer = new FakeDocumentTypeWriter();
        var useCase = new CreateDocumentTypeUseCase(writer);

        var result = await useCase.ExecuteAsync(ValidCreateCommand(name: "  Fattura  "));

        Assert.NotNull(result);
        Assert.NotNull(writer.LastCreate);
        Assert.Equal("Fattura", writer.LastCreate!.Name);
        Assert.NotEqual(Guid.Empty, writer.LastCreate.DocumentTypeId);
    }

    [Fact]
    public async Task Update_WhenWriterReturnsNull_ReturnsNull()
    {
        var writer = new FakeDocumentTypeWriter { ReturnNullOnUpdate = true };
        var useCase = new UpdateDocumentTypeUseCase(writer);

        var result = await useCase.ExecuteAsync(ValidUpdateCommand());

        Assert.Null(result);
        Assert.NotNull(writer.LastUpdate);
    }

    [Fact]
    public async Task Delete_PassesTenantAndIdToWriter()
    {
        var writer = new FakeDocumentTypeWriter();
        var useCase = new DeleteDocumentTypeUseCase(writer);
        var tenantId = Guid.NewGuid();
        var documentTypeId = Guid.NewGuid();

        var deleted = await useCase.ExecuteAsync(new DeleteDocumentTypeCommand(documentTypeId, tenantId));

        Assert.True(deleted);
        Assert.Equal(tenantId, writer.LastDeleteTenantId);
        Assert.Equal(documentTypeId, writer.LastDeleteDocumentTypeId);
    }

    [Fact]
    public async Task ListAndDetail_CallReader()
    {
        var reader = new FakeDocumentTypeReader();
        var tenantId = Guid.NewGuid();
        var documentTypeId = Guid.NewGuid();

        await new GetDocumentTypesUseCase(reader).ExecuteAsync(new GetDocumentTypesQuery(tenantId));
        await new GetDocumentTypeDetailUseCase(reader).ExecuteAsync(new GetDocumentTypeDetailQuery(documentTypeId, tenantId));

        Assert.Equal(tenantId, reader.LastListTenantId);
        Assert.Equal(documentTypeId, reader.LastDetailDocumentTypeId);
        Assert.Equal(tenantId, reader.LastDetailTenantId);
    }

    private static DocumentTypeCreateCommand ValidCreateCommand(string name = "Invoice")
    {
        return new DocumentTypeCreateCommand(
            Guid.Empty,
            Guid.NewGuid(),
            Guid.NewGuid(),
            name,
            "Invoice document",
            "file-text",
            true,
            10);
    }

    private static DocumentTypeUpdateCommand ValidUpdateCommand(string name = "Invoice")
    {
        return new DocumentTypeUpdateCommand(
            Guid.NewGuid(),
            Guid.NewGuid(),
            Guid.NewGuid(),
            name,
            "Invoice document",
            "file-text",
            true,
            10);
    }

    private sealed class FakeDocumentTypeReader : IDocumentTypeReader
    {
        public Guid LastListTenantId { get; private set; }
        public Guid LastDetailDocumentTypeId { get; private set; }
        public Guid LastDetailTenantId { get; private set; }

        public Task<IReadOnlyList<DocumentTypeListItem>> GetAllAsync(Guid tenantId, CancellationToken cancellationToken = default)
        {
            LastListTenantId = tenantId;
            return Task.FromResult<IReadOnlyList<DocumentTypeListItem>>([]);
        }

        public Task<DocumentTypeResult?> GetByIdAsync(Guid documentTypeId, Guid tenantId, CancellationToken cancellationToken = default)
        {
            LastDetailDocumentTypeId = documentTypeId;
            LastDetailTenantId = tenantId;
            return Task.FromResult<DocumentTypeResult?>(null);
        }
    }

    private sealed class FakeDocumentTypeWriter : IDocumentTypeWriter
    {
        public DocumentTypeCreateCommand? LastCreate { get; private set; }
        public DocumentTypeUpdateCommand? LastUpdate { get; private set; }
        public Guid LastDeleteDocumentTypeId { get; private set; }
        public Guid LastDeleteTenantId { get; private set; }
        public bool ReturnNullOnUpdate { get; init; }

        public Task<DocumentTypeResult?> CreateAsync(DocumentTypeCreateCommand command, CancellationToken cancellationToken = default)
        {
            LastCreate = command;
            return Task.FromResult<DocumentTypeResult?>(ToResult(command));
        }

        public Task<DocumentTypeResult?> UpdateAsync(DocumentTypeUpdateCommand command, CancellationToken cancellationToken = default)
        {
            LastUpdate = command;
            return Task.FromResult(ReturnNullOnUpdate ? null : ToResult(command));
        }

        public Task<bool> DeleteAsync(Guid documentTypeId, Guid tenantId, CancellationToken cancellationToken = default)
        {
            LastDeleteDocumentTypeId = documentTypeId;
            LastDeleteTenantId = tenantId;
            return Task.FromResult(true);
        }

        private static DocumentTypeResult ToResult(DocumentTypeCreateCommand command)
        {
            return new DocumentTypeResult(
                command.DocumentTypeId,
                command.TenantId,
                command.Name,
                command.Description,
                command.Icon,
                command.Visible,
                command.DisplayOrder,
                IsDeleted: false,
                IsDeletedBy: null,
                IsDeletedWhy: null,
                DateDeleted: null,
                CreatedBy: command.UserId,
                DateIns: DateTime.UtcNow,
                EditedBy: null,
                DateEdit: null);
        }

        private static DocumentTypeResult ToResult(DocumentTypeUpdateCommand command)
        {
            return new DocumentTypeResult(
                command.DocumentTypeId,
                command.TenantId,
                command.Name,
                command.Description,
                command.Icon,
                command.Visible,
                command.DisplayOrder,
                IsDeleted: false,
                IsDeletedBy: null,
                IsDeletedWhy: null,
                DateDeleted: null,
                CreatedBy: null,
                DateIns: DateTime.UtcNow,
                EditedBy: command.UserId,
                DateEdit: DateTime.UtcNow);
        }
    }
}
