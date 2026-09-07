$ErrorActionPreference = 'Stop'

dotnet tool restore
dotnet ef database update `
  --project src/Modules/Organizations/Organizations.Infrastructure/Organizations.Infrastructure.csproj `
  --startup-project src/AppCondominio.Api/AppCondominio.Api.csproj `
  --context OrganizationsDbContext
