import { inject, Injectable } from '@angular/core';
import { HttpClient, HttpParams } from '@angular/common/http';
import { environment } from '../../environments/environment';
import { Observable } from 'rxjs';
import { map } from 'rxjs/operators';
import { AuthService } from './auth.service';

@Injectable({ providedIn: 'root' })
export class AlertsService {
  private readonly http = inject(HttpClient);
  private readonly authService = inject(AuthService);
  private readonly baseUrl = `${environment.apiBaseUrl}/alerts`;

  getAll(): Observable<any[]> {
    const user = this.authService.getCurrentUser();
    let params = new HttpParams();
    
    if (user?.organizationId) {
      params = params.set('organizationId', user.organizationId);
    }
    
    return this.http.get<any[]>(`${this.baseUrl}`, { params });
  }

  getByStatus(status: string): Observable<any[]> {
    const user = this.authService.getCurrentUser();
    let params = new HttpParams();
    
    if (user?.organizationId) {
      params = params.set('organizationId', user.organizationId);
    }
    
    return this.http.get<any[]>(`${this.baseUrl}/status/${status}`, { params });
  }

  getById(id: string): Observable<any> {
    return this.http.get<any>(`${this.baseUrl}/${id}`);
  }

  update(id: string, payload: any): Observable<void> {
    return this.http.put<void>(`${this.baseUrl}/${id}`, payload);
  }

  approve(id: string, request: { approvedBy: string; comments: string }): Observable<any> {
    return this.http.post(`${this.baseUrl}/${id}/approve`, request);
  }

  reject(id: string, request: { rejectedBy: string; reason: string }): Observable<any> {
    return this.http.post(`${this.baseUrl}/${id}/reject`, request);
  }

  assign(id: string, request: { assignedTo: string; assignedBy: string; comments?: string }): Observable<any> {
    return this.http.post(`${this.baseUrl}/${id}/assign`, request);
  }

  escalate(id: string, request: { escalatedBy: string; reason: string }): Observable<any> {
    return this.http.post(`${this.baseUrl}/${id}/escalate`, request);
  }

  getPendingApproval(): Observable<any[]> {
    const user = this.authService.getCurrentUser();
    let params = new HttpParams();
    
    if (user?.organizationId) {
      params = params.set('organizationId', user.organizationId);
    }
    
    return this.http.get<any[]>(`${this.baseUrl}/pending-approval`, { params });
  }

  // Alert Creation
  createFromScreening(alertData: any): Observable<any> {
    return this.http.post(`${this.baseUrl}/create-from-screening`, alertData);
  }

  createFromMedia(alertData: any): Observable<any> {
    return this.http.post(`${this.baseUrl}/create-from-media`, alertData);
  }

  bulkCreateFromMedia(bulkData: any): Observable<any> {
    return this.http.post(`${this.baseUrl}/bulk-create-from-media`, bulkData);
  }
}


