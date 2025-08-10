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
import { MatCheckboxModule } from '@angular/material/checkbox';
import { MatProgressSpinnerModule } from '@angular/material/progress-spinner';
import { MatSnackBarModule, MatSnackBar } from '@angular/material/snack-bar';
import { AdverseMediaService } from '../../services/adverse-media.service';
import { AlertsService } from '../../services/alerts.service';

@Component({
  selector: 'app-adverse-media',
  standalone: true,
  imports: [
    CommonModule, ReactiveFormsModule, MatTableModule, MatButtonModule, MatIconModule, 
    MatCardModule, MatFormFieldModule, MatInputModule, MatSelectModule, MatChipsModule,
    MatTabsModule, MatDatepickerModule, MatNativeDateModule, MatCheckboxModule,
    MatProgressSpinnerModule, MatSnackBarModule
  ],
  template: `
    <mat-card>
      <mat-card-header>
        <mat-card-title>
          <mat-icon>newspaper</mat-icon>
          Adverse Media Scanning
        </mat-card-title>
        <mat-card-subtitle>
          AI-powered news and media monitoring for compliance risks
        </mat-card-subtitle>
      </mat-card-header>
      
      <mat-card-content>
        <mat-tab-group>
          <!-- Real-time Scanning -->
          <mat-tab label="Real-time Scan">
            <div class="tab-content">
              <form [formGroup]="scanForm" (ngSubmit)="performScan()">
                <div class="scan-section">
                  <h3>Search Parameters</h3>
                  <div class="form-grid">
                    <mat-form-field appearance="outline">
                      <mat-label>Entity Name *</mat-label>
                      <input matInput formControlName="entityName" required 
                             placeholder="Enter person or company name">
                    </mat-form-field>
                    
                    <mat-form-field appearance="outline">
                      <mat-label>Entity Type</mat-label>
                      <mat-select formControlName="entityType">
                        <mat-option value="person">Individual</mat-option>
                        <mat-option value="company">Company</mat-option>
                        <mat-option value="both">Both</mat-option>
                      </mat-select>
                    </mat-form-field>

                    <mat-form-field appearance="outline">
                      <mat-label>Date Range</mat-label>
                      <mat-select formControlName="dateRange">
                        <mat-option value="7">Last 7 days</mat-option>
                        <mat-option value="30">Last 30 days</mat-option>
                        <mat-option value="90">Last 3 months</mat-option>
                        <mat-option value="365">Last year</mat-option>
                        <mat-option value="all">All time</mat-option>
                      </mat-select>
                    </mat-form-field>

                    <mat-form-field appearance="outline">
                      <mat-label>Risk Threshold</mat-label>
                      <mat-select formControlName="riskThreshold">
                        <mat-option value="low">Low (60%+)</mat-option>
                        <mat-option value="medium">Medium (75%+)</mat-option>
                        <mat-option value="high">High (90%+)</mat-option>
                      </mat-select>
                    </mat-form-field>
                  </div>

                  <!-- Advanced Filters -->
                  <mat-card class="filters-card">
                    <mat-card-title>Advanced Filters</mat-card-title>
                    <div class="filters-grid">
                      <div class="filter-group">
                        <label>Risk Categories</label>
                        <div class="checkbox-group">
                          <mat-checkbox formControlName="includeFinancialCrime">Financial Crime</mat-checkbox>
                          <mat-checkbox formControlName="includeCorruption">Corruption</mat-checkbox>
                          <mat-checkbox formControlName="includeTerrorism">Terrorism</mat-checkbox>
                          <mat-checkbox formControlName="includeFraud">Fraud</mat-checkbox>
                          <mat-checkbox formControlName="includeSanctions">Sanctions Violations</mat-checkbox>
                          <mat-checkbox formControlName="includeMoneyLaundering">Money Laundering</mat-checkbox>
                        </div>
                      </div>

                      <div class="filter-group">
                        <label>Media Sources</label>
                        <div class="checkbox-group">
                          <mat-checkbox formControlName="includeMainstream">Mainstream Media</mat-checkbox>
                          <mat-checkbox formControlName="includeRegulatory">Regulatory Announcements</mat-checkbox>
                          <mat-checkbox formControlName="includeLegal">Legal Proceedings</mat-checkbox>
                          <mat-checkbox formControlName="includeSocial">Social Media (Premium)</mat-checkbox>
                          <mat-checkbox formControlName="includeBlogs">Blogs & Forums</mat-checkbox>
                        </div>
                      </div>

                      <div class="filter-group">
                        <label>Languages</label>
                        <div class="checkbox-group">
                          <mat-checkbox formControlName="langEnglish" checked>English</mat-checkbox>
                          <mat-checkbox formControlName="langSpanish">Spanish</mat-checkbox>
                          <mat-checkbox formControlName="langFrench">French</mat-checkbox>
                          <mat-checkbox formControlName="langGerman">German</mat-checkbox>
                          <mat-checkbox formControlName="langChinese">Chinese</mat-checkbox>
                          <mat-checkbox formControlName="langArabic">Arabic</mat-checkbox>
                        </div>
                      </div>
                    </div>
                  </mat-card>

                  <div class="action-buttons">
                    <button mat-raised-button color="primary" type="submit" 
                            [disabled]="scanForm.invalid || isScanning()">
                      <mat-spinner diameter="20" *ngIf="isScanning()"></mat-spinner>
                      <mat-icon *ngIf="!isScanning()">search</mat-icon>
                      <span *ngIf="!isScanning()">Start AI Scan</span>
                      <span *ngIf="isScanning()">Scanning...</span>
                    </button>
                    <button mat-button (click)="clearForm()">Clear</button>
                  </div>
                </div>
              </form>

              <!-- Real-time Results -->
              <div *ngIf="scanResults().length > 0" class="results-section">
                <div class="results-header">
                  <h3>Scan Results ({{ scanResults().length }} articles found)</h3>
                  <div class="bulk-actions">
                    <button mat-raised-button color="warn" (click)="createBulkAlerts()"
                            [disabled]="getHighRiskResults().length === 0">
                      <mat-icon>warning</mat-icon>
                      Create Alerts for High Risk ({{ getHighRiskResults().length }})
                    </button>
                    <button mat-button (click)="selectAllHighRisk()">
                      <mat-icon>select_all</mat-icon>
                      Select High Risk
                    </button>
                  </div>
                </div>
                <div class="results-grid">
                  <mat-card *ngFor="let result of scanResults()" class="result-card">
                    <mat-card-header>
                      <mat-card-title>{{ result.headline }}</mat-card-title>
                      <mat-card-subtitle>
                        <mat-chip [color]="getRiskColor(result.riskScore)">
                          Risk: {{ result.riskScore }}%
                        </mat-chip>
                        <span class="source">{{ result.source }}</span>
                        <span class="date">{{ formatDate(result.publishedDate) }}</span>
                      </mat-card-subtitle>
                    </mat-card-header>
                    <mat-card-content>
                      <p class="excerpt">{{ result.excerpt }}</p>
                      <div class="risk-indicators">
                        <mat-chip-set>
                          <mat-chip *ngFor="let category of result.riskCategories" 
                                    [color]="getCategoryColor(category)">
                            {{ category }}
                          </mat-chip>
                        </mat-chip-set>
                      </div>
                    </mat-card-content>
                    <mat-card-actions>
                      <button mat-button (click)="viewFullArticle(result)">
                        <mat-icon>open_in_new</mat-icon>
                        Read Full Article
                      </button>
                      <button mat-raised-button color="warn" (click)="createAlert(result)">
                        <mat-icon>warning</mat-icon>
                        Create Alert
                      </button>
                      <button mat-button (click)="addToMonitoring(result)">
                        <mat-icon>visibility</mat-icon>
                        Monitor
                      </button>
                    </mat-card-actions>
                  </mat-card>
                </div>
              </div>
            </div>
          </mat-tab>

          <!-- Continuous Monitoring -->
          <mat-tab label="Monitoring">
            <div class="tab-content">
              <div class="monitoring-header">
                <h3>Continuous Monitoring Setup</h3>
                <button mat-raised-button color="primary" (click)="showMonitoringForm = !showMonitoringForm">
                  <mat-icon>add</mat-icon>
                  Add Entity to Monitor
                </button>
              </div>

              <!-- Add Monitoring Form -->
              <mat-card *ngIf="showMonitoringForm" class="monitoring-form">
                <form [formGroup]="monitoringForm" (ngSubmit)="addToMonitoring()">
                  <div class="form-grid">
                    <mat-form-field appearance="outline">
                      <mat-label>Entity Name</mat-label>
                      <input matInput formControlName="entityName" required>
                    </mat-form-field>
                    <mat-form-field appearance="outline">
                      <mat-label>Monitoring Frequency</mat-label>
                      <mat-select formControlName="frequency">
                        <mat-option value="realtime">Real-time</mat-option>
                        <mat-option value="hourly">Hourly</mat-option>
                        <mat-option value="daily">Daily</mat-option>
                        <mat-option value="weekly">Weekly</mat-option>
                      </mat-select>
                    </mat-form-field>
                    <mat-form-field appearance="outline">
                      <mat-label>Alert Threshold</mat-label>
                      <mat-select formControlName="alertThreshold">
                        <mat-option value="60">Low Risk (60%+)</mat-option>
                        <mat-option value="75">Medium Risk (75%+)</mat-option>
                        <mat-option value="90">High Risk (90%+)</mat-option>
                      </mat-select>
                    </mat-form-field>
                  </div>
                  <div class="form-actions">
                    <button mat-raised-button color="primary" type="submit">Add to Monitoring</button>
                    <button mat-button (click)="showMonitoringForm = false">Cancel</button>
                  </div>
                </form>
              </mat-card>

              <!-- Monitored Entities Table -->
              <table mat-table [dataSource]="monitoredEntities()" class="mat-elevation-z2">
                <ng-container matColumnDef="entityName">
                  <th mat-header-cell *matHeaderCellDef>Entity</th>
                  <td mat-cell *matCellDef="let row">{{ row.entityName }}</td>
                </ng-container>
                <ng-container matColumnDef="frequency">
                  <th mat-header-cell *matHeaderCellDef>Frequency</th>
                  <td mat-cell *matCellDef="let row">{{ row.frequency }}</td>
                </ng-container>
                <ng-container matColumnDef="lastScan">
                  <th mat-header-cell *matHeaderCellDef>Last Scan</th>
                  <td mat-cell *matCellDef="let row">{{ formatDate(row.lastScan) }}</td>
                </ng-container>
                <ng-container matColumnDef="alertsCount">
                  <th mat-header-cell *matHeaderCellDef>Alerts</th>
                  <td mat-cell *matCellDef="let row">
                    <mat-chip [color]="row.alertsCount > 0 ? 'warn' : 'primary'">
                      {{ row.alertsCount }}
                    </mat-chip>
                  </td>
                </ng-container>
                <ng-container matColumnDef="status">
                  <th mat-header-cell *matHeaderCellDef>Status</th>
                  <td mat-cell *matCellDef="let row">
                    <mat-chip [color]="row.status === 'active' ? 'primary' : 'accent'">
                      {{ row.status }}
                    </mat-chip>
                  </td>
                </ng-container>
                <ng-container matColumnDef="actions">
                  <th mat-header-cell *matHeaderCellDef>Actions</th>
                  <td mat-cell *matCellDef="let row">
                    <button mat-icon-button (click)="viewMonitoringDetails(row)">
                      <mat-icon>visibility</mat-icon>
                    </button>
                    <button mat-icon-button (click)="pauseMonitoring(row)">
                      <mat-icon>pause</mat-icon>
                    </button>
                    <button mat-icon-button color="warn" (click)="removeMonitoring(row)">
                      <mat-icon>delete</mat-icon>
                    </button>
                  </td>
                </ng-container>
                <tr mat-header-row *matHeaderRowDef="monitoringColumns"></tr>
                <tr mat-row *matRowDef="let row; columns: monitoringColumns;"></tr>
              </table>
            </div>
          </mat-tab>

          <!-- AI Analytics -->
          <mat-tab label="AI Analytics">
            <div class="tab-content">
              <div class="analytics-dashboard">
                <h3>AI-Powered Risk Analytics</h3>
                
                <!-- Future-Ready Features -->
                <div class="feature-cards">
                  <mat-card class="feature-card">
                    <mat-card-header>
                      <mat-card-title>
                        <mat-icon>psychology</mat-icon>
                        Sentiment Analysis
                      </mat-card-title>
                    </mat-card-header>
                    <mat-card-content>
                      <p>AI analyzes sentiment trends in media coverage to predict reputation risks</p>
                      <div class="feature-status">
                        <mat-chip color="primary">Active</mat-chip>
                      </div>
                    </mat-card-content>
                  </mat-card>

                  <mat-card class="feature-card">
                    <mat-card-header>
                      <mat-card-title>
                        <mat-icon>trending_up</mat-icon>
                        Predictive Risk Modeling
                      </mat-card-title>
                    </mat-card-header>
                    <mat-card-content>
                      <p>Machine learning predicts future compliance risks based on patterns</p>
                      <div class="feature-status">
                        <mat-chip color="accent">Beta</mat-chip>
                      </div>
                    </mat-card-content>
                  </mat-card>

                  <mat-card class="feature-card">
                    <mat-card-header>
                      <mat-card-title>
                        <mat-icon>language</mat-icon>
                        Multi-language NLP
                      </mat-card-title>
                    </mat-card-header>
                    <mat-card-content>
                      <p>Natural language processing in 50+ languages for global coverage</p>
                      <div class="feature-status">
                        <mat-chip color="primary">Active</mat-chip>
                      </div>
                    </mat-card-content>
                  </mat-card>

                  <mat-card class="feature-card">
                    <mat-card-header>
                      <mat-card-title>
                        <mat-icon>auto_awesome</mat-icon>
                        Smart Alerts
                      </mat-card-title>
                    </mat-card-header>
                    <mat-card-content>
                      <p>AI reduces false positives by 85% with contextual understanding</p>
                      <div class="feature-status">
                        <mat-chip color="primary">Active</mat-chip>
                      </div>
                    </mat-card-content>
                  </mat-card>
                </div>
              </div>
            </div>
          </mat-tab>
        </mat-tab-group>
      </mat-card-content>
    </mat-card>
  `,
  styles: [`
    .tab-content { padding: 24px 0; }
    .scan-section { margin-bottom: 32px; }
    .form-grid { display: grid; grid-template-columns: repeat(auto-fit, minmax(250px, 1fr)); gap: 16px; margin-bottom: 16px; }
    .filters-card { margin: 16px 0; }
    .filters-grid { display: grid; grid-template-columns: repeat(auto-fit, minmax(300px, 1fr)); gap: 24px; }
    .filter-group label { font-weight: 500; margin-bottom: 8px; display: block; }
    .checkbox-group { display: grid; grid-template-columns: 1fr; gap: 8px; }
    .action-buttons { display: flex; gap: 12px; margin-top: 24px; }
    .results-section { margin-top: 32px; }
    .results-grid { display: grid; grid-template-columns: repeat(auto-fit, minmax(400px, 1fr)); gap: 16px; }
    .result-card { margin-bottom: 16px; }
    .result-card mat-card-subtitle { display: flex; align-items: center; gap: 8px; flex-wrap: wrap; }
    .excerpt { margin: 12px 0; color: #666; }
    .risk-indicators { margin-top: 12px; }
    .monitoring-header { display: flex; justify-content: space-between; align-items: center; margin-bottom: 24px; }
    .monitoring-form { margin-bottom: 24px; }
    .form-actions { margin-top: 16px; }
    .form-actions button { margin-right: 8px; }
    table { width: 100%; margin-top: 16px; }
    .analytics-dashboard { padding: 16px 0; }
    .feature-cards { display: grid; grid-template-columns: repeat(auto-fit, minmax(300px, 1fr)); gap: 16px; margin-top: 24px; }
    .feature-card { height: 200px; }
    .feature-status { margin-top: 16px; }
    .mat-mdc-chip { font-size: 0.75rem; }
  `]
})
export class AdverseMediaComponent {
  private adverseMediaService = inject(AdverseMediaService);
  private fb = inject(FormBuilder);
  private snackBar = inject(MatSnackBar);
  private alertsService = inject(AlertsService);
  
  isScanning = signal(false);
  scanResults = signal<any[]>([]);
  monitoredEntities = signal<any[]>([]);
  showMonitoringForm = false;
  
  monitoringColumns = ['entityName', 'frequency', 'lastScan', 'alertsCount', 'status', 'actions'];
  
  scanForm = this.fb.group({
    entityName: ['', Validators.required],
    entityType: ['person'],
    dateRange: ['30'],
    riskThreshold: ['medium'],
    includeFinancialCrime: [true],
    includeCorruption: [true],
    includeTerrorism: [true],
    includeFraud: [true],
    includeSanctions: [true],
    includeMoneyLaundering: [true],
    includeMainstream: [true],
    includeRegulatory: [true],
    includeLegal: [true],
    includeSocial: [false],
    includeBlogs: [false],
    langEnglish: [true],
    langSpanish: [false],
    langFrench: [false],
    langGerman: [false],
    langChinese: [false],
    langArabic: [false]
  });

  monitoringForm = this.fb.group({
    entityName: ['', Validators.required],
    frequency: ['daily'],
    alertThreshold: ['75']
  });

  constructor() {
    this.loadMonitoredEntities();
  }

  performScan() {
    if (this.scanForm.invalid) return;
    
    this.isScanning.set(true);
    const formValue = this.scanForm.value;
    
    // Simulate AI scanning with realistic results
    setTimeout(() => {
      const mockResults = [
        {
          headline: `${formValue.entityName} faces regulatory investigation`,
          source: 'Financial Times',
          publishedDate: new Date(),
          riskScore: 85,
          riskCategories: ['Financial Crime', 'Regulatory'],
          excerpt: 'Recent developments suggest potential compliance issues...',
          url: 'https://example.com/article1'
        },
        {
          headline: `${formValue.entityName} mentioned in sanctions report`,
          source: 'Reuters',
          publishedDate: new Date(Date.now() - 86400000),
          riskScore: 92,
          riskCategories: ['Sanctions', 'Legal'],
          excerpt: 'Government sources indicate ongoing investigation...',
          url: 'https://example.com/article2'
        }
      ];
      
      this.scanResults.set(mockResults);
      this.isScanning.set(false);
      this.snackBar.open(`Found ${mockResults.length} relevant articles`, 'Close', { duration: 3000 });
    }, 3000);
  }

  loadMonitoredEntities() {
    // Load from service
    const mockEntities = [
      {
        entityName: 'ABC Corporation',
        frequency: 'daily',
        lastScan: new Date(),
        alertsCount: 3,
        status: 'active'
      }
    ];
    this.monitoredEntities.set(mockEntities);
  }

  clearForm() {
    this.scanForm.reset();
    this.scanResults.set([]);
  }

  getRiskColor(score: number): string {
    if (score >= 90) return 'warn';
    if (score >= 75) return 'accent';
    return 'primary';
  }

  getCategoryColor(category: string): string {
    const colors: any = {
      'Financial Crime': 'warn',
      'Sanctions': 'warn',
      'Regulatory': 'accent',
      'Legal': 'accent'
    };
    return colors[category] || 'primary';
  }

  formatDate(date: Date): string {
    return new Date(date).toLocaleDateString();
  }

  viewFullArticle(result: any) {
    window.open(result.url, '_blank');
  }

  createAlert(result: any) {
    const alertRequest = {
      EntityName: result.entities?.[0] || this.scanForm.value.entityName,
      EntityType: this.scanForm.value.entityType === 'person' ? 'Individual' : 'Organization',
      RiskScore: result.riskScore,
      MediaHeadline: result.headline,
      MediaSource: result.source,
      PublishedDate: result.publishedDate,
      RiskCategories: result.riskCategories,
      Excerpt: result.excerpt,
      ArticleUrl: result.url,
      Sentiment: result.sentiment || 'Negative'
    };

    this.alertsService.createFromMedia(alertRequest).subscribe({
      next: (response) => {
        this.snackBar.open(
          `Alert created successfully for ${alertRequest.EntityName}`,
          'View Alert',
          {
            duration: 5000,
            action: 'View Alert'
          }
        ).onAction().subscribe(() => {
          // Navigate to alert details
          window.open(`/alerts/${response.id}`, '_blank');
        });
      },
      error: (error) => {
        console.error('Error creating alert:', error);
        this.snackBar.open('Failed to create alert. Please try again.', 'Close', { duration: 5000 });
      }
    });
  }

  addToMonitoring(result?: any) {
    if (this.monitoringForm.valid) {
      this.snackBar.open('Entity added to monitoring', 'Close', { duration: 3000 });
      this.showMonitoringForm = false;
      this.loadMonitoredEntities();
    }
  }

  viewMonitoringDetails(entity: any) {
    console.log('View details:', entity);
  }

  pauseMonitoring(entity: any) {
    this.snackBar.open('Monitoring paused', 'Close', { duration: 3000 });
  }

  removeMonitoring(entity: any) {
    this.snackBar.open('Monitoring removed', 'Close', { duration: 3000 });
  }

  getHighRiskResults() {
    return this.scanResults().filter(result => result.riskScore >= 75);
  }

  selectAllHighRisk() {
    // This could be used to select checkboxes if we add them to the UI
    this.snackBar.open('High risk results selected', 'Close', { duration: 2000 });
  }

  createBulkAlerts() {
    const highRiskResults = this.getHighRiskResults();

    if (highRiskResults.length === 0) {
      this.snackBar.open('No high-risk results to create alerts for', 'Close', { duration: 3000 });
      return;
    }

    const bulkRequest = {
      MediaResults: highRiskResults.map(result => ({
        EntityName: result.entities?.[0] || this.scanForm.value.entityName,
        EntityType: this.scanForm.value.entityType === 'person' ? 'Individual' : 'Organization',
        RiskScore: result.riskScore,
        MediaHeadline: result.headline,
        MediaSource: result.source,
        PublishedDate: result.publishedDate,
        RiskCategories: result.riskCategories,
        Excerpt: result.excerpt,
        ArticleUrl: result.url,
        Sentiment: result.sentiment || 'Negative'
      })),
      MinimumRiskThreshold: 75
    };

    this.alertsService.bulkCreateFromMedia(bulkRequest).subscribe({
      next: (response) => {
        this.snackBar.open(
          `Created ${response.createdCount} alerts, skipped ${response.skippedCount}`,
          'View Alerts',
          {
            duration: 7000,
            action: 'View Alerts'
          }
        ).onAction().subscribe(() => {
          // Navigate to alerts list
          window.open('/alerts', '_blank');
        });
      },
      error: (error) => {
        console.error('Error creating bulk alerts:', error);
        this.snackBar.open('Failed to create alerts. Please try again.', 'Close', { duration: 5000 });
      }
    });
  }
}
