import { Injectable, inject } from '@angular/core';
import { HttpClient } from '@angular/common/http';
import { Router } from '@angular/router';
import { BehaviorSubject, Observable, tap, catchError, of } from 'rxjs';

export interface LoginRequest {
  username: string;
  password: string;
}

export interface RegisterRequest {
  username: string;
  email: string;
  password: string;
  firstName: string;
  lastName: string;
  role: string;
  organizationName: string;
}

export interface AuthResponse {
  accessToken: string;
  refreshToken: string;
  tokenType: string;
  expiresIn: number;
  user: {
    id: string;
    username: string;
    email: string;
    firstName: string;
    lastName: string;
    role: string;
    organizationId: string;
  };
}

export interface User {
  id: string;
  username: string;
  email: string;
  firstName: string;
  lastName: string;
  role: string;
  organizationId: string;
}

@Injectable({
  providedIn: 'root'
})
export class AuthService {
  private http = inject(HttpClient);
  private router = inject(Router);
  
  private readonly API_BASE = 'http://localhost:5097/api';
  private readonly TOKEN_KEY = 'access_token';
  private readonly REFRESH_TOKEN_KEY = 'refresh_token';
  private readonly USER_KEY = 'current_user';

  private currentUserSubject = new BehaviorSubject<User | null>(this.getCurrentUser());
  public currentUser$ = this.currentUserSubject.asObservable();

  private isAuthenticatedSubject = new BehaviorSubject<boolean>(this.hasValidToken());
  public isAuthenticated$ = this.isAuthenticatedSubject.asObservable();

  constructor() {
    // Check if token is expired on service initialization
    this.checkTokenExpiration();
  }

  login(credentials: LoginRequest): Observable<AuthResponse> {
    return this.http.post<AuthResponse>(`${this.API_BASE}/auth/login`, credentials)
      .pipe(
        tap(response => {
          this.setSession(response);
          this.currentUserSubject.next(response.user);
          this.isAuthenticatedSubject.next(true);
        }),
        catchError(error => {
          console.error('Login failed:', error);
          throw error;
        })
      );
  }

  register(userData: RegisterRequest): Observable<AuthResponse> {
    return this.http.post<AuthResponse>(`${this.API_BASE}/auth/register`, userData)
      .pipe(
        tap(response => {
          this.setSession(response);
          this.currentUserSubject.next(response.user);
          this.isAuthenticatedSubject.next(true);
        }),
        catchError(error => {
          console.error('Registration failed:', error);
          throw error;
        })
      );
  }

  logout(): void {
    const userId = this.getCurrentUser()?.id;
    if (userId) {
      // Call logout endpoint to invalidate refresh token
      this.http.post(`${this.API_BASE}/auth/logout`, { userId }).subscribe({
        error: (error) => console.error('Logout API call failed:', error)
      });
    }

    this.clearSession();
    this.currentUserSubject.next(null);
    this.isAuthenticatedSubject.next(false);
    this.router.navigateByUrl('/login');
  }

  refreshToken(): Observable<AuthResponse> {
    const refreshToken = localStorage.getItem(this.REFRESH_TOKEN_KEY);
    if (!refreshToken) {
      this.logout();
      return of();
    }

    return this.http.post<AuthResponse>(`${this.API_BASE}/auth/refresh`, { refreshToken })
      .pipe(
        tap(response => {
          this.setSession(response);
          this.currentUserSubject.next(response.user);
          this.isAuthenticatedSubject.next(true);
        }),
        catchError(error => {
          console.error('Token refresh failed:', error);
          this.logout();
          return of();
        })
      );
  }

  changePassword(currentPassword: string, newPassword: string): Observable<any> {
    const userId = this.getCurrentUser()?.id;
    return this.http.post(`${this.API_BASE}/auth/change-password`, {
      userId,
      currentPassword,
      newPassword
    });
  }

  isAuthenticated(): boolean {
    return this.hasValidToken();
  }

  getCurrentUser(): User | null {
    const userJson = localStorage.getItem(this.USER_KEY);
    if (userJson) {
      try {
        return JSON.parse(userJson);
      } catch {
        return null;
      }
    }
    return null;
  }

  getToken(): string | null {
    return localStorage.getItem(this.TOKEN_KEY);
  }

  hasRole(role: string): boolean {
    const user = this.getCurrentUser();
    return user?.role === role;
  }

  hasAnyRole(roles: string[]): boolean {
    const user = this.getCurrentUser();
    return user ? roles.includes(user.role) : false;
  }

  private setSession(authResponse: AuthResponse): void {
    localStorage.setItem(this.TOKEN_KEY, authResponse.accessToken);
    localStorage.setItem(this.REFRESH_TOKEN_KEY, authResponse.refreshToken);
    localStorage.setItem(this.USER_KEY, JSON.stringify(authResponse.user));
    
    // Set token expiration time
    const expirationTime = new Date().getTime() + (authResponse.expiresIn * 1000);
    localStorage.setItem('token_expiration', expirationTime.toString());
  }

  private clearSession(): void {
    localStorage.removeItem(this.TOKEN_KEY);
    localStorage.removeItem(this.REFRESH_TOKEN_KEY);
    localStorage.removeItem(this.USER_KEY);
    localStorage.removeItem('token_expiration');
  }

  private hasValidToken(): boolean {
    const token = localStorage.getItem(this.TOKEN_KEY);
    const expiration = localStorage.getItem('token_expiration');
    
    if (!token || !expiration) {
      return false;
    }

    const expirationTime = parseInt(expiration, 10);
    const currentTime = new Date().getTime();
    
    return currentTime < expirationTime;
  }

  private checkTokenExpiration(): void {
    if (!this.hasValidToken() && this.getCurrentUser()) {
      // Token is expired, try to refresh
      const refreshToken = localStorage.getItem(this.REFRESH_TOKEN_KEY);
      if (refreshToken) {
        this.refreshToken().subscribe();
      } else {
        this.logout();
      }
    }
  }
}
