import { Injectable, inject } from '@angular/core';
import { AuthService } from './auth.service';

export interface RolePermissions {
  [key: string]: string[];
}

@Injectable({
  providedIn: 'root'
})
export class PermissionsService {
  private authService = inject(AuthService);

  private readonly rolePermissions: RolePermissions = {
    'Admin': [
      'dashboard.view',
      'screening.customer',
      'screening.transaction', 
      'adverse-media.view',
      'customer-media-scan.view',
      'financial-intelligence.view',
      'search.view',
      'customers.view',
      'customers.create',
      'customers.edit',
      'customers.delete',
      'watchlists.view',
      'watchlists.manage',
      'sanctions.view',
      'sanctions.manage',
      'alerts.view',
      'alerts.manage',
      'reports.view',
      'reports.create',
      'organizations.view',
      'organizations.manage',
      'settings.view',
      'settings.manage',
      'users.invite',
      'users.manage'
    ],
    'Manager': [
      'dashboard.view',
      'screening.customer',
      'screening.transaction',
      'adverse-media.view',
      'customer-media-scan.view', 
      'financial-intelligence.view',
      'search.view',
      'customers.view',
      'customers.create',
      'customers.edit',
      'watchlists.view',
      'watchlists.manage',
      'sanctions.view',
      'sanctions.manage',
      'alerts.view',
      'alerts.manage',
      'reports.view',
      'reports.create',
      'settings.view',
      'users.invite'
    ],
    'Analyst': [
      'dashboard.view',
      'screening.customer',
      'screening.transaction',
      'adverse-media.view',
      'customer-media-scan.view',
      'financial-intelligence.view',
      'search.view',
      'customers.view',
      'customers.create',
      'customers.edit'
    ],
    'ComplianceOfficer': [
      'dashboard.view',
      'alerts.view',
      'alerts.manage',
      'reports.view',
      'reports.create',
      'reports.submit'
    ]
  };

  hasPermission(permission: string): boolean {
    const user = this.authService.getCurrentUser();
    if (!user) return false;
    
    const userPermissions = this.rolePermissions[user.role] || [];
    return userPermissions.includes(permission);
  }

  hasAnyPermission(permissions: string[]): boolean {
    return permissions.some(permission => this.hasPermission(permission));
  }

  getUserPermissions(): string[] {
    const user = this.authService.getCurrentUser();
    if (!user) return [];
    
    return this.rolePermissions[user.role] || [];
  }

  canAccessRoute(route: string): boolean {
    const routePermissions: { [key: string]: string } = {
      'screening/customer': 'screening.customer',
      'screening/transaction': 'screening.transaction',
      'adverse-media': 'adverse-media.view',
      'customer-media-scan': 'customer-media-scan.view',
      'financial-intelligence': 'financial-intelligence.view',
      'search': 'search.view',
      'customers': 'customers.view',
      'watchlists': 'watchlists.view',
      'sanctions': 'sanctions.view',
      'alerts': 'alerts.view',
      'reports': 'reports.view',
      'organizations': 'organizations.view',
      'settings': 'settings.view'
    };

    const requiredPermission = routePermissions[route];
    return requiredPermission ? this.hasPermission(requiredPermission) : true;
  }
}