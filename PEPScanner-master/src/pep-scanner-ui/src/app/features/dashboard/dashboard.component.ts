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

      // Load dashboard data with error handling for each service
      const promises = [
        this.dashboardService.getDashboardOverview().toPromise().catch(() => null),
        this.dashboardService.getKpis().toPromise().catch(() => null),
        this.dashboardService.getAlertTrends(30).toPromise().catch(() => []),
        this.dashboardService.getScreeningMetrics(30).toPromise().catch(() => []),
        this.dashboardService.getReportStatusDistribution().toPromise().catch(() => []),
        this.dashboardService.getComplianceScoreHistory(6).toPromise().catch(() => []),
        this.dashboardService.getRecentActivities(10).toPromise().catch(() => [])
      ];

      const [
        overview,
        kpis,
        alertTrends,
        screeningMetrics,
        reportStatus,
        complianceScore,
        recentActivities
      ] = await Promise.all(promises);

      // Update signals with fallback data
      this.overview.set(overview as DashboardOverview || this.createFallbackOverview());
      this.kpis.set(kpis as DashboardKpis || this.createFallbackKpis());
      this.recentActivities.set(recentActivities as RecentActivity[] || []);

      // Process chart data with fallbacks
      this.processAlertTrends(alertTrends as ChartData[] || this.createFallbackTrendData());
      this.processScreeningMetrics(screeningMetrics as ChartData[] || this.createFallbackMetricsData());
      this.processReportStatus(reportStatus as ChartData[] || this.createFallbackStatusData());
      this.processComplianceScore(complianceScore as ChartData[] || this.createFallbackComplianceData());

    } catch (error) {
      console.error('Error loading dashboard data:', error);
      // Set fallback data
      this.overview.set(this.createFallbackOverview());
      this.kpis.set(this.createFallbackKpis());
      this.recentActivities.set([]);
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

  formatDate(date: string | Date): string {
    if (!date) return 'N/A';
    const d = typeof date === 'string' ? new Date(date) : date;
    return d.toLocaleDateString() + ' ' + d.toLocaleTimeString([], { hour: '2-digit', minute: '2-digit' });
  }

  private createFallbackOverview(): DashboardOverview {
    return {
      totalCustomers: 0,
      totalAlerts: 0,
      pendingAlerts: 0,
      highRiskAlerts: 0,
      totalSars: 0,
      totalStrs: 0,
      pendingSars: 0,
      pendingStrs: 0,
      alertsTrend: 0,
      sarsTrend: 0,
      strsTrend: 0,
      complianceScore: 95,
      lastUpdated: new Date()
    };
  }

  private createFallbackKpis(): DashboardKpis {
    return {
      totalAlerts: 0,
      pendingAlerts: 0,
      highRiskAlerts: 0,
      alertsTrend: 0,
      totalReports: 0,
      pendingReports: 0,
      complianceScore: 95,
      averageResolutionTime: 0,
      accuracyRate: 0,
      totalScreenings: 0,
      matchRate: 0
    };
  }

  private createFallbackTrendData(): ChartData[] {
    const last7Days = [];
    for (let i = 6; i >= 0; i--) {
      const date = new Date();
      date.setDate(date.getDate() - i);
      last7Days.push({
        label: date.toLocaleDateString(),
        value: 0
      });
    }
    return last7Days;
  }

  private createFallbackMetricsData(): ChartData[] {
    return [
      { label: 'PEP Screenings', value: 0 },
      { label: 'Sanctions Screenings', value: 0 },
      { label: 'Adverse Media', value: 0 }
    ];
  }

  private createFallbackStatusData(): ChartData[] {
    return [
      { label: 'Pending Review', value: 0 },
      { label: 'Under Review', value: 0 },
      { label: 'Approved', value: 0 },
      { label: 'Rejected', value: 0 }
    ];
  }

  private createFallbackComplianceData(): ChartData[] {
    const last6Months = [];
    for (let i = 5; i >= 0; i--) {
      const date = new Date();
      date.setMonth(date.getMonth() - i);
      last6Months.push({
        label: date.toLocaleDateString('en-US', { month: 'short', year: 'numeric' }),
        value: 95
      });
    }
    return last6Months;
  }
}