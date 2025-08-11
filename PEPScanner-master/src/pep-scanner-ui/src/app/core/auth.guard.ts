import { inject } from '@angular/core';
import { Router } from '@angular/router';
import { CanActivateFn } from '@angular/router';

export const authGuard: CanActivateFn = () => {
  return true; // Simplified for now
};

export const loginGuard: CanActivateFn = () => {
  return true; // Simplified for now
};

export const roleGuard = (roles: string[]): CanActivateFn => {
  return () => true; // Simplified for now
};