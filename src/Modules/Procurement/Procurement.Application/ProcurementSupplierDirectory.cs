using AppCondominio.Contracts.Procurement;

namespace AppCondominio.Modules.Procurement.Application;

public sealed class ProcurementSupplierDirectory(IProcurementRepository repository) : IProcurementSupplierDirectory
{
    public async Task<ProcurementSupplierDirectoryEntry?> FindAsync(Guid supplierId, CancellationToken ct)
    {
        var supplier = await repository.GetSupplierAsync(supplierId, ct);
        return supplier is null
            ? null
            : new ProcurementSupplierDirectoryEntry(supplier.Id, supplier.CommunityId, supplier.TaxId, supplier.LegalName, supplier.IsActive);
    }

    public async Task<IReadOnlyList<ProcurementSupplierDirectoryEntry>> ListActiveAsync(Guid communityId, CancellationToken ct) =>
        (await repository.GetActiveSuppliersAsync(communityId, ct))
            .Select(x => new ProcurementSupplierDirectoryEntry(x.Id, x.CommunityId, x.TaxId, x.LegalName, x.IsActive))
            .ToArray();
}
