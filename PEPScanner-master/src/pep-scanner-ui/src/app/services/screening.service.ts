import { inject, Injectable } from '@angular/core';
import { HttpClient, HttpParams } from '@angular/common/http';
import { environment } from '../../environments/environment';
import { Observable } from 'rxjs';

export interface CustomerScreeningRequest {
  fullName: string;
  dateOfBirth?: string;
  nationality?: string;
  country?: string;
  identificationNumber?: string;
  identificationType?: string;
  threshold?: number;
  sources?: string[];
  listTypes?: string[];
  riskCategories?: string[];
  includeAliases?: boolean;
  includeFuzzyMatching?: boolean;
  includePhoneticMatching?: boolean;
}

export interface BatchScreeningRequest {
  customers: CustomerScreeningRequest[];
  threshold?: number;
  sources?: string[];
  listTypes?: string[];
  riskCategories?: string[];
}

export interface AdvancedSearchRequest {
  searchTerm: string;
  searchType: 'name' | 'id' | 'keyword' | 'fuzzy';
  threshold?: number;
  sources?: string[];
  listTypes?: string[];
  maxResults?: number;
  country?: string;
  riskCategory?: string;
}

export interface ScreeningResult {
  customerId?: string;
  customerName: string;
  status: string;
  riskScore: number;
  riskLevel: string;
  matches: ScreeningMatch[];
  screeningDate: Date;
  processingTime: number;
  sourcesSearched: string[];
  totalRecordsSearched: number;
}

export interface ScreeningMatch {
  matchId: string;
  matchedName: string;
  source: string;
  listType: string;
  riskCategory: string;
  matchScore: number;
  country?: string;
  dateOfBirth?: string;
  aliases?: string[];
  details: any;
}

export interface ScreeningStatistics {
  totalScreenings: number;
  alertCount: number;
  customersScreened: number;
  averageRisk: number;
  sourceBreakdown: { [source: string]: number };
  riskDistribution: { [risk: string]: number };
  processingTimeStats: {
    average: number;
    min: number;
    max: number;
  };
}

@Injectable({ providedIn: 'root' })
export class ScreeningService {
  private readonly http = inject(HttpClient);
  private readonly baseUrl = `${environment.apiBaseUrl}/screening`;

  // Single customer screening with advanced options
  screenCustomer(request: CustomerScreeningRequest): Observable<ScreeningResult> {
    return this.http.post<ScreeningResult>(`${this.baseUrl}/customer`, request);
  }

  // Batch customer screening
  screenBatchCustomers(request: BatchScreeningRequest): Observable<ScreeningResult[]> {
    return this.http.post<ScreeningResult[]>(`${this.baseUrl}/batch`, request);
  }

  // Advanced search across watchlists
  search(request: AdvancedSearchRequest): Observable<ScreeningMatch[]> {
    return this.http.post<ScreeningMatch[]>(`${this.baseUrl}/search`, request);
  }

  // Transaction screening
  screenTransaction(payload: any): Observable<any> {
    return this.http.post(`${this.baseUrl}/transaction`, payload);
  }

  // Get screening statistics
  getStatistics(startDate: string, endDate: string): Observable<ScreeningStatistics> {
    const params = new HttpParams()
      .set('startDate', startDate)
      .set('endDate', endDate);
    return this.http.get<ScreeningStatistics>(`${this.baseUrl}/statistics`, { params });
  }

  // Get available sources
  getAvailableSources(): Observable<string[]> {
    return this.http.get<string[]>(`${this.baseUrl}/sources`);
  }

  // Get available list types
  getAvailableListTypes(): Observable<string[]> {
    return this.http.get<string[]>(`${this.baseUrl}/list-types`);
  }

  // Get screening history for a customer
  getCustomerScreeningHistory(customerId: string): Observable<ScreeningResult[]> {
    return this.http.get<ScreeningResult[]>(`${this.baseUrl}/customer/${customerId}/history`);
  }

  // Export screening results
  exportResults(format: 'json' | 'csv' | 'excel', results: any): Observable<Blob> {
    return this.http.post(`${this.baseUrl}/export/${format}`, results, {
      responseType: 'blob'
    });
  }

  // Get match details
  getMatchDetails(matchId: string): Observable<any> {
    return this.http.get(`${this.baseUrl}/match/${matchId}`);
  }

  // Submit false positive feedback
  submitFalsePositive(matchId: string, reason: string): Observable<any> {
    return this.http.post(`${this.baseUrl}/false-positive`, {
      matchId,
      reason
    });
  }

  // Get real-time screening status
  getScreeningStatus(screeningId: string): Observable<any> {
    return this.http.get(`${this.baseUrl}/status/${screeningId}`);
  }
}


