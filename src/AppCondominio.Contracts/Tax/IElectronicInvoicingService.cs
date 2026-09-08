namespace AppCondominio.Contracts.Tax;

public sealed record ElectronicDocumentRequest(Guid CommunityId,Guid SourceDocumentId,string DocumentType,string EstablishmentCode,string EmissionPoint,string Sequential,string AccessKey,string PayloadReference);
public sealed record ElectronicDocumentResult(string Status,string? AuthorizationNumber,DateTimeOffset? AuthorizedAtUtc,string? Message);

public interface IElectronicInvoicingService
{
    Task<ElectronicDocumentResult> SubmitAsync(ElectronicDocumentRequest request,CancellationToken cancellationToken=default);
    Task<ElectronicDocumentResult> GetStatusAsync(Guid communityId,string accessKey,CancellationToken cancellationToken=default);
}
