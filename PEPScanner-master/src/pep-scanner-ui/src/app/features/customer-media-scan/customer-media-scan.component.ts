import { Component, inject, signal, OnInit, OnDestroy } from '@angular/core';
import { CommonModule } from '@angular/common';
import { ReactiveFormsModule } from '@angular/forms';
import { MatTableModule } from '@angular/material/table';
import { MatButtonModule } from '@angular/material/button';
import { MatIconModule } from '@angular/material/icon';
import { MatCardModule } from '@angular/material/card';
import { MatFormFieldModule } from '@angular/material/form-field';
import { MatInputModule } from '@angular/material/input';
import { MatSelectModule } from '@angular/material/select';
import { MatChipsModule } from '@angular/material/chips';
import { MatTabsModule } from '@angular/material/tabs';
import { MatCheckboxModule } from '@angular/material/checkbox';
import { MatProgressSpinnerModule } from '@angular/material/progress-spinner';
import { MatProgressBarModule } from '@angular/material/progress-bar';
import { MatSnackBarModule, MatSnackBar } from '@angular/material/snack-bar';
import { MatBadgeModule } from '@angular/material/badge';
import { MatTooltipModule } from '@angular/material/tooltip';
import { MatExpansionModule } from '@angular/material/expansion';
import { CustomerMediaScanService } from '../../services/customer-media-scan.service';
import { AIRiskScoringService } from '../../services/ai-risk-scoring.service';
import { RealTimeNotificationService } from '../../services/real-time-notification.service';

@Component({
  selector: 'app-customer-media-scan',
  standalone: true,
  imports: [
    CommonModule, ReactiveFormsModule, MatTableModule, MatButtonModule, MatIconModule,
    MatCardModule, MatFormFieldModule, MatInputModule, MatSelectModule, MatChipsModule,
    MatTabsModule, MatCheckboxModule, MatProgressSpinnerModule, MatProgressBarModule,
    MatSnackBarModule, MatBadgeModule, MatTooltipModule, MatExpansionModule
  ],
  template: `
    <mat-card>
      <mat-card-header>
        <mat-card-title>
          <mat-icon>scanner</mat-icon>
          Customer Media Scanning
        </mat-card-title>
        <mat-card-subtitle>
          Periodic adverse media monitoring for all customers
        </mat-card-subtitle>
      </mat-card-header>
      
      <mat-card-content>
        <mat-tab-group>
          <!-- Scan Status Overview -->
          <mat-tab label="Overview">
            <div class="tab-content">
              <div class="overview-cards">
                <mat-card class="stat-card">
                  <mat-card-content>
                    <div class="stat-number">{{ scanStatus()?.summary?.totalCustomers || 0 }}</div>
                    <div class="stat-label">Total Customers</div>
                  </mat-card-content>
                </mat-card>

                <mat-card class="stat-card ai-insights">
                  <mat-card-content>
                    <div class="stat-number">{{ aiRiskAssessments().size }}</div>
                    <div class="stat-label">AI Risk Scores</div>
                    <button mat-icon-button (click)="refreshAIRiskScores()" matTooltip="Refresh AI Risk Scores">
                      <mat-icon>refresh</mat-icon>
                    </button>
                  </mat-card-content>
                </mat-card>

                <mat-card class="stat-card warning" *ngIf="scanStatus()?.summary?.requiresRescan > 0">
                  <mat-card-content>
                    <div class="stat-number">{{ scanStatus()?.summary?.requiresRescan || 0 }}</div>
                    <div class="stat-label">Requires Rescan</div>
                  </mat-card-content>
                </mat-card>

                <mat-card class="stat-card danger" *ngIf="scanStatus()?.summary?.highRisk > 0">
                  <mat-card-content>
                    <div class="stat-number">{{ scanStatus()?.summary?.highRisk || 0 }}</div>
                    <div class="stat-label">High Risk</div>
                  </mat-card-content>
                </mat-card>

                <mat-card class="stat-card">
                  <mat-card-content>
                    <div class="stat-number">{{ scanStatus()?.summary?.neverScanned || 0 }}</div>
                    <div class="stat-label">Never Scanned</div>
                  </mat-card-content>
                </mat-card>
              </div>

              <!-- Quick Actions -->
              <mat-card class="actions-card">
                <mat-card-title>Quick Actions</mat-card-title>
                <mat-card-content>
                  <div class="action-buttons">
                    <button mat-raised-button color="primary" (click)="scanHighRiskCustomers()" 
                            [disabled]="isScanning()">
                      <mat-spinner diameter="20" *ngIf="isScanning()"></mat-spinner>
                      <mat-icon *ngIf="!isScanning()">priority_high</mat-icon>
                      Scan High Risk Customers
                    </button>
                    
                    <button mat-raised-button color="accent" (click)="scanAllCustomers()" 
                            [disabled]="isScanning()">
                      <mat-spinner diameter="20" *ngIf="isScanning()"></mat-spinner>
                      <mat-icon *ngIf="!isScanning()">group</mat-icon>
                      Scan All Customers
                    </button>
                    
                    <button mat-button (click)="setupPeriodicScans()" 
                            [disabled]="isScanning()">
                      <mat-icon>schedule</mat-icon>
                      Setup Periodic Scans
                    </button>
                    
                    <button mat-button (click)="refreshStatus()">
                      <mat-icon>refresh</mat-icon>
                      Refresh Status
                    </button>
                  </div>
                </mat-card-content>
              </mat-card>

              <!-- Scan Progress -->
              <mat-card *ngIf="currentScan()" class="progress-card">
                <mat-card-title>Scan in Progress</mat-card-title>
                <mat-card-content>
                  <div class="progress-info">
                    <p><strong>Type:</strong> {{ currentScan()?.type }}</p>
                    <p><strong>Started:</strong> {{ formatDate(currentScan()?.startTime) }}</p>
                    <p><strong>Status:</strong> {{ currentScan()?.status }}</p>
                  </div>
                  <mat-progress-bar mode="indeterminate" color="primary"></mat-progress-bar>
                </mat-card-content>
              </mat-card>
            </div>
          </mat-tab>

          <!-- Customer Status -->
          <mat-tab label="Customer Status">
            <div class="tab-content">
              <div class="table-header">
                <h3>Customer Scan Status</h3>
                <div class="table-actions">
                  <mat-form-field appearance="outline">
                    <mat-label>Filter by Risk Level</mat-label>
                    <mat-select [(value)]="selectedRiskFilter" (selectionChange)="applyFilter()">
                      <mat-option value="">All</mat-option>
                      <mat-option value="High">High Risk</mat-option>
                      <mat-option value="Medium">Medium Risk</mat-option>
                      <mat-option value="Low">Low Risk</mat-option>
                    </mat-select>
                  </mat-form-field>
                  
                  <button mat-button (click)="scanSelectedCustomers()" 
                          [disabled]="selectedCustomers().length === 0 || isScanning()">
                    <mat-icon>play_arrow</mat-icon>
                    Scan Selected ({{ selectedCustomers().length }})
                  </button>
                </div>
              </div>

              <table mat-table [dataSource]="filteredCustomers()" class="mat-elevation-z2">
                <ng-container matColumnDef="select">
                  <th mat-header-cell *matHeaderCellDef>
                    <mat-checkbox (change)="toggleAllSelection($event)" 
                                  [checked]="isAllSelected()"
                                  [indeterminate]="isPartiallySelected()">
                    </mat-checkbox>
                  </th>
                  <td mat-cell *matCellDef="let row">
                    <mat-checkbox (change)="toggleCustomerSelection(row, $event)"
                                  [checked]="isCustomerSelected(row)">
                    </mat-checkbox>
                  </td>
                </ng-container>

                <ng-container matColumnDef="customerName">
                  <th mat-header-cell *matHeaderCellDef>Customer</th>
                  <td mat-cell *matCellDef="let row">{{ row.customerName }}</td>
                </ng-container>

                <ng-container matColumnDef="riskLevel">
                  <th mat-header-cell *matHeaderCellDef>Risk Level</th>
                  <td mat-cell *matCellDef="let row">
                    <mat-chip [color]="getRiskColor(row.riskLevel)">
                      {{ row.riskLevel }}
                    </mat-chip>
                  </td>
                </ng-container>

                <ng-container matColumnDef="aiRiskScore">
                  <th mat-header-cell *matHeaderCellDef>
                    AI Risk Score
                    <mat-icon matTooltip="AI-powered risk assessment">psychology</mat-icon>
                  </th>
                  <td mat-cell *matCellDef="let row">
                    <div class="ai-risk-cell">
                      <span class="risk-score" [style.color]="getAIRiskColor(row.customerId)">
                        {{ getAIRiskScore(row.customerId).toFixed(1) }}
                      </span>
                      <mat-icon
                        class="trend-icon"
                        [style.color]="getRiskTrendColor(row.customerId)"
                        [matTooltip]="'Risk trend: ' + (aiRiskAssessments().get(row.customerId)?.riskTrend || 'stable')">
                        {{ getRiskTrendIcon(row.customerId) }}
                      </mat-icon>
                      <button mat-icon-button
                              (click)="viewAIInsights(row.customerId)"
                              matTooltip="View AI Insights"
                              class="insights-btn">
                        <mat-icon>insights</mat-icon>
                      </button>
                    </div>
                  </td>
                </ng-container>

                <ng-container matColumnDef="lastScanDate">
                  <th mat-header-cell *matHeaderCellDef>Last Scan</th>
                  <td mat-cell *matCellDef="let row">
                    {{ row.lastScanDate ? formatDate(row.lastScanDate) : 'Never' }}
                  </td>
                </ng-container>

                <ng-container matColumnDef="daysSinceLastScan">
                  <th mat-header-cell *matHeaderCellDef>Days Since Scan</th>
                  <td mat-cell *matCellDef="let row">
                    <span [class]="getDaysClass(row.daysSinceLastScan)">
                      {{ row.daysSinceLastScan || 'N/A' }}
                    </span>
                  </td>
                </ng-container>

                <ng-container matColumnDef="requiresRescan">
                  <th mat-header-cell *matHeaderCellDef>Status</th>
                  <td mat-cell *matCellDef="let row">
                    <mat-chip [color]="row.requiresRescan ? 'warn' : 'primary'">
                      {{ row.requiresRescan ? 'Needs Scan' : 'Up to Date' }}
                    </mat-chip>
                  </td>
                </ng-container>

                <ng-container matColumnDef="actions">
                  <th mat-header-cell *matHeaderCellDef>Actions</th>
                  <td mat-cell *matCellDef="let row">
                    <button mat-icon-button
                            (click)="scanSingleCustomerWithAI(row.customerId)"
                            [disabled]="isScanning()"
                            matTooltip="Scan with AI Risk Assessment"
                            color="primary">
                      <mat-icon>psychology</mat-icon>
                    </button>
                    <button mat-icon-button
                            (click)="scanSingleCustomer(row.customerId)"
                            [disabled]="isScanning()"
                            matTooltip="Regular Scan">
                      <mat-icon>play_arrow</mat-icon>
                    </button>
                    <button mat-icon-button
                            (click)="viewCustomerDetails(row.customerId)"
                            matTooltip="View Customer Details">
                      <mat-icon>visibility</mat-icon>
                    </button>
                  </td>
                </ng-container>

                <tr mat-header-row *matHeaderRowDef="customerColumns"></tr>
                <tr mat-row *matRowDef="let row; columns: customerColumns;"></tr>
              </table>
            </div>
          </mat-tab>

          <!-- Scheduled Scans -->
          <mat-tab label="Scheduled Scans">
            <div class="tab-content">
              <mat-card class="schedule-card">
                <mat-card-title>Periodic Scan Schedule</mat-card-title>
                <mat-card-content>
                  <div class="schedule-list">
                    <div class="schedule-item">
                      <div class="schedule-info">
                        <h4>Daily High-Risk Scan</h4>
                        <p>Scans high and medium risk customers daily</p>
                        <small>Schedule: Daily at 2:00 AM UTC</small>
                      </div>
                      <div class="schedule-status">
                        <mat-chip color="primary">Active</mat-chip>
                      </div>
                    </div>

                    <div class="schedule-item">
                      <div class="schedule-info">
                        <h4>Weekly All Customers Scan</h4>
                        <p>Comprehensive scan of all active customers</p>
                        <small>Schedule: Weekly on Sunday at 3:00 AM UTC</small>
                      </div>
                      <div class="schedule-status">
                        <mat-chip color="primary">Active</mat-chip>
                      </div>
                    </div>

                    <div class="schedule-item">
                      <div class="schedule-info">
                        <h4>Monthly Dormant Scan</h4>
                        <p>Scans customers not scanned in 90+ days</p>
                        <small>Schedule: Monthly on 1st at 4:00 AM UTC</small>
                      </div>
                      <div class="schedule-status">
                        <mat-chip color="primary">Active</mat-chip>
                      </div>
                    </div>
                  </div>

                  <div class="schedule-actions">
                    <button mat-raised-button color="primary" (click)="setupPeriodicScans()">
                      <mat-icon>schedule</mat-icon>
                      Configure Schedules
                    </button>
                    <button mat-button (click)="viewScheduleStatus()">
                      <mat-icon>info</mat-icon>
                      View Schedule Details
                    </button>
                  </div>
                </mat-card-content>
              </mat-card>
            </div>
          </mat-tab>
          <!-- Real-time Notifications -->
          <mat-tab label="Notifications" [matBadge]="unreadNotifications()" matBadgeColor="warn">
            <div class="tab-content">
              <div class="notifications-header">
                <h3>Real-time Notifications</h3>
                <div class="notification-actions">
                  <button mat-button (click)="markAllNotificationsAsRead()"
                          [disabled]="unreadNotifications() === 0">
                    <mat-icon>mark_email_read</mat-icon>
                    Mark All Read
                  </button>
                  <button mat-button (click)="clearAllNotifications()">
                    <mat-icon>clear_all</mat-icon>
                    Clear All
                  </button>
                </div>
              </div>

              <div class="notifications-list" *ngIf="notifications().length > 0; else noNotifications">
                <mat-card *ngFor="let notification of notifications()"
                          class="notification-card"
                          [class.unread]="!notification.isRead">
                  <mat-card-header>
                    <mat-icon mat-card-avatar [style.color]="getNotificationColor(notification.priority)">
                      {{ getNotificationIcon(notification.type) }}
                    </mat-icon>
                    <mat-card-title>{{ notification.title }}</mat-card-title>
                    <mat-card-subtitle>
                      {{ formatDate(notification.timestamp) }} • {{ notification.priority }}
                    </mat-card-subtitle>
                  </mat-card-header>
                  <mat-card-content>
                    <p>{{ notification.message }}</p>
                  </mat-card-content>
                  <mat-card-actions *ngIf="!notification.isRead">
                    <button mat-button (click)="markNotificationAsRead(notification.id)">
                      Mark as Read
                    </button>
                  </mat-card-actions>
                </mat-card>
              </div>

              <ng-template #noNotifications>
                <div class="no-notifications">
                  <mat-icon>notifications_none</mat-icon>
                  <p>No notifications</p>
                </div>
              </ng-template>
            </div>
          </mat-tab>

          <!-- AI Insights Panel -->
          <mat-tab label="AI Insights" *ngIf="showAIInsights()">
            <div class="tab-content" *ngIf="getSelectedCustomerAssessment() as assessment">
              <div class="ai-insights-header">
                <h3>AI Risk Assessment for {{ assessment.customerName }}</h3>
                <button mat-icon-button (click)="closeAIInsights()">
                  <mat-icon>close</mat-icon>
                </button>
              </div>

              <div class="risk-overview">
                <mat-card class="risk-score-card">
                  <mat-card-content>
                    <div class="risk-score-display">
                      <span class="score" [style.color]="getAIRiskColor(assessment.customerId)">
                        {{ assessment.riskScore.toFixed(1) }}
                      </span>
                      <span class="level">{{ getAIRiskLevel(assessment.customerId) }}</span>
                    </div>
                    <div class="confidence">
                      Confidence: {{ assessment.confidenceLevel.toFixed(1) }}%
                    </div>
                  </mat-card-content>
                </mat-card>

                <mat-card class="trend-card">
                  <mat-card-content>
                    <div class="trend-display">
                      <mat-icon [style.color]="getRiskTrendColor(assessment.customerId)">
                        {{ getRiskTrendIcon(assessment.customerId) }}
                      </mat-icon>
                      <span>{{ assessment.riskTrend | titlecase }}</span>
                    </div>
                  </mat-card-content>
                </mat-card>
              </div>

              <mat-expansion-panel class="risk-factors-panel">
                <mat-expansion-panel-header>
                  <mat-panel-title>Risk Factors ({{ assessment.riskFactors.length }})</mat-panel-title>
                </mat-expansion-panel-header>
                <div class="risk-factors-list">
                  <mat-card *ngFor="let factor of assessment.riskFactors" class="factor-card">
                    <mat-card-content>
                      <div class="factor-header">
                        <span class="category">{{ factor.category | titlecase }}</span>
                        <span class="weight">Weight: {{ (factor.weight * 100).toFixed(0) }}%</span>
                      </div>
                      <p class="description">{{ factor.description }}</p>
                    </mat-card-content>
                  </mat-card>
                </div>
              </mat-expansion-panel>

              <mat-expansion-panel class="recommendations-panel">
                <mat-expansion-panel-header>
                  <mat-panel-title>Recommended Actions ({{ assessment.recommendedActions.length }})</mat-panel-title>
                </mat-expansion-panel-header>
                <div class="recommendations-list">
                  <mat-card *ngFor="let action of assessment.recommendedActions" class="action-card">
                    <mat-card-content>
                      <div class="action-header">
                        <mat-chip [style.background-color]="aiRiskService.getPriorityColor(action.priority)">
                          {{ action.priority }}
                        </mat-chip>
                        <span class="timeline">{{ action.timeline }}</span>
                      </div>
                      <h4>{{ action.action }}</h4>
                      <p class="reason">{{ action.reason }}</p>
                      <p class="assign-to" *ngIf="action.assignTo">Assign to: {{ action.assignTo }}</p>
                    </mat-card-content>
                  </mat-card>
                </div>
              </mat-expansion-panel>
            </div>
          </mat-tab>
        </mat-tab-group>
      </mat-card-content>
    </mat-card>
  `,
  styles: [`
    .tab-content { padding: 24px 0; }
    .overview-cards { display: grid; grid-template-columns: repeat(auto-fit, minmax(200px, 1fr)); gap: 16px; margin-bottom: 24px; }
    .stat-card { text-align: center; }
    .stat-card.warning { border-left: 4px solid #ff9800; }
    .stat-card.danger { border-left: 4px solid #f44336; }
    .stat-number { font-size: 2rem; font-weight: bold; color: #1976d2; }
    .stat-label { color: #666; margin-top: 8px; }
    .actions-card { margin-bottom: 24px; }
    .action-buttons { display: flex; gap: 12px; flex-wrap: wrap; }
    .progress-card { margin-bottom: 24px; }
    .progress-info { margin-bottom: 16px; }
    .table-header { display: flex; justify-content: space-between; align-items: center; margin-bottom: 16px; }
    .table-actions { display: flex; gap: 12px; align-items: center; }
    table { width: 100%; }
    .days-overdue { color: #f44336; font-weight: bold; }
    .days-warning { color: #ff9800; }
    .days-ok { color: #4caf50; }
    .schedule-card { margin-bottom: 24px; }
    .schedule-list { margin-bottom: 24px; }
    .schedule-item { display: flex; justify-content: space-between; align-items: center; padding: 16px; border: 1px solid #e0e0e0; border-radius: 4px; margin-bottom: 12px; }
    .schedule-info h4 { margin: 0 0 8px 0; }
    .schedule-info p { margin: 0 0 4px 0; color: #666; }
    .schedule-info small { color: #999; }
    .schedule-actions { display: flex; gap: 12px; }
    .mat-mdc-chip { font-size: 0.75rem; }

    /* AI Risk Scoring Styles */
    .ai-insights { border-left: 4px solid #9c27b0; }
    .ai-risk-cell { display: flex; align-items: center; gap: 8px; }
    .risk-score { font-weight: bold; font-size: 1.1rem; }
    .trend-icon { font-size: 1rem; }
    .insights-btn { color: #9c27b0; }

    .ai-insights-header { display: flex; justify-content: space-between; align-items: center; margin-bottom: 24px; }
    .risk-overview { display: grid; grid-template-columns: 1fr 1fr; gap: 16px; margin-bottom: 24px; }
    .risk-score-card { text-align: center; }
    .risk-score-display { margin-bottom: 8px; }
    .score { font-size: 2.5rem; font-weight: bold; display: block; }
    .level { font-size: 1.2rem; color: #666; }
    .confidence { color: #999; }
    .trend-card { text-align: center; }
    .trend-display { display: flex; align-items: center; justify-content: center; gap: 8px; font-size: 1.2rem; }

    .risk-factors-panel, .recommendations-panel { margin-bottom: 16px; }
    .risk-factors-list, .recommendations-list { display: grid; gap: 12px; }
    .factor-card, .action-card { margin-bottom: 8px; }
    .factor-header, .action-header { display: flex; justify-content: space-between; align-items: center; margin-bottom: 8px; }
    .category, .timeline { font-size: 0.9rem; color: #666; }
    .weight { font-weight: bold; }
    .description, .reason { margin: 8px 0; }
    .assign-to { font-style: italic; color: #666; }

    /* Real-time Notifications Styles */
    .notifications-header { display: flex; justify-content: space-between; align-items: center; margin-bottom: 16px; }
    .notification-actions { display: flex; gap: 8px; }
    .notifications-list { max-height: 500px; overflow-y: auto; }
    .notification-card { margin-bottom: 12px; }
    .notification-card.unread { border-left: 4px solid #2196f3; background-color: #f3f9ff; }
    .no-notifications { text-align: center; padding: 40px; color: #999; }
    .no-notifications mat-icon { font-size: 48px; margin-bottom: 16px; }

    /* Enhanced Progress Styles */
    .bulk-scan-progress { margin: 16px 0; }
    .progress-details { display: grid; grid-template-columns: repeat(auto-fit, minmax(150px, 1fr)); gap: 16px; margin-bottom: 16px; }
    .progress-stat { text-align: center; }
    .progress-number { font-size: 1.5rem; font-weight: bold; color: #1976d2; }
    .progress-label { color: #666; font-size: 0.9rem; }
  `]
})
export class CustomerMediaScanComponent implements OnInit, OnDestroy {
  private scanService = inject(CustomerMediaScanService);
  public aiRiskService = inject(AIRiskScoringService);
  private notificationService = inject(RealTimeNotificationService);
  private snackBar = inject(MatSnackBar);

  scanStatus = signal<any>(null);
  currentScan = signal<any>(null);
  isScanning = signal(false);
  selectedCustomers = signal<string[]>([]);
  selectedRiskFilter = '';

  // AI Risk Scoring signals
  aiRiskAssessments = signal<Map<string, any>>(new Map());
  showAIInsights = signal(false);
  selectedCustomerForAI = signal<string | null>(null);

  // Real-time notifications
  notifications = signal<any[]>([]);
  unreadNotifications = signal(0);
  bulkScanProgress = signal<any>(null);

  customerColumns = ['select', 'customerName', 'riskLevel', 'aiRiskScore', 'lastScanDate', 'daysSinceLastScan', 'requiresRescan', 'actions'];

  ngOnInit() {
    this.loadScanStatus();
    this.setupRealTimeNotifications();
    this.loadAIRiskAssessments();
  }

  ngOnDestroy() {
    this.notificationService.disconnect();
  }

  setupRealTimeNotifications() {
    // Subscribe to notifications
    this.notificationService.notifications$.subscribe(notifications => {
      this.notifications.set(notifications);
      this.unreadNotifications.set(this.notificationService.getUnreadCount());
    });

    // Subscribe to bulk scan progress
    this.notificationService.bulkScanProgress$.subscribe(progress => {
      this.bulkScanProgress.set(progress);
    });

    // Join user group for notifications
    this.notificationService.joinUserGroup();
  }

  loadAIRiskAssessments() {
    const customers = this.scanStatus()?.customers || [];
    if (customers.length === 0) return;

    // Load AI risk assessments for visible customers
    const customerIds = customers.slice(0, 10).map((c: any) => c.customerId); // Limit to first 10 for performance

    this.aiRiskService.batchCalculateRiskScores(customerIds).subscribe({
      next: (assessments) => {
        const assessmentMap = new Map();
        assessments.forEach(assessment => {
          assessmentMap.set(assessment.customerId, assessment);
        });
        this.aiRiskAssessments.set(assessmentMap);
      },
      error: (error) => {
        console.error('Error loading AI risk assessments:', error);
      }
    });
  }

  loadScanStatus() {
    this.scanService.getScanStatus().subscribe({
      next: (status) => {
        this.scanStatus.set(status);
      },
      error: (error) => {
        console.error('Error loading scan status:', error);
        this.snackBar.open('Failed to load scan status', 'Close', { duration: 5000 });
      }
    });
  }

  filteredCustomers() {
    const customers = this.scanStatus()?.customers || [];
    if (!this.selectedRiskFilter) return customers;
    return customers.filter((c: any) => c.riskLevel === this.selectedRiskFilter);
  }

  scanHighRiskCustomers() {
    this.isScanning.set(true);
    this.currentScan.set({ type: 'High Risk Customers', startTime: new Date(), status: 'Running' });
    
    this.scanService.scanHighRiskCustomers().subscribe({
      next: (result) => {
        this.isScanning.set(false);
        this.currentScan.set(null);
        this.snackBar.open(
          `High-risk scan completed: ${result.data.successfulScans} successful, ${result.data.totalAlertsCreated} alerts created`, 
          'Close', 
          { duration: 7000 }
        );
        this.loadScanStatus();
      },
      error: (error) => {
        this.isScanning.set(false);
        this.currentScan.set(null);
        console.error('Error scanning high-risk customers:', error);
        this.snackBar.open('Failed to scan high-risk customers', 'Close', { duration: 5000 });
      }
    });
  }

  scanAllCustomers() {
    this.isScanning.set(true);
    this.currentScan.set({ type: 'All Customers', startTime: new Date(), status: 'Running' });
    
    this.scanService.scanAllCustomers().subscribe({
      next: (result) => {
        this.isScanning.set(false);
        this.currentScan.set(null);
        this.snackBar.open(
          `All customers scan completed: ${result.data.successfulScans} successful, ${result.data.totalAlertsCreated} alerts created`, 
          'Close', 
          { duration: 7000 }
        );
        this.loadScanStatus();
      },
      error: (error) => {
        this.isScanning.set(false);
        this.currentScan.set(null);
        console.error('Error scanning all customers:', error);
        this.snackBar.open('Failed to scan all customers', 'Close', { duration: 5000 });
      }
    });
  }

  scanSingleCustomer(customerId: string) {
    this.scanService.scanCustomer(customerId).subscribe({
      next: (result) => {
        this.snackBar.open(
          `Customer scan completed: ${result.data.mediaResultsFound} results, ${result.data.alertsCreated} alerts created`, 
          'Close', 
          { duration: 5000 }
        );
        this.loadScanStatus();
      },
      error: (error) => {
        console.error('Error scanning customer:', error);
        this.snackBar.open('Failed to scan customer', 'Close', { duration: 5000 });
      }
    });
  }

  scanSelectedCustomers() {
    const customerIds = this.selectedCustomers();
    if (customerIds.length === 0) return;

    this.isScanning.set(true);
    this.currentScan.set({ type: `Selected Customers (${customerIds.length})`, startTime: new Date(), status: 'Running' });
    
    this.scanService.scanCustomersBatch(customerIds).subscribe({
      next: (result) => {
        this.isScanning.set(false);
        this.currentScan.set(null);
        this.snackBar.open(
          `Batch scan completed: ${result.data.successfulScans} successful, ${result.data.totalAlertsCreated} alerts created`, 
          'Close', 
          { duration: 7000 }
        );
        this.selectedCustomers.set([]);
        this.loadScanStatus();
      },
      error: (error) => {
        this.isScanning.set(false);
        this.currentScan.set(null);
        console.error('Error scanning selected customers:', error);
        this.snackBar.open('Failed to scan selected customers', 'Close', { duration: 5000 });
      }
    });
  }

  setupPeriodicScans() {
    this.scanService.setupPeriodicScans().subscribe({
      next: (result) => {
        this.snackBar.open('Periodic scans configured successfully', 'Close', { duration: 5000 });
      },
      error: (error) => {
        console.error('Error setting up periodic scans:', error);
        this.snackBar.open('Failed to setup periodic scans', 'Close', { duration: 5000 });
      }
    });
  }

  refreshStatus() {
    this.loadScanStatus();
    this.snackBar.open('Status refreshed', 'Close', { duration: 2000 });
  }

  applyFilter() {
    // Filter is applied automatically through filteredCustomers()
  }

  toggleAllSelection(event: any) {
    const customers = this.filteredCustomers();
    if (event.checked) {
      this.selectedCustomers.set(customers.map((c: any) => c.customerId));
    } else {
      this.selectedCustomers.set([]);
    }
  }

  toggleCustomerSelection(customer: any, event: any) {
    const selected = this.selectedCustomers();
    if (event.checked) {
      this.selectedCustomers.set([...selected, customer.customerId]);
    } else {
      this.selectedCustomers.set(selected.filter(id => id !== customer.customerId));
    }
  }

  isAllSelected(): boolean {
    const customers = this.filteredCustomers();
    return customers.length > 0 && this.selectedCustomers().length === customers.length;
  }

  isPartiallySelected(): boolean {
    const selected = this.selectedCustomers().length;
    const total = this.filteredCustomers().length;
    return selected > 0 && selected < total;
  }

  isCustomerSelected(customer: any): boolean {
    return this.selectedCustomers().includes(customer.customerId);
  }

  getRiskColor(riskLevel: string): string {
    switch(riskLevel?.toLowerCase()) {
      case 'high': return 'warn';
      case 'medium': return 'accent';
      default: return 'primary';
    }
  }

  getDaysClass(days: number | null): string {
    if (!days) return '';
    if (days > 30) return 'days-overdue';
    if (days > 14) return 'days-warning';
    return 'days-ok';
  }

  formatDate(date: string | Date): string {
    return new Date(date).toLocaleDateString();
  }

  viewCustomerDetails(customerId: string) {
    window.open(`/customers/${customerId}`, '_blank');
  }

  viewScheduleStatus() {
    this.scanService.getScheduleStatus().subscribe({
      next: (status) => {
        console.log('Schedule status:', status);
        this.snackBar.open('Schedule status logged to console', 'Close', { duration: 3000 });
      },
      error: (error) => {
        console.error('Error getting schedule status:', error);
        this.snackBar.open('Failed to get schedule status', 'Close', { duration: 5000 });
      }
    });
  }

  // AI Risk Scoring Methods
  getAIRiskScore(customerId: string): number {
    const assessment = this.aiRiskAssessments().get(customerId);
    return assessment?.riskScore || 0;
  }

  getAIRiskLevel(customerId: string): string {
    const riskScore = this.getAIRiskScore(customerId);
    return this.aiRiskService.getRiskLevel(riskScore);
  }

  getAIRiskColor(customerId: string): string {
    const riskScore = this.getAIRiskScore(customerId);
    return this.aiRiskService.getRiskColor(riskScore);
  }

  getRiskTrendIcon(customerId: string): string {
    const assessment = this.aiRiskAssessments().get(customerId);
    return this.aiRiskService.getTrendIcon(assessment?.riskTrend || 'stable');
  }

  getRiskTrendColor(customerId: string): string {
    const assessment = this.aiRiskAssessments().get(customerId);
    return this.aiRiskService.getTrendColor(assessment?.riskTrend || 'stable');
  }

  viewAIInsights(customerId: string) {
    this.selectedCustomerForAI.set(customerId);
    this.showAIInsights.set(true);

    // Load detailed AI insights
    this.aiRiskService.calculateRiskScore(customerId).subscribe({
      next: (assessment) => {
        const currentMap = this.aiRiskAssessments();
        currentMap.set(customerId, assessment);
        this.aiRiskAssessments.set(new Map(currentMap));
      },
      error: (error) => {
        console.error('Error loading AI insights:', error);
        this.snackBar.open('Failed to load AI insights', 'Close', { duration: 5000 });
      }
    });
  }

  closeAIInsights() {
    this.showAIInsights.set(false);
    this.selectedCustomerForAI.set(null);
  }

  getSelectedCustomerAssessment() {
    const customerId = this.selectedCustomerForAI();
    return customerId ? this.aiRiskAssessments().get(customerId) : null;
  }

  // Real-time Notification Methods
  markNotificationAsRead(notificationId: string) {
    this.notificationService.markAsRead(notificationId);
  }

  markAllNotificationsAsRead() {
    this.notificationService.markAllAsRead();
  }

  clearAllNotifications() {
    this.notificationService.clearNotifications();
  }

  getNotificationIcon(type: string): string {
    switch (type) {
      case 'scan_complete': return 'check_circle';
      case 'alert_created': return 'warning';
      case 'risk_score_update': return 'trending_up';
      case 'system': return 'info';
      default: return 'notifications';
    }
  }

  getNotificationColor(priority: string): string {
    switch (priority.toLowerCase()) {
      case 'critical': return '#d32f2f';
      case 'high': return '#f57c00';
      case 'medium': return '#fbc02d';
      case 'low': return '#388e3c';
      default: return '#1976d2';
    }
  }

  // Enhanced scan methods with AI integration
  scanSingleCustomerWithAI(customerId: string) {
    // First perform regular scan
    this.scanSingleCustomer(customerId);

    // Then calculate AI risk score
    this.aiRiskService.calculateRiskScore(customerId).subscribe({
      next: (assessment) => {
        const currentMap = this.aiRiskAssessments();
        currentMap.set(customerId, assessment);
        this.aiRiskAssessments.set(new Map(currentMap));

        this.snackBar.open(
          `AI Risk Score: ${assessment.riskScore.toFixed(1)} (${this.aiRiskService.getRiskLevel(assessment.riskScore)})`,
          'View Details',
          { duration: 7000 }
        ).onAction().subscribe(() => {
          this.viewAIInsights(customerId);
        });
      },
      error: (error) => {
        console.error('Error calculating AI risk score:', error);
      }
    });
  }

  refreshAIRiskScores() {
    this.loadAIRiskAssessments();
    this.snackBar.open('AI risk scores refreshed', 'Close', { duration: 3000 });
  }
}
