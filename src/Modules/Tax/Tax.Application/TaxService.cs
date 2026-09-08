using AppCondominio.Contracts.Tax;
using AppCondominio.Modules.Tax.Domain;

namespace AppCondominio.Modules.Tax.Application;

public interface ITaxRepository
{
    Task<bool> SourceExistsAsync(Guid communityId,Guid sourceDocumentId,CancellationToken ct);
    Task<ElectronicDocument?> GetAsync(Guid id,CancellationToken ct);
    Task AddAsync(ElectronicDocument document,CancellationToken ct);
    Task SaveChangesAsync(CancellationToken ct);
}

public sealed record ElectronicDocumentView(Guid Id,Guid CommunityId,Guid SourceDocumentId,string DocumentType,string AccessKey,ElectronicDocumentStatus Status,string? AuthorizationNumber,DateTimeOffset? AuthorizedAtUtc,string? LastMessage);

public sealed class TaxService(ITaxRepository repository,IElectronicInvoicingService invoicing)
{
    public async Task<Guid> CreateAsync(Guid communityId,Guid sourceDocumentId,string documentType,string establishmentCode,string emissionPoint,string sequential,string accessKey,string payloadReference,CancellationToken ct)
    {
        if(await repository.SourceExistsAsync(communityId,sourceDocumentId,ct))throw new InvalidOperationException("Electronic document already exists for this source.");
        var document=ElectronicDocument.Create(communityId,sourceDocumentId,documentType,establishmentCode,emissionPoint,sequential,accessKey,payloadReference);await repository.AddAsync(document,ct);await repository.SaveChangesAsync(ct);return document.Id;
    }

    public async Task<ElectronicDocumentView> SubmitAsync(Guid id,CancellationToken ct)
    {
        var document=await repository.GetAsync(id,ct)??throw new KeyNotFoundException("Electronic document was not found.");
        var result=await invoicing.SubmitAsync(new(document.CommunityId,document.SourceDocumentId,document.DocumentType,document.EstablishmentCode,document.EmissionPoint,document.Sequential,document.AccessKey,document.PayloadReference),ct);
        document.ApplyResult(result.Status,result.AuthorizationNumber,result.AuthorizedAtUtc,result.Message);await repository.SaveChangesAsync(ct);return ToView(document);
    }

    public async Task<ElectronicDocumentView> RefreshAsync(Guid id,CancellationToken ct)
    {
        var document=await repository.GetAsync(id,ct)??throw new KeyNotFoundException("Electronic document was not found.");var result=await invoicing.GetStatusAsync(document.CommunityId,document.AccessKey,ct);document.ApplyResult(result.Status,result.AuthorizationNumber,result.AuthorizedAtUtc,result.Message);await repository.SaveChangesAsync(ct);return ToView(document);
    }

    public async Task<ElectronicDocumentView> GetAsync(Guid id,CancellationToken ct)=>ToView(await repository.GetAsync(id,ct)??throw new KeyNotFoundException("Electronic document was not found."));
    private static ElectronicDocumentView ToView(ElectronicDocument x)=>new(x.Id,x.CommunityId,x.SourceDocumentId,x.DocumentType,x.AccessKey,x.Status,x.AuthorizationNumber,x.AuthorizedAtUtc,x.LastMessage);
}
