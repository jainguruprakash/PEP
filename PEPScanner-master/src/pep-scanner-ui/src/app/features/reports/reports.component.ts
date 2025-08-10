import { Component, inject, signal } from '@angular/core';
import { CommonModule } from '@angular/common';
import { ReactiveFormsModule, FormBuilder, Validators } from '@angular/forms';
import { MatTableModule } from '@angular/material/table';
import { MatButtonModule } from '@angular/material/button';
import { MatIconModule } from '@angular/material/icon';
import { MatCardModule } from '@angular/material/card';
import { MatFormFieldModule } from '@angular/material/form-field';
import { MatInputModule } from '@angular/material/input';
import { MatSelectModule } from '@angular/material/select';
import { MatChipsModule } from '@angular/material/chips';
import { MatTabsModule } from '@angular/material/tabs';
import { MatDatepickerModule } from '@angular/material/datepicker';
import { MatNativeDateModule } from '@angular/material/core';
import { ReportsService } from '../../services/reports.service';

@Component({
  selector: 'app-reports',
  standalone: true,
  imports: [
    CommonModule, 
    ReactiveFormsModule, 
    MatTableModule, 
    MatButtonModule, 
    MatIconModule, 
    MatCardModule, 
    MatFormFieldModule, 
    MatInputModule, 
    MatSelectModule, 
    MatChipsModule,
    MatTabsModule,
    MatDatepickerModule,
    MatNativeDateModule
  ],
  template: `
    <mat-card>
      <mat-card-header>
        <mat-card-title>
          <mat-icon>description</mat-icon>
          SAR & STR Reports
        </mat-card-title>
      </mat-card-header>
      
      <mat-card-content>
        <mat-tab-group>
          <!-- SAR Reports Tab -->
          <mat-tab label="SAR Reports">
            <div class="tab-content">
              <div class="actions-bar">
                <button mat-raised-button color="primary" (click)="showSarForm = !showSarForm">
                  <mat-icon>add</mat-icon>
                  Create SAR
                </button>
              </div>

              <!-- SAR Creation Form -->
              <mat-card *ngIf="showSarForm" class="form-card">
                <mat-card-title>Create Suspicious Activity Report</mat-card-title>
                <form [formGroup]="sarForm" (ngSubmit)="createSar()">
                  <div class="form-grid">
                    <mat-form-field appearance="outline">
                      <mat-label>Subject Name</mat-label>
                      <input matInput formControlName="subjectName" required>
                    </mat-form-field>
                    <mat-form-field appearance="outline">
                      <mat-label>Subject Address</mat-label>
                      <input matInput formControlName="subjectAddress">
                    </mat-form-field>
                    <mat-form-field appearance="outline">
                      <mat-label>Identification Number</mat-label>
                      <input matInput formControlName="subjectIdentification">
                    </mat-form-field>
                    <mat-form-field appearance="outline">
                      <mat-label>Date of Birth</mat-label>
                      <input matInput [matDatepicker]="dobPicker" formControlName="subjectDateOfBirth">
                      <mat-datepicker-toggle matSuffix [for]="dobPicker"></mat-datepicker-toggle>
                      <mat-datepicker #dobPicker></mat-datepicker>
                    </mat-form-field>
                    <mat-form-field appearance="outline" class="full-width">
                      <mat-label>Suspicious Activity</mat-label>
                      <textarea matInput rows="3" formControlName="suspiciousActivity" required></textarea>
                    </mat-form-field>
                    <mat-form-field appearance="outline" class="full-width">
                      <mat-label>Activity Description</mat-label>
                      <textarea matInput rows="4" formControlName="activityDescription" required></textarea>
                    </mat-form-field>
                    <mat-form-field appearance="outline">
                      <mat-label>Transaction Amount</mat-label>
                      <input matInput type="number" formControlName="transactionAmount">
                    </mat-form-field>
                    <mat-form-field appearance="outline">
                      <mat-label>Currency</mat-label>
                      <mat-select formControlName="transactionCurrency">
                        <mat-option value="USD">USD</mat-option>
                        <mat-option value="EUR">EUR</mat-option>
                        <mat-option value="INR">INR</mat-option>
                        <mat-option value="GBP">GBP</mat-option>
                      </mat-select>
                    </mat-form-field>
                    <mat-form-field appearance="outline">
                      <mat-label>Priority</mat-label>
                      <mat-select formControlName="priority">
                        <mat-option value="Low">Low</mat-option>
                        <mat-option value="Medium">Medium</mat-option>
                        <mat-option value="High">High</mat-option>
                        <mat-option value="Critical">Critical</mat-option>
                      </mat-select>
                    </mat-form-field>
                  </div>
                  <div class="form-actions">
                    <button mat-raised-button color="primary" type="submit" [disabled]="sarForm.invalid">Create SAR</button>
                    <button mat-button type="button" (click)="showSarForm = false">Cancel</button>
                  </div>
                </form>
              </mat-card>

              <!-- SAR Reports Table -->
              <table mat-table [dataSource]="sarReports()" class="mat-elevation-z2">
                <ng-container matColumnDef="reportNumber">
                  <th mat-header-cell *matHeaderCellDef>Report #</th>
                  <td mat-cell *matCellDef="let row">{{ row.reportNumber }}</td>
                </ng-container>
                <ng-container matColumnDef="subjectName">
                  <th mat-header-cell *matHeaderCellDef>Subject</th>
                  <td mat-cell *matCellDef="let row">{{ row.subjectName }}</td>
                </ng-container>
                <ng-container matColumnDef="status">
                  <th mat-header-cell *matHeaderCellDef>Status</th>
                  <td mat-cell *matCellDef="let row">
                    <mat-chip [color]="getStatusColor(row.status)">{{ row.status }}</mat-chip>
                  </td>
                </ng-container>
                <ng-container matColumnDef="priority">
                  <th mat-header-cell *matHeaderCellDef>Priority</th>
                  <td mat-cell *matCellDef="let row">
                    <mat-chip [color]="getPriorityColor(row.priority)">{{ row.priority }}</mat-chip>
                  </td>
                </ng-container>
                <ng-container matColumnDef="createdAt">
                  <th mat-header-cell *matHeaderCellDef>Created</th>
                  <td mat-cell *matCellDef="let row">{{ formatDate(row.createdAt) }}</td>
                </ng-container>
                <ng-container matColumnDef="actions">
                  <th mat-header-cell *matHeaderCellDef>Actions</th>
                  <td mat-cell *matCellDef="let row">
                    <button mat-icon-button (click)="viewReport(row)">
                      <mat-icon>visibility</mat-icon>
                    </button>
                    <button mat-icon-button (click)="updateStatus(row, 'Approved')" *ngIf="row.status === 'Draft'">
                      <mat-icon>check</mat-icon>
                    </button>
                  </td>
                </ng-container>
                <tr mat-header-row *matHeaderRowDef="sarColumns"></tr>
                <tr mat-row *matRowDef="let row; columns: sarColumns;"></tr>
              </table>
            </div>
          </mat-tab>

          <!-- STR Reports Tab -->
          <mat-tab label="STR Reports">
            <div class="tab-content">
              <div class="actions-bar">
                <button mat-raised-button color="primary" (click)="showStrForm = !showStrForm">
                  <mat-icon>add</mat-icon>
                  Create STR
                </button>
              </div>

              <!-- STR Creation Form -->
              <mat-card *ngIf="showStrForm" class="form-card">
                <mat-card-title>Create Suspicious Transaction Report</mat-card-title>
                <form [formGroup]="strForm" (ngSubmit)="createStr()">
                  <div class="form-grid">
                    <mat-form-field appearance="outline">
                      <mat-label>Transaction Reference</mat-label>
                      <input matInput formControlName="transactionReference" required>
                    </mat-form-field>
                    <mat-form-field appearance="outline">
                      <mat-label>Account Number</mat-label>
                      <input matInput formControlName="accountNumber" required>
                    </mat-form-field>
                    <mat-form-field appearance="outline">
                      <mat-label>Transaction Amount</mat-label>
                      <input matInput type="number" formControlName="transactionAmount" required>
                    </mat-form-field>
                    <mat-form-field appearance="outline">
                      <mat-label>Currency</mat-label>
                      <mat-select formControlName="transactionCurrency">
                        <mat-option value="USD">USD</mat-option>
                        <mat-option value="EUR">EUR</mat-option>
                        <mat-option value="INR">INR</mat-option>
                        <mat-option value="GBP">GBP</mat-option>
                      </mat-select>
                    </mat-form-field>
                    <mat-form-field appearance="outline">
                      <mat-label>Transaction Date</mat-label>
                      <input matInput [matDatepicker]="txnDatePicker" formControlName="transactionDate">
                      <mat-datepicker-toggle matSuffix [for]="txnDatePicker"></mat-datepicker-toggle>
                      <mat-datepicker #txnDatePicker></mat-datepicker>
                    </mat-form-field>
                    <mat-form-field appearance="outline" class="full-width">
                      <mat-label>Reason for Suspicion</mat-label>
                      <textarea matInput rows="4" formControlName="reasonForSuspicion" required></textarea>
                    </mat-form-field>
                  </div>
                  <div class="form-actions">
                    <button mat-raised-button color="primary" type="submit" [disabled]="strForm.invalid">Create STR</button>
                    <button mat-button type="button" (click)="showStrForm = false">Cancel</button>
                  </div>
                </form>
              </mat-card>

              <!-- STR Reports Table -->
              <table mat-table [dataSource]="strReports()" class="mat-elevation-z2">
                <ng-container matColumnDef="reportNumber">
                  <th mat-header-cell *matHeaderCellDef>Report #</th>
                  <td mat-cell *matCellDef="let row">{{ row.reportNumber }}</td>
                </ng-container>
                <ng-container matColumnDef="transactionReference">
                  <th mat-header-cell *matHeaderCellDef>Transaction Ref</th>
                  <td mat-cell *matCellDef="let row">{{ row.transactionReference }}</td>
                </ng-container>
                <ng-container matColumnDef="transactionAmount">
                  <th mat-header-cell *matHeaderCellDef>Amount</th>
                  <td mat-cell *matCellDef="let row">{{ row.transactionAmount | currency:row.transactionCurrency }}</td>
                </ng-container>
                <ng-container matColumnDef="status">
                  <th mat-header-cell *matHeaderCellDef>Status</th>
                  <td mat-cell *matCellDef="let row">
                    <mat-chip [color]="getStatusColor(row.status)">{{ row.status }}</mat-chip>
                  </td>
                </ng-container>
                <ng-container matColumnDef="createdAt">
                  <th mat-header-cell *matHeaderCellDef>Created</th>
                  <td mat-cell *matCellDef="let row">{{ formatDate(row.createdAt) }}</td>
                </ng-container>
                <ng-container matColumnDef="actions">
                  <th mat-header-cell *matHeaderCellDef>Actions</th>
                  <td mat-cell *matCellDef="let row">
                    <button mat-icon-button (click)="viewReport(row)">
                      <mat-icon>visibility</mat-icon>
                    </button>
                  </td>
                </ng-container>
                <tr mat-header-row *matHeaderRowDef="strColumns"></tr>
                <tr mat-row *matRowDef="let row; columns: strColumns;"></tr>
              </table>
            </div>
          </mat-tab>
        </mat-tab-group>
      </mat-card-content>
    </mat-card>
  `,
  styles: [`
    .tab-content { padding: 24px 0; }
    .actions-bar { margin-bottom: 16px; }
    .form-card { margin-bottom: 24px; }
    .form-grid { display: grid; grid-template-columns: repeat(auto-fit, minmax(250px, 1fr)); gap: 16px; }
    .full-width { grid-column: 1 / -1; }
    .form-actions { margin-top: 16px; }
    .form-actions button { margin-right: 8px; }
    table { width: 100%; margin-top: 16px; }
    .mat-mdc-chip { font-size: 0.75rem; }
  `]
})
export class ReportsComponent {
  private reportsService = inject(ReportsService);
  private fb = inject(FormBuilder);
  
  sarReports = signal<any[]>([]);
  strReports = signal<any[]>([]);
  showSarForm = false;
  showStrForm = false;
  
  sarColumns = ['reportNumber', 'subjectName', 'status', 'priority', 'createdAt', 'actions'];
  strColumns = ['reportNumber', 'transactionReference', 'transactionAmount', 'status', 'createdAt', 'actions'];
  
  sarForm = this.fb.group({
    subjectName: ['', Validators.required],
    subjectAddress: [''],
    subjectIdentification: [''],
    subjectDateOfBirth: [''],
    suspiciousActivity: ['', Validators.required],
    activityDescription: ['', Validators.required],
    transactionAmount: [''],
    transactionCurrency: ['USD'],
    priority: ['Medium']
  });

  strForm = this.fb.group({
    transactionReference: ['', Validators.required],
    accountNumber: ['', Validators.required],
    transactionAmount: ['', Validators.required],
    transactionCurrency: ['USD'],
    transactionDate: [''],
    reasonForSuspicion: ['', Validators.required]
  });

  constructor() {
    this.loadReports();
  }

  loadReports() {
    this.reportsService.getSars().subscribe(response => {
      this.sarReports.set(response.items || []);
    });
    this.reportsService.getStrs().subscribe(response => {
      this.strReports.set(response.items || []);
    });
  }

  createSar() {
    if (this.sarForm.valid) {
      const formValue = this.sarForm.value;
      const sarRequest = {
        subjectName: formValue.subjectName || '',
        subjectAddress: formValue.subjectAddress || '',
        subjectIdentification: formValue.subjectIdentification || '',
        subjectDateOfBirth: formValue.subjectDateOfBirth ? new Date(formValue.subjectDateOfBirth) : undefined,
        suspiciousActivity: formValue.suspiciousActivity || '',
        activityDescription: formValue.activityDescription || '',
        transactionAmount: formValue.transactionAmount ? Number(formValue.transactionAmount) : undefined,
        transactionCurrency: formValue.transactionCurrency || 'USD',
        priority: formValue.priority as any || 'Medium'
      };

      this.reportsService.createSar(sarRequest).subscribe(() => {
        this.loadReports();
        this.sarForm.reset();
        this.showSarForm = false;
      });
    }
  }

  createStr() {
    if (this.strForm.valid) {
      const formValue = this.strForm.value;
      const strRequest = {
        transactionReference: formValue.transactionReference || '',
        accountNumber: formValue.accountNumber || '',
        transactionAmount: Number(formValue.transactionAmount) || 0,
        transactionCurrency: formValue.transactionCurrency || 'USD',
        transactionDate: formValue.transactionDate ? new Date(formValue.transactionDate) : new Date(),
        transactionType: 'Transfer',
        suspicionReason: formValue.reasonForSuspicion || '',
        detailedDescription: formValue.reasonForSuspicion || '',
        priority: 'Medium' as any
      };

      this.reportsService.createStr(strRequest).subscribe(() => {
        this.loadReports();
        this.strForm.reset();
        this.showStrForm = false;
      });
    }
  }

  viewReport(report: any) {
    console.log('View report:', report);
  }

  updateStatus(report: any, status: string) {
    // TODO: Implement status update
    console.log('Update status:', report, status);
  }

  getStatusColor(status: string): string {
    switch(status?.toLowerCase()) {
      case 'approved': case 'submitted': return 'primary';
      case 'draft': return 'accent';
      case 'rejected': return 'warn';
      default: return 'primary';
    }
  }

  getPriorityColor(priority: string): string {
    switch(priority?.toLowerCase()) {
      case 'critical': case 'high': return 'warn';
      case 'medium': return 'accent';
      default: return 'primary';
    }
  }

  formatDate(date: string): string {
    return date ? new Date(date).toLocaleDateString() : '';
  }
}
