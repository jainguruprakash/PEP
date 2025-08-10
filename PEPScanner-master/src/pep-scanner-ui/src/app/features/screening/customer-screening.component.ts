import { Component, inject, signal, OnInit } from '@angular/core';
import { CommonModule } from '@angular/common';
import { ReactiveFormsModule, FormBuilder, Validators, FormArray } from '@angular/forms';
import { MatCardModule } from '@angular/material/card';
import { MatFormFieldModule } from '@angular/material/form-field';
import { MatInputModule } from '@angular/material/input';
import { MatButtonModule } from '@angular/material/button';
import { MatProgressSpinnerModule } from '@angular/material/progress-spinner';
import { MatSelectModule } from '@angular/material/select';
import { MatCheckboxModule } from '@angular/material/checkbox';
import { MatChipsModule } from '@angular/material/chips';
import { MatIconModule } from '@angular/material/icon';
import { MatTabsModule } from '@angular/material/tabs';
import { MatExpansionModule } from '@angular/material/expansion';
import { MatSliderModule } from '@angular/material/slider';
import { MatDatepickerModule } from '@angular/material/datepicker';
import { MatNativeDateModule } from '@angular/material/core';
import { MatTableModule } from '@angular/material/table';
import { MatPaginatorModule } from '@angular/material/paginator';
import { MatSortModule } from '@angular/material/sort';
import { MatTooltipModule } from '@angular/material/tooltip';
import { MatDialogModule } from '@angular/material/dialog';
import { MatProgressBarModule } from '@angular/material/progress-bar';
import { MatMenuModule } from '@angular/material/menu';
import { MatChipsModule } from '@angular/material/chips';
import { MatTableModule } from '@angular/material/table';
import { ScreeningService } from '../../services/screening.service';
import { AlertsService } from '../../services/alerts.service';
import { ReportService } from '../../services/report.service';
import { WebSocketService } from '../../services/websocket.service';
import { AiSuggestionsService } from '../../services/ai-suggestions.service';
import { ScreeningResultsComponent } from './screening-results.component';

@Component({
  selector: 'app-customer-screening',
  standalone: true,
  imports: [
    CommonModule,
    ReactiveFormsModule,
    MatCardModule,
    MatFormFieldModule,
    MatInputModule,
    MatButtonModule,
    MatProgressSpinnerModule,
    MatSelectModule,
    MatCheckboxModule,
    MatIconModule,
    MatSliderModule,
    MatDatepickerModule,
    MatNativeDateModule,
    MatTooltipModule,
    MatProgressBarModule,
    MatMenuModule,
    MatChipsModule,
    MatTableModule,
    ScreeningResultsComponent
  ],
  templateUrl: './customer-screening.component.html',
  styleUrls: ['./customer-screening.component.scss']
})
export class CustomerScreeningComponent implements OnInit {
  private fb = inject(FormBuilder);
  private screeningService = inject(ScreeningService);
  private alertsService = inject(AlertsService);
  private reportService = inject(ReportService);
  private webSocketService = inject(WebSocketService);
  private aiService = inject(AiSuggestionsService);

  // Signals for reactive state management
  isLoading = signal(false);
  result = signal<any>(null);
  bulkResults = signal<any[]>([]);
  screeningHistory = signal<any[]>([]);
  aiSuggestions = signal<any>(null);
  searchTemplates = signal<any[]>([]);
  notifications = signal<string[]>([]);

  // Form groups
  singleScreeningForm = this.fb.group({
    fullName: ['', Validators.required],
    dateOfBirth: [''],
    nationality: [''],
    country: [''],
    identificationNumber: [''],
    identificationType: [''],
    // Advanced filters
    threshold: [70],
    sources: this.fb.array([]),
    includeAliases: [true],
    includeFuzzyMatching: [true],
    includePhoneticMatching: [true]
  });

  bulkScreeningForm = this.fb.group({
    file: ['', Validators.required],
    fileType: ['csv', Validators.required]
  });

  // Configuration options
  availableSources = [
    { value: 'OFAC', label: 'OFAC (US Treasury)', selected: true },
    { value: 'UN', label: 'UN Sanctions', selected: true },
    { value: 'EU', label: 'EU Sanctions', selected: true },
    { value: 'RBI', label: 'RBI (India)', selected: true },
    { value: 'FIU-IND', label: 'FIU-IND', selected: true },
    { value: 'SEBI', label: 'SEBI (India)', selected: true },
    { value: 'MCA', label: 'MCA Directors (India)', selected: true },
    { value: 'LOCAL', label: 'Local Lists', selected: false }
  ];

  countries = [
    'India', 'United States', 'United Kingdom', 'Canada', 'Australia',
    'Germany', 'France', 'Japan', 'Singapore', 'UAE', 'Other'
  ];

  identificationTypes = [
    'PAN', 'Aadhaar', 'Passport', 'Driving License', 'Voter ID',
    'SSN', 'National ID', 'Tax ID', 'Other'
  ];

  totalSources = 8;

  ngOnInit() {
    this.initializeFormArrays();
    this.loadSearchTemplates();
    this.setupRealTimeUpdates();
  }

  private initializeFormArrays() {
    // Initialize sources
    const sourcesArray = this.singleScreeningForm.get('sources') as FormArray;
    sourcesArray.clear(); // Clear any existing controls
    this.availableSources.forEach(source => {
      sourcesArray.push(this.fb.control(source.selected));
    });
  }

  // Single customer screening
  screenSingleCustomer() {
    if (this.singleScreeningForm.invalid) return;

    this.isLoading.set(true);
    this.result.set(null);

    const formValue = this.singleScreeningForm.value;
    const selectedSources = this.getSelectedOptions(formValue.sources, this.availableSources);

    const screeningRequest: any = {
      fullName: formValue.fullName || '',
      dateOfBirth: formValue.dateOfBirth,
      nationality: formValue.nationality,
      country: formValue.country,
      identificationNumber: formValue.identificationNumber,
      identificationType: formValue.identificationType,
      threshold: formValue.threshold,
      sources: selectedSources,
      includeAliases: formValue.includeAliases,
      includeFuzzyMatching: formValue.includeFuzzyMatching,
      includePhoneticMatching: formValue.includePhoneticMatching
    };

    this.screeningService.screenCustomer(screeningRequest).subscribe({
      next: (res) => {
        this.result.set(res);
        this.isLoading.set(false);
        
        // Load AI suggestions and history
        this.getAISuggestions();
        if (res.customerId) {
          this.loadScreeningHistory(res.customerId);
        }
        
        // Show alert creation notification if alerts were auto-created
        if ((res as any).alertsCreated?.length > 0) {
          this.addNotification(`${(res as any).alertsCreated.length} alert(s) created automatically`);
        }
      },
      error: (error) => {
        console.error('Screening error:', error);
        this.isLoading.set(false);
      }
    });
  }

  // Utility methods
  private getSelectedOptions(selections: any[] | null | undefined, options: any[]): string[] {
    if (!selections || !Array.isArray(selections)) {
      // If no selections, return all default selected sources
      return options.filter(option => option.selected).map(option => option.value);
    }
    return options
      .filter((_, index) => selections[index] === true)
      .map(option => option.value);
  }

  // Form getters
  get sourcesFormArray() {
    return this.singleScreeningForm.get('sources') as FormArray;
  }

  // Clear forms
  clearSingleForm() {
    this.singleScreeningForm.reset();
    this.result.set(null);
    // Reinitialize form arrays after reset
    this.initializeFormArrays();
  }

  // Create alert for specific match
  createAlert(match: any) {
    const alertRequest = {
      alertType: this.determineAlertType(match.listType),
      similarityScore: match.matchScore,
      priority: this.determinePriority(match.matchScore),
      riskLevel: this.determineRiskLevel(match.matchScore),
      sourceList: match.source,
      sourceCategory: match.listType,
      matchingDetails: `Manual alert for: ${match.matchedName} (Score: ${match.matchScore})`,
      createdBy: 'current-user', // Replace with actual user
      slaHours: this.getSlaHours(match.matchScore)
    };

    this.alertsService.createFromScreening(alertRequest).subscribe({
      next: (response) => {
        console.log('Alert created:', response.alertId);
        // Update UI to show alert was created
      },
      error: (error) => {
        console.error('Error creating alert:', error);
      }
    });
  }

  private determineAlertType(listType: string): string {
    switch (listType) {
      case 'PEP': return 'PEP Match';
      case 'Sanctions': return 'Sanctions Match';
      case 'Adverse Media': return 'Adverse Media Match';
      default: return 'Name Match';
    }
  }

  private determinePriority(matchScore: number): string {
    if (matchScore >= 0.9) return 'Critical';
    if (matchScore >= 0.8) return 'High';
    if (matchScore >= 0.7) return 'Medium';
    return 'Low';
  }

  private determineRiskLevel(matchScore: number): string {
    if (matchScore >= 0.9) return 'Critical';
    if (matchScore >= 0.8) return 'High';
    if (matchScore >= 0.7) return 'Medium';
    return 'Low';
  }

  private getSlaHours(matchScore: number): number {
    if (matchScore >= 0.9) return 4;   // Critical: 4 hours
    if (matchScore >= 0.8) return 24;  // High: 24 hours
    if (matchScore >= 0.7) return 72;  // Medium: 72 hours
    return 168; // Low: 1 week
  }

  // Enhanced export functionality
  exportResults(format: 'json' | 'pdf' | 'excel' = 'json') {
    const result = this.result();
    if (!result) return;

    if (format === 'json') {
      const dataStr = JSON.stringify(result, null, 2);
      const dataBlob = new Blob([dataStr], { type: 'application/json' });
      const url = URL.createObjectURL(dataBlob);
      const link = document.createElement('a');
      link.href = url;
      link.download = `screening-results-${new Date().toISOString().split('T')[0]}.json`;
      link.click();
      URL.revokeObjectURL(url);
    } else {
      this.reportService.generateReport(result, format).subscribe(blob => {
        this.reportService.downloadReport(blob, `screening-report.${format}`);
      });
    }
  }

  // Bulk screening
  uploadBulkFile(event: any) {
    const file = event.target.files[0];
    if (file) {
      this.isLoading.set(true);
      this.screeningService.screenBatchFile(file).subscribe({
        next: (results) => {
          this.bulkResults.set(results);
          this.isLoading.set(false);
        },
        error: (error) => {
          console.error('Bulk screening error:', error);
          this.isLoading.set(false);
        }
      });
    }
  }

  // Customer actions
  approveCustomer() {
    const result = this.result();
    if (!result) return;
    
    this.screeningService.approveCustomer(result.customerId, 'Approved after screening').subscribe({
      next: () => this.addNotification('Customer approved successfully'),
      error: (error) => console.error('Approval error:', error)
    });
  }

  flagForReview() {
    const result = this.result();
    if (!result) return;
    
    this.screeningService.flagForReview(result.customerId, 'Flagged for manual review').subscribe({
      next: () => this.addNotification('Customer flagged for review'),
      error: (error) => console.error('Flag error:', error)
    });
  }

  requestEDD() {
    const result = this.result();
    if (!result) return;
    
    const requirements = ['Source of wealth documentation', 'Enhanced background check'];
    this.screeningService.requestEDD(result.customerId, requirements).subscribe({
      next: () => this.addNotification('EDD requested successfully'),
      error: (error) => console.error('EDD error:', error)
    });
  }

  // Load screening history
  loadScreeningHistory(customerId: string) {
    this.screeningService.getScreeningHistory(customerId).subscribe({
      next: (history) => this.screeningHistory.set(history),
      error: (error) => console.error('History error:', error)
    });
  }

  // AI suggestions
  getAISuggestions() {
    const formValue = this.singleScreeningForm.value;
    this.aiService.getScreeningSuggestions(formValue).subscribe({
      next: (suggestions) => this.aiSuggestions.set(suggestions),
      error: (error) => console.error('AI suggestions error:', error)
    });
  }

  // Search templates
  loadSearchTemplates() {
    this.screeningService.getSearchTemplates().subscribe({
      next: (templates) => this.searchTemplates.set(templates),
      error: (error) => console.error('Templates error:', error)
    });
  }

  loadTemplate(templateId: string) {
    const template = this.searchTemplates().find(t => t.id === templateId);
    if (template) {
      this.singleScreeningForm.patchValue(template.config);
    }
  }

  saveCurrentAsTemplate() {
    const template = {
      name: 'Custom Template',
      config: this.singleScreeningForm.value
    };
    this.screeningService.saveSearchTemplate(template).subscribe({
      next: () => {
        this.loadSearchTemplates();
        this.addNotification('Template saved successfully');
      },
      error: (error) => console.error('Save template error:', error)
    });
  }

  // Real-time updates
  private setupRealTimeUpdates() {
    this.webSocketService.connect().subscribe(update => {
      if (update.type === 'WATCHLIST_UPDATE') {
        this.addNotification('Watchlist updated - Re-screen recommended');
      }
    });
  }

  private addNotification(message: string) {
    const current = this.notifications();
    this.notifications.set([...current, message]);
    setTimeout(() => {
      const updated = this.notifications().filter(n => n !== message);
      this.notifications.set(updated);
    }, 5000);
  }
}


