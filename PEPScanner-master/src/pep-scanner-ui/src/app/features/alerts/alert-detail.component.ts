import { Component, inject, signal, OnInit } from '@angular/core';
import { CommonModule } from '@angular/common';
import { ActivatedRoute, Router } from '@angular/router';
import { ReactiveFormsModule, FormBuilder, Validators } from '@angular/forms';
import { MatCardModule } from '@angular/material/card';
import { MatButtonModule } from '@angular/material/button';
import { MatIconModule } from '@angular/material/icon';
import { MatChipsModule } from '@angular/material/chips';
import { MatTabsModule } from '@angular/material/tabs';
import { MatFormFieldModule } from '@angular/material/form-field';
import { MatInputModule } from '@angular/material/input';
import { MatSelectModule } from '@angular/material/select';
import { MatSnackBarModule, MatSnackBar } from '@angular/material/snack-bar';
import { MatProgressSpinnerModule } from '@angular/material/progress-spinner';
import { MatExpansionModule } from '@angular/material/expansion';
import { MatDividerModule } from '@angular/material/divider';
import { AlertsService } from '../../services/alerts.service';

@Component({
  selector: 'app-alert-detail',
  standalone: true,
  imports: [
    CommonModule, ReactiveFormsModule, MatCardModule, MatButtonModule, 
    MatIconModule, MatChipsModule, MatTabsModule, MatFormFieldModule, 
    MatInputModule, MatSelectModule, MatSnackBarModule, MatProgressSpinnerModule,
    MatExpansionModule, MatDividerModule
  ],
  template: `
    <div class="alert-detail-container">
      @if (loading()) {
        <div class="loading-container">
          <mat-spinner></mat-spinner>
          <p>Loading alert details...</p>
        </div>
      } @else if (alert()) {
        <mat-card class="alert-header">
          <mat-card-header>
            <mat-card-title>
              <mat-icon>warning</mat-icon>
              Alert Details - {{ alert()?.alertType }}
            </mat-card-title>
            <div class="header-actions">
              <mat-chip [color]="getPriorityColor(alert()?.priority)">{{ alert()?.priority }}</mat-chip>
              <mat-chip [color]="getStatusColor(alert()?.workflowStatus)">{{ alert()?.workflowStatus }}</mat-chip>
              <button mat-button (click)="goBack()">
                <mat-icon>arrow_back</mat-icon>
                Back to Alerts
              </button>
            </div>
          </mat-card-header>
        </mat-card>

        <mat-tab-group>
          <mat-tab label="Overview">
            <div class="tab-content">
              <div class="overview-grid">
                <mat-card>
                  <mat-card-title>Alert Information</mat-card-title>
                  <mat-card-content>
                    <div class="info-grid">
                      <div class="info-item">
                        <label>Alert Type:</label>
                        <span>{{ alert()?.alertType }}</span>
                      </div>
                      <div class="info-item">
                        <label>Risk Level:</label>
                        <mat-chip [color]="getRiskColor(alert()?.riskLevel)">{{ alert()?.riskLevel }}</mat-chip>
                      </div>
                      <div class="info-item">
                        <label>Similarity Score:</label>
                        <span>{{ alert()?.similarityScore }}%</span>
                      </div>
                      <div class="info-item">
                        <label>Source List:</label>
                        <span>{{ alert()?.sourceList || 'N/A' }}</span>
                      </div>
                      <div class="info-item">
                        <label>Created:</label>
                        <span>{{ formatDate(alert()?.createdAtUtc) }}</span>
                      </div>
                      <div class="info-item">
                        <label>Due Date:</label>
                        <span>{{ formatDate(alert()?.dueDate) }}</span>
                      </div>
                      <div class="info-item">
                        <label>Assigned To:</label>
                        <span>{{ alert()?.assignedTo || 'Unassigned' }}</span>
                      </div>
                      <div class="info-item">
                        <label>Current Reviewer:</label>
                        <span>{{ alert()?.currentReviewer || 'None' }}</span>
                      </div>
                    </div>
                  </mat-card-content>
                </mat-card>

                @if (alert()?.customer) {
                  <mat-card>
                    <mat-card-title>Customer Information</mat-card-title>
                    <mat-card-content>
                      <div class="info-grid">
                        <div class="info-item">
                          <label>Name:</label>
                          <span>{{ alert()?.customer?.fullName }}</span>
                        </div>
                        <div class="info-item">
                          <label>Date of Birth:</label>
                          <span>{{ formatDate(alert()?.customer?.dateOfBirth) }}</span>
                        </div>
                        <div class="info-item">
                          <label>Nationality:</label>
                          <span>{{ alert()?.customer?.nationality || 'N/A' }}</span>
                        </div>
                        <div class="info-item">
                          <label>Country:</label>
                          <span>{{ alert()?.customer?.country || 'N/A' }}</span>
                        </div>
                      </div>
                    </mat-card-content>
                  </mat-card>
                }

                @if (alert()?.watchlistEntry) {
                  <mat-card>
                    <mat-card-title>Watchlist Match</mat-card-title>
                    <mat-card-content>
                      <div class="info-grid">
                        <div class="info-item">
                          <label>Primary Name:</label>
                          <span>{{ alert()?.watchlistEntry?.primaryName }}</span>
                        </div>
                        <div class="info-item">
                          <label>Source:</label>
                          <span>{{ alert()?.watchlistEntry?.source }}</span>
                        </div>
                        <div class="info-item">
                          <label>List Type:</label>
                          <span>{{ alert()?.watchlistEntry?.listType }}</span>
                        </div>
                        <div class="info-item">
                          <label>Risk Category:</label>
                          <span>{{ alert()?.watchlistEntry?.riskCategory }}</span>
                        </div>
                        @if (alert()?.watchlistEntry?.alternateNames) {
                          <div class="info-item full-width">
                            <label>Alternate Names:</label>
                            <span>{{ alert()?.watchlistEntry?.alternateNames }}</span>
                          </div>
                        }
                      </div>
                    </mat-card-content>
                  </mat-card>
                }
              </div>

              @if (alert()?.matchingDetails) {
                <mat-card class="matching-details">
                  <mat-card-title>Matching Details</mat-card-title>
                  <mat-card-content>
                    <pre>{{ alert()?.matchingDetails }}</pre>
                  </mat-card-content>
                </mat-card>
              }
            </div>
          </mat-tab>

          <mat-tab label="Actions & History">
            <div class="tab-content">
              @if (alert()?.actions && alert()?.actions.length > 0) {
                <mat-card>
                  <mat-card-title>Action History</mat-card-title>
                  <mat-card-content>
                    <div class="timeline">
                      @for (action of alert()?.actions; track action.id) {
                        <div class="timeline-item">
                          <div class="timeline-marker">
                            <mat-icon>{{ getActionIcon(action.actionType) }}</mat-icon>
                          </div>
                          <div class="timeline-content">
                            <div class="action-header">
                              <strong>{{ action.actionType }}</strong>
                              <span class="action-date">{{ formatDate(action.actionDateUtc) }}</span>
                            </div>
                            <div class="action-details">
                              <p><strong>Performed by:</strong> {{ action.performedBy }}</p>
                              @if (action.previousStatus && action.newStatus) {
                                <p><strong>Status Change:</strong> {{ action.previousStatus }} → {{ action.newStatus }}</p>
                              }
                              @if (action.comments) {
                                <p><strong>Comments:</strong> {{ action.comments }}</p>
                              }
                              @if (action.reason) {
                                <p><strong>Reason:</strong> {{ action.reason }}</p>
                              }
                            </div>
                          </div>
                        </div>
                      }
                    </div>
                  </mat-card-content>
                </mat-card>
              } @else {
                <mat-card>
                  <mat-card-content>
                    <p>No actions recorded for this alert.</p>
                  </mat-card-content>
                </mat-card>
              }
            </div>
          </mat-tab>

          <mat-tab label="Take Action">
            <div class="tab-content">
              <div class="action-forms">
                @if (canApprove()) {
                  <mat-card class="action-card">
                    <mat-card-title>Approve Alert</mat-card-title>
                    <form [formGroup]="approveForm" (ngSubmit)="approveAlert()">
                      <mat-form-field appearance="outline">
                        <mat-label>Outcome</mat-label>
                        <mat-select formControlName="outcome">
                          <mat-option value="TruePositive">True Positive</mat-option>
                          <mat-option value="Approved">Approved</mat-option>
                          <mat-option value="Escalated">Escalated</mat-option>
                        </mat-select>
                      </mat-form-field>
                      <mat-form-field appearance="outline">
                        <mat-label>Comments</mat-label>
                        <textarea matInput formControlName="comments" rows="3"></textarea>
                      </mat-form-field>
                      <div class="form-actions">
                        <button mat-raised-button color="primary" type="submit" [disabled]="approveForm.invalid">
                          <mat-icon>check</mat-icon>
                          Approve Alert
                        </button>
                      </div>
                    </form>
                  </mat-card>
                }

                @if (canReject()) {
                  <mat-card class="action-card">
                    <mat-card-title>Reject Alert</mat-card-title>
                    <form [formGroup]="rejectForm" (ngSubmit)="rejectAlert()">
                      <mat-form-field appearance="outline">
                        <mat-label>Rejection Reason</mat-label>
                        <mat-select formControlName="reason" required>
                          <mat-option value="FalsePositive">False Positive</mat-option>
                          <mat-option value="InsufficientEvidence">Insufficient Evidence</mat-option>
                          <mat-option value="DataQualityIssue">Data Quality Issue</mat-option>
                          <mat-option value="Other">Other</mat-option>
                        </mat-select>
                      </mat-form-field>
                      <mat-form-field appearance="outline">
                        <mat-label>Additional Comments</mat-label>
                        <textarea matInput formControlName="comments" rows="3"></textarea>
                      </mat-form-field>
                      <div class="form-actions">
                        <button mat-raised-button color="warn" type="submit" [disabled]="rejectForm.invalid">
                          <mat-icon>close</mat-icon>
                          Reject Alert
                        </button>
                      </div>
                    </form>
                  </mat-card>
                }

                @if (canAssign()) {
                  <mat-card class="action-card">
                    <mat-card-title>Assign Alert</mat-card-title>
                    <form [formGroup]="assignForm" (ngSubmit)="assignAlert()">
                      <mat-form-field appearance="outline">
                        <mat-label>Assign To</mat-label>
                        <mat-select formControlName="assignedTo" required>
                          @for (user of availableUsers(); track user.id) {
                            <mat-option [value]="user.id">{{ user.name }} ({{ user.role }})</mat-option>
                          }
                        </mat-select>
                      </mat-form-field>
                      <mat-form-field appearance="outline">
                        <mat-label>Comments</mat-label>
                        <textarea matInput formControlName="comments" rows="2"></textarea>
                      </mat-form-field>
                      <div class="form-actions">
                        <button mat-raised-button color="accent" type="submit" [disabled]="assignForm.invalid">
                          <mat-icon>person_add</mat-icon>
                          Assign Alert
                        </button>
                      </div>
                    </form>
                  </mat-card>
                }

                @if (canEscalate()) {
                  <mat-card class="action-card">
                    <mat-card-title>Escalate Alert</mat-card-title>
                    <form [formGroup]="escalateForm" (ngSubmit)="escalateAlert()">
                      <mat-form-field appearance="outline">
                        <mat-label>Escalation Reason</mat-label>
                        <mat-select formControlName="reason" required>
                          <mat-option value="ComplexCase">Complex Case</mat-option>
                          <mat-option value="HighRisk">High Risk</mat-option>
                          <mat-option value="RequiresExpertise">Requires Expertise</mat-option>
                          <mat-option value="SLABreach">SLA Breach</mat-option>
                        </mat-select>
                      </mat-form-field>
                      <mat-form-field appearance="outline">
                        <mat-label>Additional Comments</mat-label>
                        <textarea matInput formControlName="comments" rows="3"></textarea>
                      </mat-form-field>
                      <div class="form-actions">
                        <button mat-raised-button color="warn" type="submit" [disabled]="escalateForm.invalid">
                          <mat-icon>trending_up</mat-icon>
                          Escalate Alert
                        </button>
                      </div>
                    </form>
                  </mat-card>
                }
              </div>
            </div>
          </mat-tab>
        </mat-tab-group>
      } @else {
        <mat-card>
          <mat-card-content>
            <p>Alert not found.</p>
            <button mat-button (click)="goBack()">Back to Alerts</button>
          </mat-card-content>
        </mat-card>
      }
    </div>
  `,
  styles: [`
    .alert-detail-container { padding: 20px; }
    .loading-container { display: flex; flex-direction: column; align-items: center; padding: 40px; }
    .alert-header { margin-bottom: 20px; }
    .alert-header mat-card-header { display: flex; justify-content: space-between; align-items: center; width: 100%; }
    .header-actions { display: flex; gap: 10px; align-items: center; }
    .tab-content { padding: 20px 0; }
    .overview-grid { display: grid; grid-template-columns: repeat(auto-fit, minmax(400px, 1fr)); gap: 20px; margin-bottom: 20px; }
    .info-grid { display: grid; grid-template-columns: repeat(auto-fit, minmax(200px, 1fr)); gap: 15px; }
    .info-item { display: flex; flex-direction: column; }
    .info-item.full-width { grid-column: 1 / -1; }
    .info-item label { font-weight: 500; color: #666; margin-bottom: 5px; }
    .matching-details pre { white-space: pre-wrap; background: #f5f5f5; padding: 15px; border-radius: 4px; }
    .timeline { position: relative; padding-left: 30px; }
    .timeline-item { position: relative; margin-bottom: 30px; }
    .timeline-marker { position: absolute; left: -45px; top: 0; width: 30px; height: 30px; border-radius: 50%; background: #1976d2; display: flex; align-items: center; justify-content: center; color: white; }
    .timeline-content { background: #f9f9f9; padding: 15px; border-radius: 8px; }
    .action-header { display: flex; justify-content: space-between; align-items: center; margin-bottom: 10px; }
    .action-date { color: #666; font-size: 0.875rem; }
    .action-details p { margin: 5px 0; }
    .action-forms { display: grid; grid-template-columns: repeat(auto-fit, minmax(400px, 1fr)); gap: 20px; }
    .action-card { height: fit-content; }
    .form-actions { margin-top: 15px; }
    .form-actions button { width: 100%; }
    mat-form-field { width: 100%; margin-bottom: 15px; }
  `]
})
export class AlertDetailComponent implements OnInit {
  private route = inject(ActivatedRoute);
  private router = inject(Router);
  private alertsService = inject(AlertsService);
  private fb = inject(FormBuilder);
  private snackBar = inject(MatSnackBar);

  alert = signal<any>(null);
  loading = signal(true);
  availableUsers = signal<any[]>([]);

  approveForm = this.fb.group({
    outcome: ['TruePositive', Validators.required],
    comments: ['']
  });

  rejectForm = this.fb.group({
    reason: ['', Validators.required],
    comments: ['']
  });

  assignForm = this.fb.group({
    assignedTo: ['', Validators.required],
    comments: ['']
  });

  escalateForm = this.fb.group({
    reason: ['', Validators.required],
    comments: ['']
  });

  ngOnInit() {
    const alertId = this.route.snapshot.paramMap.get('id');
    if (alertId) {
      this.loadAlert(alertId);
    }
  }

  loadAlert(id: string) {
    this.loading.set(true);
    this.alertsService.getById(id).subscribe({
      next: (alert) => {
        this.alert.set(alert);
        this.loading.set(false);
      },
      error: (error) => {
        console.error('Error loading alert:', error);
        this.loading.set(false);
        this.snackBar.open('Failed to load alert details', 'Close', { duration: 5000 });
      }
    });
  }

  approveAlert() {
    if (this.approveForm.valid && this.alert()) {
      const request = {
        approvedBy: 'current-user',
        comments: this.approveForm.value.comments || '',
        outcome: this.approveForm.value.outcome || 'TruePositive'
      };

      this.alertsService.approve(this.alert()!.id, request).subscribe({
        next: () => {
          this.snackBar.open('Alert approved successfully', 'Close', { duration: 3000 });
          this.loadAlert(this.alert()!.id);
        },
        error: (error) => {
          this.snackBar.open('Failed to approve alert', 'Close', { duration: 5000 });
        }
      });
    }
  }

  rejectAlert() {
    if (this.rejectForm.valid && this.alert()) {
      const request = {
        rejectedBy: 'current-user', // TODO: Get from auth service
        reason: this.rejectForm.value.reason + (this.rejectForm.value.comments ? ': ' + this.rejectForm.value.comments : '')
      };

      this.alertsService.reject(this.alert()!.id, request).subscribe({
        next: () => {
          this.snackBar.open('Alert rejected successfully', 'Close', { duration: 3000 });
          this.loadAlert(this.alert()!.id);
        },
        error: (error) => {
          this.snackBar.open('Failed to reject alert', 'Close', { duration: 5000 });
        }
      });
    }
  }

  assignAlert() {
    if (this.assignForm.valid && this.alert()) {
      const request = {
        assignedTo: this.assignForm.value.assignedTo || '',
        assignedBy: 'current-user',
        comments: this.assignForm.value.comments || ''
      };

      this.alertsService.assign(this.alert()!.id, request).subscribe({
        next: () => {
          this.snackBar.open('Alert assigned successfully', 'Close', { duration: 3000 });
          this.loadAlert(this.alert()!.id);
        },
        error: (error) => {
          this.snackBar.open('Failed to assign alert', 'Close', { duration: 5000 });
        }
      });
    }
  }

  escalateAlert() {
    if (this.escalateForm.valid && this.alert()) {
      const request = {
        escalatedBy: 'current-user', // TODO: Get from auth service
        reason: this.escalateForm.value.reason + (this.escalateForm.value.comments ? ': ' + this.escalateForm.value.comments : '')
      };

      this.alertsService.escalate(this.alert()!.id, request).subscribe({
        next: () => {
          this.snackBar.open('Alert escalated successfully', 'Close', { duration: 3000 });
          this.loadAlert(this.alert()!.id);
        },
        error: (error) => {
          this.snackBar.open('Failed to escalate alert', 'Close', { duration: 5000 });
        }
      });
    }
  }

  canApprove(): boolean {
    const status = this.alert()?.workflowStatus;
    return status === 'PendingApproval' || status === 'UnderReview';
  }

  canReject(): boolean {
    const status = this.alert()?.workflowStatus;
    return status === 'PendingApproval' || status === 'UnderReview';
  }

  canAssign(): boolean {
    const status = this.alert()?.workflowStatus;
    return status === 'PendingReview' || status === 'UnderReview';
  }

  canEscalate(): boolean {
    const status = this.alert()?.workflowStatus;
    return status === 'UnderReview' || status === 'PendingApproval';
  }

  getPriorityColor(priority: string): string {
    switch(priority?.toLowerCase()) {
      case 'critical': return 'warn';
      case 'high': return 'warn';
      case 'medium': return 'accent';
      default: return 'primary';
    }
  }

  getStatusColor(status: string): string {
    switch(status?.toLowerCase()) {
      case 'approved': return 'primary';
      case 'rejected': return 'warn';
      case 'underreview': return 'accent';
      default: return 'primary';
    }
  }

  getRiskColor(risk: string): string {
    switch(risk?.toLowerCase()) {
      case 'high': case 'critical': return 'warn';
      case 'medium': return 'accent';
      default: return 'primary';
    }
  }

  getActionIcon(actionType: string): string {
    switch(actionType?.toLowerCase()) {
      case 'approved': return 'check_circle';
      case 'rejected': return 'cancel';
      case 'assigned': return 'person_add';
      case 'escalated': return 'trending_up';
      case 'reviewed': return 'rate_review';
      default: return 'info';
    }
  }

  formatDate(date: string | null): string {
    if (!date) return 'N/A';
    return new Date(date).toLocaleString();
  }

  goBack() {
    this.router.navigate(['/dashboard/alerts']);
  }
}