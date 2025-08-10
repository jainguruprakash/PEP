import { Component, inject, OnInit } from '@angular/core';
import { CommonModule } from '@angular/common';
import { ReactiveFormsModule, FormBuilder, Validators } from '@angular/forms';
import { MatCardModule } from '@angular/material/card';
import { MatFormFieldModule } from '@angular/material/form-field';
import { MatInputModule } from '@angular/material/input';
import { MatButtonModule } from '@angular/material/button';
import { MatSelectModule } from '@angular/material/select';
import { MatIconModule } from '@angular/material/icon';
import { MatDividerModule } from '@angular/material/divider';
import { MatSnackBarModule, MatSnackBar } from '@angular/material/snack-bar';
import { MatTabsModule } from '@angular/material/tabs';
import { AuthService, User } from '../../services/auth.service';

@Component({
  selector: 'app-profile',
  standalone: true,
  imports: [
    CommonModule,
    ReactiveFormsModule,
    MatCardModule,
    MatFormFieldModule,
    MatInputModule,
    MatButtonModule,
    MatSelectModule,
    MatIconModule,
    MatDividerModule,
    MatSnackBarModule,
    MatTabsModule
  ],
  template: `
    <div class="profile-container">
      <mat-card class="profile-card">
        <mat-card-header>
          <div mat-card-avatar class="profile-avatar">
            <mat-icon>account_circle</mat-icon>
          </div>
          <mat-card-title>User Profile</mat-card-title>
          <mat-card-subtitle>Manage your account information</mat-card-subtitle>
        </mat-card-header>

        <mat-card-content>
          <mat-tab-group>
            <!-- Personal Information Tab -->
            <mat-tab label="Personal Information">
              <div class="tab-content">
                <form [formGroup]="profileForm" (ngSubmit)="updateProfile()">
                  <div class="form-row">
                    <mat-form-field appearance="outline">
                      <mat-label>First Name</mat-label>
                      <input matInput formControlName="firstName" required />
                      @if (profileForm.get('firstName')?.invalid && profileForm.get('firstName')?.touched) {
                        <mat-error>First name is required</mat-error>
                      }
                    </mat-form-field>
                    <mat-form-field appearance="outline">
                      <mat-label>Last Name</mat-label>
                      <input matInput formControlName="lastName" required />
                      @if (profileForm.get('lastName')?.invalid && profileForm.get('lastName')?.touched) {
                        <mat-error>Last name is required</mat-error>
                      }
                    </mat-form-field>
                  </div>

                  <mat-form-field appearance="outline">
                    <mat-label>Email</mat-label>
                    <input matInput type="email" formControlName="email" required />
                    @if (profileForm.get('email')?.invalid && profileForm.get('email')?.touched) {
                      <mat-error>Valid email is required</mat-error>
                    }
                  </mat-form-field>

                  <mat-form-field appearance="outline">
                    <mat-label>Username</mat-label>
                    <input matInput formControlName="username" required />
                    @if (profileForm.get('username')?.invalid && profileForm.get('username')?.touched) {
                      <mat-error>Username is required</mat-error>
                    }
                  </mat-form-field>

                  <div class="form-row">
                    <mat-form-field appearance="outline">
                      <mat-label>Phone Number</mat-label>
                      <input matInput formControlName="phoneNumber" />
                    </mat-form-field>
                    <mat-form-field appearance="outline">
                      <mat-label>Department</mat-label>
                      <input matInput formControlName="department" />
                    </mat-form-field>
                  </div>

                  <mat-form-field appearance="outline">
                    <mat-label>Position</mat-label>
                    <input matInput formControlName="position" />
                  </mat-form-field>

                  <div class="form-actions">
                    <button mat-raised-button color="primary" type="submit" [disabled]="profileForm.invalid || isLoading">
                      @if (isLoading) {
                        <mat-icon>hourglass_empty</mat-icon>
                        Updating...
                      } @else {
                        <mat-icon>save</mat-icon>
                        Update Profile
                      }
                    </button>
                  </div>
                </form>
              </div>
            </mat-tab>

            <!-- Security Tab -->
            <mat-tab label="Security">
              <div class="tab-content">
                <form [formGroup]="passwordForm" (ngSubmit)="changePassword()">
                  <h3>Change Password</h3>
                  <mat-divider></mat-divider>

                  <mat-form-field appearance="outline">
                    <mat-label>Current Password</mat-label>
                    <input matInput type="password" formControlName="currentPassword" required />
                    @if (passwordForm.get('currentPassword')?.invalid && passwordForm.get('currentPassword')?.touched) {
                      <mat-error>Current password is required</mat-error>
                    }
                  </mat-form-field>

                  <mat-form-field appearance="outline">
                    <mat-label>New Password</mat-label>
                    <input matInput type="password" formControlName="newPassword" required />
                    @if (passwordForm.get('newPassword')?.invalid && passwordForm.get('newPassword')?.touched) {
                      <mat-error>Password must be at least 8 characters</mat-error>
                    }
                  </mat-form-field>

                  <mat-form-field appearance="outline">
                    <mat-label>Confirm New Password</mat-label>
                    <input matInput type="password" formControlName="confirmPassword" required />
                    @if (passwordForm.get('confirmPassword')?.invalid && passwordForm.get('confirmPassword')?.touched) {
                      <mat-error>Passwords do not match</mat-error>
                    }
                  </mat-form-field>

                  <div class="form-actions">
                    <button mat-raised-button color="primary" type="submit" [disabled]="passwordForm.invalid || isPasswordLoading">
                      @if (isPasswordLoading) {
                        <mat-icon>hourglass_empty</mat-icon>
                        Changing...
                      } @else {
                        <mat-icon>lock</mat-icon>
                        Change Password
                      }
                    </button>
                  </div>
                </form>
              </div>
            </mat-tab>

            <!-- Account Information Tab -->
            <mat-tab label="Account Info">
              <div class="tab-content">
                <h3>Account Details</h3>
                <mat-divider></mat-divider>

                <div class="info-section">
                  <div class="info-item">
                    <mat-icon>badge</mat-icon>
                    <div class="info-content">
                      <div class="info-label">Role</div>
                      <div class="info-value">{{ currentUser?.role }}</div>
                    </div>
                  </div>

                  <div class="info-item">
                    <mat-icon>business</mat-icon>
                    <div class="info-content">
                      <div class="info-label">Organization ID</div>
                      <div class="info-value">{{ currentUser?.organizationId }}</div>
                    </div>
                  </div>

                  <div class="info-item">
                    <mat-icon>schedule</mat-icon>
                    <div class="info-content">
                      <div class="info-label">Account Created</div>
                      <div class="info-value">{{ accountCreatedDate | date:'medium' }}</div>
                    </div>
                  </div>

                  <div class="info-item">
                    <mat-icon>login</mat-icon>
                    <div class="info-content">
                      <div class="info-label">Last Login</div>
                      <div class="info-value">{{ lastLoginDate | date:'medium' }}</div>
                    </div>
                  </div>
                </div>
              </div>
            </mat-tab>
          </mat-tab-group>
        </mat-card-content>
      </mat-card>
    </div>
  `,
  styles: [`
    .profile-container {
      max-width: 800px;
      margin: 0 auto;
      padding: 24px;
    }
    .profile-card {
      width: 100%;
    }
    .profile-avatar {
      background: #f5f5f5;
      display: flex;
      align-items: center;
      justify-content: center;
    }
    .profile-avatar mat-icon {
      font-size: 40px;
      width: 40px;
      height: 40px;
      color: #666;
    }
    .tab-content {
      padding: 24px 0;
    }
    .form-row {
      display: grid;
      grid-template-columns: 1fr 1fr;
      gap: 16px;
    }
    mat-form-field {
      width: 100%;
      margin-bottom: 16px;
    }
    .form-actions {
      margin-top: 24px;
      display: flex;
      justify-content: flex-end;
    }
    .info-section {
      margin-top: 16px;
    }
    .info-item {
      display: flex;
      align-items: center;
      padding: 12px 0;
      border-bottom: 1px solid #eee;
    }
    .info-item:last-child {
      border-bottom: none;
    }
    .info-item mat-icon {
      margin-right: 16px;
      color: #666;
    }
    .info-content {
      flex: 1;
    }
    .info-label {
      font-size: 12px;
      color: #666;
      text-transform: uppercase;
      font-weight: 500;
      margin-bottom: 4px;
    }
    .info-value {
      font-size: 14px;
      color: #333;
    }
    h3 {
      margin: 0 0 16px 0;
      font-size: 18px;
      font-weight: 500;
    }
  `]
})
export class ProfileComponent implements OnInit {
  private fb = inject(FormBuilder);
  private authService = inject(AuthService);
  private snackBar = inject(MatSnackBar);

  currentUser: User | null = null;
  isLoading = false;
  isPasswordLoading = false;
  accountCreatedDate = new Date(); // Mock date
  lastLoginDate = new Date(); // Mock date

  profileForm = this.fb.group({
    firstName: ['', [Validators.required]],
    lastName: ['', [Validators.required]],
    email: ['', [Validators.required, Validators.email]],
    username: ['', [Validators.required]],
    phoneNumber: [''],
    department: [''],
    position: ['']
  });

  passwordForm = this.fb.group({
    currentPassword: ['', [Validators.required]],
    newPassword: ['', [Validators.required, Validators.minLength(8)]],
    confirmPassword: ['', [Validators.required]]
  }, { validators: this.passwordMatchValidator });

  ngOnInit() {
    this.authService.currentUser$.subscribe(user => {
      this.currentUser = user;
      if (user) {
        this.profileForm.patchValue({
          firstName: user.firstName,
          lastName: user.lastName,
          email: user.email,
          username: user.username,
          phoneNumber: '', // These would come from a more detailed user profile
          department: '',
          position: ''
        });
      }
    });
  }

  updateProfile() {
    if (this.profileForm.invalid) {
      this.profileForm.markAllAsTouched();
      return;
    }

    this.isLoading = true;
    // Mock API call - replace with real service
    setTimeout(() => {
      this.isLoading = false;
      this.snackBar.open('Profile updated successfully!', 'Close', {
        duration: 3000,
        panelClass: ['success-snackbar']
      });
    }, 1000);
  }

  changePassword() {
    if (this.passwordForm.invalid) {
      this.passwordForm.markAllAsTouched();
      return;
    }

    this.isPasswordLoading = true;
    const { currentPassword, newPassword } = this.passwordForm.value;

    this.authService.changePassword(currentPassword!, newPassword!).subscribe({
      next: () => {
        this.isPasswordLoading = false;
        this.passwordForm.reset();
        this.snackBar.open('Password changed successfully!', 'Close', {
          duration: 3000,
          panelClass: ['success-snackbar']
        });
      },
      error: (error) => {
        this.isPasswordLoading = false;
        const errorMessage = error.error?.error || 'Failed to change password';
        this.snackBar.open(errorMessage, 'Close', {
          duration: 5000,
          panelClass: ['error-snackbar']
        });
      }
    });
  }

  private passwordMatchValidator(form: any) {
    const newPassword = form.get('newPassword');
    const confirmPassword = form.get('confirmPassword');
    
    if (newPassword && confirmPassword && newPassword.value !== confirmPassword.value) {
      confirmPassword.setErrors({ passwordMismatch: true });
      return { passwordMismatch: true };
    }
    
    return null;
  }
}
