import { Component, inject, signal } from '@angular/core';
import { CommonModule } from '@angular/common';
import { MatCardModule } from '@angular/material/card';
import { ScreeningService } from '../../services/screening.service';

@Component({
  selector: 'app-dashboard',
  standalone: true,
  imports: [CommonModule, MatCardModule],
  template: `
  <div class="grid">
    <mat-card>
      <mat-card-title>Screening Statistics</mat-card-title>
      <mat-card-content>
        <div *ngIf="stats() as s; else loading">
          <div>Alerts: {{ s.alerts }}</div>
          <div>Customers Screened: {{ s.customers }}</div>
          <div>Average Risk: {{ s.avgRisk }}</div>
        </div>
        <ng-template #loading>Loading...</ng-template>
      </mat-card-content>
    </mat-card>
  </div>
  `,
  styles: [`
    .grid { display: grid; grid-template-columns: repeat(auto-fit, minmax(280px, 1fr)); gap: 16px; }
  `]
})
export class DashboardComponent {
  private screeningService = inject(ScreeningService);
  protected stats = signal<{ alerts: number; customers: number; avgRisk: number } | null>(null);

  constructor() {
    const end = new Date().toISOString().slice(0,10);
    const start = new Date(Date.now() - 30*24*3600*1000).toISOString().slice(0,10);
    this.screeningService.getStatistics(start, end).subscribe({
      next: (data: any) => {
        this.stats.set({ alerts: data.alertCount ?? 0, customers: data.customersScreened ?? 0, avgRisk: data.averageRisk ?? 0 });
      },
      error: () => this.stats.set({ alerts: 0, customers: 0, avgRisk: 0 })
    });
  }
}


