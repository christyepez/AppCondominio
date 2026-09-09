param(
    [switch]$Apply,
    [Parameter(Mandatory = $true)][string]$BackupEvidencePath,
    [string[]]$Context
)

$ErrorActionPreference = 'Stop'

& (Join-Path $PSScriptRoot 'apply-migrations.ps1') `
    -Apply:$Apply `
    -BackupEvidencePath $BackupEvidencePath `
    -Context $Context
