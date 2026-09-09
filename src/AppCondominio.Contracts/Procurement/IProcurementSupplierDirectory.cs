namespace AppCondominio.Contracts.Procurement;

public sealed record ProcurementSupplierDirectoryEntry(
    Guid SupplierId,
    Guid CommunityId,
    string TaxId,
    string LegalName,
    bool IsActive);

public interface IProcurementSupplierDirectory
{
    Task<ProcurementSupplierDirectoryEntry?> FindAsync(Guid supplierId, CancellationToken ct);
    Task<IReadOnlyList<ProcurementSupplierDirectoryEntry>> ListActiveAsync(Guid communityId, CancellationToken ct);
}
