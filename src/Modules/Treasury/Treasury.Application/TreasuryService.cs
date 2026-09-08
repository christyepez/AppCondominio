using AppCondominio.Modules.Treasury.Domain;

namespace AppCondominio.Modules.Treasury.Application;

public interface ITreasuryRepository
{
    Task AddAsync<T>(T entity,CancellationToken ct) where T:class;
    Task<SupplierPayable?> GetPayableAsync(Guid id,CancellationToken ct);
    Task<PaymentRequest?> GetRequestAsync(Guid id,CancellationToken ct);
    Task<TreasuryDisbursement?> GetDisbursementAsync(Guid id,CancellationToken ct);
    Task<bool> DocumentExistsAsync(Guid communityId,Guid supplierId,string documentNumber,CancellationToken ct);
    Task<bool> PaymentReferenceExistsAsync(Guid communityId,string paymentReference,CancellationToken ct);
    Task<IReadOnlyList<SupplierPayable>> GetOutstandingAsync(Guid communityId,CancellationToken ct);
    Task SaveChangesAsync(CancellationToken ct);
}

public sealed class TreasuryService(ITreasuryRepository repository)
{
    public async Task<Guid> CreatePayableAsync(Guid communityId,Guid supplierId,string documentNumber,DateOnly documentDate,DateOnly dueDate,decimal amount,string expenseAccount,string payableAccount,CancellationToken ct){if(await repository.DocumentExistsAsync(communityId,supplierId,documentNumber,ct))throw new InvalidOperationException("Supplier document already exists.");var x=SupplierPayable.Create(communityId,supplierId,documentNumber,documentDate,dueDate,amount,expenseAccount,payableAccount);await repository.AddAsync(x,ct);await repository.SaveChangesAsync(ct);return x.Id;}
    public async Task<Guid> RequestPaymentAsync(Guid payableId,decimal amount,string requestedBy,CancellationToken ct){var p=await repository.GetPayableAsync(payableId,ct)??throw new KeyNotFoundException("Payable not found.");if(amount>p.OutstandingAmount)throw new InvalidOperationException("Requested amount exceeds payable outstanding balance.");var r=PaymentRequest.Create(p.CommunityId,p.Id,amount,requestedBy);await repository.AddAsync(r,ct);await repository.SaveChangesAsync(ct);return r.Id;}
    public async Task ApproveAsync(Guid requestId,string by,CancellationToken ct){var r=await repository.GetRequestAsync(requestId,ct)??throw new KeyNotFoundException("Payment request not found.");var p=await repository.GetPayableAsync(r.PayableId,ct)??throw new KeyNotFoundException("Payable not found.");if(r.Amount>p.OutstandingAmount)throw new InvalidOperationException("Approved request exceeds current outstanding balance.");r.Approve(by);await repository.SaveChangesAsync(ct);}
    public async Task RejectAsync(Guid requestId,string by,string reason,CancellationToken ct){var r=await repository.GetRequestAsync(requestId,ct)??throw new KeyNotFoundException("Payment request not found.");r.Reject(by,reason);await repository.SaveChangesAsync(ct);}
    public async Task<Guid> PayAsync(Guid requestId,Guid bankAccountId,string paymentReference,DateOnly paidOn,CancellationToken ct){var r=await repository.GetRequestAsync(requestId,ct)??throw new KeyNotFoundException("Payment request not found.");if(r.Status!=PaymentRequestStatus.Approved)throw new InvalidOperationException("Payment request is not approved.");if(await repository.PaymentReferenceExistsAsync(r.CommunityId,paymentReference,ct))throw new InvalidOperationException("Payment reference already exists.");var p=await repository.GetPayableAsync(r.PayableId,ct)??throw new KeyNotFoundException("Payable not found.");if(r.Amount>p.OutstandingAmount)throw new InvalidOperationException("Payment exceeds current outstanding balance.");var d=TreasuryDisbursement.Create(r.CommunityId,r.Id,p.Id,bankAccountId,r.Amount,paymentReference,paidOn);p.ApplyPayment(r.Amount);r.MarkPaid();await repository.AddAsync(d,ct);await repository.SaveChangesAsync(ct);return d.Id;}
    public async Task ReverseAsync(Guid disbursementId,CancellationToken ct){var d=await repository.GetDisbursementAsync(disbursementId,ct)??throw new KeyNotFoundException("Disbursement not found.");var p=await repository.GetPayableAsync(d.PayableId,ct)??throw new KeyNotFoundException("Payable not found.");d.Reverse();p.RestorePayment(d.Amount);await repository.SaveChangesAsync(ct);}
    public async Task<IReadOnlyList<CashForecastRow>> ForecastAsync(Guid communityId,CancellationToken ct)=>(await repository.GetOutstandingAsync(communityId,ct)).Where(x=>x.Status is PayableStatus.Open or PayableStatus.PartiallyPaid).GroupBy(x=>x.DueDate).OrderBy(x=>x.Key).Select(g=>new CashForecastRow(g.Key,g.Sum(x=>x.OutstandingAmount),g.Count())).ToArray();
}
