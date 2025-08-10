import { Injectable, inject } from '@angular/core';
import { HttpClient } from '@angular/common/http';
import { Observable } from 'rxjs';
import { environment } from '../../environments/environment';

export interface CibilRequest {
  panNumber: string;
  fullName?: string;
  dateOfBirth?: string;
  mobileNumber?: string;
}

export interface CibilResponse {
  panNumber: string;
  creditScore: number;
  reportDate: string;
  accounts: CibilAccount[];
  enquiries: CibilEnquiry[];
  summary: CibilSummary;
  riskAnalysis: RiskAnalysis;
}

export interface CibilAccount {
  accountNumber: string;
  accountType: string;
  bankName: string;
  currentBalance: number;
  creditLimit: number;
  paymentStatus: string;
  dpd: number; // Days Past Due
  openDate: string;
  lastPaymentDate: string;
}

export interface CibilEnquiry {
  date: string;
  bankName: string;
  enquiryType: string;
  amount: number;
}

export interface CibilSummary {
  totalAccounts: number;
  activeAccounts: number;
  closedAccounts: number;
  totalCreditLimit: number;
  totalCurrentBalance: number;
  utilizationRatio: number;
}

export interface RiskAnalysis {
  riskLevel: 'LOW' | 'MEDIUM' | 'HIGH' | 'CRITICAL';
  riskScore: number;
  riskFactors: string[];
  recommendations: string[];
}

@Injectable({ providedIn: 'root' })
export class CibilService {
  private http = inject(HttpClient);
  private baseUrl = `${environment.apiBaseUrl}/cibil`;

  getCreditReport(request: CibilRequest): Observable<CibilResponse> {
    return this.http.post<CibilResponse>(`${this.baseUrl}/report`, request);
  }

  validatePAN(panNumber: string): Observable<{ valid: boolean; name?: string }> {
    return this.http.get<{ valid: boolean; name?: string }>(`${this.baseUrl}/validate-pan/${panNumber}`);
  }
}