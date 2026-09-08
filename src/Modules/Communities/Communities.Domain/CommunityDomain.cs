using AppCondominio.SharedKernel;

namespace AppCondominio.Modules.Communities.Domain;

public enum CommunityStatus { Draft = 0, Active = 1, Suspended = 2, Archived = 3 }
public enum StructureNodeType { Stage = 1, Block = 2, Tower = 3, Floor = 4, Unit = 5 }
public enum BankAccountType { Checking = 1, Savings = 2, Other = 9 }

public sealed class Community : AggregateRoot
{
    private Community(Guid id, Guid organizationId, string code, string name, string? taxId, string address) : base(id)
    { OrganizationId=organizationId; Code=code; Name=name; TaxId=taxId; Address=address; Status=CommunityStatus.Active; }
    public Guid OrganizationId { get; private set; }
    public string Code { get; private set; }
    public string Name { get; private set; }
    public string? TaxId { get; private set; }
    public string Address { get; private set; }
    public string? CommunityType { get; private set; }
    public string? AdministratorName { get; private set; }
    public string? PresidentName { get; private set; }
    public CommunityStatus Status { get; private set; }
    public static Community Create(Guid organizationId,string code,string name,string? taxId,string address)
    {
        if(organizationId==Guid.Empty) throw new ArgumentException("Organization is required.");
        return new(Guid.NewGuid(),organizationId,Req(code).ToLowerInvariant(),Req(name),Opt(taxId),Req(address));
    }
    public void Update(string name,string? taxId,string address,string? type,string? administrator,string? president)
    { Name=Req(name); TaxId=Opt(taxId); Address=Req(address); CommunityType=Opt(type); AdministratorName=Opt(administrator); PresidentName=Opt(president); }
    private static string Req(string v)=>string.IsNullOrWhiteSpace(v)?throw new ArgumentException("Required value missing."):v.Trim();
    private static string? Opt(string? v)=>string.IsNullOrWhiteSpace(v)?null:v.Trim();
}

public sealed class TaxSettings : AggregateRoot
{
    private TaxSettings(Guid id,Guid communityId,string legalName,string taxId):base(id){CommunityId=communityId;LegalName=legalName;TaxId=taxId;}
    public Guid CommunityId{get;private set;} public string LegalName{get;private set;} public string TaxId{get;private set;}
    public string Establishment{get;private set;}="001"; public string EmissionPoint{get;private set;}="001";
    public bool AccountingObligation{get;private set;} public string SriEnvironment{get;private set;}="TEST";
    public static TaxSettings Create(Guid communityId,string legalName,string taxId)=>new(Guid.NewGuid(),communityId,legalName.Trim(),taxId.Trim());
    public void Update(string legalName,string taxId,string establishment,string emissionPoint,bool accountingObligation,string sriEnvironment)
    {LegalName=legalName.Trim();TaxId=taxId.Trim();Establishment=establishment.Trim();EmissionPoint=emissionPoint.Trim();AccountingObligation=accountingObligation;SriEnvironment=sriEnvironment.Trim().ToUpperInvariant();}
}

public sealed class StructureNode : AggregateRoot
{
    private StructureNode(Guid id,Guid communityId,Guid? parentId,StructureNodeType type,string code,string name):base(id){CommunityId=communityId;ParentId=parentId;Type=type;Code=code;Name=name;}
    public Guid CommunityId{get;private set;} public Guid? ParentId{get;private set;} public StructureNodeType Type{get;private set;} public string Code{get;private set;} public string Name{get;private set;}
    public static StructureNode Create(Guid communityId,Guid? parentId,StructureNodeType type,string code,string name)=>new(Guid.NewGuid(),communityId,parentId,type,code.Trim(),name.Trim());
}

public sealed class CommonArea : AggregateRoot
{
    private CommonArea(Guid id,Guid communityId,string code,string name):base(id){CommunityId=communityId;Code=code;Name=name;}
    public Guid CommunityId{get;private set;} public string Code{get;private set;} public string Name{get;private set;} public bool IsReservable{get;private set;}
    public static CommonArea Create(Guid communityId,string code,string name,bool reservable){var x=new CommonArea(Guid.NewGuid(),communityId,code.Trim(),name.Trim());x.IsReservable=reservable;return x;}
}

public sealed class CommunityBankAccount : AggregateRoot
{
    private CommunityBankAccount(Guid id,Guid communityId,string bank,string number,BankAccountType type,string currency,string use):base(id){CommunityId=communityId;Bank=bank;Number=number;Type=type;Currency=currency;Use=use;}
    public Guid CommunityId{get;private set;} public string Bank{get;private set;} public string Number{get;private set;} public BankAccountType Type{get;private set;} public string Currency{get;private set;} public string Use{get;private set;} public string? AccountingAccount{get;private set;}
    public static CommunityBankAccount Create(Guid communityId,string bank,string number,BankAccountType type,string currency,string use,string? accountingAccount){var x=new CommunityBankAccount(Guid.NewGuid(),communityId,bank.Trim(),number.Trim(),type,currency.Trim().ToUpperInvariant(),use.Trim());x.AccountingAccount=string.IsNullOrWhiteSpace(accountingAccount)?null:accountingAccount.Trim();return x;}
}

public sealed class ChargeSettings : AggregateRoot
{
    private ChargeSettings(Guid id,Guid communityId):base(id){CommunityId=communityId;}
    public Guid CommunityId{get;private set;} public int IssueDay{get;private set;}=1; public int DueDay{get;private set;}=10; public decimal InterestRate{get;private set;} public string ApplicationOrder{get;private set;}="interest,oldest,current,credit"; public string Currency{get;private set;}="USD"; public int FiscalYear{get;private set;}=DateTime.UtcNow.Year;
    public static ChargeSettings Create(Guid communityId)=>new(Guid.NewGuid(),communityId);
    public void Update(int issueDay,int dueDay,decimal interest,string order,string currency,int fiscalYear){if(issueDay is<1 or>28||dueDay is<1 or>31)throw new ArgumentOutOfRangeException();IssueDay=issueDay;DueDay=dueDay;InterestRate=interest;ApplicationOrder=order.Trim();Currency=currency.Trim().ToUpperInvariant();FiscalYear=fiscalYear;}
}

public sealed class CommunityAuthority : AggregateRoot
{
    private CommunityAuthority(Guid id,Guid communityId,string role,string personName,DateOnly startsOn):base(id){CommunityId=communityId;Role=role;PersonName=personName;StartsOn=startsOn;}
    public Guid CommunityId{get;private set;} public string Role{get;private set;} public string PersonName{get;private set;} public DateOnly StartsOn{get;private set;} public DateOnly? EndsOn{get;private set;} public string? SupportDocumentReference{get;private set;}
    public static CommunityAuthority Create(Guid communityId,string role,string personName,DateOnly startsOn,string? documentRef){var x=new CommunityAuthority(Guid.NewGuid(),communityId,role.Trim(),personName.Trim(),startsOn);x.SupportDocumentReference=string.IsNullOrWhiteSpace(documentRef)?null:documentRef.Trim();return x;}
    public void End(DateOnly endsOn){if(endsOn<StartsOn)throw new ArgumentException("Invalid authority end date.");EndsOn=endsOn;}
}

public sealed class CommunityDocument : AggregateRoot
{
    private CommunityDocument(Guid id,Guid communityId,string type,string name,string externalReference):base(id){CommunityId=communityId;DocumentType=type;Name=name;ExternalReference=externalReference;}
    public Guid CommunityId{get;private set;} public string DocumentType{get;private set;} public string Name{get;private set;} public string ExternalReference{get;private set;} public DateTimeOffset RegisteredAtUtc{get;private set;}=DateTimeOffset.UtcNow;
    public static CommunityDocument Register(Guid communityId,string type,string name,string externalReference){if(string.IsNullOrWhiteSpace(externalReference))throw new ArgumentException("Content reference is required; binary storage is owned by Portal Content.");return new(Guid.NewGuid(),communityId,type.Trim(),name.Trim(),externalReference.Trim());}
}
