namespace AppCondominio.Contracts.Messaging;

public sealed record BillingStatementAvailable(Guid CommunityId,Guid UnitId,Guid ResponsiblePersonId,int Year,int Month,decimal Outstanding,DateTimeOffset OccurredAtUtc);
