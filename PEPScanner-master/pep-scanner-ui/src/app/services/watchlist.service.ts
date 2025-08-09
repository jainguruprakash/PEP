import { inject, Injectable } from '@angular/core';
import { HttpClient } from '@angular/common/http';
import { environment } from '../../environments/environment';
import { Observable } from 'rxjs';

@Injectable({ providedIn: 'root' })
export class WatchlistService {
  private readonly http = inject(HttpClient);
  private readonly baseUrl = `${environment.apiBaseUrl}`;

  getGenericSources(): Observable<any> {
    return this.http.get(`${this.baseUrl}/genericwatchlist/sources`);
  }

  searchAcross(name: string): Observable<any> {
    return this.http.get(`${this.baseUrl}/genericwatchlist/search`, { params: { name } });
  }

  searchSpecific(source: string, name: string): Observable<any> {
    return this.http.get(`${this.baseUrl}/genericwatchlist/search/${source}`, { params: { name } });
  }

  updateSource(source: string): Observable<any> {
    return this.http.post(`${this.baseUrl}/genericwatchlist/update/${source}`, {});
  }

  updateAll(): Observable<any> {
    return this.http.post(`${this.baseUrl}/genericwatchlist/update-all`, {});
  }

  getLastUpdates(): Observable<any> {
    return this.http.get(`${this.baseUrl}/genericwatchlist/last-updates`);
  }

  // OFAC
  updateOfac(): Observable<any> { return this.http.post(`${this.baseUrl}/watchlist/ofac/update`, {}); }
  searchOfac(name: string): Observable<any> { return this.http.get(`${this.baseUrl}/watchlist/ofac/search`, { params: { name } }); }

  // UN
  updateUn(): Observable<any> { return this.http.post(`${this.baseUrl}/watchlist/un/update`, {}); }
  searchUn(name: string): Observable<any> { return this.http.get(`${this.baseUrl}/watchlist/un/search`, { params: { name } }); }

  // RBI
  updateRbi(): Observable<any> { return this.http.post(`${this.baseUrl}/watchlist/rbi/update`, {}); }
  scrapeAdvancedRbi(): Observable<any> { return this.http.post(`${this.baseUrl}/watchlist/rbi/scrape-advanced`, {}); }
  searchRbi(name: string): Observable<any> { return this.http.get(`${this.baseUrl}/watchlist/rbi/search`, { params: { name } }); }
  searchRbiByCategory(category: string): Observable<any> { return this.http.get(`${this.baseUrl}/watchlist/rbi/search-by-category`, { params: { category } }); }
  uploadRbi(file: File): Observable<any> {
    const form = new FormData();
    form.append('file', file);
    return this.http.post(`${this.baseUrl}/watchlist/rbi/upload`, form);
  }

  // SEBI
  updateSebi(): Observable<any> { return this.http.post(`${this.baseUrl}/watchlist/sebi/update`, {}); }
  scrapeAdvancedSebi(): Observable<any> { return this.http.post(`${this.baseUrl}/watchlist/sebi/scrape-advanced`, {}); }
  searchSebi(name: string): Observable<any> { return this.http.get(`${this.baseUrl}/watchlist/sebi/search`, { params: { name } }); }
  searchSebiByCategory(category: string): Observable<any> { return this.http.get(`${this.baseUrl}/watchlist/sebi/search-by-category`, { params: { category } }); }
  uploadSebi(file: File): Observable<any> {
    const form = new FormData();
    form.append('file', file);
    return this.http.post(`${this.baseUrl}/watchlist/sebi/upload`, form);
  }

  // Parliament
  updateParliament(): Observable<any> { return this.http.post(`${this.baseUrl}/watchlist/parliament/update`, {}); }
  scrapeAdvancedParliament(): Observable<any> { return this.http.post(`${this.baseUrl}/watchlist/parliament/scrape-advanced`, {}); }
  searchParliament(name: string): Observable<any> { return this.http.get(`${this.baseUrl}/watchlist/parliament/search`, { params: { name } }); }
  searchParliamentByCategory(category: string): Observable<any> { return this.http.get(`${this.baseUrl}/watchlist/parliament/search-by-category`, { params: { category } }); }
}


