import { Component, inject, signal, OnInit } from '@angular/core';
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
import { MatProgressSpinnerModule } from '@angular/material/progress-spinner';
import { MatProgressBarModule } from '@angular/material/progress-bar';
import { MatSnackBarModule, MatSnackBar } from '@angular/material/snack-bar';
import { MatBadgeModule } from '@angular/material/badge';
import { MatTooltipModule } from '@angular/material/tooltip';
import { MatExpansionModule } from '@angular/material/expansion';
import { MatDividerModule } from '@angular/material/divider';
import { MatListModule } from '@angular/material/list';
import { FinancialIntelligenceService } from '../../services/financial-intelligence.service';
import { ActivatedRoute } from '@angular/router';

@Component({
  selector: 'app-financial-intelligence',
  standalone: true,
  imports: [
    CommonModule, ReactiveFormsModule, MatTableModule, MatButtonModule, MatIconModule, 
    MatCardModule, MatFormFieldModule, MatInputModule, MatSelectModule, MatChipsModule,
    MatTabsModule, MatProgressSpinnerModule, MatProgressBarModule, MatSnackBarModule, 
    MatBadgeModule, MatTooltipModule, MatExpansionModule, MatDividerModule, MatListModule
  ],
  template: `
    <mat-card>
      <mat-card-header>
        <mat-card-title>
          <mat-icon>account_balance</mat-icon>
          Financial Intelligence Dashboard
        </mat-card-title>
        <mat-card-subtitle>
          Comprehensive financial risk assessment and monitoring
        </mat-card-subtitle>
      </mat-card-header>
      
      <mat-card-content>
        <mat-tab-group>
          <!-- Financial Overview -->
          <mat-tab label="Overview">
            <div class="tab-content">
              <!-- Quick Search -->
              <mat-card class="search-card">
                <mat-card-title>Quick Financial Lookup</mat-card-title>
                <mat-card-content>
                  <form [formGroup]="searchForm" class="search-form">
                    <mat-form-field appearance="outline">
                      <mat-label>Search Type</mat-label>
                      <mat-select formControlName="searchType">
                        <mat-option value="pan">PAN (CIBIL)</mat-option>
                        <mat-option value="cin">CIN (MCA)</mat-option>
                        <mat-option value="gstin">GSTIN (GST)</mat-option>
                        <mat-option value="customer">Customer ID</mat-option>
                      </mat-select>
                    </mat-form-field>

                    <mat-form-field appearance="outline">
                      <mat-label>Search Value</mat-label>
                      <input matInput formControlName="searchValue" placeholder="Enter PAN, CIN, GSTIN, or Customer ID">
                    </mat-form-field>

                    <mat-form-field appearance="outline" *ngIf="searchForm.get('searchType')?.value === 'pan'">
                      <mat-label>Full Name</mat-label>
                      <input matInput formControlName="fullName" placeholder="Enter full name for CIBIL report">
                    </mat-form-field>

                    <button mat-raised-button color="primary" 
                            (click)="performSearch()" 
                            [disabled]="isLoading() || searchForm.invalid">
                      <mat-spinner diameter="20" *ngIf="isLoading()"></mat-spinner>
                      <mat-icon *ngIf="!isLoading()">search</mat-icon>
                      Search
                    </button>
                  </form>
                </mat-card-content>
              </mat-card>

              <!-- Financial Summary Cards -->
              <div class="summary-cards" *ngIf="financialDashboard()">
                <mat-card class="summary-card risk-card">
                  <mat-card-content>
                    <div class="card-header">
                      <mat-icon>assessment</mat-icon>
                      <span>Overall Risk Score</span>
                    </div>
                    <div class="score-display">
                      <span class="score" [style.color]="getFinancialRiskColor(financialDashboard()?.summary?.overallRiskScore || 0)">
                        {{ (financialDashboard()?.summary?.overallRiskScore || 0).toFixed(1) }}
                      </span>
                      <span class="level">{{ getFinancialRiskLevel(financialDashboard()?.summary?.overallRiskScore || 0) }}</span>
                    </div>
                  </mat-card-content>
                </mat-card>

                <mat-card class="summary-card compliance-card">
                  <mat-card-content>
                    <div class="card-header">
                      <mat-icon>verified</mat-icon>
                      <span>Compliance Score</span>
                    </div>
                    <div class="score-display">
                      <span class="score" [style.color]="getComplianceScoreColor(financialDashboard()?.summary?.complianceScore || 0)">
                        {{ (financialDashboard()?.summary?.complianceScore || 0).toFixed(0) }}%
                      </span>
                      <span class="level">{{ getComplianceLevel(financialDashboard()?.summary?.complianceScore || 0) }}</span>
                    </div>
                  </mat-card-content>
                </mat-card>

                <mat-card class="summary-card anomalies-card" *ngIf="financialDashboard()?.summary?.totalAnomalies > 0">
                  <mat-card-content>
                    <div class="card-header">
                      <mat-icon>warning</mat-icon>
                      <span>Financial Anomalies</span>
                    </div>
                    <div class="score-display">
                      <span class="score critical">{{ financialDashboard()?.summary?.totalAnomalies || 0 }}</span>
                      <span class="level">{{ financialDashboard()?.summary?.criticalAnomalies || 0 }} Critical</span>
                    </div>
                  </mat-card-content>
                </mat-card>

                <mat-card class="summary-card assessment-card">
                  <mat-card-content>
                    <div class="card-header">
                      <mat-icon>schedule</mat-icon>
                      <span>Last Assessment</span>
                    </div>
                    <div class="score-display">
                      <span class="date">{{ formatDate(financialDashboard()?.summary?.lastAssessment) }}</span>
                      <span class="level">{{ getTimeSinceAssessment(financialDashboard()?.summary?.lastAssessment) }}</span>
                    </div>
                  </mat-card-content>
                </mat-card>
              </div>
            </div>
          </mat-tab>

          <!-- CIBIL Report -->
          <mat-tab label="CIBIL Report" [matBadge]="cibilReport() ? '✓' : ''" matBadgeColor="accent">
            <div class="tab-content">
              <div *ngIf="cibilReport(); else noCibilData">
                <mat-card class="cibil-overview">
                  <mat-card-header>
                    <mat-card-title>CIBIL Credit Report</mat-card-title>
                    <mat-card-subtitle>{{ cibilReport()?.name }} ({{ cibilReport()?.pan }})</mat-card-subtitle>
                  </mat-card-header>
                  <mat-card-content>
                    <div class="cibil-score-section">
                      <div class="score-circle">
                        <span class="score" [style.color]="getCibilScoreColor(cibilReport()?.cibilScore || 0)">
                          {{ cibilReport()?.cibilScore }}
                        </span>
                        <span class="label">{{ getCibilScoreLabel(cibilReport()?.cibilScore || 0) }}</span>
                      </div>
                      <div class="score-details">
                        <p><strong>Score Date:</strong> {{ formatDate(cibilReport()?.scoreDate) }}</p>
                        <p><strong>Report Generated:</strong> {{ formatDate(cibilReport()?.reportGeneratedAt) }}</p>
                      </div>
                    </div>
                  </mat-card-content>
                </mat-card>

                <mat-expansion-panel class="credit-section">
                  <mat-expansion-panel-header>
                    <mat-panel-title>Active Loans ({{ cibilReport()?.activeLoans?.length || 0 }})</mat-panel-title>
                  </mat-expansion-panel-header>
                  <div class="loans-grid">
                    <mat-card *ngFor="let loan of cibilReport()?.activeLoans" class="loan-card">
                      <mat-card-content>
                        <h4>{{ loan.loanType }}</h4>
                        <p><strong>Lender:</strong> {{ loan.lenderName }}</p>
                        <p><strong>Outstanding:</strong> {{ formatCurrency(loan.outstandingAmount) }}</p>
                        <p><strong>Monthly EMI:</strong> {{ formatCurrency(loan.monthlyEmi) }}</p>
                        <mat-chip [color]="getPaymentStatusColor(loan.status)">{{ loan.status }}</mat-chip>
                      </mat-card-content>
                    </mat-card>
                  </div>
                </mat-expansion-panel>

                <mat-expansion-panel class="credit-section">
                  <mat-expansion-panel-header>
                    <mat-panel-title>Credit Cards ({{ cibilReport()?.creditCards?.length || 0 }})</mat-panel-title>
                  </mat-expansion-panel-header>
                  <div class="cards-grid">
                    <mat-card *ngFor="let card of cibilReport()?.creditCards" class="card-item">
                      <mat-card-content>
                        <h4>{{ card.cardType }}</h4>
                        <p><strong>Bank:</strong> {{ card.bankName }}</p>
                        <p><strong>Limit:</strong> {{ formatCurrency(card.creditLimit) }}</p>
                        <p><strong>Used:</strong> {{ formatCurrency(card.currentBalance) }}</p>
                        <p><strong>Available:</strong> {{ formatCurrency(card.availableCredit) }}</p>
                        <mat-chip [color]="getPaymentStatusColor(card.status)">{{ card.status }}</mat-chip>
                      </mat-card-content>
                    </mat-card>
                  </div>
                </mat-expansion-panel>

                <mat-expansion-panel class="credit-section">
                  <mat-expansion-panel-header>
                    <mat-panel-title>Payment History</mat-panel-title>
                  </mat-expansion-panel-header>
                  <div class="payment-history">
                    <div class="payment-stats">
                      <div class="stat-item">
                        <span class="stat-value">{{ cibilReport()?.paymentHistory?.onTimePayments || 0 }}</span>
                        <span class="stat-label">On-time Payments</span>
                      </div>
                      <div class="stat-item">
                        <span class="stat-value">{{ cibilReport()?.paymentHistory?.delayedPayments || 0 }}</span>
                        <span class="stat-label">Delayed Payments</span>
                      </div>
                      <div class="stat-item">
                        <span class="stat-value">{{ cibilReport()?.paymentHistory?.missedPayments || 0 }}</span>
                        <span class="stat-label">Missed Payments</span>
                      </div>
                      <div class="stat-item">
                        <span class="stat-value">{{ (cibilReport()?.paymentHistory?.paymentReliability || 0).toFixed(1) }}%</span>
                        <span class="stat-label">Reliability</span>
                      </div>
                    </div>
                  </div>
                </mat-expansion-panel>
              </div>

              <ng-template #noCibilData>
                <mat-card class="no-data-card">
                  <mat-card-content>
                    <mat-icon>credit_score</mat-icon>
                    <h3>No CIBIL Data Available</h3>
                    <p>Search for a customer using PAN to view CIBIL credit report</p>
                  </mat-card-content>
                </mat-card>
              </ng-template>
            </div>
          </mat-tab>

          <!-- GST Profile -->
          <mat-tab label="GST Profile" [matBadge]="gstProfile() ? '✓' : ''" matBadgeColor="accent">
            <div class="tab-content">
              <div *ngIf="gstProfile(); else noGstData">
                <mat-card class="gst-overview">
                  <mat-card-header>
                    <mat-card-title>GST Profile</mat-card-title>
                    <mat-card-subtitle>{{ gstProfile()?.legalName }} ({{ gstProfile()?.gstin }})</mat-card-subtitle>
                  </mat-card-header>
                  <mat-card-content>
                    <div class="gst-details">
                      <div class="detail-row">
                        <span class="label">Status:</span>
                        <mat-chip [style.background-color]="getGstStatusColor(gstProfile()?.gstStatus || '')">
                          {{ gstProfile()?.gstStatus }}
                        </mat-chip>
                      </div>
                      <div class="detail-row">
                        <span class="label">Compliance Rating:</span>
                        <mat-chip [style.background-color]="getComplianceScoreColor(getComplianceScoreFromRating(gstProfile()?.complianceRating || ''))">
                          {{ gstProfile()?.complianceRating }}
                        </mat-chip>
                      </div>
                      <div class="detail-row">
                        <span class="label">Turnover Range:</span>
                        <span>{{ gstProfile()?.turnoverRange }}</span>
                      </div>
                      <div class="detail-row">
                        <span class="label">Last Return Filed:</span>
                        <span>{{ formatDate(gstProfile()?.lastReturnFiled) }}</span>
                      </div>
                    </div>
                  </mat-card-content>
                </mat-card>
              </div>

              <ng-template #noGstData>
                <mat-card class="no-data-card">
                  <mat-card-content>
                    <mat-icon>receipt</mat-icon>
                    <h3>No GST Data Available</h3>
                    <p>Search for a business using GSTIN to view GST profile</p>
                  </mat-card-content>
                </mat-card>
              </ng-template>
            </div>
          </mat-tab>

          <!-- MCA Profile -->
          <mat-tab label="MCA Profile" [matBadge]="mcaProfile() ? '✓' : ''" matBadgeColor="accent">
            <div class="tab-content">
              <div *ngIf="mcaProfile(); else noMcaData">
                <mat-card class="mca-overview">
                  <mat-card-header>
                    <mat-card-title>MCA Company Profile</mat-card-title>
                    <mat-card-subtitle>{{ mcaProfile()?.companyName }} ({{ mcaProfile()?.cin }})</mat-card-subtitle>
                  </mat-card-header>
                  <mat-card-content>
                    <div class="mca-details">
                      <div class="detail-row">
                        <span class="label">Status:</span>
                        <mat-chip [style.background-color]="getCompanyStatusColor(mcaProfile()?.companyStatus || '')">
                          {{ mcaProfile()?.companyStatus }}
                        </mat-chip>
                      </div>
                      <div class="detail-row">
                        <span class="label">Incorporation Date:</span>
                        <span>{{ formatDate(mcaProfile()?.dateOfIncorporation) }}</span>
                      </div>
                      <div class="detail-row">
                        <span class="label">Paid-up Capital:</span>
                        <span>{{ formatCurrency(mcaProfile()?.paidUpCapital || 0) }}</span>
                      </div>
                      <div class="detail-row">
                        <span class="label">Authorized Capital:</span>
                        <span>{{ formatCurrency(mcaProfile()?.authorizedCapital || 0) }}</span>
                      </div>
                    </div>
                  </mat-card-content>
                </mat-card>
              </div>

              <ng-template #noMcaData>
                <mat-card class="no-data-card">
                  <mat-card-content>
                    <mat-icon>business</mat-icon>
                    <h3>No MCA Data Available</h3>
                    <p>Search for a company using CIN to view MCA profile</p>
                  </mat-card-content>
                </mat-card>
              </ng-template>
            </div>
          </mat-tab>

          <!-- Financial Risk Assessment -->
          <mat-tab label="Risk Assessment" [matBadge]="financialRiskAssessment() ? '✓' : ''" matBadgeColor="warn">
            <div class="tab-content">
              <div *ngIf="financialRiskAssessment(); else noRiskData">
                <mat-card class="risk-overview">
                  <mat-card-header>
                    <mat-card-title>Financial Risk Assessment</mat-card-title>
                    <mat-card-subtitle>{{ financialRiskAssessment()?.customerName }}</mat-card-subtitle>
                  </mat-card-header>
                  <mat-card-content>
                    <div class="risk-scores">
                      <div class="risk-score-item">
                        <span class="score-label">Overall Risk</span>
                        <span class="score-value" [style.color]="getFinancialRiskColor(financialRiskAssessment()?.overallRiskScore || 0)">
                          {{ (financialRiskAssessment()?.overallRiskScore || 0).toFixed(1) }}
                        </span>
                      </div>
                      <div class="risk-score-item">
                        <span class="score-label">Credit Risk</span>
                        <span class="score-value" [style.color]="getFinancialRiskColor(financialRiskAssessment()?.creditRisk || 0)">
                          {{ (financialRiskAssessment()?.creditRisk || 0).toFixed(1) }}
                        </span>
                      </div>
                      <div class="risk-score-item">
                        <span class="score-label">Compliance Risk</span>
                        <span class="score-value" [style.color]="getFinancialRiskColor(financialRiskAssessment()?.complianceRisk || 0)">
                          {{ (financialRiskAssessment()?.complianceRisk || 0).toFixed(1) }}
                        </span>
                      </div>
                      <div class="risk-score-item">
                        <span class="score-label">Liquidity Risk</span>
                        <span class="score-value" [style.color]="getFinancialRiskColor(financialRiskAssessment()?.liquidityRisk || 0)">
                          {{ (financialRiskAssessment()?.liquidityRisk || 0).toFixed(1) }}
                        </span>
                      </div>
                    </div>
                  </mat-card-content>
                </mat-card>

                <mat-expansion-panel class="risk-section">
                  <mat-expansion-panel-header>
                    <mat-panel-title>Risk Factors ({{ financialRiskAssessment()?.riskFactors?.length || 0 }})</mat-panel-title>
                  </mat-expansion-panel-header>
                  <mat-list>
                    <mat-list-item *ngFor="let factor of financialRiskAssessment()?.riskFactors">
                      <mat-icon matListItemIcon [style.color]="getSeverityColor(factor.severity)">warning</mat-icon>
                      <div matListItemTitle>{{ factor.description }}</div>
                      <div matListItemLine>{{ factor.category }} • Impact: {{ factor.impact.toFixed(1) }}</div>
                    </mat-list-item>
                  </mat-list>
                </mat-expansion-panel>

                <mat-expansion-panel class="risk-section">
                  <mat-expansion-panel-header>
                    <mat-panel-title>Recommendations ({{ financialRiskAssessment()?.recommendations?.length || 0 }})</mat-panel-title>
                  </mat-expansion-panel-header>
                  <mat-list>
                    <mat-list-item *ngFor="let rec of financialRiskAssessment()?.recommendations">
                      <mat-icon matListItemIcon [style.color]="getPriorityColor(rec.priority)">lightbulb</mat-icon>
                      <div matListItemTitle>{{ rec.action }}</div>
                      <div matListItemLine>{{ rec.rationale }} • {{ rec.timeline }}</div>
                    </mat-list-item>
                  </mat-list>
                </mat-expansion-panel>
              </div>

              <ng-template #noRiskData>
                <mat-card class="no-data-card">
                  <mat-card-content>
                    <mat-icon>assessment</mat-icon>
                    <h3>No Risk Assessment Available</h3>
                    <p>Search for a customer to view comprehensive financial risk assessment</p>
                  </mat-card-content>
                </mat-card>
              </ng-template>
            </div>
          </mat-tab>
        </mat-tab-group>
      </mat-card-content>
    </mat-card>
  `,
  styles: [`
    .tab-content { padding: 24px 0; }
    .search-card { margin-bottom: 24px; }
    .search-form { display: grid; grid-template-columns: repeat(auto-fit, minmax(200px, 1fr)); gap: 16px; align-items: end; }
    
    .summary-cards { display: grid; grid-template-columns: repeat(auto-fit, minmax(250px, 1fr)); gap: 16px; margin-bottom: 24px; }
    .summary-card { text-align: center; }
    .card-header { display: flex; align-items: center; justify-content: center; gap: 8px; margin-bottom: 16px; }
    .score-display { display: flex; flex-direction: column; align-items: center; }
    .score { font-size: 2rem; font-weight: bold; }
    .level { color: #666; margin-top: 4px; }
    .date { font-size: 1.2rem; font-weight: 500; }
    .critical { color: #f44336; }
    
    .cibil-overview, .gst-overview, .mca-overview, .risk-overview { margin-bottom: 24px; }
    .cibil-score-section { display: flex; align-items: center; gap: 24px; }
    .score-circle { text-align: center; }
    .score-circle .score { font-size: 3rem; font-weight: bold; display: block; }
    .score-circle .label { color: #666; }
    
    .credit-section, .risk-section { margin-bottom: 16px; }
    .loans-grid, .cards-grid { display: grid; grid-template-columns: repeat(auto-fit, minmax(300px, 1fr)); gap: 16px; }
    .loan-card, .card-item { margin-bottom: 8px; }
    
    .payment-history { padding: 16px; }
    .payment-stats { display: grid; grid-template-columns: repeat(auto-fit, minmax(150px, 1fr)); gap: 16px; }
    .stat-item { text-align: center; }
    .stat-value { font-size: 1.5rem; font-weight: bold; display: block; color: #1976d2; }
    .stat-label { color: #666; font-size: 0.9rem; }
    
    .gst-details, .mca-details { display: grid; gap: 12px; }
    .detail-row { display: flex; justify-content: space-between; align-items: center; }
    .label { font-weight: 500; }
    
    .risk-scores { display: grid; grid-template-columns: repeat(auto-fit, minmax(150px, 1fr)); gap: 16px; margin-bottom: 24px; }
    .risk-score-item { text-align: center; }
    .score-label { display: block; color: #666; margin-bottom: 8px; }
    .score-value { font-size: 1.5rem; font-weight: bold; }
    
    .no-data-card { text-align: center; padding: 40px; }
    .no-data-card mat-icon { font-size: 48px; margin-bottom: 16px; color: #999; }
    .no-data-card h3 { color: #666; margin-bottom: 8px; }
    .no-data-card p { color: #999; }
    
    .mat-mdc-chip { font-size: 0.75rem; }
  `]
})
export class FinancialIntelligenceComponent implements OnInit {
  private financialService = inject(FinancialIntelligenceService);
  private snackBar = inject(MatSnackBar);
  private route = inject(ActivatedRoute);
  private fb = inject(FormBuilder);

  // Signals for reactive state management
  isLoading = signal(false);
  cibilReport = signal<any>(null);
  gstProfile = signal<any>(null);
  mcaProfile = signal<any>(null);
  financialRiskAssessment = signal<any>(null);
  financialDashboard = signal<any>(null);

  searchForm = this.fb.group({
    searchType: ['pan', Validators.required],
    searchValue: ['', Validators.required],
    fullName: ['']
  });

  ngOnInit() {
    // Check if customer ID is provided in route
    const customerId = this.route.snapshot.paramMap.get('customerId');
    if (customerId) {
      this.loadCustomerFinancialData(customerId);
    }
  }

  performSearch() {
    const formValue = this.searchForm.value;
    if (!formValue.searchType || !formValue.searchValue) return;

    this.isLoading.set(true);

    switch (formValue.searchType) {
      case 'pan':
        if (!formValue.fullName) {
          this.snackBar.open('Full name is required for CIBIL report', 'Close', { duration: 5000 });
          this.isLoading.set(false);
          return;
        }
        this.searchCibilReport(formValue.searchValue, formValue.fullName);
        break;
      case 'cin':
        this.searchMcaProfile(formValue.searchValue);
        break;
      case 'gstin':
        this.searchGstProfile(formValue.searchValue);
        break;
      case 'customer':
        this.loadCustomerFinancialData(formValue.searchValue);
        break;
    }
  }

  searchCibilReport(pan: string, name: string) {
    this.financialService.getCibilReport(pan, name).subscribe({
      next: (response) => {
        this.cibilReport.set(response.data);
        this.isLoading.set(false);
        this.snackBar.open('CIBIL report loaded successfully', 'Close', { duration: 3000 });
      },
      error: (error) => {
        console.error('Error loading CIBIL report:', error);
        this.isLoading.set(false);
        this.snackBar.open('Failed to load CIBIL report', 'Close', { duration: 5000 });
      }
    });
  }

  searchGstProfile(gstin: string) {
    this.financialService.getGstProfile(gstin).subscribe({
      next: (response) => {
        this.gstProfile.set(response.data);
        this.isLoading.set(false);
        this.snackBar.open('GST profile loaded successfully', 'Close', { duration: 3000 });
      },
      error: (error) => {
        console.error('Error loading GST profile:', error);
        this.isLoading.set(false);
        this.snackBar.open('Failed to load GST profile', 'Close', { duration: 5000 });
      }
    });
  }

  searchMcaProfile(cin: string) {
    this.financialService.getMcaProfile(cin).subscribe({
      next: (response) => {
        this.mcaProfile.set(response.data);
        this.isLoading.set(false);
        this.snackBar.open('MCA profile loaded successfully', 'Close', { duration: 3000 });
      },
      error: (error) => {
        console.error('Error loading MCA profile:', error);
        this.isLoading.set(false);
        this.snackBar.open('Failed to load MCA profile', 'Close', { duration: 5000 });
      }
    });
  }

  loadCustomerFinancialData(customerId: string) {
    this.financialService.getFinancialDashboard(customerId).subscribe({
      next: (response) => {
        this.financialDashboard.set(response.data);
        this.financialRiskAssessment.set(response.data.riskAssessment);
        this.isLoading.set(false);
        this.snackBar.open('Financial dashboard loaded successfully', 'Close', { duration: 3000 });
      },
      error: (error) => {
        console.error('Error loading financial dashboard:', error);
        this.isLoading.set(false);
        this.snackBar.open('Failed to load financial dashboard', 'Close', { duration: 5000 });
      }
    });
  }

  // Utility methods
  getCibilScoreColor = this.financialService.getCibilScoreColor.bind(this.financialService);
  getCibilScoreLabel = this.financialService.getCibilScoreLabel.bind(this.financialService);
  getFinancialRiskColor = this.financialService.getFinancialRiskColor.bind(this.financialService);
  getFinancialRiskLevel = this.financialService.getFinancialRiskLevel.bind(this.financialService);
  getComplianceScoreColor = this.financialService.getComplianceScoreColor.bind(this.financialService);
  getPaymentStatusColor = this.financialService.getPaymentStatusColor.bind(this.financialService);
  getGstStatusColor = this.financialService.getGstStatusColor.bind(this.financialService);
  getCompanyStatusColor = this.financialService.getCompanyStatusColor.bind(this.financialService);
  formatCurrency = this.financialService.formatCurrency.bind(this.financialService);

  getComplianceLevel(score: number): string {
    if (score >= 80) return 'Excellent';
    if (score >= 60) return 'Good';
    if (score >= 40) return 'Average';
    return 'Poor';
  }

  getComplianceScoreFromRating(rating: string): number {
    switch (rating.toLowerCase()) {
      case 'excellent': return 95;
      case 'good': return 80;
      case 'average': return 60;
      case 'poor': return 40;
      default: return 50;
    }
  }

  formatDate(date: string | Date): string {
    return new Date(date).toLocaleDateString();
  }

  getTimeSinceAssessment(date: string | Date): string {
    const now = new Date();
    const assessmentDate = new Date(date);
    const diffTime = Math.abs(now.getTime() - assessmentDate.getTime());
    const diffDays = Math.ceil(diffTime / (1000 * 60 * 60 * 24));
    
    if (diffDays === 1) return '1 day ago';
    if (diffDays < 30) return `${diffDays} days ago`;
    if (diffDays < 365) return `${Math.floor(diffDays / 30)} months ago`;
    return `${Math.floor(diffDays / 365)} years ago`;
  }

  getSeverityColor(severity: string): string {
    switch (severity.toLowerCase()) {
      case 'critical': return '#f44336';
      case 'high': return '#ff9800';
      case 'medium': return '#fbc02d';
      case 'low': return '#4caf50';
      default: return '#757575';
    }
  }

  getPriorityColor(priority: string): string {
    switch (priority.toLowerCase()) {
      case 'critical': return '#f44336';
      case 'high': return '#ff9800';
      case 'medium': return '#fbc02d';
      case 'low': return '#4caf50';
      default: return '#757575';
    }
  }
}
