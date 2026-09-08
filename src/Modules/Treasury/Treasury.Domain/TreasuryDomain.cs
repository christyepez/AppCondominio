using AppCondominio.SharedKernel;

namespace AppCondominio.Modules.Treasury.Domain;

public enum PayableStatus{Open=0,PartiallyPaid=1,Paid=2,Cancelled=3}
public enum PaymentRequestStatus{Pending=0,Approved=1,Rejected=2,Paid=3}

public sealed class SupplierPayable:AggregateRoot
{
    private SupplierPayable(Guid id,Guid communityId,Guid supplierId,string documentNumber,DateOnly documentDate,DateOnly dueDate,decimal amount,string expenseAccount,string payableAccount):base(id){CommunityId=communityId;SupplierId=supplierId;DocumentNumber=documentNumber;DocumentDate=documentDate;DueDate=dueDate;OriginalAmount=OutstandingAmount=amount;ExpenseAccount=expenseAccount;PayableAccount=payableAccount;}
    public Guid CommunityId{get;private set;} public Guid SupplierId{get;private set;} public string DocumentNumber{get;private set;} public DateOnly DocumentDate{get;private set;} public DateOnly DueDate{get;private set;} public decimal OriginalAmount{get;private set;} public decimal OutstandingAmount{get;private set;} public string ExpenseAccount{get;private set;} public string PayableAccount{get;private set;} public PayableStatus Status{get;private set;}
    public static SupplierPayable Create(Guid communityId,Guid supplierId,string documentNumber,DateOnly documentDate,DateOnly dueDate,decimal amount,string expenseAccount,string payableAccount){if(communityId==Guid.Empty||supplierId==Guid.Empty)throw new ArgumentException("Community and supplier are required.");if(amount<=0)throw new ArgumentOutOfRangeException(nameof(amount));if(dueDate<documentDate)throw new ArgumentException("Due date cannot precede document date.");return new(Guid.NewGuid(),communityId,supplierId,Req(documentNumber),documentDate,dueDate,decimal.Round(amount,2),Req(expenseAccount),Req(payableAccount));}
    public void ApplyPayment(decimal amount){if(Status==PayableStatus.Cancelled||amount<=0||amount>OutstandingAmount)throw new InvalidOperationException("Invalid payable payment.");OutstandingAmount-=amount;Status=OutstandingAmount==0?PayableStatus.Paid:PayableStatus.PartiallyPaid;}
    public void RestorePayment(decimal amount){if(amount<=0)throw new ArgumentOutOfRangeException(nameof(amount));OutstandingAmount=Math.Min(OriginalAmount,OutstandingAmount+amount);Status=OutstandingAmount==OriginalAmount?PayableStatus.Open:PayableStatus.PartiallyPaid;}
    private static string Req(string x)=>string.IsNullOrWhiteSpace(x)?throw new ArgumentException("Required value missing."):x.Trim();
}

public sealed class PaymentRequest:AggregateRoot
{
    private PaymentRequest(Guid id,Guid communityId,Guid payableId,decimal amount,string requestedBy):base(id){CommunityId=communityId;PayableId=payableId;Amount=amount;RequestedBy=requestedBy;RequestedAtUtc=DateTimeOffset.UtcNow;}
    public Guid CommunityId{get;private set;} public Guid PayableId{get;private set;} public decimal Amount{get;private set;} public string RequestedBy{get;private set;} public DateTimeOffset RequestedAtUtc{get;private set;} public PaymentRequestStatus Status{get;private set;} public string? DecidedBy{get;private set;} public DateTimeOffset? DecidedAtUtc{get;private set;} public string? DecisionReason{get;private set;}
    public static PaymentRequest Create(Guid communityId,Guid payableId,decimal amount,string requestedBy){if(communityId==Guid.Empty||payableId==Guid.Empty||amount<=0)throw new ArgumentException("Invalid payment request.");return new(Guid.NewGuid(),communityId,payableId,decimal.Round(amount,2),Req(requestedBy));}
    public void Approve(string by){if(Status!=PaymentRequestStatus.Pending)throw new InvalidOperationException("Only pending request can be approved.");Status=PaymentRequestStatus.Approved;DecidedBy=Req(by);DecidedAtUtc=DateTimeOffset.UtcNow;}
    public void Reject(string by,string reason){if(Status!=PaymentRequestStatus.Pending)throw new InvalidOperationException("Only pending request can be rejected.");Status=PaymentRequestStatus.Rejected;DecidedBy=Req(by);DecisionReason=Req(reason);DecidedAtUtc=DateTimeOffset.UtcNow;}
    public void MarkPaid(){if(Status!=PaymentRequestStatus.Approved)throw new InvalidOperationException("Only approved request can be paid.");Status=PaymentRequestStatus.Paid;}
    private static string Req(string x)=>string.IsNullOrWhiteSpace(x)?throw new ArgumentException("Required value missing."):x.Trim();
}

public sealed class TreasuryDisbursement:AggregateRoot
{
    private TreasuryDisbursement(Guid id,Guid communityId,Guid paymentRequestId,Guid payableId,Guid bankAccountId,decimal amount,string paymentReference,DateOnly paidOn):base(id){CommunityId=communityId;PaymentRequestId=paymentRequestId;PayableId=payableId;BankAccountId=bankAccountId;Amount=amount;PaymentReference=paymentReference;PaidOn=paidOn;}
    public Guid CommunityId{get;private set;} public Guid PaymentRequestId{get;private set;} public Guid PayableId{get;private set;} public Guid BankAccountId{get;private set;} public decimal Amount{get;private set;} public string PaymentReference{get;private set;} public DateOnly PaidOn{get;private set;} public bool IsReversed{get;private set;} public DateTimeOffset? ReversedAtUtc{get;private set;}
    public static TreasuryDisbursement Create(Guid communityId,Guid paymentRequestId,Guid payableId,Guid bankAccountId,decimal amount,string reference,DateOnly paidOn){if(new[]{communityId,paymentRequestId,payableId,bankAccountId}.Any(x=>x==Guid.Empty)||amount<=0)throw new ArgumentException("Invalid disbursement.");return new(Guid.NewGuid(),communityId,paymentRequestId,payableId,bankAccountId,decimal.Round(amount,2),string.IsNullOrWhiteSpace(reference)?throw new ArgumentException("Reference required."):reference.Trim(),paidOn);}
    public void Reverse(){if(IsReversed)throw new InvalidOperationException("Disbursement already reversed.");IsReversed=true;ReversedAtUtc=DateTimeOffset.UtcNow;}
}

public sealed record CashForecastRow(DateOnly DueDate,decimal Amount,int Payables);
