import { Component, inject, OnInit } from '@angular/core';
import { CommonModule } from '@angular/common';
import { MatCardModule } from '@angular/material/card';
import { MatTableModule } from '@angular/material/table';
import { MatButtonModule } from '@angular/material/button';
import { MatIconModule } from '@angular/material/icon';
import { MatChipsModule } from '@angular/material/chips';
import { HttpClient } from '@angular/common/http';
import { environment } from '../../environments/environment';

@Component({
  selector: 'app-simple-alerts',
  standalone: true,
  imports: [CommonModule, MatCardModule, MatTableModule, MatButtonModule, MatIconModule, MatChipsModule],
  template: `
    <mat-card>
      <mat-card-header>
        <mat-card-title>
          <mat-icon>warning</mat-icon>
          Alerts Management
        </mat-card-title>
      </mat-card-header>
      <mat-card-content>
        <div *ngIf="loading">Loading alerts...</div>
        <div *ngIf="error" style="color: red;">Error: {{ error }}</div>
        
        <table mat-table [dataSource]="alerts" *ngIf="!loading && !error">
          <ng-container matColumnDef="alertType">
            <th mat-header-cell *matHeaderCellDef>Type</th>
            <td mat-cell *matCellDef="let alert">{{ alert.alertType }}</td>
          </ng-container>
          
          <ng-container matColumnDef="status">
            <th mat-header-cell *matHeaderCellDef>Status</th>
            <td mat-cell *matCellDef="let alert">{{ alert.status }}</td>
          </ng-container>
          
          <ng-container matColumnDef="priority">
            <th mat-header-cell *matHeaderCellDef>Priority</th>
            <td mat-cell *matCellDef="let alert">
              <mat-chip [color]="getPriorityColor(alert.priority)">{{ alert.priority }}</mat-chip>
            </td>
          </ng-container>
          
          <ng-container matColumnDef="assignedTo">
            <th mat-header-cell *matHeaderCellDef>Assigned To</th>
            <td mat-cell *matCellDef="let alert">{{ alert.assignedTo }}</td>
          </ng-container>
          
          <tr mat-header-row *matHeaderRowDef="displayedColumns"></tr>
          <tr mat-row *matRowDef="let row; columns: displayedColumns;"></tr>
        </table>
        
        <div *ngIf="!loading && !error && alerts.length === 0">
          No alerts found.
        </div>
      </mat-card-content>
    </mat-card>
  `,
  styles: [`
    table { width: 100%; margin-top: 16px; }
    .mat-mdc-chip { font-size: 0.75rem; }
  `]
})
export class SimpleAlertsComponent implements OnInit {
  private http = inject(HttpClient);
  
  alerts: any[] = [];
  loading = true;
  error = '';
  displayedColumns = ['alertType', 'status', 'priority', 'assignedTo'];

  ngOnInit() {
    this.loadAlerts();
  }

  loadAlerts() {
    this.loading = true;
    this.error = '';
    
    this.http.get<any>(`${environment.apiBaseUrl}/alerts`).subscribe({
      next: (response) => {
        this.alerts = response.alerts || [];
        this.loading = false;
      },
      error: (error) => {
        this.error = error.message || 'Failed to load alerts';
        this.loading = false;
        console.error('Error loading alerts:', error);
      }
    });
  }

  getPriorityColor(priority: string): string {
    switch(priority?.toLowerCase()) {
      case 'critical': return 'warn';
      case 'high': return 'warn';
      case 'medium': return 'accent';
      default: return 'primary';
    }
  }
}