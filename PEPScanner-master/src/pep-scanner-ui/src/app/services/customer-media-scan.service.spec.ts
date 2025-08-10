import { TestBed } from '@angular/core/testing';
import { HttpClientTestingModule, HttpTestingController } from '@angular/common/http/testing';
import { CustomerMediaScanService } from './customer-media-scan.service';
import { environment } from '../../environments/environment';

describe('CustomerMediaScanService', () => {
  let service: CustomerMediaScanService;
  let httpMock: HttpTestingController;
  const baseUrl = `${environment.apiBaseUrl}/customer-media-scan`;

  beforeEach(() => {
    TestBed.configureTestingModule({
      imports: [HttpClientTestingModule],
      providers: [CustomerMediaScanService]
    });
    service = TestBed.inject(CustomerMediaScanService);
    httpMock = TestBed.inject(HttpTestingController);
  });

  afterEach(() => {
    httpMock.verify();
  });

  it('should be created', () => {
    expect(service).toBeTruthy();
  });

  describe('scanCustomer', () => {
    it('should scan a single customer', () => {
      const customerId = '123';
      const mockResponse = {
        success: true,
        data: {
          customerId: '123',
          customerName: 'John Doe',
          status: 'Completed',
          mediaResultsFound: 5,
          alertsCreated: 2
        }
      };

      service.scanCustomer(customerId).subscribe(response => {
        expect(response).toEqual(mockResponse);
      });

      const req = httpMock.expectOne(`${baseUrl}/scan/${customerId}`);
      expect(req.request.method).toBe('POST');
      expect(req.request.body).toEqual({});
      req.flush(mockResponse);
    });

    it('should handle scan customer error', () => {
      const customerId = '123';

      service.scanCustomer(customerId).subscribe({
        next: () => fail('should have failed'),
        error: (error) => {
          expect(error.status).toBe(404);
        }
      });

      const req = httpMock.expectOne(`${baseUrl}/scan/${customerId}`);
      req.flush('Customer not found', { status: 404, statusText: 'Not Found' });
    });
  });

  describe('rescanCustomer', () => {
    it('should rescan a customer', () => {
      const customerId = '123';
      const mockResponse = {
        success: true,
        data: {
          customerId: '123',
          status: 'Completed'
        }
      };

      service.rescanCustomer(customerId).subscribe(response => {
        expect(response).toEqual(mockResponse);
      });

      const req = httpMock.expectOne(`${baseUrl}/rescan/${customerId}`);
      expect(req.request.method).toBe('POST');
      req.flush(mockResponse);
    });
  });

  describe('scanAllCustomers', () => {
    it('should scan all customers', () => {
      const mockResponse = {
        success: true,
        data: {
          totalCustomers: 100,
          successfulScans: 95,
          failedScans: 5,
          totalAlertsCreated: 20,
          status: 'Completed'
        }
      };

      service.scanAllCustomers().subscribe(response => {
        expect(response).toEqual(mockResponse);
        expect(response.data.totalCustomers).toBe(100);
        expect(response.data.successfulScans).toBe(95);
      });

      const req = httpMock.expectOne(`${baseUrl}/scan/bulk/all`);
      expect(req.request.method).toBe('POST');
      expect(req.request.body).toEqual({});
      req.flush(mockResponse);
    });

    it('should handle scan all customers error', () => {
      service.scanAllCustomers().subscribe({
        next: () => fail('should have failed'),
        error: (error) => {
          expect(error.status).toBe(500);
        }
      });

      const req = httpMock.expectOne(`${baseUrl}/scan/bulk/all`);
      req.flush('Internal Server Error', { status: 500, statusText: 'Internal Server Error' });
    });
  });

  describe('scanHighRiskCustomers', () => {
    it('should scan high risk customers', () => {
      const mockResponse = {
        success: true,
        data: {
          totalCustomers: 25,
          successfulScans: 24,
          failedScans: 1,
          totalAlertsCreated: 15,
          status: 'Completed'
        }
      };

      service.scanHighRiskCustomers().subscribe(response => {
        expect(response).toEqual(mockResponse);
        expect(response.data.totalCustomers).toBe(25);
        expect(response.data.totalAlertsCreated).toBe(15);
      });

      const req = httpMock.expectOne(`${baseUrl}/scan/bulk/high-risk`);
      expect(req.request.method).toBe('POST');
      req.flush(mockResponse);
    });
  });

  describe('scanCustomersBatch', () => {
    it('should scan customers in batch', () => {
      const customerIds = ['123', '456', '789'];
      const mockResponse = {
        success: true,
        data: {
          totalCustomers: 3,
          successfulScans: 3,
          failedScans: 0,
          totalAlertsCreated: 5,
          status: 'Completed'
        }
      };

      service.scanCustomersBatch(customerIds).subscribe(response => {
        expect(response).toEqual(mockResponse);
      });

      const req = httpMock.expectOne(`${baseUrl}/scan/bulk/batch`);
      expect(req.request.method).toBe('POST');
      expect(req.request.body).toEqual({ customerIds });
      req.flush(mockResponse);
    });

    it('should handle empty customer IDs array', () => {
      const customerIds: string[] = [];

      service.scanCustomersBatch(customerIds).subscribe({
        next: () => fail('should have failed'),
        error: (error) => {
          expect(error.status).toBe(400);
        }
      });

      const req = httpMock.expectOne(`${baseUrl}/scan/bulk/batch`);
      req.flush('Customer IDs are required', { status: 400, statusText: 'Bad Request' });
    });
  });

  describe('onDemandScan', () => {
    it('should perform on-demand scan for all customers', () => {
      const scanType = 'all';
      const mockResponse = {
        success: true,
        message: 'On-demand all scan completed',
        data: {
          totalCustomers: 100,
          successfulScans: 98,
          status: 'Completed'
        }
      };

      service.onDemandScan(scanType).subscribe(response => {
        expect(response).toEqual(mockResponse);
      });

      const req = httpMock.expectOne(`${baseUrl}/scan/on-demand`);
      expect(req.request.method).toBe('POST');
      expect(req.request.body).toEqual({
        scanType: 'all',
        customerIds: undefined
      });
      req.flush(mockResponse);
    });

    it('should perform on-demand batch scan with customer IDs', () => {
      const scanType = 'batch';
      const customerIds = ['123', '456'];
      const mockResponse = {
        success: true,
        message: 'On-demand batch scan completed',
        data: {
          totalCustomers: 2,
          successfulScans: 2,
          status: 'Completed'
        }
      };

      service.onDemandScan(scanType, customerIds).subscribe(response => {
        expect(response).toEqual(mockResponse);
      });

      const req = httpMock.expectOne(`${baseUrl}/scan/on-demand`);
      expect(req.request.body).toEqual({
        scanType: 'batch',
        customerIds: ['123', '456']
      });
      req.flush(mockResponse);
    });

    it('should handle invalid scan type', () => {
      const scanType = 'invalid';

      service.onDemandScan(scanType).subscribe({
        next: () => fail('should have failed'),
        error: (error) => {
          expect(error.status).toBe(400);
        }
      });

      const req = httpMock.expectOne(`${baseUrl}/scan/on-demand`);
      req.flush('Invalid scan type', { status: 400, statusText: 'Bad Request' });
    });
  });

  describe('getScanStatus', () => {
    it('should get scan status', () => {
      const mockResponse = {
        success: true,
        summary: {
          totalCustomers: 100,
          requiresRescan: 25,
          highRisk: 15,
          mediumRisk: 35,
          lowRisk: 50,
          neverScanned: 10
        },
        customers: [
          {
            customerId: '1',
            customerName: 'John Doe',
            riskLevel: 'High',
            lastScanDate: '2024-01-01',
            daysSinceLastScan: 30,
            requiresRescan: true
          }
        ]
      };

      service.getScanStatus().subscribe(response => {
        expect(response).toEqual(mockResponse);
        expect(response.summary.totalCustomers).toBe(100);
        expect(response.customers).toHaveLength(1);
      });

      const req = httpMock.expectOne(`${baseUrl}/status`);
      expect(req.request.method).toBe('GET');
      req.flush(mockResponse);
    });

    it('should handle get scan status error', () => {
      service.getScanStatus().subscribe({
        next: () => fail('should have failed'),
        error: (error) => {
          expect(error.status).toBe(500);
        }
      });

      const req = httpMock.expectOne(`${baseUrl}/status`);
      req.flush('Internal Server Error', { status: 500, statusText: 'Internal Server Error' });
    });
  });

  describe('getScheduleStatus', () => {
    it('should get schedule status', () => {
      const mockResponse = {
        success: true,
        schedules: {
          dailyHighRiskScan: {
            jobId: 'daily-high-risk-customer-scan',
            schedule: '0 2 * * *',
            enabled: true
          },
          weeklyAllCustomersScan: {
            jobId: 'weekly-all-customers-scan',
            schedule: '0 3 * * 0',
            enabled: true
          }
        }
      };

      service.getScheduleStatus().subscribe(response => {
        expect(response).toEqual(mockResponse);
        expect(response.schedules.dailyHighRiskScan.enabled).toBe(true);
      });

      const req = httpMock.expectOne(`${baseUrl}/schedule/status`);
      expect(req.request.method).toBe('GET');
      req.flush(mockResponse);
    });
  });

  describe('setupPeriodicScans', () => {
    it('should setup periodic scans', () => {
      const mockResponse = {
        success: true,
        message: 'Periodic scanning jobs scheduled successfully',
        schedules: {
          dailyHighRisk: 'Daily at 2:00 AM UTC',
          weeklyAll: 'Weekly on Sunday at 3:00 AM UTC',
          monthlyDormant: 'Monthly on 1st at 4:00 AM UTC'
        }
      };

      service.setupPeriodicScans().subscribe(response => {
        expect(response).toEqual(mockResponse);
        expect(response.message).toContain('scheduled successfully');
      });

      const req = httpMock.expectOne(`${baseUrl}/schedule/setup`);
      expect(req.request.method).toBe('POST');
      expect(req.request.body).toEqual({});
      req.flush(mockResponse);
    });

    it('should handle setup periodic scans error', () => {
      service.setupPeriodicScans().subscribe({
        next: () => fail('should have failed'),
        error: (error) => {
          expect(error.status).toBe(500);
        }
      });

      const req = httpMock.expectOne(`${baseUrl}/schedule/setup`);
      req.flush('Setup failed', { status: 500, statusText: 'Internal Server Error' });
    });
  });
});
