import { ComponentFixture, TestBed } from '@angular/core/testing';
import { HttpClientTestingModule } from '@angular/common/http/testing';
import { NoopAnimationsModule } from '@angular/platform-browser/animations';
import { MatSnackBarModule } from '@angular/material/snack-bar';
import { of, throwError } from 'rxjs';

import { CustomerMediaScanComponent } from './customer-media-scan.component';
import { CustomerMediaScanService } from '../../services/customer-media-scan.service';

describe('CustomerMediaScanComponent', () => {
  let component: CustomerMediaScanComponent;
  let fixture: ComponentFixture<CustomerMediaScanComponent>;
  let mockScanService: jasmine.SpyObj<CustomerMediaScanService>;

  const mockScanStatus = {
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
      },
      {
        customerId: '2',
        customerName: 'Jane Smith',
        riskLevel: 'Medium',
        lastScanDate: '2024-01-15',
        daysSinceLastScan: 15,
        requiresRescan: false
      }
    ]
  };

  beforeEach(async () => {
    const scanServiceSpy = jasmine.createSpyObj('CustomerMediaScanService', [
      'getScanStatus',
      'scanHighRiskCustomers',
      'scanAllCustomers',
      'scanCustomer',
      'scanCustomersBatch',
      'setupPeriodicScans',
      'getScheduleStatus'
    ]);

    await TestBed.configureTestingModule({
      imports: [
        CustomerMediaScanComponent,
        HttpClientTestingModule,
        NoopAnimationsModule,
        MatSnackBarModule
      ],
      providers: [
        { provide: CustomerMediaScanService, useValue: scanServiceSpy }
      ]
    }).compileComponents();

    fixture = TestBed.createComponent(CustomerMediaScanComponent);
    component = fixture.componentInstance;
    mockScanService = TestBed.inject(CustomerMediaScanService) as jasmine.SpyObj<CustomerMediaScanService>;
  });

  it('should create', () => {
    expect(component).toBeTruthy();
  });

  it('should load scan status on initialization', () => {
    // Arrange
    mockScanService.getScanStatus.and.returnValue(of(mockScanStatus));

    // Act
    component.ngOnInit?.();

    // Assert
    expect(mockScanService.getScanStatus).toHaveBeenCalled();
    expect(component.scanStatus()).toEqual(mockScanStatus);
  });

  it('should filter customers by risk level', () => {
    // Arrange
    component.scanStatus.set(mockScanStatus);
    component.selectedRiskFilter = 'High';

    // Act
    const filtered = component.filteredCustomers();

    // Assert
    expect(filtered).toHaveLength(1);
    expect(filtered[0].riskLevel).toBe('High');
  });

  it('should return all customers when no filter is selected', () => {
    // Arrange
    component.scanStatus.set(mockScanStatus);
    component.selectedRiskFilter = '';

    // Act
    const filtered = component.filteredCustomers();

    // Assert
    expect(filtered).toHaveLength(2);
  });

  it('should scan high risk customers successfully', () => {
    // Arrange
    const mockResult = {
      success: true,
      data: {
        successfulScans: 15,
        totalAlertsCreated: 5,
        status: 'Completed'
      }
    };
    mockScanService.scanHighRiskCustomers.and.returnValue(of(mockResult));
    mockScanService.getScanStatus.and.returnValue(of(mockScanStatus));

    // Act
    component.scanHighRiskCustomers();

    // Assert
    expect(component.isScanning()).toBe(true);
    expect(component.currentScan()).toBeTruthy();
    expect(mockScanService.scanHighRiskCustomers).toHaveBeenCalled();
  });

  it('should handle scan high risk customers error', () => {
    // Arrange
    mockScanService.scanHighRiskCustomers.and.returnValue(throwError(() => new Error('Scan failed')));
    spyOn(console, 'error');

    // Act
    component.scanHighRiskCustomers();

    // Assert
    expect(component.isScanning()).toBe(false);
    expect(component.currentScan()).toBeNull();
    expect(console.error).toHaveBeenCalled();
  });

  it('should scan all customers successfully', () => {
    // Arrange
    const mockResult = {
      success: true,
      data: {
        successfulScans: 100,
        totalAlertsCreated: 20,
        status: 'Completed'
      }
    };
    mockScanService.scanAllCustomers.and.returnValue(of(mockResult));
    mockScanService.getScanStatus.and.returnValue(of(mockScanStatus));

    // Act
    component.scanAllCustomers();

    // Assert
    expect(component.isScanning()).toBe(true);
    expect(component.currentScan()).toBeTruthy();
    expect(mockScanService.scanAllCustomers).toHaveBeenCalled();
  });

  it('should scan single customer successfully', () => {
    // Arrange
    const customerId = '123';
    const mockResult = {
      success: true,
      data: {
        mediaResultsFound: 5,
        alertsCreated: 2,
        status: 'Completed'
      }
    };
    mockScanService.scanCustomer.and.returnValue(of(mockResult));
    mockScanService.getScanStatus.and.returnValue(of(mockScanStatus));

    // Act
    component.scanSingleCustomer(customerId);

    // Assert
    expect(mockScanService.scanCustomer).toHaveBeenCalledWith(customerId);
  });

  it('should scan selected customers successfully', () => {
    // Arrange
    const customerIds = ['1', '2', '3'];
    component.selectedCustomers.set(customerIds);
    const mockResult = {
      success: true,
      data: {
        successfulScans: 3,
        totalAlertsCreated: 1,
        status: 'Completed'
      }
    };
    mockScanService.scanCustomersBatch.and.returnValue(of(mockResult));
    mockScanService.getScanStatus.and.returnValue(of(mockScanStatus));

    // Act
    component.scanSelectedCustomers();

    // Assert
    expect(component.isScanning()).toBe(true);
    expect(mockScanService.scanCustomersBatch).toHaveBeenCalledWith(customerIds);
  });

  it('should not scan when no customers are selected', () => {
    // Arrange
    component.selectedCustomers.set([]);

    // Act
    component.scanSelectedCustomers();

    // Assert
    expect(mockScanService.scanCustomersBatch).not.toHaveBeenCalled();
  });

  it('should setup periodic scans successfully', () => {
    // Arrange
    const mockResult = { success: true, message: 'Scans configured' };
    mockScanService.setupPeriodicScans.and.returnValue(of(mockResult));

    // Act
    component.setupPeriodicScans();

    // Assert
    expect(mockScanService.setupPeriodicScans).toHaveBeenCalled();
  });

  it('should refresh status successfully', () => {
    // Arrange
    mockScanService.getScanStatus.and.returnValue(of(mockScanStatus));

    // Act
    component.refreshStatus();

    // Assert
    expect(mockScanService.getScanStatus).toHaveBeenCalled();
  });

  it('should toggle all customer selection', () => {
    // Arrange
    component.scanStatus.set(mockScanStatus);
    const event = { checked: true };

    // Act
    component.toggleAllSelection(event);

    // Assert
    expect(component.selectedCustomers()).toHaveLength(2);
    expect(component.selectedCustomers()).toContain('1');
    expect(component.selectedCustomers()).toContain('2');
  });

  it('should toggle individual customer selection', () => {
    // Arrange
    const customer = { customerId: '1', customerName: 'John Doe' };
    const event = { checked: true };

    // Act
    component.toggleCustomerSelection(customer, event);

    // Assert
    expect(component.selectedCustomers()).toContain('1');
  });

  it('should remove customer from selection when unchecked', () => {
    // Arrange
    component.selectedCustomers.set(['1', '2']);
    const customer = { customerId: '1', customerName: 'John Doe' };
    const event = { checked: false };

    // Act
    component.toggleCustomerSelection(customer, event);

    // Assert
    expect(component.selectedCustomers()).not.toContain('1');
    expect(component.selectedCustomers()).toContain('2');
  });

  it('should check if all customers are selected', () => {
    // Arrange
    component.scanStatus.set(mockScanStatus);
    component.selectedCustomers.set(['1', '2']);

    // Act
    const allSelected = component.isAllSelected();

    // Assert
    expect(allSelected).toBe(true);
  });

  it('should check if customers are partially selected', () => {
    // Arrange
    component.scanStatus.set(mockScanStatus);
    component.selectedCustomers.set(['1']);

    // Act
    const partiallySelected = component.isPartiallySelected();

    // Assert
    expect(partiallySelected).toBe(true);
  });

  it('should check if specific customer is selected', () => {
    // Arrange
    component.selectedCustomers.set(['1', '2']);
    const customer = { customerId: '1' };

    // Act
    const isSelected = component.isCustomerSelected(customer);

    // Assert
    expect(isSelected).toBe(true);
  });

  it('should return correct risk color', () => {
    expect(component.getRiskColor('High')).toBe('warn');
    expect(component.getRiskColor('Medium')).toBe('accent');
    expect(component.getRiskColor('Low')).toBe('primary');
    expect(component.getRiskColor('Unknown')).toBe('primary');
  });

  it('should return correct days class', () => {
    expect(component.getDaysClass(35)).toBe('days-overdue');
    expect(component.getDaysClass(20)).toBe('days-warning');
    expect(component.getDaysClass(5)).toBe('days-ok');
    expect(component.getDaysClass(null)).toBe('');
  });

  it('should format date correctly', () => {
    // Arrange
    const testDate = '2024-01-15T10:30:00Z';

    // Act
    const formatted = component.formatDate(testDate);

    // Assert
    expect(formatted).toBe(new Date(testDate).toLocaleDateString());
  });

  it('should view customer details', () => {
    // Arrange
    const customerId = '123';
    spyOn(window, 'open');

    // Act
    component.viewCustomerDetails(customerId);

    // Assert
    expect(window.open).toHaveBeenCalledWith('/customers/123', '_blank');
  });

  it('should view schedule status', () => {
    // Arrange
    const mockScheduleStatus = { success: true, schedules: {} };
    mockScanService.getScheduleStatus.and.returnValue(of(mockScheduleStatus));
    spyOn(console, 'log');

    // Act
    component.viewScheduleStatus();

    // Assert
    expect(mockScanService.getScheduleStatus).toHaveBeenCalled();
    expect(console.log).toHaveBeenCalledWith('Schedule status:', mockScheduleStatus);
  });

  it('should handle load scan status error', () => {
    // Arrange
    mockScanService.getScanStatus.and.returnValue(throwError(() => new Error('Load failed')));
    spyOn(console, 'error');

    // Act
    component.loadScanStatus();

    // Assert
    expect(console.error).toHaveBeenCalled();
  });

  it('should handle setup periodic scans error', () => {
    // Arrange
    mockScanService.setupPeriodicScans.and.returnValue(throwError(() => new Error('Setup failed')));
    spyOn(console, 'error');

    // Act
    component.setupPeriodicScans();

    // Assert
    expect(console.error).toHaveBeenCalled();
  });

  it('should handle view schedule status error', () => {
    // Arrange
    mockScanService.getScheduleStatus.and.returnValue(throwError(() => new Error('Status failed')));
    spyOn(console, 'error');

    // Act
    component.viewScheduleStatus();

    // Assert
    expect(console.error).toHaveBeenCalled();
  });
});
