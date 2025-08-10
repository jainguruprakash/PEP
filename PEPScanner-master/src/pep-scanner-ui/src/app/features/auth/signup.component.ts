import { Component, inject } from '@angular/core';
import { CommonModule } from '@angular/common';
import { ReactiveFormsModule, FormBuilder, Validators } from '@angular/forms';
import { MatCardModule } from '@angular/material/card';
import { MatFormFieldModule } from '@angular/material/form-field';
import { MatInputModule } from '@angular/material/input';
import { MatButtonModule } from '@angular/material/button';
import { MatSelectModule } from '@angular/material/select';
import { MatProgressSpinnerModule } from '@angular/material/progress-spinner';
import { MatSnackBarModule, MatSnackBar } from '@angular/material/snack-bar';
import { Router } from '@angular/router';
import { AuthService } from '../../services/auth.service';

@Component({
  selector: 'app-signup',
  standalone: true,
  imports: [CommonModule, ReactiveFormsModule, MatCardModule, MatFormFieldModule, MatInputModule, MatButtonModule, MatSelectModule, MatProgressSpinnerModule, MatSnackBarModule],
  template: `
  <div class="center">
    <mat-card>
      <mat-card-title>Create Your Account</mat-card-title>
      <form [formGroup]="form" (ngSubmit)="submit()" class="form-grid">
        <div class="form-row">
          <mat-form-field appearance="outline">
            <mat-label>First Name</mat-label>
            <input matInput formControlName="firstName" required />
            @if (form.get('firstName')?.invalid && form.get('firstName')?.touched) {
              <mat-error>First name is required</mat-error>
            }
          </mat-form-field>
          <mat-form-field appearance="outline">
            <mat-label>Last Name</mat-label>
            <input matInput formControlName="lastName" required />
            @if (form.get('lastName')?.invalid && form.get('lastName')?.touched) {
              <mat-error>Last name is required</mat-error>
            }
          </mat-form-field>
        </div>

        <mat-form-field appearance="outline">
          <mat-label>Username</mat-label>
          <input matInput formControlName="username" required />
          @if (form.get('username')?.invalid && form.get('username')?.touched) {
            <mat-error>Username is required</mat-error>
          }
        </mat-form-field>

        <mat-form-field appearance="outline">
          <mat-label>Email</mat-label>
          <input matInput type="email" formControlName="email" required />
          @if (form.get('email')?.invalid && form.get('email')?.touched) {
            <mat-error>Valid email is required</mat-error>
          }
        </mat-form-field>

        <mat-form-field appearance="outline">
          <mat-label>Password</mat-label>
          <input matInput type="password" formControlName="password" required />
          @if (form.get('password')?.invalid && form.get('password')?.touched) {
            <mat-error>Password must be at least 8 characters</mat-error>
          }
        </mat-form-field>

        <mat-form-field appearance="outline">
          <mat-label>Role</mat-label>
          <mat-select formControlName="role" required>
            <mat-option value="Analyst">Analyst</mat-option>
            <mat-option value="ComplianceOfficer">Compliance Officer</mat-option>
            <mat-option value="Manager">Manager</mat-option>
            <mat-option value="Admin">Admin</mat-option>
          </mat-select>
          @if (form.get('role')?.invalid && form.get('role')?.touched) {
            <mat-error>Role is required</mat-error>
          }
        </mat-form-field>

        <mat-form-field appearance="outline">
          <mat-label>Organization Name</mat-label>
          <input matInput formControlName="organizationName" required />
          @if (form.get('organizationName')?.invalid && form.get('organizationName')?.touched) {
            <mat-error>Organization name is required</mat-error>
          }
        </mat-form-field>

        <div class="button-container">
          <button mat-raised-button color="primary" type="submit" [disabled]="form.invalid || isLoading">
            @if (isLoading) {
              <mat-spinner diameter="20"></mat-spinner>
            } @else {
              Create Account
            }
          </button>
          <button mat-button type="button" (click)="goToLogin()">
            Already have an account? Login
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
      min-height: 100vh;
      background: linear-gradient(135deg, #667eea 0%, #764ba2 100%);
      padding: 2rem;
    }
    mat-card {
      width: 500px;
      padding: 2rem;
    }
    .form-grid {
      display: flex;
      flex-direction: column;
      gap: 1rem;
    }
    .form-row {
      display: grid;
      grid-template-columns: 1fr 1fr;
      gap: 1rem;
    }
    mat-form-field {
      width: 100%;
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
export class SignupComponent {
  private fb = inject(FormBuilder);
  private authService = inject(AuthService);
  private router = inject(Router);
  private snackBar = inject(MatSnackBar);

  form = this.fb.group({
    firstName: ['', [Validators.required]],
    lastName: ['', [Validators.required]],
    username: ['', [Validators.required]],
    email: ['', [Validators.required, Validators.email]],
    password: ['', [Validators.required, Validators.minLength(8)]],
    role: ['', [Validators.required]],
    organizationName: ['', [Validators.required]]
  });

  isLoading = false;

  submit() {
    if (this.form.invalid) {
      this.form.markAllAsTouched();
      return;
    }

    this.isLoading = true;
    const userData = this.form.value as any;

    this.authService.register(userData).subscribe({
      next: (response) => {
        this.isLoading = false;
        this.snackBar.open(`Welcome, ${response.user.firstName}! Your account has been created.`, 'Close', {
          duration: 3000,
          panelClass: ['success-snackbar']
        });
        this.router.navigateByUrl('/dashboard');
      },
      error: (error) => {
        this.isLoading = false;
        const errorMessage = error.error?.error || 'Registration failed. Please try again.';
        this.snackBar.open(errorMessage, 'Close', {
          duration: 5000,
          panelClass: ['error-snackbar']
        });
      }
    });
  }

  goToLogin() {
    this.router.navigateByUrl('/login');
  }
}


