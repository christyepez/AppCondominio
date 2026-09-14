$MigrationContexts = @(
    [pscustomobject]@{ Module='Organizations'; Context='OrganizationsDbContext'; Schema='organizations' },
    [pscustomobject]@{ Module='Communities'; Context='CommunitiesDbContext'; Schema='communities' },
    [pscustomobject]@{ Module='Properties'; Context='PropertiesDbContext'; Schema='properties' },
    [pscustomobject]@{ Module='People'; Context='PeopleDbContext'; Schema='people' },
    [pscustomobject]@{ Module='Billing'; Context='BillingDbContext'; Schema='billing' },
    [pscustomobject]@{ Module='Collections'; Context='CollectionsDbContext'; Schema='collections' },
    [pscustomobject]@{ Module='Banking'; Context='BankingDbContext'; Schema='banking' },
    [pscustomobject]@{ Module='Tax'; Context='TaxDbContext'; Schema='tax' },
    [pscustomobject]@{ Module='Accounting'; Context='AccountingDbContext'; Schema='accounting' },
    [pscustomobject]@{ Module='Treasury'; Context='TreasuryDbContext'; Schema='treasury' },
    [pscustomobject]@{ Module='Budgeting'; Context='BudgetingDbContext'; Schema='budgeting' },
    [pscustomobject]@{ Module='Procurement'; Context='ProcurementDbContext'; Schema='procurement' },
    [pscustomobject]@{ Module='Maintenance'; Context='MaintenanceDbContext'; Schema='maintenance' },
    [pscustomobject]@{ Module='Reservations'; Context='ReservationsDbContext'; Schema='reservations' },
    [pscustomobject]@{ Module='SecurityOperations'; Context='SecurityOperationsDbContext'; Schema='securityops' },
    [pscustomobject]@{ Module='Governance'; Context='GovernanceDbContext'; Schema='governance' }
)

foreach ($item in $MigrationContexts) {
    $item | Add-Member NoteProperty Project "src/Modules/$($item.Module)/$($item.Module).Infrastructure/$($item.Module).Infrastructure.csproj"
}
