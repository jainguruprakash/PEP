import { Component, inject, signal } from '@angular/core';
import { CommonModule } from '@angular/common';
import { ReactiveFormsModule, FormBuilder, Validators } from '@angular/forms';
import { MatCardModule } from '@angular/material/card';
import { MatButtonModule } from '@angular/material/button';
import { MatIconModule } from '@angular/material/icon';
import { MatFormFieldModule } from '@angular/material/form-field';
import { MatInputModule } from '@angular/material/input';
import { MatSelectModule } from '@angular/material/select';
import { MatTableModule } from '@angular/material/table';
import { MatChipsModule } from '@angular/material/chips';
import { MatProgressSpinnerModule } from '@angular/material/progress-spinner';
import { MatSnackBarModule, MatSnackBar } from '@angular/material/snack-bar';
import { MatTabsModule } from '@angular/material/tabs';
import { MatExpansionModule } from '@angular/material/expansion';
import { MatDividerModule } from '@angular/material/divider';
import { HttpClient } from '@angular/common/http';
import { ToastService } from '../../services/toast.service';

@Component({
  selector: 'app-adverse-media',
  standalone: true,
  imports: [
    CommonModule, ReactiveFormsModule, MatCardModule, MatButtonModule, 
    MatIconModule, MatFormFieldModule, MatInputModule, MatSelectModule,
    MatTableModule, MatChipsModule, MatProgressSpinnerModule, MatSnackBarModule,
    MatTabsModule, MatExpansionModule, MatDividerModule
  ],
  template: `
    <div class="adverse-media-container">
      <mat-card class="header-card">
        <mat-card-header>
          <mat-card-title>
            <mat-icon>newspaper</mat-icon>
            Adverse Media Screening
          </mat-card-title>
          <mat-card-subtitle>
            Screen individuals and entities against adverse media sources
          </mat-card-subtitle>
        </mat-card-header>
      </mat-card>

      <mat-tab-group>
        <mat-tab label="Screen Individual">
          <div class="tab-content">
            <mat-card class="screening-form-card">
              <mat-card-title>Individual Screening</mat-card-title>
              <form [formGroup]="screeningForm" (ngSubmit)="performScreening()">
                <div class="form-grid">
                  <mat-form-field appearance="outline">
                    <mat-label>Full Name</mat-label>
                    <input matInput formControlName="fullName" required>
                    <mat-hint>Enter the complete name of the individual</mat-hint>
                  </mat-form-field>

                  <mat-form-field appearance="outline">
                    <mat-label>Country</mat-label>
                    <mat-select formControlName="country">
                      <mat-option value="">All Countries</mat-option>
                      @for (country of countries; track country) {
                        <mat-option [value]="country">{{ country }}</mat-option>
                      }
                    </mat-select>
                  </mat-form-field>

                  <mat-form-field appearance="outline">
                    <mat-label>Date of Birth</mat-label>
                    <input matInput type="date" formControlName="dateOfBirth">
                  </mat-form-field>

                  <mat-form-field appearance="outline">
                    <mat-label>Nationality</mat-label>
                    <mat-select formControlName="nationality">
                      <mat-option value="">Any Nationality</mat-option>
                      @for (country of countries; track country) {
                        <mat-option [value]="country">{{ country }}</mat-option>
                      }
                    </mat-select>
                  </mat-form-field>

                  <mat-form-field appearance="outline">
                    <mat-label>Similarity Threshold (%)</mat-label>
                    <input matInput type="number" formControlName="threshold" min="50" max="100">
                    <mat-hint>Minimum match percentage (50-100)</mat-hint>
                  </mat-form-field>
                </div>

                <div class="advanced-options">
                  <mat-expansion-panel>
                    <mat-expansion-panel-header>
                      <mat-panel-title>Advanced Options</mat-panel-title>
                    </mat-expansion-panel-header>
                    
                    <div class="advanced-form-grid">
                      <mat-form-field appearance="outline">
                        <mat-label>Sources</mat-label>
                        <mat-select formControlName="sources" multiple>
                          @for (source of mediaSources; track source.name) {
                            <mat-option [value]="source.name">{{ source.name }}</mat-option>
                          }
                        </mat-select>
                        <mat-hint>Select specific media sources (leave empty for all)</mat-hint>
                      </mat-form-field>

                      <mat-form-field appearance="outline">
                        <mat-label>Categories</mat-label>
                        <mat-select formControlName="categories" multiple>
                          @for (category of mediaCategories; track category) {
                            <mat-option [value]="category">{{ category }}</mat-option>
                          }
                        </mat-select>
                        <mat-hint>Filter by specific categories</mat-hint>
                      </mat-form-field>
                    </div>
                  </mat-expansion-panel>
                </div>

                <div class="form-actions">
                  <button mat-raised-button color="primary" type="submit" 
                          [disabled]="screeningForm.invalid || isScreening()">
                    @if (isScreening()) {
                      <mat-spinner diameter="20"></mat-spinner>
                      Screening...
                    } @else {
                      <mat-icon>search</mat-icon>
                      Start Screening
                    }
                  </button>
                  <button mat-button type="button" (click)="clearForm()">
                    <mat-icon>clear</mat-icon>
                    Clear
                  </button>
                </div>
              </form>
            </mat-card>

            @if (screeningResults()) {
              <mat-card class="results-card">
                <mat-card-header>
                  <mat-card-title>Screening Results</mat-card-title>
                  <div class="results-summary">
                    <mat-chip [color]="getOverallRiskColor(screeningResults()?.overallRiskLevel)">
                      {{ screeningResults()?.overallRiskLevel }} Risk
                    </mat-chip>
                    <span class="match-count">{{ screeningResults()?.totalMatches }} matches found</span>
                  </div>
                </mat-card-header>

                <mat-card-content>
                  <div class="screening-summary">
                    <div class="summary-item">
                      <label>Screened Name:</label>
                      <span>{{ screeningResults()?.fullName }}</span>
                    </div>
                    <div class="summary-item">
                      <label>Screening Date:</label>
                      <span>{{ formatDate(screeningResults()?.screeningDate) }}</span>
                    </div>
                    <div class="summary-item">
                      <label>High Risk Matches:</label>
                      <span>{{ screeningResults()?.highRiskMatches }}</span>
                    </div>
                    <div class="summary-item">
                      <label>Recommended Action:</label>
                      <mat-chip [color]="getActionColor(screeningResults()?.recommendedAction)">
                        {{ screeningResults()?.recommendedAction }}
                      </mat-chip>
                    </div>
                  </div>

                  @if (screeningResults()?.results && screeningResults()?.results.length > 0) {
                    <mat-divider></mat-divider>
                    <h3>Detailed Matches</h3>
                    
                    <div class="matches-list">
                      @for (match of screeningResults()?.results; track match.id) {
                        <mat-expansion-panel class="match-panel">
                          <mat-expansion-panel-header>
                            <mat-panel-title>
                              <div class="match-header">
                                <span class="match-name">{{ match.matchedName }}</span>
                                <mat-chip [color]="getSeverityColor(match.severity)" class="severity-chip">
                                  {{ match.severity }}
                                </mat-chip>
                                <span class="similarity-score">{{ match.similarityScore }}% match</span>
                              </div>
                            </mat-panel-title>
                          </mat-expansion-panel-header>

                          <div class="match-details">
                            <div class="match-info-grid">
                              <div class="info-item">
                                <label>Source:</label>
                                <span>{{ match.source }}</span>
                              </div>
                              <div class="info-item">
                                <label>Category:</label>
                                <span>{{ match.category }}</span>
                              </div>
                              <div class="info-item">
                                <label>Publication Date:</label>
                                <span>{{ formatDate(match.publicationDate) }}</span>
                              </div>
                              <div class="info-item">
                                <label>Country:</label>
                                <span>{{ match.country || 'N/A' }}</span>
                              </div>
                            </div>

                            <div class="match-content">
                              <h4>{{ match.headline }}</h4>
                              <p>{{ match.summary }}</p>
                              @if (match.url) {
                                <a [href]="match.url" target="_blank" mat-button color="primary">
                                  <mat-icon>open_in_new</mat-icon>
                                  Read Full Article
                                </a>
                              }
                            </div>

                            <div class="match-actions">
                              <button mat-raised-button color="warn" (click)="createAlert(match)">
                                <mat-icon>warning</mat-icon>
                                Create Alert
                              </button>
                              <button mat-button (click)="markAsFalsePositive(match)">
                                <mat-icon>block</mat-icon>
                                Mark as False Positive
                              </button>
                            </div>
                          </div>
                        </mat-expansion-panel>
                      }
                    </div>
                  } @else {
                    <div class="no-matches">
                      <mat-icon>check_circle</mat-icon>
                      <p>No adverse media matches found for this individual.</p>
                    </div>
                  }
                </mat-card-content>
              </mat-card>
            }
          </div>
        </mat-tab>

        <mat-tab label="Screening History">
          <div class="tab-content">
            <mat-card>
              <mat-card-title>Recent Screenings</mat-card-title>
              <mat-card-content>
                @if (screeningHistory().length > 0) {
                  <table mat-table [dataSource]="screeningHistory()" class="mat-elevation-z2">
                    <ng-container matColumnDef="name">
                      <th mat-header-cell *matHeaderCellDef>Name</th>
                      <td mat-cell *matCellDef="let row">{{ row.fullName }}</td>
                    </ng-container>
                    <ng-container matColumnDef="date">
                      <th mat-header-cell *matHeaderCellDef>Date</th>
                      <td mat-cell *matCellDef="let row">{{ formatDate(row.screeningDate) }}</td>
                    </ng-container>
                    <ng-container matColumnDef="matches">
                      <th mat-header-cell *matHeaderCellDef>Matches</th>
                      <td mat-cell *matCellDef="let row">{{ row.totalMatches }}</td>
                    </ng-container>
                    <ng-container matColumnDef="risk">
                      <th mat-header-cell *matHeaderCellDef>Risk Level</th>
                      <td mat-cell *matCellDef="let row">
                        <mat-chip [color]="getOverallRiskColor(row.overallRiskLevel)">
                          {{ row.overallRiskLevel }}
                        </mat-chip>
                      </td>
                    </ng-container>
                    <ng-container matColumnDef="actions">
                      <th mat-header-cell *matHeaderCellDef>Actions</th>
                      <td mat-cell *matCellDef="let row">
                        <button mat-icon-button (click)="viewScreeningDetails(row)">
                          <mat-icon>visibility</mat-icon>
                        </button>
                      </td>
                    </ng-container>
                    <tr mat-header-row *matHeaderRowDef="historyColumns"></tr>
                    <tr mat-row *matRowDef="let row; columns: historyColumns;"></tr>
                  </table>
                } @else {
                  <div class="no-history">
                    <mat-icon>history</mat-icon>
                    <p>No screening history available.</p>
                  </div>
                }
              </mat-card-content>
            </mat-card>
          </div>
        </mat-tab>

        <mat-tab label="Sources & Settings">
          <div class="tab-content">
            <mat-card>
              <mat-card-title>Available Media Sources</mat-card-title>
              <mat-card-content>
                <div class="sources-grid">
                  @for (source of mediaSources; track source.name) {
                    <div class="source-card">
                      <div class="source-info">
                        <h4>{{ source.name }}</h4>
                        <p>{{ source.type }} • {{ source.coverage }}</p>
                      </div>
                      <mat-icon class="source-status" color="primary">check_circle</mat-icon>
                    </div>
                  }
                </div>
              </mat-card-content>
            </mat-card>

            <mat-card>
              <mat-card-title>Screening Categories</mat-card-title>
              <mat-card-content>
                <div class="categories-list">
                  @for (category of mediaCategories; track category) {
                    <mat-chip>{{ category }}</mat-chip>
                  }
                </div>
              </mat-card-content>
            </mat-card>
          </div>
        </mat-tab>
      </mat-tab-group>
    </div>
  `,
  styles: [`
    .adverse-media-container { padding: 20px; }
    .header-card { margin-bottom: 20px; }
    .tab-content { padding: 20px 0; }
    .screening-form-card { margin-bottom: 20px; }
    .form-grid { display: grid; grid-template-columns: repeat(auto-fit, minmax(250px, 1fr)); gap: 16px; margin-bottom: 20px; }
    .advanced-form-grid { display: grid; grid-template-columns: repeat(auto-fit, minmax(300px, 1fr)); gap: 16px; margin-top: 16px; }
    .form-actions { display: flex; gap: 10px; margin-top: 20px; }
    .results-card { margin-top: 20px; }
    .results-summary { display: flex; align-items: center; gap: 15px; }
    .match-count { color: #666; font-size: 0.875rem; }
    .screening-summary { display: grid; grid-template-columns: repeat(auto-fit, minmax(200px, 1fr)); gap: 15px; margin-bottom: 20px; }
    .summary-item { display: flex; flex-direction: column; }
    .summary-item label { font-weight: 500; color: #666; margin-bottom: 5px; }
    .matches-list { margin-top: 20px; }
    .match-panel { margin-bottom: 15px; }
    .match-header { display: flex; align-items: center; gap: 15px; width: 100%; }
    .match-name { font-weight: 500; }
    .severity-chip { font-size: 0.75rem; }
    .similarity-score { color: #666; font-size: 0.875rem; margin-left: auto; }
    .match-details { padding: 15px 0; }
    .match-info-grid { display: grid; grid-template-columns: repeat(auto-fit, minmax(150px, 1fr)); gap: 15px; margin-bottom: 15px; }
    .info-item { display: flex; flex-direction: column; }
    .info-item label { font-weight: 500; color: #666; margin-bottom: 5px; }
    .match-content { margin: 20px 0; }
    .match-content h4 { margin: 0 0 10px 0; color: #333; }
    .match-content p { margin: 0 0 15px 0; color: #666; line-height: 1.5; }
    .match-actions { display: flex; gap: 10px; }
    .no-matches, .no-history { display: flex; flex-direction: column; align-items: center; padding: 40px; color: #999; }
    .no-matches mat-icon, .no-history mat-icon { font-size: 48px; width: 48px; height: 48px; margin-bottom: 10px; }
    .historyColumns { width: 100%; }
    .sources-grid { display: grid; grid-template-columns: repeat(auto-fit, minmax(300px, 1fr)); gap: 15px; }
    .source-card { display: flex; justify-content: space-between; align-items: center; padding: 15px; border: 1px solid #e0e0e0; border-radius: 8px; }
    .source-info h4 { margin: 0 0 5px 0; }
    .source-info p { margin: 0; color: #666; font-size: 0.875rem; }
    .categories-list { display: flex; flex-wrap: wrap; gap: 10px; }
    table { width: 100%; margin-top: 16px; }
  `]
})
export class AdverseMediaComponent {
  private fb = inject(FormBuilder);
  private http = inject(HttpClient);
  private snackBar = inject(MatSnackBar);
  private toastService = inject(ToastService);

  isScreening = signal(false);
  screeningResults = signal<any>(null);
  screeningHistory = signal<any[]>([]);

  countries = [
    'United States', 'United Kingdom', 'Canada', 'Australia', 'Germany',
    'France', 'India', 'China', 'Japan', 'Brazil', 'Russia', 'South Africa'
  ];

  mediaSources = [
    { name: 'Reuters', type: 'News Agency', coverage: 'Global' },
    { name: 'BBC News', type: 'News Agency', coverage: 'Global' },
    { name: 'Financial Times', type: 'Financial News', coverage: 'Global' },
    { name: 'Bloomberg', type: 'Financial News', coverage: 'Global' },
    { name: 'Wall Street Journal', type: 'Financial News', coverage: 'Global' },
    { name: 'Associated Press', type: 'News Agency', coverage: 'Global' },
    { name: 'CNN', type: 'News Network', coverage: 'Global' },
    { name: 'CNBC', type: 'Financial News', coverage: 'Global' }
  ];

  mediaCategories = [
    'Financial Crime', 'Corruption', 'Money Laundering', 'Sanctions Violation',
    'Fraud', 'Tax Evasion', 'Bribery', 'Embezzlement', 'Terrorism Financing',
    'Drug Trafficking', 'Human Trafficking', 'Cybercrime'
  ];

  historyColumns = ['name', 'date', 'matches', 'risk', 'actions'];

  screeningForm = this.fb.group({
    fullName: ['', Validators.required],
    country: [''],
    dateOfBirth: [''],
    nationality: [''],
    threshold: [75, [Validators.min(50), Validators.max(100)]],
    sources: [null],
    categories: [null]
  });

  performScreening() {
    if (this.screeningForm.valid) {
      this.isScreening.set(true);
      const formData = this.screeningForm.value;

      this.http.post('http://localhost:5098/api/adverse-media/screen', formData).subscribe({
        next: (response: any) => {
          this.screeningResults.set(response);
          this.addToHistory(response);
          this.isScreening.set(false);
          this.toastService.showSuccess(`Screening completed. Found ${response.totalMatches} matches.`);
        },
        error: (error) => {
          console.error('Screening error:', error);
          this.isScreening.set(false);
          this.toastService.showError('Screening failed. Please try again.');
        }
      });
    }
  }

  createAlert(match: any) {
    const alertRequest = {
      customerName: this.screeningResults()?.fullName,
      source: match.source,
      category: match.category,
      severity: match.severity,
      similarityScore: match.similarityScore,
      headline: match.headline,
      summary: match.summary,
      url: match.url,
      publicationDate: match.publicationDate,
      createdBy: 'AdverseMediaUser'
    };

    this.http.post('http://localhost:5098/api/adverse-media/create-alert', alertRequest).subscribe({
      next: (response: any) => {
        this.toastService.showSuccess('Alert created successfully');
      },
      error: (error) => {
        console.error('Alert creation error:', error);
        this.toastService.showError('Failed to create alert. Please try again.');
      }
    });
  }

  markAsFalsePositive(match: any) {
    // In a real implementation, this would mark the match as a false positive
    this.toastService.showInfo('Match marked as false positive');
  }

  clearForm() {
    this.screeningForm.reset();
    this.screeningForm.patchValue({
      threshold: 75,
      sources: null,
      categories: null
    });
    this.screeningResults.set(null);
  }

  viewScreeningDetails(screening: any) {
    this.screeningResults.set(screening);
  }

  private addToHistory(result: any) {
    const history = this.screeningHistory();
    history.unshift(result);
    this.screeningHistory.set(history.slice(0, 50)); // Keep last 50 screenings
  }

  getSeverityColor(severity: string): string {
    switch(severity?.toLowerCase()) {
      case 'critical': return 'warn';
      case 'high': return 'warn';
      case 'medium': return 'accent';
      default: return 'primary';
    }
  }

  getOverallRiskColor(risk: string): string {
    switch(risk?.toLowerCase()) {
      case 'critical': return 'warn';
      case 'high': return 'warn';
      case 'medium': return 'accent';
      default: return 'primary';
    }
  }

  getActionColor(action: string): string {
    switch(action?.toLowerCase()) {
      case 'reject': return 'warn';
      case 'enhanced due diligence': return 'accent';
      case 'additional review': return 'accent';
      default: return 'primary';
    }
  }

  formatDate(date: string | Date): string {
    if (!date) return 'N/A';
    const d = typeof date === 'string' ? new Date(date) : date;
    return d.toLocaleDateString() + ' ' + d.toLocaleTimeString([], { hour: '2-digit', minute: '2-digit' });
  }
}