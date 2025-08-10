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

  // Chart options
  lineChartOptions = {
    responsive: true,
    plugins: {
      legend: {
        position: 'top' as const,
      },
      title: {
        display: true,
        text: 'Trends Over Time'
      }
    },
    scales: {
      y: {
        beginAtZero: true
      }
    }
  };

  pieChartOptions = {
    responsive: true,
    plugins: {
      legend: {
        position: 'right' as const,
      },
      title: {
        display: true,
        text: 'Distribution'
      }
    }
  };

  ngOnInit() {
    this.loadDashboardData();
  }

  async loadDashboardData() {
    try {
      this.loading.set(true);

      // Load all dashboard data in parallel
      const [
        overview,
        kpis,
        alertTrends,
        screeningMetrics,
        reportStatus,
        complianceScore,
        recentActivities
      ] = await Promise.all([
        this.dashboardService.getDashboardOverview().toPromise(),
        this.dashboardService.getKpis().toPromise(),
        this.dashboardService.getAlertTrends(30).toPromise(),
        this.dashboardService.getScreeningMetrics(30).toPromise(),
        this.dashboardService.getReportStatusDistribution().toPromise(),
        this.dashboardService.getComplianceScoreHistory(6).toPromise(),
        this.dashboardService.getRecentActivities(10).toPromise()
      ]);

      // Update signals
      this.overview.set(overview || null);
      this.kpis.set(kpis || null);
      this.recentActivities.set(recentActivities || []);

      // Process chart data
      this.processAlertTrends(alertTrends || []);
      this.processScreeningMetrics(screeningMetrics || []);
      this.processReportStatus(reportStatus || []);
      this.processComplianceScore(complianceScore || []);

    } catch (error) {
      console.error('Error loading dashboard data:', error);
    } finally {
      this.loading.set(false);
    }
  }

  private processAlertTrends(data: ChartData[]) {
    const labels = data.map(item => item.label);
    const values = data.map(item => item.value);

    this.alertTrendsData.set(
      ChartDataHelper.createLineChartData(labels, [
        {
          label: 'Alerts Generated',
          data: values,
          borderColor: ChartDataHelper.getDefaultColors().primary,
          backgroundColor: 'rgba(59, 130, 246, 0.1)'
        }
      ])
    );
  }

  private processScreeningMetrics(data: ChartData[]) {
    const labels = data.map(item => item.label);
    const values = data.map(item => item.value);

    this.screeningMetricsData.set(
      ChartDataHelper.createBarChartData(labels, [
        {
          label: 'Screenings Performed',
          data: values,
          backgroundColor: [ChartDataHelper.getDefaultColors().success]
        }
      ])
    );
  }

  private processReportStatus(data: ChartData[]) {
    const labels = data.map(item => item.label);
    const values = data.map(item => item.value);

    this.reportStatusData.set(
      ChartDataHelper.createDoughnutChartData(labels, values)
    );
  }

  private processComplianceScore(data: ChartData[]) {
    const labels = data.map(item => item.label);
    const values = data.map(item => item.value);

    this.complianceScoreData.set(
      ChartDataHelper.createLineChartData(labels, [
        {
          label: 'Compliance Score',
          data: values,
          borderColor: ChartDataHelper.getDefaultColors().success,
          backgroundColor: 'rgba(16, 185, 129, 0.1)'
        }
      ])
    );
  }

  async refreshDashboard() {
    try {
      await this.dashboardService.refreshMetrics().toPromise();
      await this.loadDashboardData();
    } catch (error) {
      console.error('Error refreshing dashboard:', error);
    }
  }

  getStatusColor(status: string): string {
    switch (status.toLowerCase()) {
      case 'high':
      case 'critical':
        return 'warn';
      case 'medium':
        return 'accent';
      case 'low':
        return 'primary';
      default:
        return 'primary';
    }
  }

  getStatusIcon(type: string): string {
    switch (type.toLowerCase()) {
      case 'alert':
        return 'warning';
      case 'report':
        return 'description';
      case 'screening':
        return 'search';
      case 'compliance':
        return 'verified';
      default:
        return 'info';
    }
  }

  formatNumber(value: number): string {
    if (value >= 1000000) {
      return (value / 1000000).toFixed(1) + 'M';
    } else if (value >= 1000) {
      return (value / 1000).toFixed(1) + 'K';
    }
    return value.toString();
  }

  formatPercentage(value: number): string {
    return value.toFixed(1) + '%';
  }

  formatTrend(value: number): { text: string; icon: string; color: string } {
    const isPositive = value > 0;
    return {
      text: `${isPositive ? '+' : ''}${value.toFixed(1)}%`,
      icon: isPositive ? 'trending_up' : 'trending_down',
      color: isPositive ? 'success' : 'warn'
    };
  }
}