import { inject, Injectable } from '@angular/core';
import { HttpClient } from '@angular/common/http';
import { environment } from '../../environments/environment';
import { Observable } from 'rxjs';
import { map } from 'rxjs/operators';

@Injectable({ providedIn: 'root' })
export class AlertsService {
  private readonly http = inject(HttpClient);
  private readonly baseUrl = `${environment.apiBaseUrl}/alerts`;

  getAll(): Observable<any[]> {
    return this.http.get<any>(`${this.baseUrl}`).pipe(
      map((response: any) => response.alerts || [])
    );
  }

  getByStatus(status: string): Observable<any[]> {
    return this.http.get<any[]>(`${this.baseUrl}/status/${status}`);
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

  assign(id: string, request: { assignedTo: string; assignedBy: string }): Observable<any> {
    return this.http.post(`${this.baseUrl}/${id}/assign`, request);
  }

  getPendingApproval(): Observable<any[]> {
    return this.http.get<any[]>(`${this.baseUrl}/pending-approval`);
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


