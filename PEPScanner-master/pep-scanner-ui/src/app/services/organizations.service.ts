import { inject, Injectable } from '@angular/core';
import { HttpClient } from '@angular/common/http';
import { environment } from '../../environments/environment';
import { Observable } from 'rxjs';

@Injectable({ providedIn: 'root' })
export class OrganizationsService {
  private readonly http = inject(HttpClient);
  private readonly baseUrl = `${environment.apiBaseUrl}/organizations`;

  list(): Observable<any[]> { return this.http.get<any[]>(`${this.baseUrl}`); }
  get(id: string): Observable<any> { return this.http.get<any>(`${this.baseUrl}/${id}`); }
  create(payload: any): Observable<any> { return this.http.post<any>(`${this.baseUrl}`, payload); }
  update(id: string, payload: any): Observable<void> { return this.http.put<void>(`${this.baseUrl}/${id}`, payload); }
  remove(id: string): Observable<void> { return this.http.delete<void>(`${this.baseUrl}/${id}`); }
  stats(id: string): Observable<any> { return this.http.get<any>(`${this.baseUrl}/${id}/statistics`); }
  users(id: string): Observable<any[]> { return this.http.get<any[]>(`${this.baseUrl}/${id}/users`); }
  watchlists(id: string): Observable<any[]> { return this.http.get<any[]>(`${this.baseUrl}/${id}/watchlists`); }
  configurations(id: string): Observable<any[]> { return this.http.get<any[]>(`${this.baseUrl}/${id}/configurations`); }
  search(q: string): Observable<any[]> { return this.http.get<any[]>(`${this.baseUrl}/search`, { params: { q } as any }); }
}


