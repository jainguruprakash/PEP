import { Component, inject } from '@angular/core';
import { CommonModule } from '@angular/common';
import { ReactiveFormsModule, FormBuilder, Validators } from '@angular/forms';
import { MatCardModule } from '@angular/material/card';
import { MatFormFieldModule } from '@angular/material/form-field';
import { MatInputModule } from '@angular/material/input';
import { MatButtonModule } from '@angular/material/button';
import { MatProgressSpinnerModule } from '@angular/material/progress-spinner';
import { MatSnackBarModule, MatSnackBar } from '@angular/material/snack-bar';
import { Router } from '@angular/router';
import { AuthService } from '../../services/auth.service';

@Component({
  selector: 'app-login',
  standalone: true,
  imports: [CommonModule, ReactiveFormsModule, MatCardModule, MatFormFieldModule, MatInputModule, MatButtonModule, MatProgressSpinnerModule, MatSnackBarModule],
  template: `
  <div class="center">
    <mat-card>
      <mat-card-title>Login to PEP Scanner</mat-card-title>
      <mat-card-subtitle style="color: #666; margin-bottom: 1rem;">Test credentials: admin / admin123</mat-card-subtitle>
      <form [formGroup]="form" (ngSubmit)="submit()">
        <mat-form-field appearance="outline">
          <mat-label>Username or Email</mat-label>
          <input matInput formControlName="username" required />
          @if (form.get('username')?.invalid && form.get('username')?.touched) {
            <mat-error>Username is required</mat-error>
          }
        </mat-form-field>
        <mat-form-field appearance="outline">
          <mat-label>Password</mat-label>
          <input matInput type="password" formControlName="password" required />
          @if (form.get('password')?.invalid && form.get('password')?.touched) {
            <mat-error>Password is required</mat-error>
          }
        </mat-form-field>
        <div class="button-container">
          <button mat-raised-button color="primary" type="submit" [disabled]="form.invalid || isLoading">
            @if (isLoading) {
              <mat-spinner diameter="20"></mat-spinner>
            } @else {
              Login
            }
          </button>
          <button mat-button type="button" (click)="goToSignup()">
            Don't have an account? Sign up
          </button>
        </div>
      </form>
    </mat-card>
  </div>
  `,
  styles: [`
    .center {
      display: flex;
      align-items: center;
      justify-content: center;
      height: 100vh;
      background: linear-gradient(135deg, #667eea 0%, #764ba2 100%);
    }
    mat-card {
      width: 400px;
      padding: 2rem;
    }
    mat-form-field {
      width: 100%;
      margin-bottom: 1rem;
    }
    .button-container {
      display: flex;
      flex-direction: column;
      gap: 1rem;
      margin-top: 1rem;
    }
    button[mat-raised-button] {
      height: 48px;
    }
    mat-spinner {
      margin: 0 auto;
    }
  `]
})
export class LoginComponent {
  private fb = inject(FormBuilder);
  private authService = inject(AuthService);
  private router = inject(Router);
  private snackBar = inject(MatSnackBar);

  form = this.fb.group({
    username: ['admin', [Validators.required]], // Pre-filled with test credentials
    password: ['admin123', [Validators.required, Validators.minLength(8)]] // Pre-filled with test credentials
  });

  isLoading = false;

  submit() {
    if (this.form.invalid) {
      this.form.markAllAsTouched();
      return;
    }

    this.isLoading = true;
    const credentials = this.form.value as { username: string; password: string };

    this.authService.login(credentials).subscribe({
      next: (response) => {
        this.isLoading = false;
        this.snackBar.open(`Welcome back, ${response.user.firstName}!`, 'Close', {
          duration: 3000,
          panelClass: ['success-snackbar']
        });
        this.router.navigateByUrl('/dashboard');
      },
      error: (error) => {
        this.isLoading = false;
        const errorMessage = error.error?.error || 'Login failed. Please check your credentials.';
        this.snackBar.open(errorMessage, 'Close', {
          duration: 5000,
          panelClass: ['error-snackbar']
        });
      }
    });
  }

  goToSignup() {
    this.router.navigateByUrl('/signup');
  }
}


