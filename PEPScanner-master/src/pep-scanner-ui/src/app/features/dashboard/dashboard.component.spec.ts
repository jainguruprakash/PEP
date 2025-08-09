import { ComponentFixture, TestBed } from '@angular/core/testing';
import { HttpClientTestingModule } from '@angular/common/http/testing';
import { NoopAnimationsModule } from '@angular/platform-browser/animations';
import { of, throwError } from 'rxjs';
import { DashboardComponent } from './dashboard.component';
import { ScreeningService } from '../../services/screening.service';

describe('DashboardComponent', () => {
  let component: DashboardComponent;
  let fixture: ComponentFixture<DashboardComponent>;
  let screeningService: jasmine.SpyObj<ScreeningService>;

  beforeEach(async () => {
    const screeningServiceSpy = jasmine.createSpyObj('ScreeningService', ['getStatistics']);

    await TestBed.configureTestingModule({
      imports: [
        DashboardComponent,
        HttpClientTestingModule,
        NoopAnimationsModule
      ],
      providers: [
        { provide: ScreeningService, useValue: screeningServiceSpy }
      ]
    }).compileComponents();

    fixture = TestBed.createComponent(DashboardComponent);
    component = fixture.componentInstance;
    screeningService = TestBed.inject(ScreeningService) as jasmine.SpyObj<ScreeningService>;
  });

  it('should create', () => {
    expect(component).toBeTruthy();
  });

  it('should initialize with null stats', () => {
    expect(component['stats']()).toBeNull();
  });

  describe('constructor', () => {
    it('should load statistics on initialization', () => {
      const mockStats = {
        alertCount: 25,
        customersScreened: 150,
        averageRisk: 0.3
      };
      screeningService.getStatistics.and.returnValue(of(mockStats));

      // Create new component instance to trigger constructor
      fixture = TestBed.createComponent(DashboardComponent);
      component = fixture.componentInstance;

      expect(screeningService.getStatistics).toHaveBeenCalled();
      expect(component['stats']()).toEqual({
        alerts: 25,
        customers: 150,
        avgRisk: 0.3
      });
    });

    it('should handle missing data in statistics response', () => {
      const mockStats = {
        alertCount: undefined,
        customersScreened: null,
        averageRisk: undefined
      };
      screeningService.getStatistics.and.returnValue(of(mockStats));

      fixture = TestBed.createComponent(DashboardComponent);
      component = fixture.componentInstance;

      expect(component['stats']()).toEqual({
        alerts: 0,
        customers: 0,
        avgRisk: 0
      });
    });

    it('should handle error when loading statistics', () => {
      screeningService.getStatistics.and.returnValue(throwError(() => new Error('API Error')));

      fixture = TestBed.createComponent(DashboardComponent);
      component = fixture.componentInstance;

      expect(component['stats']()).toEqual({
        alerts: 0,
        customers: 0,
        avgRisk: 0
      });
    });

    it('should call getStatistics with correct date range', () => {
      const mockStats = { alertCount: 0, customersScreened: 0, averageRisk: 0 };
      screeningService.getStatistics.and.returnValue(of(mockStats));

      fixture = TestBed.createComponent(DashboardComponent);
      component = fixture.componentInstance;

      expect(screeningService.getStatistics).toHaveBeenCalledWith(
        jasmine.any(String),
        jasmine.any(String)
      );

      // Verify the date format (YYYY-MM-DD)
      const calls = screeningService.getStatistics.calls.mostRecent();
      const startDate = calls.args[0];
      const endDate = calls.args[1];

      expect(startDate).toMatch(/^\d{4}-\d{2}-\d{2}$/);
      expect(endDate).toMatch(/^\d{4}-\d{2}-\d{2}$/);

      // Verify end date is today
      const today = new Date().toISOString().slice(0, 10);
      expect(endDate).toBe(today);

      // Verify start date is 30 days ago
      const thirtyDaysAgo = new Date(Date.now() - 30 * 24 * 3600 * 1000).toISOString().slice(0, 10);
      expect(startDate).toBe(thirtyDaysAgo);
    });
  });

  describe('template rendering', () => {
    it('should display loading state when stats is null', () => {
      component['stats'].set(null);
      fixture.detectChanges();

      const compiled = fixture.nativeElement;
      expect(compiled.textContent).toContain('Loading...');
    });

    it('should display statistics when stats is available', () => {
      const mockStats = {
        alerts: 25,
        customers: 150,
        avgRisk: 0.3
      };
      component['stats'].set(mockStats);
      fixture.detectChanges();

      const compiled = fixture.nativeElement;
      expect(compiled.textContent).toContain('Alerts: 25');
      expect(compiled.textContent).toContain('Customers Screened: 150');
      expect(compiled.textContent).toContain('Average Risk: 0.3');
      expect(compiled.textContent).not.toContain('Loading...');
    });

    it('should display zero values correctly', () => {
      const mockStats = {
        alerts: 0,
        customers: 0,
        avgRisk: 0
      };
      component['stats'].set(mockStats);
      fixture.detectChanges();

      const compiled = fixture.nativeElement;
      expect(compiled.textContent).toContain('Alerts: 0');
      expect(compiled.textContent).toContain('Customers Screened: 0');
      expect(compiled.textContent).toContain('Average Risk: 0');
    });

    it('should have proper card structure', () => {
      component['stats'].set({ alerts: 10, customers: 50, avgRisk: 0.2 });
      fixture.detectChanges();

      const compiled = fixture.nativeElement;
      const card = compiled.querySelector('mat-card');
      const cardTitle = compiled.querySelector('mat-card-title');
      const cardContent = compiled.querySelector('mat-card-content');

      expect(card).toBeTruthy();
      expect(cardTitle?.textContent).toContain('Screening Statistics');
      expect(cardContent).toBeTruthy();
    });

    it('should have grid layout class', () => {
      fixture.detectChanges();
      const compiled = fixture.nativeElement;
      const gridElement = compiled.querySelector('.grid');
      expect(gridElement).toBeTruthy();
    });
  });
});
