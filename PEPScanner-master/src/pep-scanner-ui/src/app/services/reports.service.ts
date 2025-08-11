import { Injectable, inject } from '@angular/core';
import { HttpClient, HttpParams } from '@angular/common/http';
import { Observable } from 'rxjs';

export enum ReportStatus {
  Draft = 0,
  UnderReview = 1,
  RequiresMoreInfo = 2,
  Approved = 3,
  Submitted = 4,
  Filed = 5,
  Rejected = 6,
  Closed = 7
}

export enum ReportPriority {
  Low = 0,
  Medium = 1,
  High = 2,
  Critical = 3
}

export interface SuspiciousActivityReport {
  id: string;
  reportNumber: string;
  organizationId: string;
  reportedById: string;
  reviewedById?: string;
  customerId?: string;
  subjectName: string;
  subjectAddress?: string;
  subjectIdentification?: string;
  subjectDateOfBirth?: Date;
  suspiciousActivity: string;
  activityDescription: string;
  transactionAmount?: number;
  transactionCurrency?: string;
  transactionDate?: Date;
  transactionLocation?: string;
  status: ReportStatus;
  priority: ReportPriority;
  incidentDate?: Date;
  discoveryDate?: Date;
  regulatoryReferences?: string;
  attachedDocuments?: string;
  internalNotes?: string;
  complianceComments?: string;
  submissionDate?: Date;
  regulatoryFilingDate?: Date;
  regulatoryReferenceNumber?: string;
  createdAt: Date;
  updatedAt: Date;
}

export interface SuspiciousTransactionReport {
  id: string;
  reportNumber: string;
  organizationId: string;
  reportedById: string;
  reviewedById?: string;
  customerId?: string;
  transactionReference: string;
  transactionAmount: number;
  transactionCurrency: string;
  transactionDate: Date;
  transactionType: string;
  originatorName?: string;
  originatorAccount?: string;
  originatorBank?: string;
  beneficiaryName?: string;
  beneficiaryAccount?: string;
  beneficiaryBank?: string;
  suspicionReason: string;
  detailedDescription: string;
  countryOfOrigin?: string;
  countryOfDestination?: string;
  status: ReportStatus;
  priority: ReportPriority;
  regulatoryReferences?: string;
  attachedDocuments?: string;
  internalNotes?: string;
  complianceComments?: string;
  submissionDate?: Date;
  regulatoryFilingDate?: Date;
  regulatoryReferenceNumber?: string;
  createdAt: Date;
  updatedAt: Date;
}

export interface ReportComment {
  id: string;
  userId: string;
  comment: string;
  createdAt: Date;
}

export interface ReportStatusHistory {
  id: string;
  fromStatus: ReportStatus;
  toStatus: ReportStatus;
  changedById: string;
  reason?: string;
  changedAt: Date;
}

export interface CreateSarRequest {
  customerId?: string;
  subjectName: string;
  subjectAddress?: string;
  subjectIdentification?: string;
  subjectDateOfBirth?: Date;
  suspiciousActivity: string;
  activityDescription: string;
  transactionAmount?: number;
  transactionCurrency?: string;
  transactionDate?: Date;
  transactionLocation?: string;
  priority: ReportPriority;
  incidentDate?: Date;
  discoveryDate?: Date;
  regulatoryReferences?: string;
  internalNotes?: string;
}

export interface CreateStrRequest {
  customerId?: string;
  transactionReference: string;
  transactionAmount: number;
  transactionCurrency: string;
  transactionDate: Date;
  transactionType: string;
  originatorName?: string;
  originatorAccount?: string;
  originatorBank?: string;
  beneficiaryName?: string;
  beneficiaryAccount?: string;
  beneficiaryBank?: string;
  suspicionReason: string;
  detailedDescription: string;
  countryOfOrigin?: string;
  countryOfDestination?: string;
  priority: ReportPriority;
  regulatoryReferences?: string;
  internalNotes?: string;
}

export interface UpdateReportStatusRequest {
  status: ReportStatus;
  reason?: string;
  complianceComments?: string;
}

export interface ReportSearchParams {
  status?: ReportStatus;
  priority?: ReportPriority;
  reportedById?: string;
  reviewedById?: string;
  customerId?: string;
  fromDate?: Date;
  toDate?: Date;
  searchTerm?: string;
  page?: number;
  pageSize?: number;
}

@Injectable({
  providedIn: 'root'
})
export class ReportsService {
  private http = inject(HttpClient);
  private readonly API_BASE = 'http://localhost:5098/api';

  // SAR Methods
  getSars(params?: ReportSearchParams): Observable<{ items: SuspiciousActivityReport[]; totalCount: number }> {
    let httpParams = new HttpParams();
    if (params) {
      Object.keys(params).forEach(key => {
        const value = (params as any)[key];
        if (value !== undefined && value !== null) {
          httpParams = httpParams.set(key, value.toString());
        }
      });
    }
    return this.http.get<{ items: SuspiciousActivityReport[]; totalCount: number }>(`${this.API_BASE}/reports/sars`, { params: httpParams });
  }

  getSar(id: string): Observable<SuspiciousActivityReport> {
    return this.http.get<SuspiciousActivityReport>(`${this.API_BASE}/reports/sars/${id}`);
  }

  createSar(request: CreateSarRequest): Observable<SuspiciousActivityReport> {
    return this.http.post<SuspiciousActivityReport>(`${this.API_BASE}/reports/sars`, request);
  }

  updateSar(id: string, request: Partial<CreateSarRequest>): Observable<SuspiciousActivityReport> {
    return this.http.put<SuspiciousActivityReport>(`${this.API_BASE}/reports/sars/${id}`, request);
  }

  updateSarStatus(id: string, request: UpdateReportStatusRequest): Observable<SuspiciousActivityReport> {
    return this.http.patch<SuspiciousActivityReport>(`${this.API_BASE}/reports/sars/${id}/status`, request);
  }

  deleteSar(id: string): Observable<void> {
    return this.http.delete<void>(`${this.API_BASE}/reports/sars/${id}`);
  }

  // STR Methods
  getStrs(params?: ReportSearchParams): Observable<{ items: SuspiciousTransactionReport[]; totalCount: number }> {
    let httpParams = new HttpParams();
    if (params) {
      Object.keys(params).forEach(key => {
        const value = (params as any)[key];
        if (value !== undefined && value !== null) {
          httpParams = httpParams.set(key, value.toString());
        }
      });
    }
    return this.http.get<{ items: SuspiciousTransactionReport[]; totalCount: number }>(`${this.API_BASE}/reports/strs`, { params: httpParams });
  }

  getStr(id: string): Observable<SuspiciousTransactionReport> {
    return this.http.get<SuspiciousTransactionReport>(`${this.API_BASE}/reports/strs/${id}`);
  }

  createStr(request: CreateStrRequest): Observable<SuspiciousTransactionReport> {
    return this.http.post<SuspiciousTransactionReport>(`${this.API_BASE}/reports/strs`, request);
  }

  updateStr(id: string, request: Partial<CreateStrRequest>): Observable<SuspiciousTransactionReport> {
    return this.http.put<SuspiciousTransactionReport>(`${this.API_BASE}/reports/strs/${id}`, request);
  }

  updateStrStatus(id: string, request: UpdateReportStatusRequest): Observable<SuspiciousTransactionReport> {
    return this.http.patch<SuspiciousTransactionReport>(`${this.API_BASE}/reports/strs/${id}/status`, request);
  }

  deleteStr(id: string): Observable<void> {
    return this.http.delete<void>(`${this.API_BASE}/reports/strs/${id}`);
  }

  // Comments
  getSarComments(sarId: string): Observable<ReportComment[]> {
    return this.http.get<ReportComment[]>(`${this.API_BASE}/reports/sars/${sarId}/comments`);
  }

  addSarComment(sarId: string, comment: string): Observable<ReportComment> {
    return this.http.post<ReportComment>(`${this.API_BASE}/reports/sars/${sarId}/comments`, { comment });
  }

  getStrComments(strId: string): Observable<ReportComment[]> {
    return this.http.get<ReportComment[]>(`${this.API_BASE}/reports/strs/${strId}/comments`);
  }

  addStrComment(strId: string, comment: string): Observable<ReportComment> {
    return this.http.post<ReportComment>(`${this.API_BASE}/reports/strs/${strId}/comments`, { comment });
  }

  // Status History
  getSarStatusHistory(sarId: string): Observable<ReportStatusHistory[]> {
    return this.http.get<ReportStatusHistory[]>(`${this.API_BASE}/reports/sars/${sarId}/status-history`);
  }

  getStrStatusHistory(strId: string): Observable<ReportStatusHistory[]> {
    return this.http.get<ReportStatusHistory[]>(`${this.API_BASE}/reports/strs/${strId}/status-history`);
  }

  // Utility Methods
  getStatusText(status: ReportStatus): string {
    return ReportStatus[status];
  }

  getPriorityText(priority: ReportPriority): string {
    return ReportPriority[priority];
  }

  getStatusColor(status: ReportStatus): string {
    switch (status) {
      case ReportStatus.Draft:
        return 'primary';
      case ReportStatus.UnderReview:
        return 'accent';
      case ReportStatus.RequiresMoreInfo:
        return 'warn';
      case ReportStatus.Approved:
        return 'primary';
      case ReportStatus.Submitted:
        return 'primary';
      case ReportStatus.Rejected:
        return 'warn';
      default:
        return 'primary';
    }
  }

  getPriorityColor(priority: ReportPriority): string {
    switch (priority) {
      case ReportPriority.Low:
        return 'primary';
      case ReportPriority.Medium:
        return 'accent';
      case ReportPriority.High:
        return 'warn';
      case ReportPriority.Critical:
        return 'warn';
      default:
        return 'primary';
    }
  }
}
