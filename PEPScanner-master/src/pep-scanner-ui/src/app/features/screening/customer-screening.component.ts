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
import { ScreeningService } from '../../services/screening.service';
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
    ScreeningResultsComponent
  ],
  templateUrl: './customer-screening.component.html',
  styleUrls: ['./customer-screening.component.scss']
})
export class CustomerScreeningComponent implements OnInit {
  private fb = inject(FormBuilder);
  private screeningService = inject(ScreeningService);

  // Signals for reactive state management
  isLoading = signal(false);
  result = signal<any>(null);

  // Form group
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

  // Configuration options
  availableSources = [
    { value: 'OFAC', label: 'OFAC (US Treasury)', selected: true },
    { value: 'UN', label: 'UN Sanctions', selected: true },
    { value: 'EU', label: 'EU Sanctions', selected: true },
    { value: 'RBI', label: 'RBI (India)', selected: true },
    { value: 'FIU-IND', label: 'FIU-IND', selected: true },
    { value: 'SEBI', label: 'SEBI (India)', selected: true },
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

  totalSources = 7;

  ngOnInit() {
    this.initializeFormArrays();
  }

  private initializeFormArrays() {
    // Initialize sources
    const sourcesArray = this.singleScreeningForm.get('sources') as FormArray;
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
      },
      error: (error) => {
        console.error('Screening error:', error);
        this.isLoading.set(false);
      }
    });
  }

  // Utility methods
  private getSelectedOptions(selections: any[] | null | undefined, options: any[]): string[] {
    if (!selections || !Array.isArray(selections)) return [];
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
  }

  // Export functionality
  exportResults() {
    const result = this.result();
    if (!result) return;

    const dataStr = JSON.stringify(result, null, 2);
    const dataBlob = new Blob([dataStr], { type: 'application/json' });
    const url = URL.createObjectURL(dataBlob);
    const link = document.createElement('a');
    link.href = url;
    link.download = `screening-results-${new Date().toISOString().split('T')[0]}.json`;
    link.click();
    URL.revokeObjectURL(url);
  }
}


