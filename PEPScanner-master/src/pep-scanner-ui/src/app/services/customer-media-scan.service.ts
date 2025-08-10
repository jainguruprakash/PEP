import { Injectable, inject } from '@angular/core';
import { HttpClient } from '@angular/common/http';
import { Observable } from 'rxjs';
import { environment } from '../../environments/environment';

@Injectable({
  providedIn: 'root'
})
export class CustomerMediaScanService {
  private http = inject(HttpClient);
  private baseUrl = `${environment.apiBaseUrl}/customer-media-scan`;

  // Single customer scanning
  scanCustomer(customerId: string): Observable<any> {
    return this.http.post(`${this.baseUrl}/scan/${customerId}`, {});
  }

  rescanCustomer(customerId: string): Observable<any> {
    return this.http.post(`${this.baseUrl}/rescan/${customerId}`, {});
  }

  // Bulk scanning operations
  scanAllCustomers(): Observable<any> {
    return this.http.post(`${this.baseUrl}/scan/bulk/all`, {});
  }

  scanHighRiskCustomers(): Observable<any> {
    return this.http.post(`${this.baseUrl}/scan/bulk/high-risk`, {});
  }

  scanCustomersBatch(customerIds: string[]): Observable<any> {
    return this.http.post(`${this.baseUrl}/scan/bulk/batch`, { customerIds });
  }

  // On-demand scanning
  onDemandScan(scanType: string, customerIds?: string[]): Observable<any> {
    return this.http.post(`${this.baseUrl}/scan/on-demand`, {
      scanType,
      customerIds
    });
  }

  // Status and monitoring
  getScanStatus(): Observable<any> {
    return this.http.get(`${this.baseUrl}/status`);
  }

  getScheduleStatus(): Observable<any> {
    return this.http.get(`${this.baseUrl}/schedule/status`);
  }

  // Periodic scan management
  setupPeriodicScans(): Observable<any> {
    return this.http.post(`${this.baseUrl}/schedule/setup`, {});
  }
}
