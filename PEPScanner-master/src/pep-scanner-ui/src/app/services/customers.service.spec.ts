import { TestBed } from '@angular/core/testing';
import { HttpClientTestingModule, HttpTestingController } from '@angular/common/http/testing';
import { CustomersService } from './customers.service';
import { environment } from '../../environments/environment';

describe('CustomersService', () => {
  let service: CustomersService;
  let httpMock: HttpTestingController;
  const baseUrl = `${environment.apiBaseUrl}/customers`;

  beforeEach(() => {
    TestBed.configureTestingModule({
      imports: [HttpClientTestingModule],
      providers: [CustomersService]
    });
    service = TestBed.inject(CustomersService);
    httpMock = TestBed.inject(HttpTestingController);
  });

  afterEach(() => {
    httpMock.verify();
  });

  it('should be created', () => {
    expect(service).toBeTruthy();
  });

  describe('getAll', () => {
    it('should retrieve all customers', () => {
      const mockCustomers = [
        {
          id: '1',
          fullName: 'John Doe',
          dateOfBirth: '1990-01-01',
          nationality: 'US',
          riskLevel: 'Low'
        },
        {
          id: '2',
          fullName: 'Jane Smith',
          dateOfBirth: '1985-05-15',
          nationality: 'UK',
          riskLevel: 'Medium'
        }
      ];

      service.getAll().subscribe(customers => {
        expect(customers.length).toBe(2);
        expect(customers).toEqual(mockCustomers);
      });

      const req = httpMock.expectOne(baseUrl);
      expect(req.request.method).toBe('GET');
      req.flush(mockCustomers);
    });

    it('should handle empty customer list', () => {
      service.getAll().subscribe(customers => {
        expect(customers).toEqual([]);
      });

      const req = httpMock.expectOne(baseUrl);
      req.flush([]);
    });

    it('should handle error when retrieving customers', () => {
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

  describe('getById', () => {
    it('should retrieve customer by id', () => {
      const customerId = '123';
      const mockCustomer = {
        id: customerId,
        fullName: 'John Doe',
        dateOfBirth: '1990-01-01',
        nationality: 'US',
        riskLevel: 'Low'
      };

      service.getById(customerId).subscribe(customer => {
        expect(customer).toEqual(mockCustomer);
      });

      const req = httpMock.expectOne(`${baseUrl}/${customerId}`);
      expect(req.request.method).toBe('GET');
      req.flush(mockCustomer);
    });

    it('should handle customer not found', () => {
      const customerId = '999';

      service.getById(customerId).subscribe({
        next: () => fail('Expected error'),
        error: (error) => {
          expect(error.status).toBe(404);
        }
      });

      const req = httpMock.expectOne(`${baseUrl}/${customerId}`);
      req.flush('Customer not found', { status: 404, statusText: 'Not Found' });
    });
  });

  describe('create', () => {
    it('should create a new customer', () => {
      const newCustomer = {
        fullName: 'Alice Johnson',
        dateOfBirth: '1992-03-20',
        nationality: 'CA',
        email: 'alice@example.com'
      };
      const createdCustomer = {
        id: '456',
        ...newCustomer,
        riskLevel: 'Low',
        createdDate: '2024-01-15T10:00:00Z'
      };

      service.create(newCustomer).subscribe(customer => {
        expect(customer).toEqual(createdCustomer);
      });

      const req = httpMock.expectOne(baseUrl);
      expect(req.request.method).toBe('POST');
      expect(req.request.body).toEqual(newCustomer);
      req.flush(createdCustomer);
    });

    it('should handle validation errors when creating customer', () => {
      const invalidCustomer = {
        fullName: '', // Invalid empty name
        dateOfBirth: 'invalid-date'
      };

      service.create(invalidCustomer).subscribe({
        next: () => fail('Expected error'),
        error: (error) => {
          expect(error.status).toBe(400);
        }
      });

      const req = httpMock.expectOne(baseUrl);
      req.flush('Validation failed', { status: 400, statusText: 'Bad Request' });
    });
  });

  describe('update', () => {
    it('should update existing customer', () => {
      const customerId = '123';
      const updateData = {
        fullName: 'John Updated',
        email: 'john.updated@example.com'
      };

      service.update(customerId, updateData).subscribe(response => {
        expect(response).toBeUndefined(); // Void response
      });

      const req = httpMock.expectOne(`${baseUrl}/${customerId}`);
      expect(req.request.method).toBe('PUT');
      expect(req.request.body).toEqual(updateData);
      req.flush(null);
    });

    it('should handle customer not found during update', () => {
      const customerId = '999';
      const updateData = { fullName: 'Updated Name' };

      service.update(customerId, updateData).subscribe({
        next: () => fail('Expected error'),
        error: (error) => {
          expect(error.status).toBe(404);
        }
      });

      const req = httpMock.expectOne(`${baseUrl}/${customerId}`);
      req.flush('Customer not found', { status: 404, statusText: 'Not Found' });
    });
  });

  describe('delete', () => {
    it('should delete customer', () => {
      const customerId = '123';

      service.delete(customerId).subscribe(response => {
        expect(response).toBeUndefined(); // Void response
      });

      const req = httpMock.expectOne(`${baseUrl}/${customerId}`);
      expect(req.request.method).toBe('DELETE');
      req.flush(null);
    });

    it('should handle customer not found during delete', () => {
      const customerId = '999';

      service.delete(customerId).subscribe({
        next: () => fail('Expected error'),
        error: (error) => {
          expect(error.status).toBe(404);
        }
      });

      const req = httpMock.expectOne(`${baseUrl}/${customerId}`);
      req.flush('Customer not found', { status: 404, statusText: 'Not Found' });
    });

    it('should handle delete conflict error', () => {
      const customerId = '123';

      service.delete(customerId).subscribe({
        next: () => fail('Expected error'),
        error: (error) => {
          expect(error.status).toBe(409);
        }
      });

      const req = httpMock.expectOne(`${baseUrl}/${customerId}`);
      req.flush('Cannot delete customer with active alerts', { status: 409, statusText: 'Conflict' });
    });
  });
});
