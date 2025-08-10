import { Component, inject, signal } from '@angular/core';
import { CommonModule } from '@angular/common';
import { ReactiveFormsModule, FormBuilder, Validators } from '@angular/forms';
import { MatCardModule } from '@angular/material/card';
import { MatFormFieldModule } from '@angular/material/form-field';
import { MatInputModule } from '@angular/material/input';
import { MatButtonModule } from '@angular/material/button';
import { MatProgressSpinnerModule } from '@angular/material/progress-spinner';
import { MatIconModule } from '@angular/material/icon';
import { MatTabsModule } from '@angular/material/tabs';
import { MatTableModule } from '@angular/material/table';
import { MatChipsModule } from '@angular/material/chips';
import { CibilService, CibilResponse } from '../../services/cibil.service';

@Component({
  selector: 'app-cibil',
  standalone: true,
  imports: [
    CommonModule,
    ReactiveFormsModule,
    MatCardModule,
    MatFormFieldModule,
    MatInputModule,
    MatButtonModule,
    MatProgressSpinnerModule,
    MatIconModule,
    MatTabsModule,
    MatTableModule,
    MatChipsModule
  ],
  template: `
    <div class="cibil-container">
      <mat-card class="main-card">
        <mat-card-header>
          <mat-card-title>
            <mat-icon>account_balance</mat-icon>
            CIBIL Credit Report
          </mat-card-title>
          <mat-card-subtitle>
            Fetch credit report using PAN number
          </mat-card-subtitle>
        </mat-card-header>

        <mat-card-content>
          <form [formGroup]="cibilForm" (ngSubmit)="fetchCreditReport()">
            <div class="form-grid">
              <mat-form-field appearance="outline">
                <mat-label>PAN Number *</mat-label>
                <input matInput formControlName="panNumber" 
                       placeholder="ABCDE1234F" 
                       (blur)="validatePAN()"
                       maxlength="10">
                <mat-icon matSuffix>credit_card</mat-icon>
              </mat-form-field>

              <mat-form-field appearance="outline">
                <mat-label>Full Name</mat-label>
                <input matInput formControlName="fullName" 
                       placeholder="As per PAN card">
              </mat-form-field>

              <mat-form-field appearance="outline">
                <mat-label>Date of Birth</mat-label>
                <input matInput type="date" formControlName="dateOfBirth">
              </mat-form-field>

              <mat-form-field appearance="outline">
                <mat-label>Mobile Number</mat-label>
                <input matInput formControlName="mobileNumber" 
                       placeholder="10-digit mobile number"
                       maxlength="10">
              </mat-form-field>
            </div>

            <div class="action-buttons">
              <button mat-raised-button color="primary" type="submit"
                      [disabled]="cibilForm.invalid || isLoading()">
                <mat-spinner diameter="20" *ngIf="isLoading()"></mat-spinner>
                <mat-icon *ngIf="!isLoading()">search</mat-icon>
                <span *ngIf="!isLoading()">Fetch Credit Report</span>
                <span *ngIf="isLoading()">Fetching...</span>
              </button>

              <button mat-button type="button" (click)="clearForm()">
                <mat-icon>clear</mat-icon>
                Clear
              </button>
            </div>
          </form>
        </mat-card-content>
      </mat-card>

      <!-- Credit Report Results -->
      <div *ngIf="creditReport() && !isLoading()" class="results-container">
        <mat-tab-group>
          <!-- Summary Tab -->
          <mat-tab label="Summary">
            <mat-card class="summary-card">
              <div class="credit-score-section">
                <div class="credit-score">
                  <div class="score-circle" [class]="getScoreClass(creditReport()!.creditScore)">
                    <span class="score-number">{{ creditReport()!.creditScore }}</span>
                    <span class="score-label">CIBIL Score</span>
                  </div>
                </div>
                <div class="score-details">
                  <div class="detail-item">
                    <span class="label">Report Date:</span>
                    <span class="value">{{ creditReport()!.reportDate | date:'short' }}</span>
                  </div>
                  <div class="detail-item">
                    <span class="label">PAN Number:</span>
                    <span class="value">{{ creditReport()!.panNumber }}</span>
                  </div>
                </div>
              </div>

              <div class="summary-grid">
                <div class="summary-item">
                  <mat-icon>account_balance_wallet</mat-icon>
                  <div class="summary-content">
                    <div class="summary-value">{{ creditReport()!.summary.totalAccounts }}</div>
                    <div class="summary-label">Total Accounts</div>
                  </div>
                </div>
                <div class="summary-item">
                  <mat-icon>trending_up</mat-icon>
                  <div class="summary-content">
                    <div class="summary-value">{{ creditReport()!.summary.activeAccounts }}</div>
                    <div class="summary-label">Active Accounts</div>
                  </div>
                </div>
                <div class="summary-item">
                  <mat-icon>credit_card</mat-icon>
                  <div class="summary-content">
                    <div class="summary-value">₹{{ creditReport()!.summary.totalCreditLimit | number }}</div>
                    <div class="summary-label">Total Credit Limit</div>
                  </div>
                </div>
                <div class="summary-item">
                  <mat-icon>account_balance</mat-icon>
                  <div class="summary-content">
                    <div class="summary-value">{{ creditReport()!.summary.utilizationRatio }}%</div>
                    <div class="summary-label">Utilization Ratio</div>
                  </div>
                </div>
              </div>
            </mat-card>
          </mat-tab>

          <!-- Accounts Tab -->
          <mat-tab label="Accounts">
            <mat-card class="accounts-card">
              <mat-table [dataSource]="creditReport()!.accounts">
                <ng-container matColumnDef="bankName">
                  <mat-header-cell *matHeaderCellDef>Bank</mat-header-cell>
                  <mat-cell *matCellDef="let account">{{ account.bankName }}</mat-cell>
                </ng-container>
                <ng-container matColumnDef="accountType">
                  <mat-header-cell *matHeaderCellDef>Type</mat-header-cell>
                  <mat-cell *matCellDef="let account">{{ account.accountType }}</mat-cell>
                </ng-container>
                <ng-container matColumnDef="currentBalance">
                  <mat-header-cell *matHeaderCellDef>Balance</mat-header-cell>
                  <mat-cell *matCellDef="let account">₹{{ account.currentBalance | number }}</mat-cell>
                </ng-container>
                <ng-container matColumnDef="creditLimit">
                  <mat-header-cell *matHeaderCellDef>Limit</mat-header-cell>
                  <mat-cell *matCellDef="let account">₹{{ account.creditLimit | number }}</mat-cell>
                </ng-container>
                <ng-container matColumnDef="paymentStatus">
                  <mat-header-cell *matHeaderCellDef>Status</mat-header-cell>
                  <mat-cell *matCellDef="let account">
                    <span class="status-badge" [class]="account.paymentStatus.toLowerCase()">
                      {{ account.paymentStatus }}
                    </span>
                  </mat-cell>
                </ng-container>
                <ng-container matColumnDef="dpd">
                  <mat-header-cell *matHeaderCellDef>DPD</mat-header-cell>
                  <mat-cell *matCellDef="let account">{{ account.dpd }}</mat-cell>
                </ng-container>
                <mat-header-row *matHeaderRowDef="['bankName', 'accountType', 'currentBalance', 'creditLimit', 'paymentStatus', 'dpd']"></mat-header-row>
                <mat-row *matRowDef="let row; columns: ['bankName', 'accountType', 'currentBalance', 'creditLimit', 'paymentStatus', 'dpd']"></mat-row>
              </mat-table>
            </mat-card>
          </mat-tab>

          <!-- Risk Analysis Tab -->
          <mat-tab label="Risk Analysis">
            <mat-card class="risk-card">
              <div class="risk-header">
                <div class="risk-level" [class]="creditReport()!.riskAnalysis.riskLevel.toLowerCase()">
                  {{ creditReport()!.riskAnalysis.riskLevel }} RISK
                </div>
                <div class="risk-score">
                  Risk Score: {{ creditReport()!.riskAnalysis.riskScore }}/100
                </div>
              </div>

              <div class="risk-section">
                <h4>Risk Factors</h4>
                <mat-chip-set>
                  <mat-chip *ngFor="let factor of creditReport()!.riskAnalysis.riskFactors">
                    {{ factor }}
                  </mat-chip>
                </mat-chip-set>
              </div>

              <div class="risk-section">
                <h4>Recommendations</h4>
                <ul class="recommendations-list">
                  <li *ngFor="let recommendation of creditReport()!.riskAnalysis.recommendations">
                    {{ recommendation }}
                  </li>
                </ul>
              </div>
            </mat-card>
          </mat-tab>
        </mat-tab-group>
      </div>
    </div>
  `,
  styleUrls: ['./cibil.component.scss']
})
export class CibilComponent {
  private fb = inject(FormBuilder);
  private cibilService = inject(CibilService);

  isLoading = signal(false);
  creditReport = signal<CibilResponse | null>(null);

  cibilForm = this.fb.group({
    panNumber: ['', [Validators.required, Validators.pattern(/^[A-Z]{5}[0-9]{4}[A-Z]{1}$/)]],
    fullName: [''],
    dateOfBirth: [''],
    mobileNumber: ['', [Validators.pattern(/^[0-9]{10}$/)]]
  });

  fetchCreditReport() {
    if (this.cibilForm.invalid) return;

    this.isLoading.set(true);
    const request = this.cibilForm.value as any;

    this.cibilService.getCreditReport(request).subscribe({
      next: (response) => {
        this.creditReport.set(response);
        this.isLoading.set(false);
      },
      error: (error) => {
        console.error('CIBIL fetch error:', error);
        this.isLoading.set(false);
      }
    });
  }

  validatePAN() {
    const panNumber = this.cibilForm.get('panNumber')?.value;
    if (panNumber && panNumber.length === 10) {
      this.cibilService.validatePAN(panNumber).subscribe({
        next: (response) => {
          if (response.valid && response.name) {
            this.cibilForm.patchValue({ fullName: response.name });
          }
        },
        error: (error) => console.error('PAN validation error:', error)
      });
    }
  }

  clearForm() {
    this.cibilForm.reset();
    this.creditReport.set(null);
  }

  getScoreClass(score: number): string {
    if (score >= 750) return 'excellent';
    if (score >= 700) return 'good';
    if (score >= 650) return 'fair';
    return 'poor';
  }
}