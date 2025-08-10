import { Component, inject, signal } from '@angular/core';
import { CommonModule } from '@angular/common';
import { ReactiveFormsModule, FormBuilder, Validators } from '@angular/forms';
import { MatCardModule } from '@angular/material/card';
import { MatFormFieldModule } from '@angular/material/form-field';
import { MatInputModule } from '@angular/material/input';
import { MatButtonModule } from '@angular/material/button';
import { MatSelectModule } from '@angular/material/select';
import { MatProgressSpinnerModule } from '@angular/material/progress-spinner';
import { MatSnackBarModule, MatSnackBar } from '@angular/material/snack-bar';
import { MatIconModule } from '@angular/material/icon';
import { MatDialogModule, MatDialogRef } from '@angular/material/dialog';
import { AuthService } from '../../services/auth.service';

@Component({
  selector: 'app-invite-user',
  standalone: true,
  imports: [
    CommonModule, ReactiveFormsModule, MatCardModule, MatFormFieldModule,
    MatInputModule, MatButtonModule, MatSelectModule, MatProgressSpinnerModule,
    MatSnackBarModule, MatIconModule, MatDialogModule
  ],
  template: `
    <div class="invite-container">
      <h2 mat-dialog-title>
        <mat-icon>person_add</mat-icon>
        Invite New User
      </h2>

      <mat-dialog-content>
        <form [formGroup]="inviteForm" class="invite-form">
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
              <input matInput type="email" formControlName="email" placeholder="user@bank.com">
            </mat-form-field>

            <mat-form-field appearance="outline">
              <mat-label>Role *</mat-label>
              <mat-select formControlName="role">
                <mat-option value="Analyst">Analyst</mat-option>
                <mat-option value="ComplianceOfficer">Compliance Officer</mat-option>
                <mat-option value="Manager">Manager</mat-option>
                <mat-option value="Admin">Admin</mat-option>
              </mat-select>
            </mat-form-field>

            <mat-form-field appearance="outline">
              <mat-label>Department</mat-label>
              <mat-select formControlName="department">
                <mat-option value="Compliance">Compliance</mat-option>
                <mat-option value="Risk Management">Risk Management</mat-option>
                <mat-option value="Operations">Operations</mat-option>
                <mat-option value="IT">IT</mat-option>
                <mat-option value="Legal">Legal</mat-option>
                <mat-option value="Audit">Audit</mat-option>
              </mat-select>
            </mat-form-field>

            <mat-form-field appearance="outline">
              <mat-label>Employee ID</mat-label>
              <input matInput formControlName="employeeId" placeholder="EMP001">
            </mat-form-field>
          </div>

          <div class="permissions-section">
            <h4>Role Permissions</h4>
            <div class="role-description" [ngSwitch]="inviteForm.get('role')?.value">
              <div *ngSwitchCase="'Analyst'">
                <strong>Analyst:</strong> Can perform customer screening, view alerts, and generate basic reports.
              </div>
              <div *ngSwitchCase="'ComplianceOfficer'">
                <strong>Compliance Officer:</strong> Full screening access, alert management, report generation, and compliance oversight.
              </div>
              <div *ngSwitchCase="'Manager'">
                <strong>Manager:</strong> All compliance features plus user management and advanced reporting.
              </div>
              <div *ngSwitchCase="'Admin'">
                <strong>Admin:</strong> Full system access including organization settings, user management, and system configuration.
              </div>
              <div *ngSwitchDefault>
                Select a role to see permissions
              </div>
            </div>
          </div>
        </form>
      </mat-dialog-content>

      <mat-dialog-actions align="end">
        <button mat-button (click)="cancel()">Cancel</button>
        <button mat-raised-button color="primary" 
                (click)="sendInvite()" 
                [disabled]="inviteForm.invalid || isLoading()">
          <mat-spinner diameter="20" *ngIf="isLoading()"></mat-spinner>
          <span *ngIf="!isLoading()">Send Invitation</span>
          <span *ngIf="isLoading()">Sending...</span>
        </button>
      </mat-dialog-actions>
    </div>
  `,
  styles: [`
    .invite-container {
      width: 500px;
      max-width: 90vw;
    }

    h2 {
      display: flex;
      align-items: center;
      gap: 8px;
      margin: 0;
    }

    .invite-form {
      padding: 16px 0;
    }

    .form-grid {
      display: grid;
      grid-template-columns: 1fr 1fr;
      gap: 16px;
      margin-bottom: 24px;
    }

    .form-grid mat-form-field:nth-child(3) {
      grid-column: 1 / -1;
    }

    .permissions-section {
      margin-top: 16px;
      padding: 16px;
      background: #f5f5f5;
      border-radius: 8px;
    }

    .permissions-section h4 {
      margin: 0 0 8px 0;
      color: #333;
    }

    .role-description {
      font-size: 0.9rem;
      color: #666;
      line-height: 1.4;
    }

    mat-form-field {
      width: 100%;
    }

    @media (max-width: 600px) {
      .form-grid {
        grid-template-columns: 1fr;
      }
    }
  `]
})
export class InviteUserComponent {
  private fb = inject(FormBuilder);
  private authService = inject(AuthService);
  private snackBar = inject(MatSnackBar);
  private dialogRef = inject(MatDialogRef<InviteUserComponent>);

  isLoading = signal(false);

  inviteForm = this.fb.group({
    firstName: ['', Validators.required],
    lastName: ['', Validators.required],
    email: ['', [Validators.required, Validators.email]],
    role: ['', Validators.required],
    department: [''],
    employeeId: ['']
  });

  sendInvite() {
    if (this.inviteForm.invalid) {
      this.inviteForm.markAllAsTouched();
      return;
    }

    this.isLoading.set(true);
    const currentUser = this.authService.getCurrentUser();
    
    const inviteData = {
      ...this.inviteForm.value,
      organizationId: currentUser?.organizationId
    };

    this.authService.inviteUser(inviteData).subscribe({
      next: (response) => {
        this.isLoading.set(false);
        this.snackBar.open('Invitation sent successfully!', 'Close', {
          duration: 3000,
          panelClass: ['success-snackbar']
        });
        this.dialogRef.close(response);
      },
      error: (error) => {
        this.isLoading.set(false);
        const errorMessage = error.error?.error || 'Failed to send invitation';
        this.snackBar.open(errorMessage, 'Close', {
          duration: 5000,
          panelClass: ['error-snackbar']
        });
      }
    });
  }

  cancel() {
    this.dialogRef.close();
  }
}