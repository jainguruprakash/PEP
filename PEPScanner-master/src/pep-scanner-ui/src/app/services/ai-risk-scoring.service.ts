import { Injectable, inject } from '@angular/core';
import { HttpClient } from '@angular/common/http';
import { Observable } from 'rxjs';
import { environment } from '../../environments/environment';

export interface AIRiskAssessment {
  customerId: string;
  customerName: string;
  riskScore: number;
  confidenceLevel: number;
  riskFactors: RiskFactor[];
  predictiveInsights: PredictiveRiskInsight;
  recommendedActions: RecommendedAction[];
  calculatedAt: Date;
  modelVersion: string;
  riskTrend: string;
  componentScores: RiskComponentScores;
}

export interface RiskFactor {
  category: string;
  weight: number;
  description: string;
  evidence: Evidence[];
}

export interface Evidence {
  type: string;
  value: string;
  date?: Date;
  source: string;
}

export interface PredictiveRiskInsight {
  predictedRiskIn30Days: number;
  predictedRiskIn90Days: number;
  riskTrend: string;
  keyDrivers: string[];
  earlyWarningSignals: string[];
  recommendedMonitoringFrequency: string;
}

export interface RecommendedAction {
  priority: string;
  action: string;
  reason: string;
  timeline: string;
  assignTo: string;
}

export interface RiskComponentScores {
  baseRisk: number;
  mediaRisk: number;
  behavioralRisk: number;
  networkRisk: number;
}

export interface RiskTrendAnalysis {
  trend: string;
  trendStrength: number;
  dataPoints: number;
  averageRisk: number;
  minRisk: number;
  maxRisk: number;
  volatility: number;
}

@Injectable({
  providedIn: 'root'
})
export class AIRiskScoringService {
  private http = inject(HttpClient);
  private baseUrl = `${environment.apiBaseUrl}/ai-risk-scoring`;

  // Calculate AI risk score for a customer
  calculateRiskScore(customerId: string): Observable<AIRiskAssessment> {
    return this.http.post<AIRiskAssessment>(`${this.baseUrl}/calculate/${customerId}`, {});
  }

  // Get predictive insights for a customer
  getPredictiveInsights(customerId: string): Observable<PredictiveRiskInsight> {
    return this.http.get<PredictiveRiskInsight>(`${this.baseUrl}/insights/${customerId}`);
  }

  // Get risk trend analysis
  getRiskTrend(customerId: string, daysBack: number = 90): Observable<RiskTrendAnalysis> {
    return this.http.get<RiskTrendAnalysis>(`${this.baseUrl}/trend/${customerId}?daysBack=${daysBack}`);
  }

  // Get recommended actions based on risk assessment
  getRecommendedActions(customerId: string): Observable<RecommendedAction[]> {
    return this.http.get<RecommendedAction[]>(`${this.baseUrl}/actions/${customerId}`);
  }

  // Batch calculate risk scores for multiple customers
  batchCalculateRiskScores(customerIds: string[]): Observable<AIRiskAssessment[]> {
    return this.http.post<AIRiskAssessment[]>(`${this.baseUrl}/batch-calculate`, { customerIds });
  }

  // Get historical risk assessments for a customer
  getHistoricalAssessments(customerId: string): Observable<AIRiskAssessment[]> {
    return this.http.get<AIRiskAssessment[]>(`${this.baseUrl}/history/${customerId}`);
  }

  // Get risk factors analysis
  getRiskFactors(customerId: string): Observable<RiskFactor[]> {
    return this.http.get<RiskFactor[]>(`${this.baseUrl}/factors/${customerId}`);
  }

  // Get risk level label based on score
  getRiskLevel(riskScore: number): string {
    if (riskScore >= 90) return 'Critical';
    if (riskScore >= 75) return 'High';
    if (riskScore >= 50) return 'Medium';
    if (riskScore >= 25) return 'Low';
    return 'Minimal';
  }

  // Get risk color based on score
  getRiskColor(riskScore: number): string {
    if (riskScore >= 90) return '#d32f2f'; // Critical - Red
    if (riskScore >= 75) return '#f57c00'; // High - Orange
    if (riskScore >= 50) return '#fbc02d'; // Medium - Yellow
    if (riskScore >= 25) return '#388e3c'; // Low - Green
    return '#1976d2'; // Minimal - Blue
  }

  // Get priority color for recommended actions
  getPriorityColor(priority: string): string {
    switch (priority.toLowerCase()) {
      case 'critical': return '#d32f2f';
      case 'high': return '#f57c00';
      case 'medium': return '#fbc02d';
      case 'low': return '#388e3c';
      default: return '#757575';
    }
  }

  // Get trend icon based on trend direction
  getTrendIcon(trend: string): string {
    switch (trend.toLowerCase()) {
      case 'increasing': return 'trending_up';
      case 'decreasing': return 'trending_down';
      case 'stable': return 'trending_flat';
      default: return 'help_outline';
    }
  }

  // Get trend color based on trend direction
  getTrendColor(trend: string): string {
    switch (trend.toLowerCase()) {
      case 'increasing': return '#d32f2f'; // Red for increasing risk
      case 'decreasing': return '#388e3c'; // Green for decreasing risk
      case 'stable': return '#1976d2'; // Blue for stable risk
      default: return '#757575';
    }
  }

  // Format confidence level for display
  formatConfidenceLevel(confidence: number): string {
    if (confidence >= 90) return 'Very High';
    if (confidence >= 75) return 'High';
    if (confidence >= 60) return 'Medium';
    if (confidence >= 40) return 'Low';
    return 'Very Low';
  }

  // Get monitoring frequency recommendation
  getMonitoringFrequency(riskScore: number): string {
    if (riskScore >= 90) return 'Daily';
    if (riskScore >= 75) return 'Weekly';
    if (riskScore >= 50) return 'Bi-weekly';
    if (riskScore >= 25) return 'Monthly';
    return 'Quarterly';
  }

  // Calculate risk score change percentage
  calculateRiskChange(currentScore: number, previousScore: number): number {
    if (previousScore === 0) return 0;
    return ((currentScore - previousScore) / previousScore) * 100;
  }

  // Format risk change for display
  formatRiskChange(change: number): string {
    const absChange = Math.abs(change);
    const direction = change > 0 ? '↑' : change < 0 ? '↓' : '→';
    return `${direction} ${absChange.toFixed(1)}%`;
  }

  // Get risk change color
  getRiskChangeColor(change: number): string {
    if (change > 5) return '#d32f2f'; // Red for significant increase
    if (change > 0) return '#f57c00'; // Orange for slight increase
    if (change < -5) return '#388e3c'; // Green for significant decrease
    if (change < 0) return '#4caf50'; // Light green for slight decrease
    return '#757575'; // Gray for no change
  }
}
