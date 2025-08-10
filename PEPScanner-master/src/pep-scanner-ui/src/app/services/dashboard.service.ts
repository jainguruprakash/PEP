import { Injectable, inject } from '@angular/core';
import { HttpClient } from '@angular/common/http';
import { Observable } from 'rxjs';

export interface DashboardOverview {
  totalCustomers: number;
  totalAlerts: number;
  pendingAlerts: number;
  highRiskAlerts: number;
  totalSars: number;
  totalStrs: number;
  pendingSars: number;
  pendingStrs: number;
  alertsTrend: number;
  sarsTrend: number;
  strsTrend: number;
  complianceScore: number;
  lastUpdated: Date;
}

export interface ChartData {
  label: string;
  value: number;
  date?: Date;
  category?: string;
  additionalData?: { [key: string]: any };
}

export interface RecentActivity {
  id: string;
  type: string;
  title: string;
  description: string;
  timestamp: Date;
  priority: string;
  url: string;
}

export interface PerformanceMetrics {
  averageResolutionTimeHours: number;
  totalScreenings: number;
  totalMatches: number;
  accuracyRate: number;
  matchRate: number;
}

export interface DashboardKpis {
  totalAlerts: number;
  pendingAlerts: number;
  highRiskAlerts: number;
  alertsTrend: number;
  totalReports: number;
  pendingReports: number;
  complianceScore: number;
  averageResolutionTime: number;
  accuracyRate: number;
  totalScreenings: number;
  matchRate: number;
}

export interface DashboardWidgets {
  overview: DashboardOverview;
  recentActivities: RecentActivity[];
  alertTrends: ChartData[];
  reportStatusDistribution: ChartData[];
}

@Injectable({
  providedIn: 'root'
})
export class DashboardService {
  private http = inject(HttpClient);
  private readonly API_BASE = 'http://localhost:5097/api';

  getDashboardOverview(): Observable<DashboardOverview> {
    return this.http.get<DashboardOverview>(`${this.API_BASE}/dashboard/overview`);
  }

  getAlertTrends(days: number = 30): Observable<ChartData[]> {
    return this.http.get<ChartData[]>(`${this.API_BASE}/dashboard/charts/alert-trends?days=${days}`);
  }

  getScreeningMetrics(days: number = 30): Observable<ChartData[]> {
    return this.http.get<ChartData[]>(`${this.API_BASE}/dashboard/charts/screening-metrics?days=${days}`);
  }

  getReportStatusDistribution(): Observable<ChartData[]> {
    return this.http.get<ChartData[]>(`${this.API_BASE}/dashboard/charts/report-status`);
  }

  getComplianceScoreHistory(months: number = 6): Observable<ChartData[]> {
    return this.http.get<ChartData[]>(`${this.API_BASE}/dashboard/charts/compliance-score?months=${months}`);
  }

  getRecentActivities(count: number = 10): Observable<RecentActivity[]> {
    return this.http.get<RecentActivity[]>(`${this.API_BASE}/dashboard/recent-activities?count=${count}`);
  }

  getPerformanceMetrics(): Observable<PerformanceMetrics> {
    return this.http.get<PerformanceMetrics>(`${this.API_BASE}/dashboard/performance`);
  }

  getKpis(): Observable<DashboardKpis> {
    return this.http.get<DashboardKpis>(`${this.API_BASE}/dashboard/kpis`);
  }

  getWidgets(): Observable<DashboardWidgets> {
    return this.http.get<DashboardWidgets>(`${this.API_BASE}/dashboard/widgets`);
  }

  refreshMetrics(): Observable<any> {
    return this.http.post(`${this.API_BASE}/dashboard/refresh-metrics`, {});
  }
}
