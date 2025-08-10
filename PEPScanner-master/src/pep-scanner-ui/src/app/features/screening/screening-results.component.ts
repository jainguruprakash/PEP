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
  templateUrl: './screening-results.component.html',
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

    .alerts-notification {
      display: flex;
      align-items: center;
      gap: 12px;
      padding: 12px 16px;
      background-color: #fff3e0;
      border: 1px solid #ffb74d;
      border-radius: 4px;
      margin-bottom: 16px;
      color: #e65100;
    }

    .alerts-notification mat-icon {
      color: #ff9800;
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

    .match-actions {
      margin-top: 16px;
      padding-top: 16px;
      border-top: 1px solid #e0e0e0;
      display: flex;
      gap: 8px;
    }

    .match-actions button {
      min-width: 120px;
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

    .source-mca {
      background-color: #795548 !important;
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
  @Input() onCreateAlert: ((match: any) => void) | null = null;

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

  createAlert(match: any): void {
    if (this.onCreateAlert) {
      this.onCreateAlert(match);
    }
  }

  viewAlerts(): void {
    // Navigate to alerts page - you can implement routing here
    console.log('Navigate to alerts page');
  }
}
