import { Component, inject, signal, ViewChild, OnInit } from '@angular/core';
import { CommonModule } from '@angular/common';
import { ReactiveFormsModule, FormBuilder, Validators } from '@angular/forms';
import { MatFormFieldModule } from '@angular/material/form-field';
import { MatInputModule } from '@angular/material/input';
import { MatButtonModule } from '@angular/material/button';
import { MatTableModule } from '@angular/material/table';
import { MatPaginatorModule, MatPaginator } from '@angular/material/paginator';
import { MatSortModule, MatSort } from '@angular/material/sort';
import { MatSelectModule } from '@angular/material/select';
import { MatCheckboxModule } from '@angular/material/checkbox';
import { MatChipsModule } from '@angular/material/chips';
import { MatIconModule } from '@angular/material/icon';
import { MatCardModule } from '@angular/material/card';
import { MatExpansionModule } from '@angular/material/expansion';
import { MatSliderModule } from '@angular/material/slider';
import { MatProgressSpinnerModule } from '@angular/material/progress-spinner';
import { MatTooltipModule } from '@angular/material/tooltip';
import { MatDialogModule, MatDialog } from '@angular/material/dialog';
import { MatSnackBarModule, MatSnackBar } from '@angular/material/snack-bar';
import { MatTabsModule } from '@angular/material/tabs';
import { MatTableDataSource } from '@angular/material/table';
import { ScreeningService } from '../../services/screening.service';

@Component({
  selector: 'app-search',
  standalone: true,
  imports: [
    CommonModule,
    ReactiveFormsModule,
    MatFormFieldModule,
    MatInputModule,
    MatButtonModule,
    MatTableModule,
    MatPaginatorModule,
    MatSortModule,
    MatSelectModule,
    MatCheckboxModule,
    MatChipsModule,
    MatIconModule,
    MatCardModule,
    MatExpansionModule,
    MatSliderModule,
    MatProgressSpinnerModule,
    MatTooltipModule,
    MatDialogModule,
    MatSnackBarModule,
    MatTabsModule
  ],
  templateUrl: './search.component.html',
  styleUrls: ['./search.component.scss']
})
export interface SearchResult {
  id: string;
  name: string;
  aliases?: string[];
  source: string;
  listType: string;
  riskLevel: string;
  country?: string;
  dateOfBirth?: string;
  nationality?: string;
  occupation?: string;
  matchScore: number;
  lastUpdated: Date;
  details: any;
}

export interface SearchFilters {
  searchTerm: string;
  searchType: 'name' | 'id' | 'keyword' | 'fuzzy' | 'phonetic';
  sources: string[];
  listTypes: string[];
  countries: string[];
  riskLevels: string[];
  threshold: number;
  maxResults: number;
  includeAliases: boolean;
  includeFuzzyMatching: boolean;
  dateRange?: {
    from: Date;
    to: Date;
  };
}

export class SearchComponent implements OnInit {
  private fb = inject(FormBuilder);
  private screeningService = inject(ScreeningService);
  private dialog = inject(MatDialog);
  private snackBar = inject(MatSnackBar);

  @ViewChild(MatPaginator) paginator!: MatPaginator;
  @ViewChild(MatSort) sort!: MatSort;

  // Signals for reactive state management
  isLoading = signal(false);
  searchResults = signal<SearchResult[]>([]);
  totalResults = signal(0);
  selectedResults = signal<SearchResult[]>([]);

  // Data source for table
  dataSource = new MatTableDataSource<SearchResult>([]);

  // Form for search
  searchForm = this.fb.group({
    searchTerm: ['', Validators.required],
    searchType: ['name'],
    threshold: [75],
    maxResults: [100],
    includeAliases: [true],
    includeFuzzyMatching: [true],
    sources: this.fb.array([]),
    listTypes: this.fb.array([]),
    countries: this.fb.array([]),
    riskLevels: this.fb.array([])
  });

  // Configuration options
  searchTypes = [
    { value: 'name', label: 'Name Search', description: 'Search by person or entity name' },
    { value: 'id', label: 'ID Number Search', description: 'Search by identification number' },
    { value: 'keyword', label: 'Keyword Search', description: 'Search by keywords in descriptions' },
    { value: 'fuzzy', label: 'Fuzzy Search', description: 'Approximate matching with variations' },
    { value: 'phonetic', label: 'Phonetic Search', description: 'Sound-alike name matching' }
  ];

  availableSources = [
    { value: 'OFAC', label: 'OFAC (US Treasury)', selected: true },
    { value: 'UN', label: 'UN Sanctions', selected: true },
    { value: 'EU', label: 'EU Sanctions', selected: true },
    { value: 'RBI', label: 'RBI (India)', selected: true },
    { value: 'FIU-IND', label: 'FIU-IND', selected: true },
    { value: 'SEBI', label: 'SEBI (India)', selected: true },
    { value: 'HMT', label: 'HM Treasury (UK)', selected: false },
    { value: 'AUSTRAC', label: 'AUSTRAC (Australia)', selected: false },
    { value: 'LOCAL', label: 'Local Lists', selected: false }
  ];

  availableListTypes = [
    { value: 'PEP', label: 'Politically Exposed Persons', selected: true },
    { value: 'SANCTIONS', label: 'Sanctions Lists', selected: true },
    { value: 'ADVERSE_MEDIA', label: 'Adverse Media', selected: false },
    { value: 'ENFORCEMENT', label: 'Enforcement Actions', selected: false },
    { value: 'TERRORISM', label: 'Terrorism Lists', selected: true },
    { value: 'CUSTOM', label: 'Custom Lists', selected: false }
  ];

  availableRiskLevels = [
    { value: 'HIGH', label: 'High Risk', selected: true },
    { value: 'MEDIUM', label: 'Medium Risk', selected: true },
    { value: 'LOW', label: 'Low Risk', selected: false }
  ];

  countries = [
    'Afghanistan', 'Albania', 'Algeria', 'Argentina', 'Australia', 'Austria',
    'Bangladesh', 'Belgium', 'Brazil', 'Canada', 'China', 'France', 'Germany',
    'India', 'Indonesia', 'Iran', 'Iraq', 'Italy', 'Japan', 'Malaysia',
    'Netherlands', 'Pakistan', 'Russia', 'Saudi Arabia', 'Singapore',
    'South Africa', 'Spain', 'Switzerland', 'Turkey', 'United Kingdom',
    'United States', 'Other'
  ];

  // Table configuration
  displayedColumns = [
    'select', 'name', 'source', 'listType', 'riskLevel',
    'country', 'matchScore', 'lastUpdated', 'actions'
  ];

  ngOnInit() {
    this.initializeFormArrays();
    this.setupTableDataSource();
  }

  private initializeFormArrays() {
    // Initialize sources checkboxes
    const sourcesArray = this.searchForm.get('sources') as any;
    this.availableSources.forEach(source => {
      sourcesArray.push(this.fb.control(source.selected));
    });

    // Initialize list types checkboxes
    const listTypesArray = this.searchForm.get('listTypes') as any;
    this.availableListTypes.forEach(listType => {
      listTypesArray.push(this.fb.control(listType.selected));
    });

    // Initialize risk levels checkboxes
    const riskLevelsArray = this.searchForm.get('riskLevels') as any;
    this.availableRiskLevels.forEach(riskLevel => {
      riskLevelsArray.push(this.fb.control(riskLevel.selected));
    });
  }

  private setupTableDataSource() {
    this.dataSource.paginator = this.paginator;
    this.dataSource.sort = this.sort;

    // Custom filter predicate for advanced filtering
    this.dataSource.filterPredicate = (data: SearchResult, filter: string) => {
      const searchStr = filter.toLowerCase();
      return data.name.toLowerCase().includes(searchStr) ||
             data.source.toLowerCase().includes(searchStr) ||
             data.listType.toLowerCase().includes(searchStr) ||
             data.riskLevel.toLowerCase().includes(searchStr) ||
             (data.country && data.country.toLowerCase().includes(searchStr)) ||
             (data.aliases && data.aliases.some(alias => alias.toLowerCase().includes(searchStr)));
    };
  }

  // Search functionality
  performSearch() {
    if (this.searchForm.invalid) {
      this.snackBar.open('Please enter a search term', 'Close', { duration: 3000 });
      return;
    }

    this.isLoading.set(true);
    const formValue = this.searchForm.value;

    const searchFilters: SearchFilters = {
      searchTerm: formValue.searchTerm || '',
      searchType: formValue.searchType as any || 'name',
      threshold: formValue.threshold || 75,
      maxResults: formValue.maxResults || 100,
      includeAliases: formValue.includeAliases || false,
      includeFuzzyMatching: formValue.includeFuzzyMatching || false,
      sources: this.getSelectedOptions(formValue.sources, this.availableSources),
      listTypes: this.getSelectedOptions(formValue.listTypes, this.availableListTypes),
      countries: this.getSelectedOptions(formValue.countries, this.countries.map(c => ({ value: c, label: c }))),
      riskLevels: this.getSelectedOptions(formValue.riskLevels, this.availableRiskLevels)
    };

    this.screeningService.search(searchFilters).subscribe({
      next: (results) => {
        const searchResults = Array.isArray(results) ? results : [];
        this.searchResults.set(searchResults);
        this.totalResults.set(searchResults.length);
        this.dataSource.data = searchResults;
        this.isLoading.set(false);

        this.snackBar.open(`Found ${searchResults.length} results`, 'Close', { duration: 3000 });
      },
      error: (error) => {
        console.error('Search error:', error);
        this.isLoading.set(false);
        this.snackBar.open('Search failed. Please try again.', 'Close', { duration: 5000 });
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

  // Table selection methods
  isAllSelected() {
    const numSelected = this.selectedResults().length;
    const numRows = this.dataSource.data.length;
    return numSelected === numRows;
  }

  masterToggle() {
    if (this.isAllSelected()) {
      this.selectedResults.set([]);
    } else {
      this.selectedResults.set([...this.dataSource.data]);
    }
  }

  toggleSelection(result: SearchResult) {
    const selected = this.selectedResults();
    const index = selected.findIndex(r => r.id === result.id);

    if (index >= 0) {
      const newSelected = [...selected];
      newSelected.splice(index, 1);
      this.selectedResults.set(newSelected);
    } else {
      this.selectedResults.set([...selected, result]);
    }
  }

  isSelected(result: SearchResult): boolean {
    return this.selectedResults().some(r => r.id === result.id);
  }

  // Filter and sort methods
  applyFilter(event: Event) {
    const filterValue = (event.target as HTMLInputElement).value;
    this.dataSource.filter = filterValue.trim().toLowerCase();

    if (this.dataSource.paginator) {
      this.dataSource.paginator.firstPage();
    }
  }

  clearFilters() {
    this.searchForm.reset();
    this.dataSource.filter = '';
    this.searchResults.set([]);
    this.selectedResults.set([]);
    this.dataSource.data = [];
  }

  // Export functionality
  exportResults() {
    const results = this.selectedResults().length > 0 ? this.selectedResults() : this.searchResults();

    if (results.length === 0) {
      this.snackBar.open('No results to export', 'Close', { duration: 3000 });
      return;
    }

    const dataStr = JSON.stringify(results, null, 2);
    const dataBlob = new Blob([dataStr], { type: 'application/json' });
    const url = URL.createObjectURL(dataBlob);
    const link = document.createElement('a');
    link.href = url;
    link.download = `search-results-${new Date().toISOString().split('T')[0]}.json`;
    link.click();
    URL.revokeObjectURL(url);

    this.snackBar.open(`Exported ${results.length} results`, 'Close', { duration: 3000 });
  }

  // View details
  viewDetails(result: SearchResult) {
    // TODO: Open details dialog
    console.log('View details for:', result);
  }

  // Risk level styling
  getRiskLevelColor(riskLevel: string): string {
    switch (riskLevel.toLowerCase()) {
      case 'high':
      case 'critical':
        return 'warn';
      case 'medium':
        return 'accent';
      case 'low':
        return 'primary';
      default:
        return 'primary';
    }
  }

  // Source styling
  getSourceChipColor(source: string): string {
    switch (source.toLowerCase()) {
      case 'ofac':
        return 'primary';
      case 'un':
        return 'accent';
      case 'eu':
        return 'warn';
      default:
        return 'primary';
    }
  }

  // Format match score
  formatMatchScore(score: number): string {
    return `${score.toFixed(1)}%`;
  }

  // Format date
  formatDate(date: Date): string {
    return new Date(date).toLocaleDateString();
  }
}


