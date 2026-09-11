using AppCondominio.Contracts.Messaging;
using AppCondominio.Modules.Collections.Domain;

namespace AppCondominio.Modules.Collections.Application;

public interface ICollectionsRepository
{
    Task<bool> ReceivableExistsForBillingObligationAsync(Guid billingObligationId,CancellationToken ct);
    Task<bool> PaymentReferenceExistsAsync(Guid communityId,string reference,string? externalTransactionId,CancellationToken ct);
    Task<Receivable?> GetReceivableAsync(Guid id,CancellationToken ct);
    Task<Payment?> GetPaymentAsync(Guid id,CancellationToken ct);
    Task<IReadOnlyList<PaymentApplication>> GetApplicationsForPaymentAsync(Guid paymentId,CancellationToken ct);
    Task<IReadOnlyList<Receivable>> GetOpenReceivablesAsync(Guid communityId,Guid? unitId,CancellationToken ct);
    Task AddAsync<T>(T entity,CancellationToken ct) where T:class;
    Task SaveChangesAsync(CancellationToken ct);
}

public sealed record PaymentResult(Guid PaymentId,decimal Amount,decimal UnappliedAmount,PaymentStatus Status);
public sealed record ReceivableResult(Guid ReceivableId,Guid BillingObligationId,Guid UnitId,Guid ResponsiblePersonId,decimal OriginalAmount,decimal OutstandingAmount,DateOnly DueOn,ReceivableStatus Status,AgingBucket AgingBucket);

public sealed class CollectionsService(ICollectionsRepository repository,IIntegrationEventPublisher publisher)
{
    public async Task<Guid> CreateReceivableAsync(Guid communityId,Guid unitId,Guid responsiblePersonId,Guid billingObligationId,decimal amount,DateOnly issuedOn,DateOnly dueOn,CancellationToken ct)
    {
        if(await repository.ReceivableExistsForBillingObligationAsync(billingObligationId,ct))throw new InvalidOperationException("Receivable already exists for this billing obligation.");
        var receivable=Receivable.Create(communityId,unitId,responsiblePersonId,billingObligationId,amount,issuedOn,dueOn);await repository.AddAsync(receivable,ct);await repository.SaveChangesAsync(ct);return receivable.Id;
    }

    public async Task<Guid> RegisterPaymentAsync(Guid communityId,string reference,PaymentMethod method,DateOnly receivedOn,decimal amount,string? externalTransactionId,CancellationToken ct)
    {
        if(await repository.PaymentReferenceExistsAsync(communityId,reference.Trim(),externalTransactionId?.Trim(),ct))throw new InvalidOperationException("Payment reference/transaction already exists.");
        var payment=Payment.Register(communityId,reference,method,receivedOn,amount,externalTransactionId);await repository.AddAsync(payment,ct);await repository.SaveChangesAsync(ct);return payment.Id;
    }

    public async Task<Guid> ApplyPaymentAsync(Guid paymentId,Guid receivableId,decimal amount,string appliedBy,CancellationToken ct)
    {
        var payment=await repository.GetPaymentAsync(paymentId,ct)??throw new KeyNotFoundException("Payment was not found.");
        var receivable=await repository.GetReceivableAsync(receivableId,ct)??throw new KeyNotFoundException("Receivable was not found.");
        if(payment.CommunityId!=receivable.CommunityId)throw new InvalidOperationException("Payment and receivable belong to different communities.");
        decimal applied=Math.Min(amount,Math.Min(payment.UnappliedAmount,receivable.OutstandingAmount));if(applied<=0)throw new InvalidOperationException("No amount is available to apply.");
        payment.Apply(applied);receivable.Apply(applied);var application=PaymentApplication.Create(payment.CommunityId,payment.Id,receivable.Id,applied,appliedBy);await repository.AddAsync(application,ct);await repository.SaveChangesAsync(ct);
        await publisher.PublishAsync(new PaymentApplied(payment.CommunityId,payment.Id,receivable.Id,receivable.BillingObligationId,applied,DateTimeOffset.UtcNow),ct);return application.Id;
    }

    public async Task ReversePaymentAsync(Guid paymentId,string reversedBy,CancellationToken ct)
    {
        var payment=await repository.GetPaymentAsync(paymentId,ct)??throw new KeyNotFoundException("Payment was not found.");if(payment.Status==PaymentStatus.Reversed)return;
        var applications=await repository.GetApplicationsForPaymentAsync(paymentId,ct);
        foreach(var application in applications.Where(x=>!x.IsReversed))
        {
            var receivable=await repository.GetReceivableAsync(application.ReceivableId,ct)??throw new InvalidOperationException("Applied receivable was not found.");
            receivable.Restore(application.Amount);application.Reverse(reversedBy);
        }
        decimal amount=payment.Amount;payment.Reverse();await repository.SaveChangesAsync(ct);await publisher.PublishAsync(new PaymentReversed(payment.CommunityId,payment.Id,amount,DateTimeOffset.UtcNow),ct);
    }

    public async Task<PaymentResult> GetPaymentAsync(Guid paymentId,CancellationToken ct)
    {
        var x=await repository.GetPaymentAsync(paymentId,ct)??throw new KeyNotFoundException("Payment was not found.");return new(x.Id,x.Amount,x.UnappliedAmount,x.Status);
    }

    public async Task<IReadOnlyList<ReceivableResult>> GetReceivablesAsync(Guid communityId,Guid? unitId,DateOnly on,CancellationToken ct)=>(await repository.GetOpenReceivablesAsync(communityId,unitId,ct)).Select(x=>new ReceivableResult(x.Id,x.BillingObligationId,x.UnitId,x.ResponsiblePersonId,x.OriginalAmount,x.OutstandingAmount,x.DueOn,x.Status,x.GetAging(on))).ToArray();

    public async Task<AgingSummary> GetAgingAsync(Guid communityId,DateOnly on,CancellationToken ct)
    {
        var rows=await repository.GetOpenReceivablesAsync(communityId,null,ct);decimal c=0,a=0,b=0,d=0,e=0;
        foreach(var x in rows.Where(x=>x.OutstandingAmount>0))switch(x.GetAging(on)){case AgingBucket.Current:c+=x.OutstandingAmount;break;case AgingBucket.Days1To30:a+=x.OutstandingAmount;break;case AgingBucket.Days31To60:b+=x.OutstandingAmount;break;case AgingBucket.Days61To90:d+=x.OutstandingAmount;break;case AgingBucket.Days91Plus:e+=x.OutstandingAmount;break;}
        return new(c,a,b,d,e,c+a+b+d+e);
    }
}
