import { TestBed } from '@angular/core/testing';
import { HttpClientTestingModule, HttpTestingController } from '@angular/common/http/testing';
import { ScreeningService } from './screening.service';
import { environment } from '../../environments/environment';

describe('ScreeningService', () => {
  let service: ScreeningService;
  let httpMock: HttpTestingController;
  const baseUrl = `${environment.apiBaseUrl}/screening`;

  beforeEach(() => {
    TestBed.configureTestingModule({
      imports: [HttpClientTestingModule],
      providers: [ScreeningService]
    });
    service = TestBed.inject(ScreeningService);
    httpMock = TestBed.inject(HttpTestingController);
  });

  afterEach(() => {
    httpMock.verify();
  });

  it('should be created', () => {
    expect(service).toBeTruthy();
  });

  describe('screenCustomer', () => {
    it('should send POST request to screen customer endpoint', () => {
      const mockPayload = {
        fullName: 'John Doe',
        dateOfBirth: '1990-01-01',
        nationality: 'US'
      };
      const mockResponse = {
        customerId: '123',
        hasMatches: false,
        riskScore: 0.1
      };

      service.screenCustomer(mockPayload).subscribe(response => {
        expect(response).toEqual(mockResponse);
      });

      const req = httpMock.expectOne(`${baseUrl}/customer`);
      expect(req.request.method).toBe('POST');
      expect(req.request.body).toEqual(mockPayload);
      req.flush(mockResponse);
    });

    it('should handle error response for customer screening', () => {
      const mockPayload = { fullName: 'John Doe' };
      const errorMessage = 'Screening failed';

      service.screenCustomer(mockPayload).subscribe({
        next: () => fail('Expected error'),
        error: (error) => {
          expect(error.status).toBe(500);
        }
      });

      const req = httpMock.expectOne(`${baseUrl}/customer`);
      req.flush(errorMessage, { status: 500, statusText: 'Internal Server Error' });
    });
  });

  describe('screenTransaction', () => {
    it('should send POST request to screen transaction endpoint', () => {
      const mockPayload = {
        transactionId: 'TXN123',
        amount: 10000,
        beneficiaryName: 'Jane Smith'
      };
      const mockResponse = {
        transactionId: 'TXN123',
        hasMatches: true,
        riskScore: 0.8
      };

      service.screenTransaction(mockPayload).subscribe(response => {
        expect(response).toEqual(mockResponse);
      });

      const req = httpMock.expectOne(`${baseUrl}/transaction`);
      expect(req.request.method).toBe('POST');
      expect(req.request.body).toEqual(mockPayload);
      req.flush(mockResponse);
    });
  });

  describe('search', () => {
    it('should send POST request to search endpoint', () => {
      const mockPayload = {
        searchTerm: 'John',
        searchType: 'name'
      };
      const mockResponse = {
        results: [
          { name: 'John Doe', source: 'OFAC', riskLevel: 'High' }
        ],
        totalCount: 1
      };

      service.search(mockPayload).subscribe(response => {
        expect(response).toEqual(mockResponse);
      });

      const req = httpMock.expectOne(`${baseUrl}/search`);
      expect(req.request.method).toBe('POST');
      expect(req.request.body).toEqual(mockPayload);
      req.flush(mockResponse);
    });
  });

  describe('getStatistics', () => {
    it('should send GET request to statistics endpoint with date parameters', () => {
      const startDate = '2024-01-01';
      const endDate = '2024-01-31';
      const mockResponse = {
        alertCount: 25,
        customersScreened: 150,
        averageRisk: 0.3
      };

      service.getStatistics(startDate, endDate).subscribe(response => {
        expect(response).toEqual(mockResponse);
      });

      const req = httpMock.expectOne(`${baseUrl}/statistics?startDate=${startDate}&endDate=${endDate}`);
      expect(req.request.method).toBe('GET');
      req.flush(mockResponse);
    });

    it('should handle empty statistics response', () => {
      const startDate = '2024-01-01';
      const endDate = '2024-01-31';
      const mockResponse = {
        alertCount: 0,
        customersScreened: 0,
        averageRisk: 0
      };

      service.getStatistics(startDate, endDate).subscribe(response => {
        expect(response.alertCount).toBe(0);
        expect(response.customersScreened).toBe(0);
        expect(response.averageRisk).toBe(0);
      });

      const req = httpMock.expectOne(`${baseUrl}/statistics?startDate=${startDate}&endDate=${endDate}`);
      req.flush(mockResponse);
    });
  });
});
