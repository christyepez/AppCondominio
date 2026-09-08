import { Routes } from '@angular/router';

export const routes: Routes = [
  { path: '', pathMatch: 'full', redirectTo: 'dashboard' },
  { path: 'dashboard', loadComponent: () => import('./features/dashboard/dashboard.component').then(m => m.DashboardComponent) },
  { path: 'organizations', loadComponent: () => import('./features/organizations/organizations.component').then(m => m.OrganizationsComponent) },
  { path: 'communities', loadComponent: () => import('./features/communities/communities.component').then(m => m.CommunitiesComponent) },
  { path: 'properties', loadComponent: () => import('./features/properties/properties.component').then(m => m.PropertiesComponent) },
  { path: 'governance', loadComponent: () => import('./features/governance/governance.component').then(m => m.GovernanceComponent) },
  { path: '**', redirectTo: 'dashboard' }
];
