param(
    [string]$DatabaseName = 'AppCondominioMigrationRecoveryProbe',
    [string]$BackupFile = '/var/opt/mssql/data/appcondominio-rc06-migration-recovery.bak',
    [string]$EvidenceDirectory = 'artifacts/release'
)

$ErrorActionPreference = 'Stop'
if ($DatabaseName -notmatch '^[A-Za-z0-9_]+$') { throw 'DatabaseName may contain only letters, digits and underscore.' }
$repoRoot = Resolve-Path (Join-Path $PSScriptRoot '..\..')
Push-Location $repoRoot

$sqlcmd = 'exec /opt/mssql-tools18/bin/sqlcmd -S localhost -U sa -P "$MSSQL_SA_PASSWORD" -C -b -i /dev/stdin'
function Invoke-SqlInContainer([string]$Sql, [switch]$Capture) {
    $output = $Sql | & docker compose exec -T sqlserver bash -lc $sqlcmd 2>&1
    if ($LASTEXITCODE -ne 0) { throw ($output -join "`n") }
    if ($Capture) { return $output }
}

function Get-EnvFileValue([string]$Name) {
    $line = Get-Content '.env' | Where-Object { $_ -match "^$([regex]::Escape($Name))=" } | Select-Object -First 1
    if (-not $line) { throw "Required .env value '$Name' was not found." }
    return ($line -split '=', 2)[1]
}

$cleanupSql = @"
USE [master];
IF DB_ID(N'$DatabaseName') IS NOT NULL BEGIN
    EXEC(N'ALTER DATABASE [$DatabaseName] SET SINGLE_USER WITH ROLLBACK IMMEDIATE');
    EXEC(N'DROP DATABASE [$DatabaseName]');
END;
"@
$previousOrganizationsConnection = [Environment]::GetEnvironmentVariable('ConnectionStrings__Organizations')
try {
    if ([string]::IsNullOrWhiteSpace((& docker compose ps -q sqlserver).Trim())) {
        throw 'SQL Server compose service is not running.'
    }

    Invoke-SqlInContainer $cleanupSql
    Invoke-SqlInContainer @"
USE [master];
CREATE DATABASE [$DatabaseName];
"@
    Invoke-SqlInContainer @"
USE [$DatabaseName];
CREATE TABLE dbo.RecoveryBaseline(Id int NOT NULL PRIMARY KEY, Marker nvarchar(100) NOT NULL);
INSERT INTO dbo.RecoveryBaseline(Id, Marker) VALUES (1, N'AppCondominio-RC06-BASELINE');
"@
    Invoke-SqlInContainer @"
USE [master];
BACKUP DATABASE [$DatabaseName] TO DISK = N'$BackupFile' WITH INIT, COPY_ONLY, CHECKSUM;
"@

    New-Item -ItemType Directory -Force -Path $EvidenceDirectory | Out-Null
    $backupEvidencePath = Join-Path $EvidenceDirectory 'rc06-pre-migration-backup.json'
    [ordered]@{ generatedAtUtc=(Get-Date).ToUniversalTime().ToString('o'); commitSha=(git rev-parse HEAD).Trim(); database=$DatabaseName; checksum=$true; result='PASS' } |
        ConvertTo-Json | Set-Content -Path $backupEvidencePath -Encoding utf8

    $port = Get-EnvFileValue 'SQLSERVER_PORT'
    $password = Get-EnvFileValue 'SQLSERVER_SA_PASSWORD'
    $connection = "Server=127.0.0.1,$port;Database=$DatabaseName;User Id=sa;Password=$password;TrustServerCertificate=True;Encrypt=False"
    [Environment]::SetEnvironmentVariable('ConnectionStrings__Organizations', $connection)
    & (Join-Path $PSScriptRoot 'apply-migrations.ps1') -Apply -BackupEvidencePath $backupEvidencePath -Context 'OrganizationsDbContext' -EvidenceDirectory $EvidenceDirectory
    if ($LASTEXITCODE -ne 0) { throw 'Real Organizations migration apply failed in recovery rehearsal.' }

    $historyOutput = Invoke-SqlInContainer @"
SET NOCOUNT ON;
SELECT COUNT(*) AS MigrationCount FROM [$DatabaseName].[organizations].[__EFMigrationsHistory];
"@ -Capture
    $migrationCountLine = @($historyOutput | ForEach-Object { $_.ToString().Trim() } | Where-Object { $_ -match '^\d+$' }) | Select-Object -Last 1
    if (-not $migrationCountLine -or [int]$migrationCountLine -lt 1) { throw 'Expected applied Organizations migrations were not found.' }
    $migrationCount = [int]$migrationCountLine

    $failureSql = @"
USE [$DatabaseName];
BEGIN TRY
    BEGIN TRANSACTION;
    CREATE TABLE organizations.Rc06FailedMigrationProbe(Id int NOT NULL PRIMARY KEY);
    THROW 51006, 'RC06 controlled migration failure', 1;
    COMMIT TRANSACTION;
END TRY
BEGIN CATCH
    IF @@TRANCOUNT > 0 ROLLBACK TRANSACTION;
    THROW;
END CATCH;
"@
    $failureOutput = $failureSql | & docker compose exec -T sqlserver bash -lc $sqlcmd 2>&1
    $expectedFailureObserved = $LASTEXITCODE -ne 0 -and (($failureOutput -join "`n") -match 'RC06 controlled migration failure')
    if (-not $expectedFailureObserved) { throw 'Controlled migration failure was not observed.' }
    $partialCheck = Invoke-SqlInContainer @"
SET NOCOUNT ON;
SELECT CASE WHEN OBJECT_ID(N'[$DatabaseName].[organizations].[Rc06FailedMigrationProbe]', N'U') IS NULL THEN N'PARTIAL_ABSENT' ELSE N'PARTIAL_PRESENT' END;
"@ -Capture
    if (($partialCheck -join "`n") -notmatch 'PARTIAL_ABSENT') { throw 'Failed migration left a partial object behind.' }

    Invoke-SqlInContainer @"
USE [$DatabaseName];
CREATE TABLE organizations.Rc06ForwardFixProbe(Id int NOT NULL PRIMARY KEY, Marker nvarchar(100) NOT NULL);
INSERT INTO organizations.Rc06ForwardFixProbe(Id, Marker) VALUES (1, N'FORWARD_FIX_APPLIED');
"@
    $forwardFixCheck = Invoke-SqlInContainer @"
SET NOCOUNT ON;
SELECT Marker FROM [$DatabaseName].[organizations].[Rc06ForwardFixProbe] WHERE Id = 1;
"@ -Capture
    if (($forwardFixCheck -join "`n") -notmatch 'FORWARD_FIX_APPLIED') { throw 'Forward-fix recovery marker was not verified.' }

    Invoke-SqlInContainer @"
USE [master];
ALTER DATABASE [$DatabaseName] SET SINGLE_USER WITH ROLLBACK IMMEDIATE;
RESTORE DATABASE [$DatabaseName] FROM DISK = N'$BackupFile' WITH REPLACE, CHECKSUM;
ALTER DATABASE [$DatabaseName] SET MULTI_USER;
"@

    $restoreCheck = Invoke-SqlInContainer @"
SET NOCOUNT ON;
SELECT Marker FROM [$DatabaseName].dbo.RecoveryBaseline WHERE Id = 1;
SELECT CASE WHEN OBJECT_ID(N'[$DatabaseName].[organizations].[__EFMigrationsHistory]', N'U') IS NULL THEN N'MIGRATIONS_REMOVED' ELSE N'MIGRATIONS_PRESENT' END;
SELECT CASE WHEN OBJECT_ID(N'[$DatabaseName].[organizations].[Rc06ForwardFixProbe]', N'U') IS NULL THEN N'FORWARD_FIX_REMOVED' ELSE N'FORWARD_FIX_PRESENT' END;
"@ -Capture
    $restoreText = $restoreCheck -join "`n"
    if ($restoreText -notmatch 'AppCondominio-RC06-BASELINE' -or $restoreText -notmatch 'MIGRATIONS_REMOVED' -or $restoreText -notmatch 'FORWARD_FIX_REMOVED') {
        throw 'Pre-migration backup restore did not return the expected baseline state.'
    }
    $evidencePath = Join-Path $EvidenceDirectory ("migration-recovery-rehearsal-{0}.json" -f (Get-Date).ToUniversalTime().ToString('yyyyMMddTHHmmssZ'))
    [ordered]@{
        generatedAtUtc = (Get-Date).ToUniversalTime().ToString('o')
        commitSha = (git rev-parse HEAD).Trim()
        database = $DatabaseName
        context = 'OrganizationsDbContext'
        appliedMigrationCount = $migrationCount
        controlledFailureObserved = $true
        partialObjectAbsent = $true
        forwardFixVerified = $true
        backupRestoreVerified = $true
        restoredToPreMigrationBaseline = $true
        result = 'PASS'
    } | ConvertTo-Json -Depth 4 | Set-Content -Path $evidencePath -Encoding utf8

    Write-Host "Migration recovery rehearsal passed. Evidence: $evidencePath"
}
finally {
    [Environment]::SetEnvironmentVariable('ConnectionStrings__Organizations', $previousOrganizationsConnection)
    try {
        if (-not [string]::IsNullOrWhiteSpace((& docker compose ps -q sqlserver).Trim())) {
            Invoke-SqlInContainer $cleanupSql | Out-Null
        }
    } catch {
        Write-Warning "RC-06 cleanup failed: $($_.Exception.Message)"
    }
    Pop-Location
}
