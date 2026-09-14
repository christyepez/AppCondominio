namespace AppCondominio.Contracts.Messaging;

public sealed record PaymentApplied(Guid CommunityId,Guid PaymentId,Guid ReceivableId,Guid BillingObligationId,decimal Amount,DateTimeOffset OccurredAtUtc);
public sealed record PaymentReversed(Guid CommunityId,Guid PaymentId,decimal Amount,DateTimeOffset OccurredAtUtc);
