import { Injectable, inject } from '@angular/core';
import { HttpClient } from '@angular/common/http';
import { Observable } from 'rxjs';
import { environment } from '../../environments/environment';

export interface AdverseMediaScanRequest {
  entityName: string;
  entityType: 'person' | 'company' | 'both';
  dateRange: string;
  riskThreshold: string;
  riskCategories: string[];
  mediaSources: string[];
  languages: string[];
}

export interface AdverseMediaResult {
  id: string;
  headline: string;
  source: string;
  publishedDate: Date;
  riskScore: number;
  riskCategories: string[];
  excerpt: string;
  fullText?: string;
  url: string;
  sentiment: 'positive' | 'negative' | 'neutral';
  entities: string[];
  keywords: string[];
}

export interface MonitoringEntity {
  id: string;
  entityName: string;
  frequency: 'realtime' | 'hourly' | 'daily' | 'weekly';
  alertThreshold: number;
  lastScan: Date;
  alertsCount: number;
  status: 'active' | 'paused' | 'inactive';
  createdAt: Date;
}

@Injectable({
  providedIn: 'root'
})
export class AdverseMediaService {
  private http = inject(HttpClient);
  private baseUrl = `${environment.apiBaseUrl}/api/adverse-media`;

  // Real-time scanning
  performScan(request: AdverseMediaScanRequest): Observable<AdverseMediaResult[]> {
    return this.http.post<AdverseMediaResult[]>(`${this.baseUrl}/scan`, request);
  }

  // Get scan history
  getScanHistory(entityName?: string): Observable<AdverseMediaResult[]> {
    const params: any = {};
    if (entityName) {
      params.entityName = entityName;
    }
    return this.http.get<AdverseMediaResult[]>(`${this.baseUrl}/history`, { params });
  }

  // Continuous monitoring
  addToMonitoring(entity: Partial<MonitoringEntity>): Observable<MonitoringEntity> {
    return this.http.post<MonitoringEntity>(`${this.baseUrl}/monitoring`, entity);
  }

  getMonitoredEntities(): Observable<MonitoringEntity[]> {
    return this.http.get<MonitoringEntity[]>(`${this.baseUrl}/monitoring`);
  }

  updateMonitoring(id: string, updates: Partial<MonitoringEntity>): Observable<MonitoringEntity> {
    return this.http.put<MonitoringEntity>(`${this.baseUrl}/monitoring/${id}`, updates);
  }

  removeFromMonitoring(id: string): Observable<void> {
    return this.http.delete<void>(`${this.baseUrl}/monitoring/${id}`);
  }

  // AI Analytics
  getSentimentAnalysis(entityName: string, dateRange: string): Observable<any> {
    return this.http.get(`${this.baseUrl}/analytics/sentiment`, {
      params: { entityName, dateRange }
    });
  }

  getRiskTrends(entityName: string): Observable<any> {
    return this.http.get(`${this.baseUrl}/analytics/risk-trends`, {
      params: { entityName }
    });
  }

  getPredictiveRisk(entityName: string): Observable<any> {
    return this.http.get(`${this.baseUrl}/analytics/predictive-risk`, {
      params: { entityName }
    });
  }

  // Future-ready features
  getEntityNetwork(entityName: string): Observable<any> {
    return this.http.get(`${this.baseUrl}/network-analysis`, {
      params: { entityName }
    });
  }

  getComplianceScore(entityName: string): Observable<any> {
    return this.http.get(`${this.baseUrl}/compliance-score`, {
      params: { entityName }
    });
  }

  // Bulk operations
  bulkScan(entities: string[]): Observable<any> {
    return this.http.post(`${this.baseUrl}/bulk-scan`, { entities });
  }

  // Export functionality
  exportResults(format: 'csv' | 'excel' | 'pdf', filters?: any): Observable<Blob> {
    return this.http.post(`${this.baseUrl}/export`, 
      { format, filters }, 
      { responseType: 'blob' }
    );
  }
}
