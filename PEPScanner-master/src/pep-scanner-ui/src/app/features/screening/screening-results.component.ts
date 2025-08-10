import { Component, Input } from '@angular/core';
import { CommonModule } from '@angular/common';
import { MatCardModule } from '@angular/material/card';
import { MatChipsModule } from '@angular/material/chips';
import { MatIconModule } from '@angular/material/icon';
import { MatButtonModule } from '@angular/material/button';
import { MatExpansionModule } from '@angular/material/expansion';
import { MatDividerModule } from '@angular/material/divider';
import { MatBadgeModule } from '@angular/material/badge';

@Component({
  selector: 'app-screening-results',
  standalone: true,
  imports: [
    CommonModule, 
    MatCardModule, 
    MatChipsModule, 
    MatIconModule, 
    MatButtonModule, 
    MatExpansionModule,
    MatDividerModule,
    MatBadgeModule
  ],
  template: `
    <mat-card class="results-card" *ngIf="result">
      <!-- Header Section -->
      <mat-card-header class="results-header">
        <div class="header-content">
          <div class="customer-info">
            <h2>{{ result.fullName || result.customerName }}</h2>
            <div class="customer-details">
              <span *ngIf="result.nationality">{{ result.nationality }}</span>
              <span *ngIf="result.country" class="separator">{{ result.country }}</span>
              <span *ngIf="result.dateOfBirth" class="separator">DOB: {{ result.dateOfBirth | date }}</span>
            </div>
          </div>
          <div class="status-section">
            <mat-chip-set>
              <mat-chip [class]="getStatusClass(result.status)">
                <mat-icon>{{ getStatusIcon(result.status) }}</mat-icon>
                {{ result.status }}
              </mat-chip>
            </mat-chip-set>
            <div class="risk-score">
              Risk Score: <strong>{{ (result.riskScore * 100) | number:'1.0-0' }}%</strong>
            </div>
          </div>
        </div>
      </mat-card-header>

      <mat-card-content>
        <!-- Summary Section -->
        <div class="summary-section">
          <div class="summary-stats">
            <div class="stat-item">
              <mat-icon>search</mat-icon>
              <div>
                <div class="stat-value">{{ result.matchCount }}</div>
                <div class="stat-label">Matches Found</div>
              </div>
            </div>
            <div class="stat-item">
              <mat-icon>database</mat-icon>
              <div>
                <div class="stat-value">{{ result.screeningDetails?.totalWatchlistEntries }}</div>
                <div class="stat-label">Records Searched</div>
              </div>
            </div>
            <div class="stat-item">
              <mat-icon>source</mat-icon>
              <div>
                <div class="stat-value">{{ result.screeningDetails?.sourcesSearched?.length }}</div>
                <div class="stat-label">Sources</div>
              </div>
            </div>
          </div>
        </div>

        <mat-divider></mat-divider>

        <!-- Matches Section -->
        <div class="matches-section" *ngIf="result.matches && result.matches.length > 0">
          <h3>
            <mat-icon>warning</mat-icon>
            Watchlist Matches ({{ result.matches.length }})
          </h3>
          
          <mat-accordion>
            <mat-expansion-panel *ngFor="let match of result.matches; let i = index" class="match-panel">
              <mat-expansion-panel-header>
                <mat-panel-title>
                  <div class="match-header">
                    <div class="match-name">{{ match.matchedName }}</div>
                    <mat-chip-set>
                      <mat-chip [class]="getSourceClass(match.source)">{{ match.source }}</mat-chip>
                      <mat-chip [class]="getRiskClass(match.riskCategory)">{{ match.riskCategory }}</mat-chip>
                    </mat-chip-set>
                  </div>
                </mat-panel-title>
                <mat-panel-description>
                  <div class="match-score">
                    Match: {{ (match.matchScore * 100) | number:'1.0-0' }}%
                  </div>
                </mat-panel-description>
              </mat-expansion-panel-header>

              <div class="match-details">
                <div class="detail-grid">
                  <div class="detail-item" *ngIf="match.alternateNames">
                    <strong>Alternate Names:</strong>
                    <span>{{ match.alternateNames }}</span>
                  </div>
                  <div class="detail-item" *ngIf="match.country">
                    <strong>Country:</strong>
                    <span>{{ match.country }}</span>
                  </div>
                  <div class="detail-item" *ngIf="match.positionOrRole">
                    <strong>Position/Role:</strong>
                    <span>{{ match.positionOrRole }}</span>
                  </div>
                  <div class="detail-item" *ngIf="match.listType">
                    <strong>List Type:</strong>
                    <span>{{ match.listType }}</span>
                  </div>
                  <div class="detail-item" *ngIf="match.sanctionType">
                    <strong>Sanction Type:</strong>
                    <span>{{ match.sanctionType }}</span>
                  </div>
                  <div class="detail-item" *ngIf="match.sanctionReason">
                    <strong>Sanction Reason:</strong>
                    <span>{{ match.sanctionReason }}</span>
                  </div>
                  <div class="detail-item" *ngIf="match.pepCategory">
                    <strong>PEP Category:</strong>
                    <span>{{ match.pepCategory }}</span>
                  </div>
                  <div class="detail-item" *ngIf="match.pepPosition">
                    <strong>PEP Position:</strong>
                    <span>{{ match.pepPosition }}</span>
                  </div>
                </div>
                
                <mat-divider></mat-divider>
                
                <div class="match-metadata">
                  <div class="metadata-item">
                    <mat-icon>schedule</mat-icon>
                    <span>Added: {{ match.dateAdded | date:'short' }}</span>
                  </div>
                  <div class="metadata-item">
                    <mat-icon>update</mat-icon>
                    <span>Updated: {{ match.lastUpdated | date:'short' }}</span>
                  </div>
                </div>
              </div>
            </mat-expansion-panel>
          </mat-accordion>
        </div>

        <!-- No Matches Section -->
        <div class="no-matches-section" *ngIf="!result.matches || result.matches.length === 0">
          <div class="no-matches-content">
            <mat-icon class="success-icon">check_circle</mat-icon>
            <h3>No Watchlist Matches Found</h3>
            <p>This individual was not found on any monitored watchlists.</p>
          </div>
        </div>

        <mat-divider></mat-divider>

        <!-- Screening Details -->
        <div class="screening-details">
          <h4>Screening Details</h4>
          <div class="details-grid">
            <div class="detail-item">
              <strong>Search Criteria:</strong>
              <span>{{ result.screeningDetails?.searchCriteria }}</span>
            </div>
            <div class="detail-item">
              <strong>Sources Searched:</strong>
              <mat-chip-set>
                <mat-chip *ngFor="let source of result.screeningDetails?.sourcesSearched" 
                         [class]="getSourceClass(source)">
                  {{ source }}
                </mat-chip>
              </mat-chip-set>
            </div>
            <div class="detail-item">
              <strong>Screened At:</strong>
              <span>{{ result.screenedAt | date:'medium' }}</span>
            </div>
          </div>
        </div>
      </mat-card-content>

      <mat-card-actions>
        <button mat-raised-button color="primary" (click)="exportResults()">
          <mat-icon>download</mat-icon>
          Export Results
        </button>
        <button mat-button (click)="printResults()">
          <mat-icon>print</mat-icon>
          Print
        </button>
      </mat-card-actions>
    </mat-card>
  `,
  styles: [`
    .results-card {
      margin: 16px 0;
      max-width: 1200px;
    }

    .results-header {
      padding: 16px;
      background: linear-gradient(135deg, #f5f7fa 0%, #c3cfe2 100%);
    }

    .header-content {
      display: flex;
      justify-content: space-between;
      align-items: flex-start;
      width: 100%;
    }

    .customer-info h2 {
      margin: 0 0 8px 0;
      font-size: 24px;
      font-weight: 500;
    }

    .customer-details {
      color: #666;
      font-size: 14px;
    }

    .separator::before {
      content: " • ";
      margin: 0 4px;
    }

    .status-section {
      text-align: right;
    }

    .risk-score {
      margin-top: 8px;
      font-size: 14px;
    }

    .summary-section {
      padding: 16px 0;
    }

    .summary-stats {
      display: flex;
      gap: 32px;
      justify-content: center;
    }

    .stat-item {
      display: flex;
      align-items: center;
      gap: 8px;
      text-align: center;
    }

    .stat-value {
      font-size: 24px;
      font-weight: bold;
      color: #1976d2;
    }

    .stat-label {
      font-size: 12px;
      color: #666;
    }

    .matches-section {
      margin: 16px 0;
    }

    .matches-section h3 {
      display: flex;
      align-items: center;
      gap: 8px;
      color: #f57c00;
      margin-bottom: 16px;
    }

    .match-panel {
      margin-bottom: 8px;
    }

    .match-header {
      display: flex;
      justify-content: space-between;
      align-items: center;
      width: 100%;
    }

    .match-name {
      font-weight: 500;
      font-size: 16px;
    }

    .match-score {
      font-weight: bold;
      color: #1976d2;
    }

    .match-details {
      padding: 16px 0;
    }

    .detail-grid {
      display: grid;
      grid-template-columns: repeat(auto-fit, minmax(300px, 1fr));
      gap: 12px;
      margin-bottom: 16px;
    }

    .detail-item {
      display: flex;
      flex-direction: column;
      gap: 4px;
    }

    .detail-item strong {
      font-size: 12px;
      color: #666;
      text-transform: uppercase;
    }

    .match-metadata {
      display: flex;
      gap: 24px;
      margin-top: 16px;
    }

    .metadata-item {
      display: flex;
      align-items: center;
      gap: 4px;
      font-size: 12px;
      color: #666;
    }

    .no-matches-section {
      text-align: center;
      padding: 32px;
    }

    .no-matches-content .success-icon {
      font-size: 48px;
      color: #4caf50;
      margin-bottom: 16px;
    }

    .screening-details {
      margin-top: 16px;
    }

    .screening-details h4 {
      margin-bottom: 12px;
      color: #333;
    }

    .details-grid {
      display: grid;
      gap: 12px;
    }

    /* Status Classes */
    .status-clear {
      background-color: #4caf50 !important;
      color: white !important;
    }

    .status-low-risk {
      background-color: #ff9800 !important;
      color: white !important;
    }

    .status-medium-risk {
      background-color: #f57c00 !important;
      color: white !important;
    }

    .status-high-risk {
      background-color: #f44336 !important;
      color: white !important;
    }

    /* Source Classes */
    .source-ofac {
      background-color: #d32f2f !important;
      color: white !important;
    }

    .source-un {
      background-color: #1976d2 !important;
      color: white !important;
    }

    .source-eu {
      background-color: #303f9f !important;
      color: white !important;
    }

    .source-uk {
      background-color: #7b1fa2 !important;
      color: white !important;
    }

    .source-rbi, .source-sebi, .source-parliament {
      background-color: #388e3c !important;
      color: white !important;
    }

    /* Risk Classes */
    .risk-high {
      background-color: #f44336 !important;
      color: white !important;
    }

    .risk-medium {
      background-color: #ff9800 !important;
      color: white !important;
    }

    .risk-low {
      background-color: #4caf50 !important;
      color: white !important;
    }

    @media (max-width: 768px) {
      .header-content {
        flex-direction: column;
        gap: 16px;
      }

      .summary-stats {
        flex-direction: column;
        gap: 16px;
      }

      .match-header {
        flex-direction: column;
        align-items: flex-start;
        gap: 8px;
      }

      .detail-grid {
        grid-template-columns: 1fr;
      }
    }
  `]
})
export class ScreeningResultsComponent {
  @Input() result: any = null;

  getStatusClass(status: string): string {
    return `status-${status.toLowerCase().replace(' ', '-')}`;
  }

  getStatusIcon(status: string): string {
    switch (status.toLowerCase()) {
      case 'clear': return 'check_circle';
      case 'low risk': return 'warning';
      case 'medium risk': return 'error';
      case 'high risk': return 'dangerous';
      default: return 'help';
    }
  }

  getSourceClass(source: string): string {
    return `source-${source.toLowerCase()}`;
  }

  getRiskClass(risk: string): string {
    return `risk-${risk.toLowerCase()}`;
  }

  exportResults(): void {
    const dataStr = JSON.stringify(this.result, null, 2);
    const dataBlob = new Blob([dataStr], { type: 'application/json' });
    const url = URL.createObjectURL(dataBlob);
    const link = document.createElement('a');
    link.href = url;
    link.download = `screening-results-${new Date().toISOString().split('T')[0]}.json`;
    link.click();
    URL.revokeObjectURL(url);
  }

  printResults(): void {
    window.print();
  }
}
