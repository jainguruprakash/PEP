import { inject, Injectable } from '@angular/core';
import { HttpClient } from '@angular/common/http';
import { environment } from '../../environments/environment';
import { Observable } from 'rxjs';

@Injectable({ providedIn: 'root' })
export class ScreeningService {
  private readonly http = inject(HttpClient);
  private readonly baseUrl = `${environment.apiBaseUrl}/screening`;

  screenCustomer(payload: any): Observable<any> {
    return this.http.post(`${this.baseUrl}/customer`, payload);
  }

  screenTransaction(payload: any): Observable<any> {
    return this.http.post(`${this.baseUrl}/transaction`, payload);
  }

  search(payload: any): Observable<any> {
    return this.http.post(`${this.baseUrl}/search`, payload);
  }

  getStatistics(startDate: string, endDate: string): Observable<any> {
    return this.http.get(`${this.baseUrl}/statistics`, { params: { startDate, endDate } });
  }
}


