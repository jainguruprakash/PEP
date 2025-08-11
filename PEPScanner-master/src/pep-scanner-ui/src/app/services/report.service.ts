import { Injectable, inject } from '@angular/core';
import { HttpClient } from '@angular/common/http';
import { Observable } from 'rxjs';
import { environment } from '../../environments/environment';

@Injectable({ providedIn: 'root' })
export class ReportService {
  private readonly http = inject(HttpClient);
  private readonly baseUrl = `${environment.apiBaseUrl}/reports`;

  generateReport(data: any, format: 'pdf' | 'excel'): Observable<Blob> {
    return this.http.post(`${this.baseUrl}/generate/${format}`, data, {
      responseType: 'blob'
    });
  }

  generateBulkReport(results: any[], format: 'pdf' | 'excel'): Observable<Blob> {
    return this.http.post(`${this.baseUrl}/bulk/${format}`, { results }, {
      responseType: 'blob'
    });
  }

  generateSampleTemplate(data: any[], format: 'excel'): Observable<Blob> {
    return this.http.post(`${this.baseUrl}/template/${format}`, { data }, {
      responseType: 'blob'
    });
  }

  downloadReport(blob: Blob, fileName: string) {
    const url = URL.createObjectURL(blob);
    const link = document.createElement('a');
    link.href = url;
    link.download = fileName;
    link.click();
    URL.revokeObjectURL(url);
  }
}