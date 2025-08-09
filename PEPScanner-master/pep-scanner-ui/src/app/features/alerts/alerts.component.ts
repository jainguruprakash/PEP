import { Component, inject } from '@angular/core';
import { CommonModule } from '@angular/common';
import { MatTableModule } from '@angular/material/table';
import { AlertsService } from '../../services/alerts.service';

@Component({
  selector: 'app-alerts',
  standalone: true,
  imports: [CommonModule, MatTableModule],
  template: `
    <h2>Alerts</h2>
    <table mat-table [dataSource]="alerts" class="mat-elevation-z2" *ngIf="alerts">
      <ng-container matColumnDef="alertType">
        <th mat-header-cell *matHeaderCellDef>Type</th>
        <td mat-cell *matCellDef="let row">{{ row.alertType }}</td>
      </ng-container>
      <ng-container matColumnDef="riskLevel">
        <th mat-header-cell *matHeaderCellDef>Risk</th>
        <td mat-cell *matCellDef="let row">{{ row.riskLevel }}</td>
      </ng-container>
      <ng-container matColumnDef="status">
        <th mat-header-cell *matHeaderCellDef>Status</th>
        <td mat-cell *matCellDef="let row">{{ row.status }}</td>
      </ng-container>
      <tr mat-header-row *matHeaderRowDef="displayedColumns"></tr>
      <tr mat-row *matRowDef="let row; columns: displayedColumns;"></tr>
    </table>
  `
})
export class AlertsComponent {
  private alertsService = inject(AlertsService);
  alerts: any[] = [];
  displayedColumns = ['alertType', 'riskLevel', 'status'];

  constructor(){
    this.alertsService.getAll().subscribe(a => this.alerts = a);
  }
}


