using AppCondominio.SharedKernel;

namespace AppCondominio.Modules.Tax.Domain;

public enum ElectronicDocumentStatus { Draft=0,PendingConfiguration=1,Submitted=2,Authorized=3,Rejected=4,Cancelled=5 }

public sealed class ElectronicDocument:AggregateRoot
{
    private ElectronicDocument(Guid id,Guid communityId,Guid sourceDocumentId,string documentType,string establishmentCode,string emissionPoint,string sequential,string accessKey,string payloadReference):base(id)
    {CommunityId=communityId;SourceDocumentId=sourceDocumentId;DocumentType=documentType;EstablishmentCode=establishmentCode;EmissionPoint=emissionPoint;Sequential=sequential;AccessKey=accessKey;PayloadReference=payloadReference;}
    public Guid CommunityId{get;private set;} public Guid SourceDocumentId{get;private set;} public string DocumentType{get;private set;} public string EstablishmentCode{get;private set;} public string EmissionPoint{get;private set;} public string Sequential{get;private set;} public string AccessKey{get;private set;} public string PayloadReference{get;private set;} public ElectronicDocumentStatus Status{get;private set;}
    public string? AuthorizationNumber{get;private set;} public DateTimeOffset? AuthorizedAtUtc{get;private set;} public string? LastMessage{get;private set;} public DateTimeOffset? SubmittedAtUtc{get;private set;}
    public static ElectronicDocument Create(Guid communityId,Guid sourceDocumentId,string documentType,string establishmentCode,string emissionPoint,string sequential,string accessKey,string payloadReference)
    {if(communityId==Guid.Empty||sourceDocumentId==Guid.Empty)throw new ArgumentException("Community and source document are required.");return new(Guid.NewGuid(),communityId,sourceDocumentId,Req(documentType),Req(establishmentCode),Req(emissionPoint),Req(sequential),Req(accessKey),Req(payloadReference));}
    public void MarkSubmitted(string? message=null){Status=ElectronicDocumentStatus.Submitted;SubmittedAtUtc=DateTimeOffset.UtcNow;LastMessage=message;}
    public void ApplyResult(string status,string? authorizationNumber,DateTimeOffset? authorizedAtUtc,string? message)
    {LastMessage=message;Status=status.ToLowerInvariant() switch{"authorized"=>ElectronicDocumentStatus.Authorized,"rejected"=>ElectronicDocumentStatus.Rejected,"submitted"=>ElectronicDocumentStatus.Submitted,"pendingconfiguration"=>ElectronicDocumentStatus.PendingConfiguration,_=>Status};AuthorizationNumber=authorizationNumber;AuthorizedAtUtc=authorizedAtUtc;}
    private static string Req(string x)=>string.IsNullOrWhiteSpace(x)?throw new ArgumentException("Required value missing."):x.Trim();
}
