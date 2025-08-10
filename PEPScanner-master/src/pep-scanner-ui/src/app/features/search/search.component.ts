import { Component, inject, signal } from '@angular/core';
import { CommonModule } from '@angular/common';
import { ReactiveFormsModule, FormBuilder, Validators } from '@angular/forms';
import { MatFormFieldModule } from '@angular/material/form-field';
import { MatInputModule } from '@angular/material/input';
import { MatButtonModule } from '@angular/material/button';
import { MatTableModule } from '@angular/material/table';
import { MatSelectModule } from '@angular/material/select';
import { MatChipsModule } from '@angular/material/chips';
import { MatIconModule } from '@angular/material/icon';
import { MatCardModule } from '@angular/material/card';
import { MatProgressSpinnerModule } from '@angular/material/progress-spinner';
import { ScreeningService } from '../../services/screening.service';

export interface SearchResult {
  id: string;
  name: string;
  source: string;
  listType: string;
  riskLevel: string;
  country?: string;
  matchScore: number;
  lastUpdated: Date;
}

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
    MatSelectModule,
    MatChipsModule,
    MatIconModule,
    MatCardModule,
    MatProgressSpinnerModule
  ],
  templateUrl: './search.component.html',
  styleUrls: ['./search.component.scss']
})
export class SearchComponent {
  private fb = inject(FormBuilder);
  private screeningService = inject(ScreeningService);

  // Signals for reactive state management
  isLoading = signal(false);
  searchResults = signal<SearchResult[]>([]);

  // Form for search
  searchForm = this.fb.group({
    searchTerm: ['', Validators.required],
    searchType: ['name'],
    threshold: [75],
    maxResults: [100]
  });

  // Configuration options
  searchTypes = [
    { value: 'name', label: 'Name Search' },
    { value: 'id', label: 'ID Number Search' },
    { value: 'keyword', label: 'Keyword Search' },
    { value: 'fuzzy', label: 'Fuzzy Search' }
  ];

  // Table configuration
  displayedColumns = ['name', 'source', 'listType', 'riskLevel', 'country', 'matchScore', 'actions'];

  // Search functionality
  performSearch() {
    if (this.searchForm.invalid) {
      return;
    }

    this.isLoading.set(true);
    const formValue = this.searchForm.value;

    const searchRequest: any = {
      searchTerm: formValue.searchTerm || '',
      searchType: formValue.searchType || 'name',
      threshold: formValue.threshold || 75,
      maxResults: formValue.maxResults || 100
    };

    this.screeningService.search(searchRequest).subscribe({
      next: (results) => {
        // Convert results to SearchResult[]
        const searchResults: SearchResult[] = Array.isArray(results) ? results.map(match => ({
          id: match.matchId || Math.random().toString(),
          name: match.matchedName || '',
          source: match.source || '',
          listType: match.listType || '',
          riskLevel: match.riskCategory || 'Medium',
          country: match.country || '',
          matchScore: match.matchScore || 0,
          lastUpdated: new Date()
        })) : [];

        this.searchResults.set(searchResults);
        this.isLoading.set(false);
      },
      error: (error) => {
        console.error('Search error:', error);
        this.isLoading.set(false);
      }
    });
  }

  // View details
  viewDetails(result: SearchResult) {
    console.log('View details for:', result);
  }

  // Format match score
  formatMatchScore(score: number): string {
    return `${score.toFixed(1)}%`;
  }

  // Risk level styling
  getRiskLevelColor(riskLevel: string): string {
    switch (riskLevel.toLowerCase()) {
      case 'high':
        return 'warn';
      case 'medium':
        return 'accent';
      case 'low':
        return 'primary';
      default:
        return 'primary';
    }
  }
}


