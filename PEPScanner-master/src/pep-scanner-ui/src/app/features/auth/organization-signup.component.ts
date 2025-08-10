import { Component, inject, signal } from '@angular/core';
import { CommonModule } from '@angular/common';
import { ReactiveFormsModule, FormBuilder, Validators } from '@angular/forms';
import { MatCardModule } from '@angular/material/card';
import { MatFormFieldModule } from '@angular/material/form-field';
import { MatInputModule } from '@angular/material/input';
import { MatButtonModule } from '@angular/material/button';
import { MatSelectModule } from '@angular/material/select';
import { MatStepperModule } from '@angular/material/stepper';
import { MatProgressSpinnerModule } from '@angular/material/progress-spinner';
import { MatSnackBarModule, MatSnackBar } from '@angular/material/snack-bar';
import { MatIconModule } from '@angular/material/icon';
import { Router } from '@angular/router';
import { AuthService } from '../../services/auth.service';

@Component({
  selector: 'app-organization-signup',
  standalone: true,
  imports: [
    CommonModule, ReactiveFormsModule, MatCardModule, MatFormFieldModule, 
    MatInputModule, MatButtonModule, MatSelectModule, MatStepperModule,
    MatProgressSpinnerModule, MatSnackBarModule, MatIconModule
  ],
  template: `
    <div class="onboarding-container">
      <mat-card class="onboarding-card">
        <mat-card-header>
          <mat-card-title>
            <mat-icon>account_balance</mat-icon>
            Bank Organization Onboarding
          </mat-card-title>
          <mat-card-subtitle>
            Set up your organization and create the first admin user
          </mat-card-subtitle>
        </mat-card-header>

        <mat-card-content>
          <mat-stepper [linear]="true" #stepper>
            <!-- Step 1: Organization Details -->
            <mat-step [stepControl]="organizationForm" label="Organization Details">
              <form [formGroup]="organizationForm">
                <div class="form-section">
                  <h3>Bank Information</h3>
                  
                  <div class="form-grid">
                    <mat-form-field appearance="outline">
                      <mat-label>Bank Name *</mat-label>
                      <input matInput formControlName="name" placeholder="e.g., State Bank of India">
                      <mat-error *ngIf="organizationForm.get('name')?.hasError('required')">
                        Bank name is required
                      </mat-error>
                    </mat-form-field>

                    <mat-form-field appearance="outline">
                      <mat-label>Bank Type *</mat-label>
                      <mat-select formControlName="type">
                        <mat-option value="Public Sector Bank">Public Sector Bank</mat-option>
                        <mat-option value="Private Sector Bank">Private Sector Bank</mat-option>
                        <mat-option value="Foreign Bank">Foreign Bank</mat-option>
                        <mat-option value="Cooperative Bank">Cooperative Bank</mat-option>
                        <mat-option value="Regional Rural Bank">Regional Rural Bank</mat-option>
                        <mat-option value="NBFC">NBFC</mat-option>
                      </mat-select>
                    </mat-form-field>

                    <mat-form-field appearance="outline">
                      <mat-label>RBI License Number</mat-label>
                      <input matInput formControlName="rbiLicenseNumber" placeholder="RBI/DPSS/2023/1234">
                    </mat-form-field>

                    <mat-form-field appearance="outline">
                      <mat-label>SWIFT Code</mat-label>
                      <input matInput formControlName="swiftCode" placeholder="SBININBB123">
                    </mat-form-field>

                    <mat-form-field appearance="outline">
                      <mat-label>Head Office Address *</mat-label>
                      <textarea matInput formControlName="address" rows="3" placeholder="Complete address"></textarea>
                    </mat-form-field>

                    <mat-form-field appearance="outline">
                      <mat-label>City *</mat-label>
                      <input matInput formControlName="city" placeholder="Mumbai">
                    </mat-form-field>

                    <mat-form-field appearance="outline">
                      <mat-label>State *</mat-label>
                      <mat-select formControlName="state">
                        <mat-option value="Maharashtra">Maharashtra</mat-option>
                        <mat-option value="Delhi">Delhi</mat-option>
                        <mat-option value="Karnataka">Karnataka</mat-option>
                        <mat-option value="Tamil Nadu">Tamil Nadu</mat-option>
                        <mat-option value="Gujarat">Gujarat</mat-option>
                        <mat-option value="West Bengal">West Bengal</mat-option>
                        <mat-option value="Other">Other</mat-option>
                      </mat-select>
                    </mat-form-field>

                    <mat-form-field appearance="outline">
                      <mat-label>PIN Code *</mat-label>
                      <input matInput formControlName="pinCode" placeholder="400001">
                    </mat-form-field>

                    <mat-form-field appearance="outline">
                      <mat-label>Phone Number *</mat-label>
                      <input matInput formControlName="phoneNumber" placeholder="+91-22-12345678">
                    </mat-form-field>

                    <mat-form-field appearance="outline">
                      <mat-label>Official Email *</mat-label>
                      <input matInput type="email" formControlName="email" placeholder="compliance@bank.com">
                    </mat-form-field>

                    <mat-form-field appearance="outline">
                      <mat-label>Website</mat-label>
                      <input matInput formControlName="website" placeholder="https://www.bank.com">
                    </mat-form-field>
                  </div>
                </div>

                <div class="step-actions">
                  <button mat-raised-button color="primary" matStepperNext 
                          [disabled]="organizationForm.invalid">
                    Next: Admin User
                  </button>
                </div>
              </form>
            </mat-step>

            <!-- Step 2: Admin User -->
            <mat-step [stepControl]="adminUserForm" label="Admin User">
              <form [formGroup]="adminUserForm">
                <div class="form-section">
                  <h3>Primary Administrator</h3>
                  <p class="section-description">
                    This user will have full administrative access to manage the organization and other users.
                  </p>
                  
                  <div class="form-grid">
                    <mat-form-field appearance="outline">
                      <mat-label>First Name *</mat-label>
                      <input matInput formControlName="firstName" placeholder="John">
                    </mat-form-field>

                    <mat-form-field appearance="outline">
                      <mat-label>Last Name *</mat-label>
                      <input matInput formControlName="lastName" placeholder="Doe">
                    </mat-form-field>

                    <mat-form-field appearance="outline">
                      <mat-label>Email Address *</mat-label>
                      <input matInput type="email" formControlName="email" placeholder="admin@bank.com">
                    </mat-form-field>

                    <mat-form-field appearance="outline">
                      <mat-label>Phone Number *</mat-label>
                      <input matInput formControlName="phoneNumber" placeholder="+91-9876543210">
                    </mat-form-field>

                    <mat-form-field appearance="outline">
                      <mat-label>Employee ID</mat-label>
                      <input matInput formControlName="employeeId" placeholder="EMP001">
                    </mat-form-field>

                    <mat-form-field appearance="outline">
                      <mat-label>Department</mat-label>
                      <mat-select formControlName="department">
                        <mat-option value="Compliance">Compliance</mat-option>
                        <mat-option value="Risk Management">Risk Management</mat-option>
                        <mat-option value="Operations">Operations</mat-option>
                        <mat-option value="IT">IT</mat-option>
                        <mat-option value="Legal">Legal</mat-option>
                      </mat-select>
                    </mat-form-field>

                    <mat-form-field appearance="outline">
                      <mat-label>Designation</mat-label>
                      <input matInput formControlName="designation" placeholder="Chief Compliance Officer">
                    </mat-form-field>

                    <mat-form-field appearance="outline">
                      <mat-label>Username *</mat-label>
                      <input matInput formControlName="username" placeholder="admin.user">
                    </mat-form-field>

                    <mat-form-field appearance="outline">
                      <mat-label>Password *</mat-label>
                      <input matInput type="password" formControlName="password" placeholder="Minimum 8 characters">
                      <mat-error *ngIf="adminUserForm.get('password')?.hasError('minlength')">
                        Password must be at least 8 characters
                      </mat-error>
                    </mat-form-field>

                    <mat-form-field appearance="outline">
                      <mat-label>Confirm Password *</mat-label>
                      <input matInput type="password" formControlName="confirmPassword">
                      <mat-error *ngIf="adminUserForm.get('confirmPassword')?.hasError('passwordMismatch')">
                        Passwords do not match
                      </mat-error>
                    </mat-form-field>
                  </div>
                </div>

                <div class="step-actions">
                  <button mat-button matStepperPrevious>Back</button>
                  <button mat-raised-button color="primary" matStepperNext 
                          [disabled]="adminUserForm.invalid">
                    Next: Configuration
                  </button>
                </div>
              </form>
            </mat-step>

            <!-- Step 3: Configuration -->
            <mat-step [stepControl]="configurationForm" label="Configuration">
              <form [formGroup]="configurationForm">
                <div class="form-section">
                  <h3>System Configuration</h3>
                  
                  <div class="form-grid">
                    <mat-form-field appearance="outline">
                      <mat-label>Time Zone</mat-label>
                      <mat-select formControlName="timeZone">
                        <mat-option value="Asia/Kolkata">Asia/Kolkata (IST)</mat-option>
                        <mat-option value="Asia/Dubai">Asia/Dubai (GST)</mat-option>
                        <mat-option value="Europe/London">Europe/London (GMT)</mat-option>
                        <mat-option value="America/New_York">America/New_York (EST)</mat-option>
                      </mat-select>
                    </mat-form-field>

                    <mat-form-field appearance="outline">
                      <mat-label>Currency</mat-label>
                      <mat-select formControlName="currency">
                        <mat-option value="INR">INR - Indian Rupee</mat-option>
                        <mat-option value="USD">USD - US Dollar</mat-option>
                        <mat-option value="EUR">EUR - Euro</mat-option>
                        <mat-option value="GBP">GBP - British Pound</mat-option>
                      </mat-select>
                    </mat-form-field>

                    <mat-form-field appearance="outline">
                      <mat-label>Default Risk Threshold (%)</mat-label>
                      <input matInput type="number" formControlName="riskThreshold" 
                             min="50" max="100" placeholder="70">
                    </mat-form-field>

                    <mat-form-field appearance="outline">
                      <mat-label>Alert Retention Days</mat-label>
                      <input matInput type="number" formControlName="alertRetentionDays" 
                             min="30" max="2555" placeholder="365">
                    </mat-form-field>
                  </div>
                </div>

                <div class="step-actions">
                  <button mat-button matStepperPrevious>Back</button>
                  <button mat-raised-button color="primary" (click)="submitOnboarding()" 
                          [disabled]="configurationForm.invalid || isLoading()">
                    <mat-spinner diameter="20" *ngIf="isLoading()"></mat-spinner>
                    <span *ngIf="!isLoading()">Complete Setup</span>
                    <span *ngIf="isLoading()">Setting up...</span>
                  </button>
                </div>
              </form>
            </mat-step>
          </mat-stepper>
        </mat-card-content>
      </mat-card>
    </div>
  `,
  styles: [`
    .onboarding-container {
      min-height: 100vh;
      background: linear-gradient(135deg, #667eea 0%, #764ba2 100%);
      padding: 24px;
      display: flex;
      align-items: center;
      justify-content: center;
    }

    .onboarding-card {
      max-width: 800px;
      width: 100%;
      margin: 0 auto;
    }

    .form-section {
      margin-bottom: 24px;
    }

    .form-section h3 {
      margin: 0 0 8px 0;
      color: #333;
      font-size: 1.2rem;
    }

    .section-description {
      color: #666;
      margin-bottom: 16px;
      font-size: 0.9rem;
    }

    .form-grid {
      display: grid;
      grid-template-columns: repeat(auto-fit, minmax(300px, 1fr));
      gap: 16px;
    }

    .step-actions {
      display: flex;
      gap: 12px;
      justify-content: flex-end;
      margin-top: 24px;
      padding-top: 16px;
      border-top: 1px solid #e0e0e0;
    }

    mat-form-field {
      width: 100%;
    }

    @media (max-width: 768px) {
      .onboarding-container {
        padding: 16px;
      }
      
      .form-grid {
        grid-template-columns: 1fr;
      }
      
      .step-actions {
        flex-direction: column;
      }
    }
  `]
})
export class OrganizationSignupComponent {
  private fb = inject(FormBuilder);
  private authService = inject(AuthService);
  private router = inject(Router);
  private snackBar = inject(MatSnackBar);

  isLoading = signal(false);

  organizationForm = this.fb.group({
    name: ['', Validators.required],
    type: ['', Validators.required],
    rbiLicenseNumber: [''],
    swiftCode: [''],
    address: ['', Validators.required],
    city: ['', Validators.required],
    state: ['', Validators.required],
    pinCode: ['', Validators.required],
    phoneNumber: ['', Validators.required],
    email: ['', [Validators.required, Validators.email]],
    website: ['']
  });

  adminUserForm = this.fb.group({
    firstName: ['', Validators.required],
    lastName: ['', Validators.required],
    email: ['', [Validators.required, Validators.email]],
    phoneNumber: ['', Validators.required],
    employeeId: [''],
    department: [''],
    designation: [''],
    username: ['', Validators.required],
    password: ['', [Validators.required, Validators.minLength(8)]],
    confirmPassword: ['', Validators.required]
  }, { validators: this.passwordMatchValidator });

  configurationForm = this.fb.group({
    timeZone: ['Asia/Kolkata'],
    currency: ['INR'],
    riskThreshold: [70, [Validators.min(50), Validators.max(100)]],
    alertRetentionDays: [365, [Validators.min(30), Validators.max(2555)]]
  });

  passwordMatchValidator(form: any) {
    const password = form.get('password');
    const confirmPassword = form.get('confirmPassword');
    
    if (password && confirmPassword && password.value !== confirmPassword.value) {
      confirmPassword.setErrors({ passwordMismatch: true });
      return { passwordMismatch: true };
    }
    
    return null;
  }

  submitOnboarding() {
    if (this.organizationForm.invalid || this.adminUserForm.invalid || this.configurationForm.invalid) {
      this.snackBar.open('Please fill all required fields correctly', 'Close', { duration: 5000 });
      return;
    }

    this.isLoading.set(true);

    const onboardingData = {
      organization: this.organizationForm.value,
      adminUser: this.adminUserForm.value,
      configuration: this.configurationForm.value
    };

    this.authService.onboardOrganization(onboardingData).subscribe({
      next: (response) => {
        this.isLoading.set(false);
        this.snackBar.open('Organization setup completed successfully!', 'Close', { 
          duration: 5000,
          panelClass: ['success-snackbar']
        });
        this.router.navigateByUrl('/dashboard');
      },
      error: (error) => {
        this.isLoading.set(false);
        const errorMessage = error.error?.error || 'Setup failed. Please try again.';
        this.snackBar.open(errorMessage, 'Close', { 
          duration: 5000,
          panelClass: ['error-snackbar']
        });
      }
    });
  }
}