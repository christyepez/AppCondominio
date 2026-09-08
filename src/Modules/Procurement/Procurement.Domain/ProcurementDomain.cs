using AppCondominio.SharedKernel;

namespace AppCondominio.Modules.Procurement.Domain;

public enum RequisitionStatus{Draft=0,Approved=1,Sourcing=2,Awarded=3,Cancelled=4}
public enum SourcingRoundStatus{Open=0,Closed=1,Awarded=2,Cancelled=3}
public enum PurchaseOrderStatus{Issued=0,Acknowledged=1,Completed=2,Cancelled=3}
public enum SupplierAccessStatus{Active=1,Revoked=2,Expired=3}

public sealed class SupplierProfile:AggregateRoot
{
    private SupplierProfile(Guid id,Guid communityId,Guid? personId,string taxId,string legalName,string email):base(id){CommunityId=communityId;PersonId=personId;TaxId=taxId;LegalName=legalName;Email=email;IsActive=true;}
    public Guid CommunityId{get;private set;} public Guid? PersonId{get;private set;} public string TaxId{get;private set;} public string LegalName{get;private set;} public string Email{get;private set;} public bool IsActive{get;private set;}
    public static SupplierProfile Create(Guid communityId,Guid? personId,string taxId,string legalName,string email){if(communityId==Guid.Empty)throw new ArgumentException("Community required.");return new(Guid.NewGuid(),communityId,personId,Req(taxId),Req(legalName),Req(email));}
    public void Deactivate()=>IsActive=false;private static string Req(string x)=>string.IsNullOrWhiteSpace(x)?throw new ArgumentException("Required value missing."):x.Trim();
}

public sealed class SupplierAccessGrant:AggregateRoot
{
    private SupplierAccessGrant(Guid id,Guid communityId,Guid supplierId,string externalUserId,DateTimeOffset startsAtUtc,DateTimeOffset? expiresAtUtc):base(id){CommunityId=communityId;SupplierId=supplierId;ExternalUserId=externalUserId;StartsAtUtc=startsAtUtc;ExpiresAtUtc=expiresAtUtc;}
    public Guid CommunityId{get;private set;} public Guid SupplierId{get;private set;} public string ExternalUserId{get;private set;} public DateTimeOffset StartsAtUtc{get;private set;} public DateTimeOffset? ExpiresAtUtc{get;private set;} public SupplierAccessStatus Status{get;private set;}=SupplierAccessStatus.Active; public string? RevocationReason{get;private set;}
    public static SupplierAccessGrant Create(Guid communityId,Guid supplierId,string externalUserId,DateTimeOffset startsAtUtc,DateTimeOffset? expiresAtUtc){if(communityId==Guid.Empty||supplierId==Guid.Empty)throw new ArgumentException("Community and supplier are required.");if(string.IsNullOrWhiteSpace(externalUserId))throw new ArgumentException("External user is required.");if(expiresAtUtc is not null&&expiresAtUtc<=startsAtUtc)throw new ArgumentException("Expiry must be after access start.");return new(Guid.NewGuid(),communityId,supplierId,externalUserId.Trim(),startsAtUtc,expiresAtUtc);}
    public bool IsUsable(DateTimeOffset now)=>Status==SupplierAccessStatus.Active&&StartsAtUtc<=now&&(ExpiresAtUtc is null||ExpiresAtUtc>now);
    public bool ExpireIfDue(DateTimeOffset now){if(Status==SupplierAccessStatus.Active&&ExpiresAtUtc is not null&&now>=ExpiresAtUtc){Status=SupplierAccessStatus.Expired;return true;}return false;}
    public void Revoke(string reason){if(Status!=SupplierAccessStatus.Active)return;Status=SupplierAccessStatus.Revoked;RevocationReason=string.IsNullOrWhiteSpace(reason)?"Revoked":reason.Trim();}
}

public sealed class PurchaseRequisition:AggregateRoot
{
    private PurchaseRequisition(Guid id,Guid communityId,string number,string description,decimal estimatedAmount,string requestedBy):base(id){CommunityId=communityId;Number=number;Description=description;EstimatedAmount=estimatedAmount;RequestedBy=requestedBy;RequestedAtUtc=DateTimeOffset.UtcNow;}
    public Guid CommunityId{get;private set;} public string Number{get;private set;} public string Description{get;private set;} public decimal EstimatedAmount{get;private set;} public string RequestedBy{get;private set;} public DateTimeOffset RequestedAtUtc{get;private set;} public RequisitionStatus Status{get;private set;} public string? ApprovedBy{get;private set;} public DateTimeOffset? ApprovedAtUtc{get;private set;}
    public static PurchaseRequisition Create(Guid communityId,string number,string description,decimal estimatedAmount,string requestedBy){if(communityId==Guid.Empty||estimatedAmount<=0)throw new ArgumentException("Invalid requisition.");return new(Guid.NewGuid(),communityId,Req(number),Req(description),decimal.Round(estimatedAmount,2),Req(requestedBy));}
    public void Approve(string by){if(Status!=RequisitionStatus.Draft)throw new InvalidOperationException("Only draft requisitions can be approved.");Status=RequisitionStatus.Approved;ApprovedBy=Req(by);ApprovedAtUtc=DateTimeOffset.UtcNow;}
    public void StartSourcing(){if(Status!=RequisitionStatus.Approved)throw new InvalidOperationException("Requisition must be approved before sourcing.");Status=RequisitionStatus.Sourcing;}
    public void MarkAwarded(){if(Status!=RequisitionStatus.Sourcing)throw new InvalidOperationException("Requisition is not in sourcing.");Status=RequisitionStatus.Awarded;}
    private static string Req(string x)=>string.IsNullOrWhiteSpace(x)?throw new ArgumentException("Required value missing."):x.Trim();
}

public sealed class SourcingRound:AggregateRoot
{
    private SourcingRound(Guid id,Guid communityId,Guid requisitionId,string title,DateTimeOffset closesAtUtc):base(id){CommunityId=communityId;RequisitionId=requisitionId;Title=title;ClosesAtUtc=closesAtUtc;OpenedAtUtc=DateTimeOffset.UtcNow;}
    public Guid CommunityId{get;private set;} public Guid RequisitionId{get;private set;} public string Title{get;private set;} public DateTimeOffset OpenedAtUtc{get;private set;} public DateTimeOffset ClosesAtUtc{get;private set;} public SourcingRoundStatus Status{get;private set;} public Guid? AwardedBidId{get;private set;}
    public static SourcingRound Create(Guid communityId,Guid requisitionId,string title,DateTimeOffset closesAtUtc){if(communityId==Guid.Empty||requisitionId==Guid.Empty||closesAtUtc<=DateTimeOffset.UtcNow)throw new ArgumentException("Invalid sourcing round.");return new(Guid.NewGuid(),communityId,requisitionId,Req(title),closesAtUtc);}
    public void Close(){if(Status!=SourcingRoundStatus.Open)throw new InvalidOperationException("Only open rounds can be closed.");Status=SourcingRoundStatus.Closed;}
    public void Award(Guid bidId){if(Status!=SourcingRoundStatus.Closed||bidId==Guid.Empty)throw new InvalidOperationException("Round must be closed before award.");Status=SourcingRoundStatus.Awarded;AwardedBidId=bidId;}
    private static string Req(string x)=>string.IsNullOrWhiteSpace(x)?throw new ArgumentException("Required value missing."):x.Trim();
}

public sealed class SupplierBid:AggregateRoot
{
    private SupplierBid(Guid id,Guid communityId,Guid sourcingRoundId,Guid supplierId,decimal amount,int deliveryDays,string proposalReference):base(id){CommunityId=communityId;SourcingRoundId=sourcingRoundId;SupplierId=supplierId;Amount=amount;DeliveryDays=deliveryDays;ProposalReference=proposalReference;SubmittedAtUtc=DateTimeOffset.UtcNow;}
    public Guid CommunityId{get;private set;} public Guid SourcingRoundId{get;private set;} public Guid SupplierId{get;private set;} public decimal Amount{get;private set;} public int DeliveryDays{get;private set;} public string ProposalReference{get;private set;} public DateTimeOffset SubmittedAtUtc{get;private set;} public decimal? EvaluationScore{get;private set;}
    public static SupplierBid Create(Guid communityId,Guid roundId,Guid supplierId,decimal amount,int deliveryDays,string proposalReference){if(new[]{communityId,roundId,supplierId}.Any(x=>x==Guid.Empty)||amount<=0||deliveryDays<0)throw new ArgumentException("Invalid bid.");return new(Guid.NewGuid(),communityId,roundId,supplierId,decimal.Round(amount,2),deliveryDays,Req(proposalReference));}
    public void Score(decimal score){if(score<0||score>100)throw new ArgumentOutOfRangeException(nameof(score));EvaluationScore=decimal.Round(score,2);}
    private static string Req(string x)=>string.IsNullOrWhiteSpace(x)?throw new ArgumentException("Required value missing."):x.Trim();
}

public sealed class PurchaseOrder:AggregateRoot
{
    private PurchaseOrder(Guid id,Guid communityId,Guid requisitionId,Guid supplierId,Guid awardedBidId,string number,decimal amount,string payableReference):base(id){CommunityId=communityId;RequisitionId=requisitionId;SupplierId=supplierId;AwardedBidId=awardedBidId;Number=number;Amount=amount;PayableReference=payableReference;IssuedAtUtc=DateTimeOffset.UtcNow;}
    public Guid CommunityId{get;private set;} public Guid RequisitionId{get;private set;} public Guid SupplierId{get;private set;} public Guid AwardedBidId{get;private set;} public string Number{get;private set;} public decimal Amount{get;private set;} public string PayableReference{get;private set;} public PurchaseOrderStatus Status{get;private set;} public DateTimeOffset IssuedAtUtc{get;private set;}
    public static PurchaseOrder Create(Guid communityId,Guid requisitionId,Guid supplierId,Guid bidId,string number,decimal amount,string payableReference){if(new[]{communityId,requisitionId,supplierId,bidId}.Any(x=>x==Guid.Empty)||amount<=0)throw new ArgumentException("Invalid purchase order.");return new(Guid.NewGuid(),communityId,requisitionId,supplierId,bidId,Req(number),decimal.Round(amount,2),Req(payableReference));}
    public void Acknowledge(){if(Status!=PurchaseOrderStatus.Issued)throw new InvalidOperationException("Order is not issuable.");Status=PurchaseOrderStatus.Acknowledged;}
    public void Complete(){if(Status is not (PurchaseOrderStatus.Issued or PurchaseOrderStatus.Acknowledged))throw new InvalidOperationException("Order cannot be completed.");Status=PurchaseOrderStatus.Completed;}
    private static string Req(string x)=>string.IsNullOrWhiteSpace(x)?throw new ArgumentException("Required value missing."):x.Trim();
}