import { Component, OnInit, inject, signal } from '@angular/core';
import { CommonModule } from '@angular/common';
import { RouterModule } from '@angular/router';
import { MatCardModule } from '@angular/material/card';
import { MatButtonModule } from '@angular/material/button';
import { MatIconModule } from '@angular/material/icon';
import { MatProgressSpinnerModule } from '@angular/material/progress-spinner';
import { MatTabsModule } from '@angular/material/tabs';
import { MatGridListModule } from '@angular/material/grid-list';

import { ChartComponent, ChartDataHelper } from '../../shared/components/chart.component';
import { DashboardService, DashboardOverview, ChartData, RecentActivity, DashboardKpis } from '../../services/dashboard.service';

@Component({
  selector: 'app-dashboard',
  standalone: true,
  imports: [
    CommonModule,
    RouterModule,
    MatCardModule,
    MatButtonModule,
    MatIconModule,
    MatProgressSpinnerModule,
    MatTabsModule,
    MatGridListModule,
    ChartComponent
  ],
  templateUrl: './dashboard.component.html',
  styleUrls: ['./dashboard.component.scss']
})
export class DashboardComponent implements OnInit {
  private dashboardService = inject(DashboardService);

  // Signals for reactive state management
  loading = signal(true);
  overview = signal<DashboardOverview | null>(null);
  kpis = signal<DashboardKpis | null>(null);
  recentActivities = signal<RecentActivity[]>([]);

  // Chart data signals
  alertTrendsData = signal<any>(null);
  screeningMetricsData = signal<any>(null);
  reportStatusData = signal<any>(null);
  complianceScoreData = signal<any>(null);


