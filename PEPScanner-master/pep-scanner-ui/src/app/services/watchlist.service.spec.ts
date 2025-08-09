import { TestBed } from '@angular/core/testing';
import { HttpClientTestingModule, HttpTestingController } from '@angular/common/http/testing';
import { WatchlistService } from './watchlist.service';
import { environment } from '../../environments/environment';

describe('WatchlistService', () => {
  let service: WatchlistService;
  let httpMock: HttpTestingController;
  const baseUrl = environment.apiBaseUrl;

  beforeEach(() => {
    TestBed.configureTestingModule({
      imports: [HttpClientTestingModule],
      providers: [WatchlistService]
    });
    service = TestBed.inject(WatchlistService);
    httpMock = TestBed.inject(HttpTestingController);
  });

  afterEach(() => {
    httpMock.verify();
  });

  it('should be created', () => {
    expect(service).toBeTruthy();
  });

  describe('Generic Watchlist Operations', () => {
    it('should get generic sources', () => {
      const mockSources = [
        { name: 'OFAC', lastUpdated: '2024-01-15T10:00:00Z', status: 'Active' },
        { name: 'UN', lastUpdated: '2024-01-14T15:30:00Z', status: 'Active' }
      ];

      service.getGenericSources().subscribe(sources => {
        expect(sources).toEqual(mockSources);
      });

      const req = httpMock.expectOne(`${baseUrl}/genericwatchlist/sources`);
      expect(req.request.method).toBe('GET');
      req.flush(mockSources);
    });

    it('should search across all watchlists', () => {
      const searchName = 'John Doe';
      const mockResults = {
        results: [
          { name: 'John Doe', source: 'OFAC', riskLevel: 'High' },
          { name: 'John A. Doe', source: 'UN', riskLevel: 'Medium' }
        ],
        totalCount: 2
      };

      service.searchAcross(searchName).subscribe(results => {
        expect(results).toEqual(mockResults);
      });

      const req = httpMock.expectOne(`${baseUrl}/genericwatchlist/search?name=${encodeURIComponent(searchName)}`);
      expect(req.request.method).toBe('GET');
      req.flush(mockResults);
    });

    it('should search specific watchlist source', () => {
      const source = 'OFAC';
      const searchName = 'Jane Smith';
      const mockResults = {
        results: [
          { name: 'Jane Smith', source: 'OFAC', riskLevel: 'High', details: 'SDN List' }
        ],
        totalCount: 1
      };

      service.searchSpecific(source, searchName).subscribe(results => {
        expect(results).toEqual(mockResults);
      });

      const req = httpMock.expectOne(`${baseUrl}/genericwatchlist/search/${source}?name=${encodeURIComponent(searchName)}`);
      expect(req.request.method).toBe('GET');
      req.flush(mockResults);
    });

    it('should update specific source', () => {
      const source = 'OFAC';
      const mockResponse = { success: true, message: 'OFAC updated successfully' };

      service.updateSource(source).subscribe(response => {
        expect(response).toEqual(mockResponse);
      });

      const req = httpMock.expectOne(`${baseUrl}/genericwatchlist/update/${source}`);
      expect(req.request.method).toBe('POST');
      expect(req.request.body).toEqual({});
      req.flush(mockResponse);
    });

    it('should update all sources', () => {
      const mockResponse = { success: true, message: 'All sources updated successfully' };

      service.updateAll().subscribe(response => {
        expect(response).toEqual(mockResponse);
      });

      const req = httpMock.expectOne(`${baseUrl}/genericwatchlist/update-all`);
      expect(req.request.method).toBe('POST');
      expect(req.request.body).toEqual({});
      req.flush(mockResponse);
    });

    it('should get last updates', () => {
      const mockUpdates = {
        OFAC: '2024-01-15T10:00:00Z',
        UN: '2024-01-14T15:30:00Z',
        RBI: '2024-01-13T09:15:00Z'
      };

      service.getLastUpdates().subscribe(updates => {
        expect(updates).toEqual(mockUpdates);
      });

      const req = httpMock.expectOne(`${baseUrl}/genericwatchlist/last-updates`);
      expect(req.request.method).toBe('GET');
      req.flush(mockUpdates);
    });
  });

  describe('OFAC Operations', () => {
    it('should update OFAC watchlist', () => {
      const mockResponse = { success: true, recordsUpdated: 1500 };

      service.updateOfac().subscribe(response => {
        expect(response).toEqual(mockResponse);
      });

      const req = httpMock.expectOne(`${baseUrl}/watchlist/ofac/update`);
      expect(req.request.method).toBe('POST');
      req.flush(mockResponse);
    });

    it('should search OFAC watchlist', () => {
      const searchName = 'Terrorist Name';
      const mockResults = {
        results: [
          { name: 'Terrorist Name', program: 'SDN', uid: '12345' }
        ]
      };

      service.searchOfac(searchName).subscribe(results => {
        expect(results).toEqual(mockResults);
      });

      const req = httpMock.expectOne(`${baseUrl}/watchlist/ofac/search?name=${encodeURIComponent(searchName)}`);
      expect(req.request.method).toBe('GET');
      req.flush(mockResults);
    });
  });

  describe('UN Operations', () => {
    it('should update UN watchlist', () => {
      const mockResponse = { success: true, recordsUpdated: 800 };

      service.updateUn().subscribe(response => {
        expect(response).toEqual(mockResponse);
      });

      const req = httpMock.expectOne(`${baseUrl}/watchlist/un/update`);
      expect(req.request.method).toBe('POST');
      req.flush(mockResponse);
    });

    it('should search UN watchlist', () => {
      const searchName = 'Sanctioned Person';
      const mockResults = {
        results: [
          { name: 'Sanctioned Person', listType: 'Al-Qaida', referenceNumber: 'QDi.001' }
        ]
      };

      service.searchUn(searchName).subscribe(results => {
        expect(results).toEqual(mockResults);
      });

      const req = httpMock.expectOne(`${baseUrl}/watchlist/un/search?name=${encodeURIComponent(searchName)}`);
      expect(req.request.method).toBe('GET');
      req.flush(mockResults);
    });
  });

  describe('RBI Operations', () => {
    it('should update RBI watchlist', () => {
      const mockResponse = { success: true, recordsUpdated: 200 };

      service.updateRbi().subscribe(response => {
        expect(response).toEqual(mockResponse);
      });

      const req = httpMock.expectOne(`${baseUrl}/watchlist/rbi/update`);
      expect(req.request.method).toBe('POST');
      req.flush(mockResponse);
    });

    it('should scrape advanced RBI data', () => {
      const mockResponse = { success: true, recordsScraped: 150 };

      service.scrapeAdvancedRbi().subscribe(response => {
        expect(response).toEqual(mockResponse);
      });

      const req = httpMock.expectOne(`${baseUrl}/watchlist/rbi/scrape-advanced`);
      expect(req.request.method).toBe('POST');
      req.flush(mockResponse);
    });

    it('should search RBI watchlist', () => {
      const searchName = 'Defaulter Name';
      const mockResults = {
        results: [
          { name: 'Defaulter Name', category: 'Wilful Defaulter', amount: 50000000 }
        ]
      };

      service.searchRbi(searchName).subscribe(results => {
        expect(results).toEqual(mockResults);
      });

      const req = httpMock.expectOne(`${baseUrl}/watchlist/rbi/search?name=${encodeURIComponent(searchName)}`);
      expect(req.request.method).toBe('GET');
      req.flush(mockResults);
    });

    it('should search RBI by category', () => {
      const category = 'Wilful Defaulter';
      const mockResults = {
        results: [
          { name: 'Company A', category: 'Wilful Defaulter', amount: 100000000 },
          { name: 'Company B', category: 'Wilful Defaulter', amount: 75000000 }
        ]
      };

      service.searchRbiByCategory(category).subscribe(results => {
        expect(results).toEqual(mockResults);
      });

      const req = httpMock.expectOne(`${baseUrl}/watchlist/rbi/search-by-category?category=${encodeURIComponent(category)}`);
      expect(req.request.method).toBe('GET');
      req.flush(mockResults);
    });
  });

  describe('Error Handling', () => {
    it('should handle search errors gracefully', () => {
      const searchName = 'Test Name';

      service.searchAcross(searchName).subscribe({
        next: () => fail('Expected error'),
        error: (error) => {
          expect(error.status).toBe(500);
        }
      });

      const req = httpMock.expectOne(`${baseUrl}/genericwatchlist/search?name=${encodeURIComponent(searchName)}`);
      req.flush('Server Error', { status: 500, statusText: 'Internal Server Error' });
    });

    it('should handle update errors gracefully', () => {
      service.updateAll().subscribe({
        next: () => fail('Expected error'),
        error: (error) => {
          expect(error.status).toBe(503);
        }
      });

      const req = httpMock.expectOne(`${baseUrl}/genericwatchlist/update-all`);
      req.flush('Service Unavailable', { status: 503, statusText: 'Service Unavailable' });
    });

    it('should handle empty search results', () => {
      const searchName = 'NonExistent Name';
      const mockResults = { results: [], totalCount: 0 };

      service.searchAcross(searchName).subscribe(results => {
        expect(results.results).toEqual([]);
        expect(results.totalCount).toBe(0);
      });

      const req = httpMock.expectOne(`${baseUrl}/genericwatchlist/search?name=${encodeURIComponent(searchName)}`);
      req.flush(mockResults);
    });
  });
});
