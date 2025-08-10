import { Injectable, inject } from '@angular/core';
import { HttpClient } from '@angular/common/http';
import { Observable } from 'rxjs';
import { environment } from '../../environments/environment';

export interface McaDirectorRequest {
  name?: string;
  cin?: string;
  companyName?: string;
  din?: string; // Director Identification Number
}

export interface McaDirector {
  din: string;
  name: string;
  companyName: string;
  cin: string;
  designation: string;
  appointmentDate: string;
  status: string;
  panNumber?: string;
  address?: string;
  nationality: string;
  riskLevel: string;
}

export interface McaCompany {
  cin: string;
  companyName: string;
  status: string;
  incorporationDate: string;
  directors: McaDirector[];
  authorizedCapital: number;
  paidUpCapital: number;
}

@Injectable({ providedIn: 'root' })
export class McaService {
  private http = inject(HttpClient);
  private baseUrl = `${environment.apiBaseUrl}/mca`;

  searchDirectors(request: McaDirectorRequest): Observable<McaDirector[]> {
    return this.http.post<McaDirector[]>(`${this.baseUrl}/directors/search`, request);
  }

  getCompanyDirectors(cin: string): Observable<McaCompany> {
    return this.http.get<McaCompany>(`${this.baseUrl}/company/${cin}/directors`);
  }

  getDirectorDetails(din: string): Observable<McaDirector> {
    return this.http.get<McaDirector>(`${this.baseUrl}/director/${din}`);
  }
}