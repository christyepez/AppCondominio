using AppCondominio.Modules.Communities.Domain;

namespace AppCondominio.Modules.Communities.Application;

public interface ICommunityRepository
{
    Task<Community?> GetCommunityAsync(Guid id,CancellationToken ct);
    Task AddAsync<T>(T entity,CancellationToken ct) where T:class;
    Task<IReadOnlyCollection<Community>> ListAsync(CancellationToken ct);
    Task<int> CountAsync<T>(Guid communityId,CancellationToken ct) where T:class;
    Task SaveChangesAsync(CancellationToken ct);
}

public sealed record CreateCommunityCommand(Guid OrganizationId,string Code,string Name,string? TaxId,string Address);
public sealed record CommunityResult(Guid Id,Guid OrganizationId,string Code,string Name,string? TaxId,string Address,string? Type,string? Administrator,string? President,CommunityStatus Status);
public sealed record CommunityDashboard(int Communities,int ActiveCommunities,int StructureNodes,int CommonAreas,int BankAccounts,int Authorities,int Documents);

public sealed class CommunityService(ICommunityRepository repository)
{
    public async Task<CommunityResult> CreateAsync(CreateCommunityCommand c,CancellationToken ct)
    {var x=Community.Create(c.OrganizationId,c.Code,c.Name,c.TaxId,c.Address);await repository.AddAsync(x,ct);await repository.SaveChangesAsync(ct);return Map(x);}
    public async Task<CommunityResult> GetAsync(Guid id,CancellationToken ct)=>Map(await Require(id,ct));
    public async Task UpdateAsync(Guid id,string name,string? taxId,string address,string? type,string? administrator,string? president,CancellationToken ct)
    {var x=await Require(id,ct);x.Update(name,taxId,address,type,administrator,president);await repository.SaveChangesAsync(ct);}
    public async Task AddTaxAsync(Guid communityId,string legalName,string taxId,string establishment,string emissionPoint,bool accounting,string environment,CancellationToken ct)
    {await Require(communityId,ct);var x=TaxSettings.Create(communityId,legalName,taxId);x.Update(legalName,taxId,establishment,emissionPoint,accounting,environment);await repository.AddAsync(x,ct);await repository.SaveChangesAsync(ct);}
    public async Task<Guid> AddNodeAsync(Guid communityId,Guid? parentId,StructureNodeType type,string code,string name,CancellationToken ct)
    {await Require(communityId,ct);var x=StructureNode.Create(communityId,parentId,type,code,name);await repository.AddAsync(x,ct);await repository.SaveChangesAsync(ct);return x.Id;}
    public async Task<Guid> AddCommonAreaAsync(Guid communityId,string code,string name,bool reservable,CancellationToken ct)
    {await Require(communityId,ct);var x=CommonArea.Create(communityId,code,name,reservable);await repository.AddAsync(x,ct);await repository.SaveChangesAsync(ct);return x.Id;}
    public async Task<Guid> AddBankAsync(Guid communityId,string bank,string number,BankAccountType type,string currency,string use,string? accountingAccount,CancellationToken ct)
    {await Require(communityId,ct);var x=CommunityBankAccount.Create(communityId,bank,number,type,currency,use,accountingAccount);await repository.AddAsync(x,ct);await repository.SaveChangesAsync(ct);return x.Id;}
    public async Task<Guid> AddChargeSettingsAsync(Guid communityId,int issueDay,int dueDay,decimal interest,string order,string currency,int year,CancellationToken ct)
    {await Require(communityId,ct);var x=ChargeSettings.Create(communityId);x.Update(issueDay,dueDay,interest,order,currency,year);await repository.AddAsync(x,ct);await repository.SaveChangesAsync(ct);return x.Id;}
    public async Task<Guid> AddAuthorityAsync(Guid communityId,string role,string person,DateOnly startsOn,string? documentRef,CancellationToken ct)
    {await Require(communityId,ct);var x=CommunityAuthority.Create(communityId,role,person,startsOn,documentRef);await repository.AddAsync(x,ct);await repository.SaveChangesAsync(ct);return x.Id;}
    public async Task<Guid> AddDocumentAsync(Guid communityId,string type,string name,string reference,CancellationToken ct)
    {await Require(communityId,ct);var x=CommunityDocument.Register(communityId,type,name,reference);await repository.AddAsync(x,ct);await repository.SaveChangesAsync(ct);return x.Id;}
    public async Task<CommunityDashboard> DashboardAsync(CancellationToken ct)
    {var all=await repository.ListAsync(ct);return new(all.Count,all.Count(x=>x.Status==CommunityStatus.Active),await CountAll<StructureNode>(all,ct),await CountAll<CommonArea>(all,ct),await CountAll<CommunityBankAccount>(all,ct),await CountAll<CommunityAuthority>(all,ct),await CountAll<CommunityDocument>(all,ct));}
    private async Task<int> CountAll<T>(IReadOnlyCollection<Community> communities,CancellationToken ct) where T:class {var n=0;foreach(var c in communities)n+=await repository.CountAsync<T>(c.Id,ct);return n;}
    private async Task<Community> Require(Guid id,CancellationToken ct)=>await repository.GetCommunityAsync(id,ct)??throw new KeyNotFoundException("Community was not found.");
    private static CommunityResult Map(Community x)=>new(x.Id,x.OrganizationId,x.Code,x.Name,x.TaxId,x.Address,x.CommunityType,x.AdministratorName,x.PresidentName,x.Status);
}
