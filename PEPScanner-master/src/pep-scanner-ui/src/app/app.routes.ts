import { Routes } from '@angular/router';
import { authGuard, loginGuard, roleGuard } from './core/auth.guard';

export const routes: Routes = [
  // Landing page (no guard)
  { path: '', pathMatch: 'full', redirectTo: 'home' },
  { path: 'home', loadComponent: () => import('./features/home/home.component').then(m => m.HomeComponent) },
  
  // Auth routes (no guard)
  { path: 'login', loadComponent: () => import('./features/auth/login.component').then(m => m.LoginComponent), canActivate: [loginGuard] },
  { path: 'signup', loadComponent: () => import('./features/auth/signup.component').then(m => m.SignupComponent), canActivate: [loginGuard] },
  { path: 'onboard-organization', loadComponent: () => import('./features/auth/organization-signup.component').then(m => m.OrganizationSignupComponent), canActivate: [loginGuard] },

  // Protected routes
  {
    path: 'dashboard',
    loadComponent: () => import('./layout/app-shell.component').then(m => m.AppShellComponent),
    canActivate: [authGuard],
    children: [
      { path: '', pathMatch: 'full', redirectTo: 'overview' },
      { path: 'overview', loadComponent: () => import('./features/dashboard/dashboard.component').then(m => m.DashboardComponent) },
      { path: 'screening', redirectTo: 'screening/customer' },
      { 
        path: 'screening/customer', 
        loadComponent: () => import('./features/screening/customer-screening.component').then(m => m.CustomerScreeningComponent),
        canActivate: [roleGuard(['Admin', 'Manager', 'Analyst'])]
      },
      { 
        path: 'screening/transaction', 
        loadComponent: () => import('./features/screening/transaction-screening.component').then(m => m.TransactionScreeningComponent),
        canActivate: [roleGuard(['Admin', 'Manager', 'Analyst'])]
      },
      { 
        path: 'adverse-media', 
        loadComponent: () => import('./features/adverse-media/adverse-media.component').then(m => m.AdverseMediaComponent),
        canActivate: [roleGuard(['Admin', 'Manager', 'Analyst'])]
      },
      { 
        path: 'customer-media-scan', 
        loadComponent: () => import('./features/customer-media-scan/customer-media-scan.component').then(m => m.CustomerMediaScanComponent),
        canActivate: [roleGuard(['Admin', 'Manager', 'Analyst'])]
      },
      { 
        path: 'financial-intelligence', 
        loadComponent: () => import('./features/financial-intelligence/financial-intelligence.component').then(m => m.FinancialIntelligenceComponent),
        canActivate: [roleGuard(['Admin', 'Manager', 'Analyst'])]
      },
      { 
        path: 'financial-intelligence/:customerId', 
        loadComponent: () => import('./features/financial-intelligence/financial-intelligence.component').then(m => m.FinancialIntelligenceComponent),
        canActivate: [roleGuard(['Admin', 'Manager', 'Analyst'])]
      },
      { 
        path: 'search', 
        loadComponent: () => import('./features/search/search.component').then(m => m.SearchComponent),
        canActivate: [roleGuard(['Admin', 'Manager', 'Analyst'])]
      },
      { 
        path: 'watchlists', 
        loadComponent: () => import('./features/watchlists/watchlists.component').then(m => m.WatchlistsComponent),
        canActivate: [roleGuard(['Admin', 'Manager'])]
      },
      { 
        path: 'sanctions', 
        loadComponent: () => import('./features/sanctions/sanctions.component').then(m => m.SanctionsComponent),
        canActivate: [roleGuard(['Admin', 'Manager'])]
      },
      { 
        path: 'alerts', 
        loadComponent: () => import('./features/alerts/alerts.component').then(m => m.AlertsComponent),
        canActivate: [roleGuard(['Admin', 'Manager', 'ComplianceOfficer'])]
      },
      { 
        path: 'alerts/:id', 
        loadComponent: () => import('./features/alerts/alert-detail.component').then(m => m.AlertDetailComponent),
        canActivate: [roleGuard(['Admin', 'Manager', 'ComplianceOfficer'])]
      },
      { 
        path: 'customers', 
        loadComponent: () => import('./features/customers/customers.component').then(m => m.CustomersComponent),
        canActivate: [roleGuard(['Admin', 'Manager', 'Analyst'])]
      },
      { 
        path: 'reports', 
        loadComponent: () => import('./features/reports/reports.component').then(m => m.ReportsComponent),
        canActivate: [roleGuard(['Admin', 'Manager', 'ComplianceOfficer'])]
      },
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

  { path: '**', redirectTo: '' }
];
