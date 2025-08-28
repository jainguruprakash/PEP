import { Component, inject, signal, OnInit } from '@angular/core';
import { CommonModule } from '@angular/common';
import { ReactiveFormsModule, FormBuilder, Validators, FormArray, FormsModule } from '@angular/forms';
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
import { ScreeningService } from '../../services/screening.service';
import { AlertsService } from '../../services/alerts.service';
import { ReportService } from '../../services/report.service';
import { WebSocketService } from '../../services/websocket.service';
import { AiSuggestionsService } from '../../services/ai-suggestions.service';
import { ToastService } from '../../services/toast.service';
import { AuthService } from '../../services/auth.service';
import { ScreeningConfigService, ScreeningFieldConfig } from '../../services/screening-config.service';
import { ScreeningResultsComponent } from './screening-results.component';
import { MatSnackBarModule } from '@angular/material/snack-bar';
import { MatSlideToggleModule } from '@angular/material/slide-toggle';
import { MatRadioModule } from '@angular/material/radio';

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
    MatTabsModule,
    MatSliderModule,
    MatDatepickerModule,
    MatNativeDateModule,
    MatTooltipModule,
    MatProgressBarModule,
    MatMenuModule,
    MatChipsModule,
    MatTableModule,
    MatPaginatorModule,
    MatSnackBarModule,
    MatSlideToggleModule,
    MatRadioModule,
    FormsModule,
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
  private toastService = inject(ToastService);
  private authService = inject(AuthService);
  private screeningConfigService = inject(ScreeningConfigService);

  // Signals for reactive state management
  isLoading = signal(false);
  result = signal<any>(null);
  bulkResults = signal<any[]>([]);
  filteredBulkResults = signal<any[]>([]);
  screeningHistory = signal<any[]>([]);
  aiSuggestions = signal<any>(null);
  searchTemplates = signal<any[]>([]);
  notifications = signal<string[]>([]);
  
  // Bulk screening signals
  uploadedFile = signal<File | null>(null);
  filePreview = signal<any[]>([]);
  totalCustomersToScreen = signal(0);
  isBulkProcessing = signal(false);
  bulkProgress = signal({
    processed: 0,
    total: 0,
    percentage: 0,
    estimatedTime: '0 min',
    clear: 0,
    lowRisk: 0,
    mediumRisk: 0,
    highRisk: 0
  });
  
  // UI state
  isDragOver = false;
  bulkResultsFilter = 'all';
  bulkSearchTerm = '';

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
    fileType: ['csv', Validators.required],
    threshold: [70],
    bulkSources: this.fb.array([]),
    autoCreateAlerts: [true],
    skipDuplicates: [false],
    enableParallelProcessing: [true]
  });

  // Configuration options - now loaded from service
  screeningConfig: ScreeningFieldConfig[] = [];
  availableSources: any[] = []; // Backward compatibility
  showConfigOptions = false;

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
    this.loadScreeningConfiguration();
    this.initializeFormArrays();
    this.loadSearchTemplates();
    this.setupRealTimeUpdates();
  }

  private loadScreeningConfiguration() {
    // Load configuration from service
    this.screeningConfig = this.screeningConfigService.getScreeningConfig();

    // Extract sources for backward compatibility
    const sourcesConfig = this.screeningConfig.find(config => config.fieldName === 'sources');
    if (sourcesConfig?.options) {
      this.availableSources = sourcesConfig.options;
    }

    // Create dynamic form controls
    this.createFormControlsForConfig();

    // Update form with default values from configuration
    const defaultValues = this.screeningConfigService.getDefaultFormValues();
    this.singleScreeningForm.patchValue(defaultValues);
    this.bulkScreeningForm.patchValue(defaultValues);
  }

  private initializeFormArrays() {
    // Initialize single screening sources
    const sourcesArray = this.singleScreeningForm.get('sources') as FormArray;
    sourcesArray.clear();
    this.availableSources.forEach(source => {
      sourcesArray.push(this.fb.control(source.selected));
    });
    
    // Initialize bulk screening sources
    const bulkSourcesArray = this.bulkScreeningForm.get('bulkSources') as FormArray;
    bulkSourcesArray.clear();
    this.availableSources.forEach(source => {
      bulkSourcesArray.push(this.fb.control(source.selected));
    });
  }

  // Single customer screening
  screenSingleCustomer() {
    if (this.singleScreeningForm.invalid) return;

    this.isLoading.set(true);
    this.result.set(null);
    this.bulkResults.set([]);
    this.filteredBulkResults.set([]);

    const formValue = this.singleScreeningForm.value;

    // Build base payload with customer data
    const basePayload = {
      fullName: formValue.fullName || '',
      dateOfBirth: formValue.dateOfBirth,
      nationality: formValue.nationality,
      country: formValue.country,
      identificationNumber: formValue.identificationNumber,
      identificationType: formValue.identificationType
    };

    // Use configurable service to build complete payload
    const screeningRequest = this.screeningConfigService.buildApiPayload(formValue, basePayload);

    this.screeningService.screenCustomer(screeningRequest).subscribe({
      next: (res) => {
        console.log('Screening response received:', res);
        console.log('Response type:', typeof res);
        console.log('Response has matches:', res?.matches?.length || 0);
        console.log('Full response structure:', JSON.stringify(res, null, 2));

        this.result.set(res);
        this.isLoading.set(false);
        
        // Show success message
        this.toastService.success(`Screening completed for ${formValue.fullName}`);
        
        // Show alert creation notification if alerts were auto-created
        if ((res as any).alertsCreated?.length > 0) {
          this.toastService.success(`${(res as any).alertsCreated.length} alert(s) created and assigned to senior for review`);
        }
        
        // Show existing alert notification if customer was already screened
        if ((res as any).existingAlerts?.length > 0) {
          this.toastService.warning(`${(res as any).existingAlerts.length} existing alert(s) found for this customer`);
        }
        
        // Load AI suggestions and history
        this.getAISuggestions();
        if (res.customerId) {
          this.loadScreeningHistory(res.customerId);
        }
      },
      error: (error) => {
        console.error('Screening error:', error);
        this.toastService.error('Screening failed. Please try again.');
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
    if (!match) {
      this.toastService.error('Invalid match data provided');
      return;
    }

    // Get current user information
    const currentUser = this.authService.getCurrentUser();
    const currentResult = this.result();
    const formData = this.singleScreeningForm.value;

    // Get customer name from form data or screening result
    const customerName = formData.fullName ||
                        currentResult?.customerName ||
                        currentResult?.fullName ||
                        match.matchedName ||
                        match.fullName ||
                        match.name ||
                        'Unknown Customer';

    const alertRequest = {
      alertType: this.determineAlertType(match.listType || match.riskCategory),
      similarityScore: match.matchScore || match.similarityScore || 0,
      priority: this.determinePriority(match.matchScore || match.similarityScore || 0),
      riskLevel: this.determineRiskLevel(match.matchScore || match.similarityScore || 0),
      sourceList: match.source,
      sourceCategory: match.listType || match.riskCategory,
      matchingDetails: `Manual alert created for customer screening`,
      createdBy: currentUser?.username || currentUser?.email || 'System User',
      customerName: customerName.trim(),
      matchedName: match.matchedName || match.fullName || match.name,
      slaHours: this.getSlaHours(match.matchScore || match.similarityScore || 0)
    };

    console.log('Creating alert with request:', alertRequest);
    this.toastService.info('Creating alert...');

    this.alertsService.createFromScreening(alertRequest).subscribe({
      next: (response) => {
        console.log('Alert created successfully:', response);

        // Update the match to show alert was created
        if (match) {
          match.alertCreated = true;
          match.alertId = response.alertId;
          match.customerName = response.customerName;
          match.alertCreatedAt = new Date();
          match.alertCreatedBy = alertRequest.createdBy;
        }

        // Show success message with action options
        this.toastService.success(`Alert created successfully! Alert ID: ${response.alertId}`);

        // Optional: Show a confirmation dialog with next steps
        this.showAlertCreatedDialog(response, match);

        // Update the screening results to reflect the new alert status
        this.updateScreeningResultsAfterAlert(match);
      },
      error: (error) => {
        console.error('Error creating alert:', error);
        const errorMessage = error?.error?.message || error?.message || 'Failed to create alert';
        this.toastService.error(`Alert creation failed: ${errorMessage}`);
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

  // Enhanced bulk screening methods
  uploadBulkFile(event: any) {
    const file = event.target.files[0];
    if (file) {
      this.processUploadedFile(file);
    }
  }
  
  onDragOver(event: DragEvent) {
    event.preventDefault();
    this.isDragOver = true;
  }
  
  onDragLeave(event: DragEvent) {
    event.preventDefault();
    this.isDragOver = false;
  }
  
  onFileDrop(event: DragEvent) {
    event.preventDefault();
    this.isDragOver = false;
    
    const files = event.dataTransfer?.files;
    if (files && files.length > 0) {
      this.processUploadedFile(files[0]);
    }
  }
  
  private processUploadedFile(file: File) {
    // Validate file type
    const allowedTypes = ['.csv', '.xlsx', '.xls'];
    const fileExtension = file.name.toLowerCase().substring(file.name.lastIndexOf('.'));
    
    if (!allowedTypes.includes(fileExtension)) {
      this.toastService.error('Invalid file type. Please upload CSV or Excel files only.');
      return;
    }
    
    // Validate file size (max 10MB)
    if (file.size > 10 * 1024 * 1024) {
      this.toastService.error('File size too large. Maximum size is 10MB.');
      return;
    }
    
    this.uploadedFile.set(file);
    this.bulkScreeningForm.patchValue({ file: file.name, fileType: fileExtension.substring(1) });
    
    // Preview file contents
    this.previewFile(file);
    
    this.toastService.success(`File "${file.name}" uploaded successfully`);
  }
  
  private previewFile(file: File) {
    const reader = new FileReader();
    reader.onload = (e) => {
      const content = e.target?.result as string;
      const lines = content.split('\n').filter(line => line.trim());
      
      if (lines.length < 2) {
        this.toastService.error('File must contain at least a header row and one data row.');
        return;
      }
      
      // Parse header and first few rows
      const header = lines[0].split(',').map(h => h.trim().toLowerCase());
      const preview = [];
      
      for (let i = 1; i < Math.min(6, lines.length); i++) {
        const row = lines[i].split(',').map(cell => cell.trim());
        const nameIndex = header.findIndex(h => h.includes('name'));
        const countryIndex = header.findIndex(h => h.includes('country'));
        const dobIndex = header.findIndex(h => h.includes('dob') || h.includes('birth'));
        
        if (nameIndex >= 0 && row[nameIndex]) {
          preview.push({
            name: row[nameIndex],
            country: countryIndex >= 0 ? row[countryIndex] : '',
            dob: dobIndex >= 0 ? row[dobIndex] : ''
          });
        }
      }
      
      this.filePreview.set(preview);
      this.totalCustomersToScreen.set(lines.length - 1); // Exclude header
    };
    
    reader.readAsText(file);
  }
  
  startBulkScreening() {
    const file = this.uploadedFile();
    if (!file) {
      this.toastService.error('Please upload a file first.');
      return;
    }
    
    const formValue = this.bulkScreeningForm.value;
    const selectedSources = this.getSelectedOptions(formValue.bulkSources, this.availableSources);
    
    this.isBulkProcessing.set(true);
    this.bulkResults.set([]);
    this.result.set(null);
    
    // Reset progress
    this.bulkProgress.set({
      processed: 0,
      total: this.totalCustomersToScreen(),
      percentage: 0,
      estimatedTime: 'Calculating...',
      clear: 0,
      lowRisk: 0,
      mediumRisk: 0,
      highRisk: 0
    });
    
    const bulkRequest = {
      file: file,
      threshold: formValue.threshold || 70,
      sources: selectedSources,
      autoCreateAlerts: formValue.autoCreateAlerts || false,
      skipDuplicates: formValue.skipDuplicates || false,
      enableParallelProcessing: formValue.enableParallelProcessing || true
    };
    
    // Start processing with progress updates
    this.screeningService.screenBulkWithProgress(bulkRequest).subscribe({
      next: (update) => {
        if (update.type === 'progress') {
          this.updateBulkProgress(update.data);
        } else if (update.type === 'complete') {
          this.bulkResults.set(update.data);
          this.filteredBulkResults.set(update.data);
          this.isBulkProcessing.set(false);
          this.toastService.success(`Bulk screening completed! ${update.data.length} customers processed.`);
        }
      },
      error: (error) => {
        console.error('Bulk screening error:', error);
        this.toastService.error('Bulk screening failed. Please try again.');
        this.isBulkProcessing.set(false);
      }
    });
  }
  
  private updateBulkProgress(progressData: any) {
    const progress = {
      processed: progressData.processed,
      total: progressData.total,
      percentage: Math.round((progressData.processed / progressData.total) * 100),
      estimatedTime: this.calculateEstimatedTime(progressData),
      clear: progressData.clear || 0,
      lowRisk: progressData.lowRisk || 0,
      mediumRisk: progressData.mediumRisk || 0,
      highRisk: progressData.highRisk || 0
    };
    this.bulkProgress.set(progress);
  }
  
  private calculateEstimatedTime(progressData: any): string {
    if (progressData.processed === 0) return 'Calculating...';
    
    const avgTimePerCustomer = progressData.elapsedTime / progressData.processed;
    const remainingCustomers = progressData.total - progressData.processed;
    const estimatedSeconds = avgTimePerCustomer * remainingCustomers;
    
    if (estimatedSeconds < 60) {
      return `${Math.round(estimatedSeconds)} sec`;
    } else {
      return `${Math.round(estimatedSeconds / 60)} min`;
    }
  }
  
  cancelBulkScreening() {
    this.screeningService.cancelBulkScreening().subscribe({
      next: () => {
        this.isBulkProcessing.set(false);
        this.toastService.info('Bulk screening cancelled.');
      },
      error: (error) => {
        console.error('Cancel error:', error);
        this.isBulkProcessing.set(false);
      }
    });
  }
  
  clearBulkForm() {
    this.bulkScreeningForm.reset();
    this.uploadedFile.set(null);
    this.filePreview.set([]);
    this.totalCustomersToScreen.set(0);
    this.bulkResults.set([]);
    this.filteredBulkResults.set([]);
    this.initializeFormArrays();
  }
  
  // Sample file downloads
  downloadSampleFile(format: 'csv' | 'excel') {
    const sampleData = [
      ['Name', 'Country', 'Date of Birth', 'ID Number', 'ID Type'],
      ['John Doe', 'United States', '1980-01-15', 'SSN123456789', 'SSN'],
      ['Jane Smith', 'United Kingdom', '1975-06-22', 'PASS987654321', 'Passport'],
      ['Raj Patel', 'India', '1985-03-10', 'ABCDE1234F', 'PAN'],
      ['Maria Garcia', 'Spain', '1990-12-05', 'ESP123456789', 'National ID'],
      ['Ahmed Hassan', 'UAE', '1988-08-20', 'UAE987654321', 'Emirates ID']
    ];
    
    if (format === 'csv') {
      const csvContent = sampleData.map(row => row.join(',')).join('\n');
      const blob = new Blob([csvContent], { type: 'text/csv;charset=utf-8' });
      const url = URL.createObjectURL(blob);
      const link = document.createElement('a');
      link.href = url;
      link.download = 'bulk-screening-template.csv';
      link.click();
      URL.revokeObjectURL(url);
      this.toastService.success('CSV template downloaded successfully');
    } else {
      // Create simple Excel-like CSV for now
      const csvContent = sampleData.map(row => row.join(',')).join('\n');
      const blob = new Blob([csvContent], { type: 'application/vnd.ms-excel' });
      const url = URL.createObjectURL(blob);
      const link = document.createElement('a');
      link.href = url;
      link.download = 'bulk-screening-template.xlsx';
      link.click();
      URL.revokeObjectURL(url);
      this.toastService.success('Excel template downloaded successfully');
    }
  }

  // Alert creation success handlers
  private showAlertCreatedDialog(response: any, match: any) {
    // You can implement a dialog here if needed
    console.log('Alert created dialog:', { response, match });

    // For now, just log the success and provide guidance
    console.log(`
      Alert Created Successfully!
      - Alert ID: ${response.alertId}
      - Customer: ${response.customerName || 'Unknown'}
      - Match: ${match.matchedName || match.fullName || match.name}
      - Created By: ${response.createdBy || 'Unknown'}

      Next Steps:
      1. Review the alert in the alerts dashboard
      2. Assign to appropriate reviewer
      3. Continue with customer screening if needed
    `);
  }

  private updateScreeningResultsAfterAlert(match: any) {
    // Update the current result to reflect alert creation
    const currentResult = this.result();
    if (currentResult && currentResult.matches) {
      const matchIndex = currentResult.matches.findIndex((m: any) =>
        m.matchedName === match.matchedName ||
        m.fullName === match.fullName ||
        m.name === match.name
      );

      if (matchIndex >= 0) {
        currentResult.matches[matchIndex] = { ...match };
        // Trigger change detection
        this.result.set({ ...currentResult });
      }
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

  // Bulk results management
  getBulkSummary() {
    const results = this.bulkResults();
    return {
      clear: results.filter(r => r.status === 'Clear').length,
      lowRisk: results.filter(r => r.status === 'Low Risk' || r.status === 'Minimal Risk').length,
      mediumRisk: results.filter(r => r.status === 'Medium Risk').length,
      highRisk: results.filter(r => r.status === 'High Risk').length
    };
  }
  
  filterBulkResults() {
    let filtered = this.bulkResults();
    
    // Filter by status
    if (this.bulkResultsFilter !== 'all') {
      const statusMap: { [key: string]: string[] } = {
        'high-risk': ['High Risk'],
        'medium-risk': ['Medium Risk'],
        'low-risk': ['Low Risk', 'Minimal Risk'],
        'clear': ['Clear']
      };
      const allowedStatuses = statusMap[this.bulkResultsFilter] || [];
      filtered = filtered.filter(r => allowedStatuses.includes(r.status));
    }
    
    // Filter by search term
    if (this.bulkSearchTerm.trim()) {
      const searchTerm = this.bulkSearchTerm.toLowerCase();
      filtered = filtered.filter(r => 
        r.customerName.toLowerCase().includes(searchTerm) ||
        (r.country && r.country.toLowerCase().includes(searchTerm))
      );
    }
    
    this.filteredBulkResults.set(filtered);
  }
  
  getRiskClass(riskScore: number): string {
    if (riskScore >= 80) return 'high-risk';
    if (riskScore >= 60) return 'medium-risk';
    if (riskScore >= 40) return 'low-risk';
    return 'clear';
  }
  
  getStatusClass(status: string): string {
    return status.toLowerCase().replace(' ', '-');
  }
  
  exportBulkResults(format: 'excel' | 'pdf') {
    const results = this.filteredBulkResults();
    if (results.length === 0) {
      this.toastService.warning('No results to export.');
      return;
    }
    
    this.reportService.generateBulkReport(results, format).subscribe({
      next: (blob) => {
        const fileName = `bulk-screening-results-${new Date().toISOString().split('T')[0]}.${format}`;
        this.reportService.downloadReport(blob, fileName);
        this.toastService.success(`Report exported successfully as ${format.toUpperCase()}`);
      },
      error: (error) => {
        console.error('Export error:', error);
        this.toastService.error('Failed to export results.');
      }
    });
  }
  
  createBulkAlerts() {
    const highRiskResults = this.bulkResults().filter(r => r.status === 'High Risk');
    if (highRiskResults.length === 0) {
      this.toastService.warning('No high-risk customers found.');
      return;
    }
    
    this.screeningService.createBulkAlerts(highRiskResults).subscribe({
      next: (response) => {
        this.toastService.success(`${response.alertsCreated} alerts created for high-risk customers.`);
      },
      error: (error) => {
        console.error('Bulk alert creation error:', error);
        this.toastService.error('Failed to create alerts.');
      }
    });
  }
  
  viewBulkResultDetails(result: any) {
    // Set the result as current single result for detailed view
    this.result.set({
      ...result,
      customerId: result.customerId || '',
      fullName: result.customerName,
      screenedAt: result.screenedAt
    });
    
    // Switch to single screening tab to show details
    // This would require tab group reference - simplified for now
    this.toastService.info(`Viewing details for ${result.customerName}`);
  }
  
  createAlertFromBulkResult(result: any) {
    if (!result.matches || result.matches.length === 0) {
      this.toastService.warning('No matches found to create alert.');
      return;
    }
    
    const alertRequest = {
      customerName: result.customerName,
      matches: result.matches,
      riskScore: result.riskScore / 100,
      source: 'BulkScreening'
    };
    
    this.screeningService.createAlertFromBulkResult(alertRequest).subscribe({
      next: (response) => {
        this.toastService.success(`Alert created for ${result.customerName}`);
      },
      error: (error) => {
        console.error('Alert creation error:', error);
        this.toastService.error('Failed to create alert.');
      }
    });
  }
  
  approveBulkCustomer(result: any) {
    this.screeningService.approveBulkCustomer(result.customerName, 'Approved from bulk screening').subscribe({
      next: () => {
        // Update the result status
        result.status = 'Approved';
        this.toastService.success(`${result.customerName} approved successfully`);
      },
      error: (error) => {
        console.error('Approval error:', error);
        this.toastService.error('Failed to approve customer.');
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

  // Configuration helper methods for template
  getScreeningCategories(): string[] {
    return this.screeningConfigService.getCategories();
  }

  getConfigByCategory(category: string): ScreeningFieldConfig[] {
    return this.screeningConfigService.getConfigByCategory(category);
  }

  getCategoryTitle(category: string): string {
    const categoryTitles: { [key: string]: string } = {
      'sources': 'Data Sources',
      'matching': 'Matching Options',
      'scope': 'Search Scope',
      'risk': 'Risk Assessment',
      'filters': 'Filters',
      'advanced': 'Advanced Options'
    };
    return categoryTitles[category] || category.charAt(0).toUpperCase() + category.slice(1);
  }

  // Dynamic form control creation
  createFormControlsForConfig() {
    this.screeningConfig.forEach(config => {
      if (!this.singleScreeningForm.get(config.fieldName)) {
        if (config.type === 'checkbox-group' && config.options) {
          const formArray = this.fb.array(
            config.options.map(option => this.fb.control(option.selected))
          );
          this.singleScreeningForm.addControl(config.fieldName, formArray);
        } else {
          this.singleScreeningForm.addControl(
            config.fieldName,
            this.fb.control(config.defaultValue || null)
          );
        }
      }
    });
  }
}


