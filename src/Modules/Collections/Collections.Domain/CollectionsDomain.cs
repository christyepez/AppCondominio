using AppCondominio.SharedKernel;

namespace AppCondominio.Modules.Collections.Domain;

public enum ReceivableStatus { Open=0,PartiallyPaid=1,Paid=2,Reversed=3 }
public enum PaymentStatus { Registered=0,PartiallyApplied=1,Applied=2,Reversed=3 }
public enum PaymentMethod { Cash=1,BankTransfer=2,Card=3,Deposit=4,Cheque=5,Other=99 }
public enum AgingBucket { Current=0,Days1To30=1,Days31To60=2,Days61To90=3,Days91Plus=4 }

public sealed class Receivable:AggregateRoot
{
    private Receivable(Guid id,Guid communityId,Guid unitId,Guid responsiblePersonId,Guid billingObligationId,decimal originalAmount,DateOnly issuedOn,DateOnly dueOn):base(id)
    {CommunityId=communityId;UnitId=unitId;ResponsiblePersonId=responsiblePersonId;BillingObligationId=billingObligationId;OriginalAmount=originalAmount;OutstandingAmount=originalAmount;IssuedOn=issuedOn;DueOn=dueOn;}
    public Guid CommunityId{get;private set;} public Guid UnitId{get;private set;} public Guid ResponsiblePersonId{get;private set;} public Guid BillingObligationId{get;private set;}
    public decimal OriginalAmount{get;private set;} public decimal OutstandingAmount{get;private set;} public DateOnly IssuedOn{get;private set;} public DateOnly DueOn{get;private set;} public ReceivableStatus Status{get;private set;}
    public static Receivable Create(Guid communityId,Guid unitId,Guid responsiblePersonId,Guid billingObligationId,decimal amount,DateOnly issuedOn,DateOnly dueOn)
    {if(new[]{communityId,unitId,responsiblePersonId,billingObligationId}.Any(x=>x==Guid.Empty))throw new ArgumentException("Required identifiers are missing.");if(amount<=0)throw new ArgumentOutOfRangeException(nameof(amount));if(dueOn<issuedOn)throw new ArgumentException("Due date cannot precede issue date.");return new(Guid.NewGuid(),communityId,unitId,responsiblePersonId,billingObligationId,decimal.Round(amount,2),issuedOn,dueOn);}
    public void Apply(decimal amount){if(Status==ReceivableStatus.Reversed)throw new InvalidOperationException("Reversed receivable cannot receive payments.");if(amount<=0||amount>OutstandingAmount)throw new InvalidOperationException("Invalid application amount.");OutstandingAmount=decimal.Round(OutstandingAmount-amount,2);Status=OutstandingAmount==0?ReceivableStatus.Paid:ReceivableStatus.PartiallyPaid;}
    public void Restore(decimal amount){if(Status==ReceivableStatus.Reversed)throw new InvalidOperationException("Reversed receivable cannot be restored.");if(amount<=0||OutstandingAmount+amount>OriginalAmount)throw new InvalidOperationException("Invalid restore amount.");OutstandingAmount=decimal.Round(OutstandingAmount+amount,2);Status=OutstandingAmount==OriginalAmount?ReceivableStatus.Open:ReceivableStatus.PartiallyPaid;}
    public void Reverse(){if(OutstandingAmount!=OriginalAmount)throw new InvalidOperationException("Receivable with applied payments cannot be reversed.");OutstandingAmount=0;Status=ReceivableStatus.Reversed;}
    public AgingBucket GetAging(DateOnly on){if(Status is ReceivableStatus.Paid or ReceivableStatus.Reversed||on<=DueOn)return AgingBucket.Current;int days=on.DayNumber-DueOn.DayNumber;return days<=30?AgingBucket.Days1To30:days<=60?AgingBucket.Days31To60:days<=90?AgingBucket.Days61To90:AgingBucket.Days91Plus;}
}

public sealed class Payment:AggregateRoot
{
    private Payment(Guid id,Guid communityId,string reference,PaymentMethod method,DateOnly receivedOn,decimal amount,string? externalTransactionId):base(id)
    {CommunityId=communityId;Reference=reference;Method=method;ReceivedOn=receivedOn;Amount=amount;UnappliedAmount=amount;ExternalTransactionId=externalTransactionId;}
    public Guid CommunityId{get;private set;} public string Reference{get;private set;} public PaymentMethod Method{get;private set;} public DateOnly ReceivedOn{get;private set;} public decimal Amount{get;private set;} public decimal UnappliedAmount{get;private set;} public string? ExternalTransactionId{get;private set;} public PaymentStatus Status{get;private set;}
    public static Payment Register(Guid communityId,string reference,PaymentMethod method,DateOnly receivedOn,decimal amount,string? externalTransactionId)
    {if(communityId==Guid.Empty)throw new ArgumentException("Community is required.");if(string.IsNullOrWhiteSpace(reference))throw new ArgumentException("Reference is required.");if(amount<=0)throw new ArgumentOutOfRangeException(nameof(amount));return new(Guid.NewGuid(),communityId,reference.Trim(),method,receivedOn,decimal.Round(amount,2),string.IsNullOrWhiteSpace(externalTransactionId)?null:externalTransactionId.Trim());}
    public void Apply(decimal amount){if(Status==PaymentStatus.Reversed)throw new InvalidOperationException("Reversed payment cannot be applied.");if(amount<=0||amount>UnappliedAmount)throw new InvalidOperationException("Invalid application amount.");UnappliedAmount=decimal.Round(UnappliedAmount-amount,2);Status=UnappliedAmount==0?PaymentStatus.Applied:PaymentStatus.PartiallyApplied;}
    public void Restore(decimal amount){if(Status==PaymentStatus.Reversed)throw new InvalidOperationException("Reversed payment cannot be restored before reversal processing.");if(amount<=0||UnappliedAmount+amount>Amount)throw new InvalidOperationException("Invalid restore amount.");UnappliedAmount=decimal.Round(UnappliedAmount+amount,2);Status=UnappliedAmount==Amount?PaymentStatus.Registered:PaymentStatus.PartiallyApplied;}
    public void Reverse(){if(Status==PaymentStatus.Reversed)return;UnappliedAmount=0;Status=PaymentStatus.Reversed;}
}

public sealed class PaymentApplication:AggregateRoot
{
    private PaymentApplication(Guid id,Guid communityId,Guid paymentId,Guid receivableId,decimal amount,DateTimeOffset appliedAtUtc,string appliedBy):base(id)
    {CommunityId=communityId;PaymentId=paymentId;ReceivableId=receivableId;Amount=amount;AppliedAtUtc=appliedAtUtc;AppliedBy=appliedBy;}
    public Guid CommunityId{get;private set;} public Guid PaymentId{get;private set;} public Guid ReceivableId{get;private set;} public decimal Amount{get;private set;} public DateTimeOffset AppliedAtUtc{get;private set;} public string AppliedBy{get;private set;} public DateTimeOffset? ReversedAtUtc{get;private set;} public string? ReversedBy{get;private set;}
    public bool IsReversed=>ReversedAtUtc is not null;
    public static PaymentApplication Create(Guid communityId,Guid paymentId,Guid receivableId,decimal amount,string appliedBy)
    {if(new[]{communityId,paymentId,receivableId}.Any(x=>x==Guid.Empty))throw new ArgumentException("Required identifiers are missing.");if(amount<=0)throw new ArgumentOutOfRangeException(nameof(amount));if(string.IsNullOrWhiteSpace(appliedBy))throw new ArgumentException("Applied by is required.");return new(Guid.NewGuid(),communityId,paymentId,receivableId,decimal.Round(amount,2),DateTimeOffset.UtcNow,appliedBy.Trim());}
    public void Reverse(string reversedBy){if(IsReversed)return;if(string.IsNullOrWhiteSpace(reversedBy))throw new ArgumentException("Reversed by is required.");ReversedAtUtc=DateTimeOffset.UtcNow;ReversedBy=reversedBy.Trim();}
}

public sealed record AgingSummary(decimal Current,decimal Days1To30,decimal Days31To60,decimal Days61To90,decimal Days91Plus,decimal Total);
