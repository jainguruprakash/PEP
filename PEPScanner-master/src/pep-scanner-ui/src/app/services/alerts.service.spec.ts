import { TestBed } from '@angular/core/testing';
import { HttpClientTestingModule, HttpTestingController } from '@angular/common/http/testing';
import { AlertsService } from './alerts.service';
import { environment } from '../../environments/environment';

describe('AlertsService', () => {
  let service: AlertsService;
  let httpMock: HttpTestingController;
  const baseUrl = `${environment.apiBaseUrl}/alerts`;

  beforeEach(() => {
    TestBed.configureTestingModule({
      imports: [HttpClientTestingModule],
      providers: [AlertsService]
    });
    service = TestBed.inject(AlertsService);
    httpMock = TestBed.inject(HttpTestingController);
  });

  afterEach(() => {
    httpMock.verify();
  });

  it('should be created', () => {
    expect(service).toBeTruthy();
  });

  describe('getAll', () => {
    it('should retrieve all alerts', () => {
      const mockAlerts = [
        {
          id: '1',
          customerId: 'CUST001',
          customerName: 'John Doe',
          alertType: 'OFAC_MATCH',
          riskScore: 0.85,
          status: 'PENDING',
          createdDate: '2024-01-15T10:00:00Z'
        },
        {
          id: '2',
          customerId: 'CUST002',
          customerName: 'Jane Smith',
          alertType: 'UN_SANCTIONS',
          riskScore: 0.92,
          status: 'REVIEWED',
          createdDate: '2024-01-14T15:30:00Z'
        }
      ];

      service.getAll().subscribe(alerts => {
        expect(alerts.length).toBe(2);
        expect(alerts).toEqual(mockAlerts);
      });

      const req = httpMock.expectOne(baseUrl);
      expect(req.request.method).toBe('GET');
      req.flush(mockAlerts);
    });

    it('should handle empty alerts list', () => {
      service.getAll().subscribe(alerts => {
        expect(alerts).toEqual([]);
      });

      const req = httpMock.expectOne(baseUrl);
      req.flush([]);
    });

    it('should handle error when retrieving alerts', () => {
      service.getAll().subscribe({
        next: () => fail('Expected error'),
        error: (error) => {
          expect(error.status).toBe(500);
        }
      });

      const req = httpMock.expectOne(baseUrl);
      req.flush('Server Error', { status: 500, statusText: 'Internal Server Error' });
    });
  });

  describe('getByStatus', () => {
    it('should retrieve alerts by status', () => {
      const status = 'PENDING';
      const mockAlerts = [
        {
          id: '1',
          customerId: 'CUST001',
          customerName: 'John Doe',
          alertType: 'OFAC_MATCH',
          riskScore: 0.85,
          status: 'PENDING',
          createdDate: '2024-01-15T10:00:00Z'
        }
      ];

      service.getByStatus(status).subscribe(alerts => {
        expect(alerts.length).toBe(1);
        expect(alerts[0].status).toBe('PENDING');
      });

      const req = httpMock.expectOne(`${baseUrl}/status/${status}`);
      expect(req.request.method).toBe('GET');
      req.flush(mockAlerts);
    });

    it('should handle different status values', () => {
      const statuses = ['PENDING', 'REVIEWED', 'CLOSED', 'FALSE_POSITIVE'];
      
      statuses.forEach(status => {
        const mockAlerts = [
          { id: '1', status: status, customerName: 'Test Customer' }
        ];

        service.getByStatus(status).subscribe(alerts => {
          expect(alerts[0].status).toBe(status);
        });

        const req = httpMock.expectOne(`${baseUrl}/status/${status}`);
        req.flush(mockAlerts);
      });
    });

    it('should handle empty results for status filter', () => {
      const status = 'NONEXISTENT_STATUS';

      service.getByStatus(status).subscribe(alerts => {
        expect(alerts).toEqual([]);
      });

      const req = httpMock.expectOne(`${baseUrl}/status/${status}`);
      req.flush([]);
    });

    it('should handle URL encoding for status parameter', () => {
      const status = 'PENDING REVIEW'; // Status with space

      service.getByStatus(status).subscribe();

      const req = httpMock.expectOne(`${baseUrl}/status/${status}`);
      expect(req.request.url).toContain('PENDING%20REVIEW');
      req.flush([]);
    });
  });

  describe('getById', () => {
    it('should retrieve alert by id', () => {
      const alertId = '123';
      const mockAlert = {
        id: alertId,
        customerId: 'CUST001',
        customerName: 'John Doe',
        alertType: 'OFAC_MATCH',
        riskScore: 0.85,
        status: 'PENDING',
        createdDate: '2024-01-15T10:00:00Z',
        matchDetails: {
          matchedName: 'John A. Doe',
          source: 'OFAC SDN',
          confidence: 0.85
        }
      };

      service.getById(alertId).subscribe(alert => {
        expect(alert).toEqual(mockAlert);
        expect(alert.id).toBe(alertId);
      });

      const req = httpMock.expectOne(`${baseUrl}/${alertId}`);
      expect(req.request.method).toBe('GET');
      req.flush(mockAlert);
    });

    it('should handle alert not found', () => {
      const alertId = '999';

      service.getById(alertId).subscribe({
        next: () => fail('Expected error'),
        error: (error) => {
          expect(error.status).toBe(404);
        }
      });

      const req = httpMock.expectOne(`${baseUrl}/${alertId}`);
      req.flush('Alert not found', { status: 404, statusText: 'Not Found' });
    });

    it('should handle malformed alert id', () => {
      const alertId = 'invalid-id';

      service.getById(alertId).subscribe({
        next: () => fail('Expected error'),
        error: (error) => {
          expect(error.status).toBe(400);
        }
      });

      const req = httpMock.expectOne(`${baseUrl}/${alertId}`);
      req.flush('Invalid alert ID format', { status: 400, statusText: 'Bad Request' });
    });
  });

  describe('update', () => {
    it('should update alert successfully', () => {
      const alertId = '123';
      const updatePayload = {
        status: 'REVIEWED',
        reviewerNotes: 'Reviewed and cleared',
        reviewedBy: 'analyst@example.com',
        reviewedDate: '2024-01-16T09:00:00Z'
      };

      service.update(alertId, updatePayload).subscribe(response => {
        expect(response).toBeUndefined(); // Void response
      });

      const req = httpMock.expectOne(`${baseUrl}/${alertId}`);
      expect(req.request.method).toBe('PUT');
      expect(req.request.body).toEqual(updatePayload);
      req.flush(null);
    });

    it('should handle different update scenarios', () => {
      const alertId = '123';
      const updateScenarios = [
        {
          payload: { status: 'FALSE_POSITIVE', reason: 'Name similarity only' },
          description: 'false positive'
        },
        {
          payload: { status: 'CLOSED', resolution: 'Customer cleared after investigation' },
          description: 'closure'
        },
        {
          payload: { status: 'ESCALATED', escalatedTo: 'senior-analyst@example.com' },
          description: 'escalation'
        }
      ];

      updateScenarios.forEach(scenario => {
        service.update(alertId, scenario.payload).subscribe();

        const req = httpMock.expectOne(`${baseUrl}/${alertId}`);
        expect(req.request.body).toEqual(scenario.payload);
        req.flush(null);
      });
    });

    it('should handle update validation errors', () => {
      const alertId = '123';
      const invalidPayload = {
        status: 'INVALID_STATUS',
        invalidField: 'invalid value'
      };

      service.update(alertId, invalidPayload).subscribe({
        next: () => fail('Expected error'),
        error: (error) => {
          expect(error.status).toBe(400);
        }
      });

      const req = httpMock.expectOne(`${baseUrl}/${alertId}`);
      req.flush('Validation failed', { status: 400, statusText: 'Bad Request' });
    });

    it('should handle alert not found during update', () => {
      const alertId = '999';
      const updatePayload = { status: 'REVIEWED' };

      service.update(alertId, updatePayload).subscribe({
        next: () => fail('Expected error'),
        error: (error) => {
          expect(error.status).toBe(404);
        }
      });

      const req = httpMock.expectOne(`${baseUrl}/${alertId}`);
      req.flush('Alert not found', { status: 404, statusText: 'Not Found' });
    });

    it('should handle concurrent update conflicts', () => {
      const alertId = '123';
      const updatePayload = { status: 'REVIEWED' };

      service.update(alertId, updatePayload).subscribe({
        next: () => fail('Expected error'),
        error: (error) => {
          expect(error.status).toBe(409);
        }
      });

      const req = httpMock.expectOne(`${baseUrl}/${alertId}`);
      req.flush('Alert was modified by another user', { status: 409, statusText: 'Conflict' });
    });
  });

  describe('Integration scenarios', () => {
    it('should handle network timeout', () => {
      service.getAll().subscribe({
        next: () => fail('Expected error'),
        error: (error) => {
          expect(error.name).toBe('TimeoutError');
        }
      });

      const req = httpMock.expectOne(baseUrl);
      req.error(new ProgressEvent('timeout'), { status: 0, statusText: 'Unknown Error' });
    });

    it('should handle malformed JSON response', () => {
      service.getAll().subscribe({
        next: () => fail('Expected error'),
        error: (error) => {
          expect(error.status).toBe(200);
        }
      });

      const req = httpMock.expectOne(baseUrl);
      req.flush('invalid json{', { status: 200, statusText: 'OK' });
    });
  });
});
