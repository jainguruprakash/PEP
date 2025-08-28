import { Component, inject, signal } from '@angular/core';
import { CommonModule } from '@angular/common';
import { MatButtonModule } from '@angular/material/button';
import { MatListModule } from '@angular/material/list';
import { MatCardModule } from '@angular/material/card';
import { MatIconModule } from '@angular/material/icon';
import { MatChipsModule } from '@angular/material/chips';
import { MatProgressSpinnerModule } from '@angular/material/progress-spinner';
import { MatDividerModule } from '@angular/material/divider';
import { MatTooltipModule } from '@angular/material/tooltip';
import { WatchlistService } from '../../services/watchlist.service';
import { ToastService } from '../../services/toast.service';

@Component({
  selector: 'app-watchlists',
  standalone: true,
  imports: [
    CommonModule,
    MatButtonModule,
    MatListModule,
    MatCardModule,
    MatIconModule,
    MatChipsModule,
    MatProgressSpinnerModule,
    MatDividerModule,
    MatTooltipModule
  ],
  template: `
    <div class="watchlists-container">
      <mat-card class="main-card">
        <mat-card-header>
          <mat-card-title>
            <mat-icon>list_alt</mat-icon>
            Watchlist Management
          </mat-card-title>
          <mat-card-subtitle>
            Manage and update global watchlist sources
          </mat-card-subtitle>
        </mat-card-header>

        <mat-card-content>
          <!-- Update All Section -->
          <div class="update-all-section">
            <button mat-raised-button color="primary"
                    [disabled]="isUpdating()"
                    (click)="updateAll()">
              <mat-spinner diameter="20" *ngIf="isUpdating()"></mat-spinner>
              <mat-icon *ngIf="!isUpdating()">refresh</mat-icon>
              <span>{{ isUpdating() ? 'Updating...' : 'Update All Watchlists' }}</span>
            </button>
            <p class="update-help">Updates all watchlist sources with latest data</p>
          </div>

          <mat-divider></mat-divider>

          <!-- Individual Sources -->
          <div class="sources-section">
            <h3>Individual Sources</h3>
            <div class="sources-grid">
              <mat-card *ngFor="let source of sources" class="source-card">
                <mat-card-header>
                  <mat-card-title>{{ source.name }}</mat-card-title>
                  <mat-card-subtitle>{{ source.type }} - {{ source.country }}</mat-card-subtitle>
                </mat-card-header>
                <mat-card-content>
                  <div class="source-info">
                    <div class="info-item">
                      <span class="label">Last Updated:</span>
                      <span class="value">{{ source.lastUpdated | date:'medium' || 'Never' }}</span>
                    </div>
                    <div class="info-item">
                      <span class="label">Records:</span>
                      <span class="value">{{ source.recordCount || 'N/A' }}</span>
                    </div>
                    <div class="info-item">
                      <span class="label">Status:</span>
                      <mat-chip [class]="getStatusClass(source.status)">
                        {{ source.status || 'Unknown' }}
                      </mat-chip>
                    </div>
                  </div>
                </mat-card-content>
                <mat-card-actions>
                  <button mat-button color="primary"
                          [disabled]="updatingSource() === source.name"
                          (click)="update(source.name)">
                    <mat-spinner diameter="16" *ngIf="updatingSource() === source.name"></mat-spinner>
                    <mat-icon *ngIf="updatingSource() !== source.name">refresh</mat-icon>
                    <span>{{ updatingSource() === source.name ? 'Updating...' : 'Update' }}</span>
                  </button>
                </mat-card-actions>
              </mat-card>
            </div>
          </div>

          <!-- Last Updates Summary -->
          <div class="updates-summary" *ngIf="lastUpdates">
            <h3>Recent Updates</h3>
            <mat-card class="summary-card">
              <mat-card-content>
                <pre class="updates-json">{{ lastUpdates | json }}</pre>
              </mat-card-content>
            </mat-card>
          </div>
        </mat-card-content>
      </mat-card>
    </div>
  `,
  styles: [`
    .watchlists-container {
      padding: 24px;
      max-width: 1200px;
      margin: 0 auto;
    }

    .main-card {
      margin-bottom: 24px;
    }

    .update-all-section {
      text-align: center;
      padding: 24px 0;
    }

    .update-all-section button {
      min-width: 200px;
      height: 48px;
    }

    .update-help {
      margin-top: 8px;
      color: #666;
      font-size: 14px;
    }

    .sources-section {
      margin-top: 24px;
    }

    .sources-section h3 {
      margin-bottom: 16px;
      color: #333;
    }

    .sources-grid {
      display: grid;
      grid-template-columns: repeat(auto-fit, minmax(300px, 1fr));
      gap: 16px;
    }

    .source-card {
      border: 1px solid #e0e0e0;
    }

    .source-info {
      margin: 16px 0;
    }

    .info-item {
      display: flex;
      justify-content: space-between;
      margin-bottom: 8px;
    }

    .label {
      font-weight: 500;
      color: #666;
    }

    .value {
      color: #333;
    }

    .updates-summary {
      margin-top: 32px;
    }

    .updates-summary h3 {
      margin-bottom: 16px;
      color: #333;
    }

    .summary-card {
      background-color: #f8f9fa;
    }

    .updates-json {
      font-size: 12px;
      max-height: 200px;
      overflow-y: auto;
      background-color: #fff;
      padding: 12px;
      border-radius: 4px;
      border: 1px solid #e0e0e0;
    }

    .status-active {
      background-color: #4caf50 !important;
      color: white !important;
    }

    .status-updating {
      background-color: #ff9800 !important;
      color: white !important;
    }

    .status-error {
      background-color: #f44336 !important;
      color: white !important;
    }

    .status-unknown {
      background-color: #9e9e9e !important;
      color: white !important;
    }

    @media (max-width: 768px) {
      .watchlists-container {
        padding: 16px;
      }

      .sources-grid {
        grid-template-columns: 1fr;
      }

      .update-all-section button {
        width: 100%;
      }
    }
  `]
})
export class WatchlistsComponent {
  private watchlistService = inject(WatchlistService);
  private toastService = inject(ToastService);

  sources: any[] = [];
  lastUpdates: any;
  isUpdating = signal(false);
  updatingSource = signal<string | null>(null);

  constructor(){
    this.loadSources();
    this.loadLastUpdates();
  }

  private loadSources() {
    this.watchlistService.getGenericSources().subscribe({
      next: (sources) => {
        this.sources = sources as any[] || [];
      },
      error: (error) => {
        console.error('Error loading sources:', error);
        this.toastService.error('Failed to load watchlist sources');
      }
    });
  }

  private loadLastUpdates() {
    this.watchlistService.getLastUpdates().subscribe({
      next: (updates) => {
        this.lastUpdates = updates;
      },
      error: (error) => {
        console.error('Error loading last updates:', error);
      }
    });
  }

  updateAll() {
    this.isUpdating.set(true);
    this.toastService.info('Starting update for all watchlists...');

    this.watchlistService.updateAll().subscribe({
      next: (response) => {
        this.isUpdating.set(false);
        this.toastService.success('All watchlists updated successfully');
        this.loadLastUpdates();
      },
      error: (error) => {
        this.isUpdating.set(false);
        console.error('Error updating all watchlists:', error);
        this.toastService.error('Failed to update watchlists');
      }
    });
  }

  update(sourceName: string) {
    this.updatingSource.set(sourceName);
    this.toastService.info(`Updating ${sourceName} watchlist...`);

    this.watchlistService.updateSource(sourceName).subscribe({
      next: (response) => {
        this.updatingSource.set(null);
        this.toastService.success(`${sourceName} watchlist updated successfully`);
        this.loadLastUpdates();
      },
      error: (error) => {
        this.updatingSource.set(null);
        console.error(`Error updating ${sourceName}:`, error);
        this.toastService.error(`Failed to update ${sourceName} watchlist`);
      }
    });
  }

  getStatusClass(status: string): string {
    if (!status) return 'status-unknown';

    switch (status.toLowerCase()) {
      case 'active':
      case 'updated':
      case 'success':
        return 'status-active';
      case 'updating':
      case 'in-progress':
        return 'status-updating';
      case 'error':
      case 'failed':
        return 'status-error';
      default:
        return 'status-unknown';
    }
  }
}


