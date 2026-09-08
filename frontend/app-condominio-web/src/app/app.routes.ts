import { Routes } from '@angular/router';

export const routes: Routes = [
  { path: '', pathMatch: 'full', redirectTo: 'dashboard' },
  { path: 'dashboard', loadComponent: () => import('./features/dashboard/dashboard.component').then(m => m.DashboardComponent) },
  { path: 'organizations', loadComponent: () => import('./features/organizations/organizations.component').then(m => m.OrganizationsComponent) },
  { path: 'saas', loadComponent: () => import('./features/saas/saas.component').then(m => m.SaasComponent) },
  { path: 'communities', loadComponent: () => import('./features/communities/communities.component').then(m => m.CommunitiesComponent) },
  { path: 'properties', loadComponent: () => import('./features/properties/properties.component').then(m => m.PropertiesComponent) },
  { path: 'people', loadComponent: () => import('./features/people/people.component').then(m => m.PeopleComponent) },
  { path: 'billing', loadComponent: () => import('./features/billing/billing.component').then(m => m.BillingComponent) },
  { path: 'collections', loadComponent: () => import('./features/collections/collections.component').then(m => m.CollectionsComponent) },
  { path: 'banking', loadComponent: () => import('./features/banking/banking.component').then(m => m.BankingComponent) },
  { path: 'tax', loadComponent: () => import('./features/tax/tax.component').then(m => m.TaxComponent) },
  { path: 'accounting', loadComponent: () => import('./features/accounting/accounting.component').then(m => m.AccountingComponent) },
  { path: 'treasury', loadComponent: () => import('./features/treasury/treasury.component').then(m => m.TreasuryComponent) },
  { path: 'budgeting', loadComponent: () => import('./features/budgeting/budgeting.component').then(m => m.BudgetingComponent) },
  { path: 'procurement', loadComponent: () => import('./features/procurement/procurement.component').then(m => m.ProcurementComponent) },
  { path: 'maintenance', loadComponent: () => import('./features/maintenance/maintenance.component').then(m => m.MaintenanceComponent) },
  { path: 'reservations', loadComponent: () => import('./features/reservations/reservations.component').then(m => m.ReservationsComponent) },
  { path: 'security', loadComponent: () => import('./features/security-operations/security-operations.component').then(m => m.SecurityOperationsComponent) },
  { path: 'governance', loadComponent: () => import('./features/governance/governance.component').then(m => m.GovernanceComponent) },
  { path: 'resident', loadComponent: () => import('./features/resident-portal/resident-portal.component').then(m => m.ResidentPortalComponent) },
  { path: '**', redirectTo: 'dashboard' }
];
