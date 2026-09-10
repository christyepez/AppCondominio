namespace AppCondominio.Contracts.Messaging;

public sealed record FoundationPing(Guid MessageId, DateTimeOffset CreatedAtUtc, string Source);
