param(
    [string]$DatabaseName = 'AppCondominioBackupProbe',
    [string]$BackupFile = '/var/opt/mssql/data/appcondominio-rc-backup-restore.bak',
    [string]$EvidenceDirectory = 'artifacts/release'
)

$ErrorActionPreference = 'Stop'
if ($DatabaseName -notmatch '^[A-Za-z0-9_]+$') { throw 'DatabaseName may contain only letters, digits and underscore.' }
$repoRoot = Resolve-Path (Join-Path $PSScriptRoot '..\..')
Push-Location $repoRoot

$sqlcmd = 'exec /opt/mssql-tools18/bin/sqlcmd -S localhost -U sa -P "$MSSQL_SA_PASSWORD" -C -b -i /dev/stdin'
function Invoke-SqlInContainer([string]$Sql, [switch]$Capture) {
    if ($Capture) {
        $output = $Sql | & docker compose exec -T sqlserver bash -lc $sqlcmd 2>&1
        if ($LASTEXITCODE -ne 0) { throw ($output -join "`n") }
        return $output
    }

    $Sql | & docker compose exec -T sqlserver bash -lc $sqlcmd
    if ($LASTEXITCODE -ne 0) { throw 'SQL command failed inside compose SQL Server.' }
}

$cleanupSql = @"
USE [master];
IF DB_ID(N'$DatabaseName') IS NOT NULL BEGIN
    EXEC(N'ALTER DATABASE [$DatabaseName] SET SINGLE_USER WITH ROLLBACK IMMEDIATE');
    EXEC(N'DROP DATABASE [$DatabaseName]');
END;
"@
try {
    $containerId = (& docker compose ps -q sqlserver).Trim()
    if ([string]::IsNullOrWhiteSpace($containerId)) {
        throw 'SQL Server compose service is not running.'
    }

    Invoke-SqlInContainer $cleanupSql

    $backupRestoreSql = @"
USE [master];
CREATE DATABASE [$DatabaseName];
USE [$DatabaseName];
CREATE TABLE dbo.RestoreProbe(Id int NOT NULL PRIMARY KEY, Marker nvarchar(100) NOT NULL);
INSERT INTO dbo.RestoreProbe(Id, Marker) VALUES (1, N'AppCondominio-RC03');
BACKUP DATABASE [$DatabaseName] TO DISK = N'$BackupFile' WITH INIT, COPY_ONLY, CHECKSUM;
USE [master];
ALTER DATABASE [$DatabaseName] SET SINGLE_USER WITH ROLLBACK IMMEDIATE;
DROP DATABASE [$DatabaseName];
RESTORE DATABASE [$DatabaseName] FROM DISK = N'$BackupFile' WITH CHECKSUM;
"@
    Invoke-SqlInContainer $backupRestoreSql

    $verifySql = @"
SET NOCOUNT ON;
SELECT Marker FROM [$DatabaseName].dbo.RestoreProbe WHERE Id = 1;
"@
    $verifyOutput = Invoke-SqlInContainer $verifySql -Capture
    if (($verifyOutput -join "`n") -notmatch 'AppCondominio-RC03') {
        throw 'Restored database marker was not found.'
    }
    New-Item -ItemType Directory -Force -Path $EvidenceDirectory | Out-Null
    $evidencePath = Join-Path $EvidenceDirectory ("backup-restore-smoke-{0}.json" -f (Get-Date).ToUniversalTime().ToString('yyyyMMddTHHmmssZ'))
    [ordered]@{
        generatedAtUtc = (Get-Date).ToUniversalTime().ToString('o')
        commitSha = (git rev-parse HEAD).Trim()
        database = $DatabaseName
        backupFile = (Split-Path $BackupFile -Leaf)
        checksum = $true
        markerVerified = $true
        result = 'PASS'
    } | ConvertTo-Json -Depth 4 | Set-Content -Path $evidencePath -Encoding utf8

    Write-Host "Backup/restore smoke passed. Evidence: $evidencePath"
}
finally {
    try {
        if (-not [string]::IsNullOrWhiteSpace((& docker compose ps -q sqlserver).Trim())) {
            Invoke-SqlInContainer $cleanupSql | Out-Null
        }
    } catch {
        Write-Warning "Cleanup failed: $($_.Exception.Message)"
    }
    Pop-Location
}
