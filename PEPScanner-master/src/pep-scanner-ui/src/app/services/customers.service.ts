import { inject, Injectable } from '@angular/core';
import { HttpClient } from '@angular/common/http';
import { environment } from '../../environments/environment';
import { Observable } from 'rxjs';

@Injectable({ providedIn: 'root' })
export class CustomersService {
  private readonly http = inject(HttpClient);
  private readonly baseUrl = `${environment.apiBaseUrl}/customers`;

  getAll(): Observable<any[]> { return this.http.get<any[]>(this.baseUrl); }
  getById(id: string): Observable<any> { return this.http.get<any>(`${this.baseUrl}/${id}`); }
  create(payload: any): Observable<any> { return this.http.post<any>(this.baseUrl, payload); }
  update(id: string, payload: any): Observable<void> { return this.http.put<void>(`${this.baseUrl}/${id}`, payload); }
  delete(id: string): Observable<void> { return this.http.delete<void>(`${this.baseUrl}/${id}`); }
}


