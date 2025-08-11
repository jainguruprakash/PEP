import { Component, inject, signal } from '@angular/core';
import { CommonModule } from '@angular/common';
import { MatTableModule } from '@angular/material/table';
import { MatButtonModule } from '@angular/material/button';
import { MatChipsModule } from '@angular/material/chips';
import { MatIconModule } from '@angular/material/icon';
import { MatCardModule } from '@angular/material/card';
import { MatTabsModule } from '@angular/material/tabs';
import { RouterLink } from '@angular/router';
import { AlertsService } from '../../services/alerts.service';

@Component({
  selector: 'app-alerts',
  standalone: true,
  imports: [CommonModule, MatTableModule, MatButtonModule, MatChipsModule, MatIconModule, MatCardModule, MatTabsModule, RouterLink],
  template: `
    <mat-card>
      <mat-card-header>
        <mat-card-title>
          <mat-icon>warning</mat-icon>
          Alert Management
        </mat-card-title>
      </mat-card-header>
      <mat-card-content>
        <mat-tab-group>
          <mat-tab label="Pending Approval">
            <table mat-table [dataSource]="pendingAlerts()" class="mat-elevation-z2">
              <ng-container matColumnDef="alertType">
                <th mat-header-cell *matHeaderCellDef>Type</th>
                <td mat-cell *matCellDef="let row">{{ row.alertType }}</td>
              </ng-container>
              <ng-container matColumnDef="riskLevel">
                <th mat-header-cell *matHeaderCellDef>Risk</th>
                <td mat-cell *matCellDef="let row">
                  <mat-chip [color]="getRiskColor(row.riskLevel)">{{ row.riskLevel }}</mat-chip>
                </td>
              </ng-container>
              <ng-container matColumnDef="workflowStatus">
                <th mat-header-cell *matHeaderCellDef>Status</th>
                <td mat-cell *matCellDef="let row">{{ row.workflowStatus }}</td>
              </ng-container>
              <ng-container matColumnDef="actions">
                <th mat-header-cell *matHeaderCellDef>Actions</th>
                <td mat-cell *matCellDef="let row">
                  <button mat-raised-button color="primary" (click)="approveAlert(row.id)">Approve</button>
                  <button mat-raised-button color="warn" (click)="rejectAlert(row.id)">Reject</button>
                  <button mat-button [routerLink]="['/dashboard/alerts', row.id]">Details</button>
                </td>
              </ng-container>
              <tr mat-header-row *matHeaderRowDef="pendingColumns"></tr>
              <tr mat-row *matRowDef="let row; columns: pendingColumns;"></tr>
            </table>
          </mat-tab>

          <mat-tab label="All Alerts">
            <table mat-table [dataSource]="allAlerts()" class="mat-elevation-z2">
              <ng-container matColumnDef="alertType">
                <th mat-header-cell *matHeaderCellDef>Type</th>
                <td mat-cell *matCellDef="let row">{{ row.alertType }}</td>
              </ng-container>
              <ng-container matColumnDef="riskLevel">
                <th mat-header-cell *matHeaderCellDef>Risk</th>
                <td mat-cell *matCellDef="let row">
                  <mat-chip [color]="getRiskColor(row.riskLevel)">{{ row.riskLevel }}</mat-chip>
                </td>
              </ng-container>
              <ng-container matColumnDef="status">
                <th mat-header-cell *matHeaderCellDef>Status</th>
                <td mat-cell *matCellDef="let row">{{ row.status }}</td>
              </ng-container>
              <ng-container matColumnDef="workflowStatus">
                <th mat-header-cell *matHeaderCellDef>Workflow</th>
                <td mat-cell *matCellDef="let row">{{ row.workflowStatus }}</td>
              </ng-container>
              <ng-container matColumnDef="actions">
                <th mat-header-cell *matHeaderCellDef>Actions</th>
                <td mat-cell *matCellDef="let row">
                  <button mat-button [routerLink]="['/dashboard/alerts', row.id]">View</button>
                </td>
              </ng-container>
              <tr mat-header-row *matHeaderRowDef="allColumns"></tr>
              <tr mat-row *matRowDef="let row; columns: allColumns;"></tr>
            </table>
          </mat-tab>
        </mat-tab-group>
      </mat-card-content>
    </mat-card>
  `,
  styles: [`
    table { width: 100%; margin-top: 16px; }
    .mat-mdc-chip { font-size: 0.75rem; }
    button { margin-right: 8px; }
  `]
})
export class AlertsComponent {
  private alertsService = inject(AlertsService);

  allAlerts = signal<any[]>([]);
  pendingAlerts = signal<any[]>([]);

  allColumns = ['alertType', 'riskLevel', 'status', 'workflowStatus', 'actions'];
  pendingColumns = ['alertType', 'riskLevel', 'workflowStatus', 'actions'];

  constructor(){
    this.loadAlerts();
  }

  loadAlerts() {
    this.alertsService.getAll().subscribe({
      next: (alerts) => {
        this.allAlerts.set(alerts || []);
        this.pendingAlerts.set((alerts || []).filter(a => a.workflowStatus === 'PendingApproval' || a.workflowStatus === 'UnderReview'));
      },
      error: (error) => {
        console.error('Error loading alerts:', error);
        this.allAlerts.set([]);
        this.pendingAlerts.set([]);
      }
    });
  }

  approveAlert(id: string) {
    this.alertsService.approve(id, { approvedBy: 'CurrentUser', comments: 'Approved via UI' }).subscribe(() => {
      this.loadAlerts();
    });
  }

  rejectAlert(id: string) {
    this.alertsService.reject(id, { rejectedBy: 'CurrentUser', reason: 'Rejected via UI' }).subscribe(() => {
      this.loadAlerts();
    });
  }

  getRiskColor(risk: string): string {
    switch(risk?.toLowerCase()) {
      case 'high': case 'critical': return 'warn';
      case 'medium': return 'accent';
      default: return 'primary';
    }
  }
}


