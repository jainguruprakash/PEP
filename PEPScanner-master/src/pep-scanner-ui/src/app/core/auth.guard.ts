import { inject } from '@angular/core';
import { Router } from '@angular/router';
import { AuthService } from '../services/auth.service';

export const authGuard = () => {
  const authService = inject(AuthService);
  const router = inject(Router);

  if (authService.isAuthenticated()) {
    return true;
  }

  router.navigateByUrl('/login');
  return false;
};

export const loginGuard = () => {
  const authService = inject(AuthService);
  const router = inject(Router);

  if (!authService.isAuthenticated()) {
    return true;
  }

  router.navigateByUrl('/dashboard');
  return false;
};

export const roleGuard = (allowedRoles: string[]) => {
  return () => {
    const authService = inject(AuthService);
    const router = inject(Router);

    if (!authService.isAuthenticated()) {
      router.navigateByUrl('/login');
      return false;
    }

    if (authService.hasAnyRole(allowedRoles)) {
      return true;
    }

    router.navigateByUrl('/unauthorized');
    return false;
  };
};
