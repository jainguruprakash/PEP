import { Routes } from '@angular/router';
import { authGuard, loginGuard, roleGuard } from './core/auth.guard';

export const routes: Routes = [
  {
    path: '',
    loadComponent: () => import('./layout/app-shell.component').then(m => m.AppShellComponent),
    canActivate: [authGuard],
    children: [
      { path: '', pathMatch: 'full', redirectTo: 'dashboard' },
      { path: 'dashboard', loadComponent: () => import('./features/dashboard/dashboard.component').then(m => m.DashboardComponent) },
      { path: 'screening/customer', loadComponent: () => import('./features/screening/customer-screening.component').then(m => m.CustomerScreeningComponent) },
      { path: 'screening/transaction', loadComponent: () => import('./features/screening/transaction-screening.component').then(m => m.TransactionScreeningComponent) },
      { path: 'search', loadComponent: () => import('./features/search/search.component').then(m => m.SearchComponent) },
      { path: 'watchlists', loadComponent: () => import('./features/watchlists/watchlists.component').then(m => m.WatchlistsComponent) },
      { path: 'sanctions', loadComponent: () => import('./features/sanctions/sanctions.component').then(m => m.SanctionsComponent) },
      { path: 'alerts', loadComponent: () => import('./features/alerts/alerts.component').then(m => m.AlertsComponent) },
      { path: 'alerts/:id', loadComponent: () => import('./features/alerts/alert-detail.component').then(m => m.AlertDetailComponent) },
      { path: 'customers', loadComponent: () => import('./features/customers/customers.component').then(m => m.CustomersComponent) },
      {
        path: 'settings',
        loadComponent: () => import('./features/settings/settings.component').then(m => m.SettingsComponent),
        canActivate: [roleGuard(['Admin', 'Manager'])]
      },
      {
        path: 'organizations',
        loadComponent: () => import('./features/organizations/organizations.component').then(m => m.OrganizationsComponent),
        canActivate: [roleGuard(['Admin'])]
      }
    ]
  },
  {
    path: 'login',
    loadComponent: () => import('./features/auth/login.component').then(m => m.LoginComponent),
    canActivate: [loginGuard]
  },
  {
    path: 'signup',
    loadComponent: () => import('./features/auth/signup.component').then(m => m.SignupComponent),
    canActivate: [loginGuard]
  },
  { path: '**', redirectTo: '' }
];
