using AppCondominio.SharedKernel;

namespace AppCondominio.Modules.Banking.Domain;

public enum BankTransactionType { Credit=1,Debit=2 }
public enum ReconciliationStatus { Unmatched=0,Suggested=1,Matched=2,Rejected=3 }

public sealed class BankAccount:AggregateRoot
{
    private BankAccount(Guid id,Guid communityId,string bankCode,string accountNumber,string currency):base(id){CommunityId=communityId;BankCode=bankCode;AccountNumber=accountNumber;Currency=currency;}
    public Guid CommunityId{get;private set;} public string BankCode{get;private set;} public string AccountNumber{get;private set;} public string Currency{get;private set;} public bool IsActive{get;private set;}=true;
    public static BankAccount Create(Guid communityId,string bankCode,string accountNumber,string currency="USD")=>communityId==Guid.Empty?throw new ArgumentException("Community is required."):new(Guid.NewGuid(),communityId,Req(bankCode),Req(accountNumber),Req(currency).ToUpperInvariant());
    public void Deactivate()=>IsActive=false;private static string Req(string x)=>string.IsNullOrWhiteSpace(x)?throw new ArgumentException("Required value missing."):x.Trim();
}

public sealed class BankTransaction:AggregateRoot
{
    private BankTransaction(Guid id,Guid communityId,Guid bankAccountId,string importKey,DateOnly bookingDate,decimal amount,BankTransactionType type,string reference,string description):base(id){CommunityId=communityId;BankAccountId=bankAccountId;ImportKey=importKey;BookingDate=bookingDate;Amount=amount;Type=type;Reference=reference;Description=description;}
    public Guid CommunityId{get;private set;} public Guid BankAccountId{get;private set;} public string ImportKey{get;private set;} public DateOnly BookingDate{get;private set;} public decimal Amount{get;private set;} public BankTransactionType Type{get;private set;} public string Reference{get;private set;} public string Description{get;private set;}
    public static BankTransaction Import(Guid communityId,Guid accountId,string importKey,DateOnly bookingDate,decimal amount,BankTransactionType type,string reference,string description){if(communityId==Guid.Empty||accountId==Guid.Empty)throw new ArgumentException("Community and bank account are required.");if(string.IsNullOrWhiteSpace(importKey)||amount<=0)throw new ArgumentException("Import key and positive amount are required.");return new(Guid.NewGuid(),communityId,accountId,importKey.Trim(),bookingDate,decimal.Round(amount,2),type,reference?.Trim()??string.Empty,description?.Trim()??string.Empty);}
}

public sealed class Reconciliation:AggregateRoot
{
    private Reconciliation(Guid id,Guid communityId,Guid bankTransactionId):base(id){CommunityId=communityId;BankTransactionId=bankTransactionId;}
    public Guid CommunityId{get;private set;} public Guid BankTransactionId{get;private set;} public Guid? PaymentId{get;private set;} public ReconciliationStatus Status{get;private set;} public decimal Confidence{get;private set;} public string? Reason{get;private set;} public string? DecidedBy{get;private set;} public DateTimeOffset? DecidedAtUtc{get;private set;}
    public static Reconciliation Create(Guid communityId,Guid bankTransactionId)=>new(Guid.NewGuid(),communityId,bankTransactionId);
    public void Suggest(Guid paymentId,decimal confidence,string reason){if(paymentId==Guid.Empty||confidence<0||confidence>1)throw new ArgumentException("Invalid suggestion.");PaymentId=paymentId;Confidence=confidence;Reason=reason.Trim();Status=ReconciliationStatus.Suggested;}
    public void Match(Guid paymentId,string decidedBy){if(paymentId==Guid.Empty||string.IsNullOrWhiteSpace(decidedBy))throw new ArgumentException("Payment and decision actor are required.");PaymentId=paymentId;Status=ReconciliationStatus.Matched;DecidedBy=decidedBy.Trim();DecidedAtUtc=DateTimeOffset.UtcNow;}
    public void Reject(string decidedBy,string reason){if(string.IsNullOrWhiteSpace(decidedBy)||string.IsNullOrWhiteSpace(reason))throw new ArgumentException("Decision actor and reason are required.");Status=ReconciliationStatus.Rejected;DecidedBy=decidedBy.Trim();Reason=reason.Trim();DecidedAtUtc=DateTimeOffset.UtcNow;}
}

public sealed record PaymentCandidate(Guid PaymentId,DateOnly ReceivedOn,decimal Amount,string Reference,string? ExternalTransactionId);
public static class ReconciliationScorer
{
    public static decimal Score(BankTransaction tx,PaymentCandidate payment)
    {
        decimal score=0;if(tx.Amount==payment.Amount)score+=0.55m;else if(Math.Abs(tx.Amount-payment.Amount)<=0.01m)score+=0.45m;
        int days=Math.Abs(tx.BookingDate.DayNumber-payment.ReceivedOn.DayNumber);score+=days switch{0=>0.25m,1=>0.20m,<=3=>0.10m,_=>0};
        if(!string.IsNullOrWhiteSpace(tx.Reference)&&(!string.IsNullOrWhiteSpace(payment.Reference)&&tx.Reference.Contains(payment.Reference,StringComparison.OrdinalIgnoreCase)||!string.IsNullOrWhiteSpace(payment.ExternalTransactionId)&&tx.Reference.Contains(payment.ExternalTransactionId,StringComparison.OrdinalIgnoreCase)))score+=0.20m;
        return Math.Min(1m,score);
    }
}
