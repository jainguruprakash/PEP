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
      { path: 'screening', redirectTo: 'screening/customer' },
      { path: 'screening/customer', loadComponent: () => import('./features/screening/customer-screening.component').then(m => m.CustomerScreeningComponent) },
      { path: 'screening/transaction', loadComponent: () => import('./features/screening/transaction-screening.component').then(m => m.TransactionScreeningComponent) },
      { path: 'adverse-media', loadComponent: () => import('./features/adverse-media/adverse-media.component').then(m => m.AdverseMediaComponent) },
      { path: 'search', loadComponent: () => import('./features/search/search.component').then(m => m.SearchComponent) },
      { path: 'watchlists', loadComponent: () => import('./features/watchlists/watchlists.component').then(m => m.WatchlistsComponent) },
      { path: 'sanctions', loadComponent: () => import('./features/sanctions/sanctions.component').then(m => m.SanctionsComponent) },
      { path: 'alerts', loadComponent: () => import('./features/alerts/alerts.component').then(m => m.AlertsComponent) },
      { path: 'alerts/:id', loadComponent: () => import('./features/alerts/alert-detail.component').then(m => m.AlertDetailComponent) },
      { path: 'customers', loadComponent: () => import('./features/customers/customers.component').then(m => m.CustomersComponent) },
      { path: 'reports', loadComponent: () => import('./features/reports/reports.component').then(m => m.ReportsComponent) },
      { path: 'future-features', loadComponent: () => import('./features/future-features/future-features.component').then(m => m.FutureFeaturesComponent) },
      { path: 'profile', loadComponent: () => import('./features/profile/profile.component').then(m => m.ProfileComponent) },
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
