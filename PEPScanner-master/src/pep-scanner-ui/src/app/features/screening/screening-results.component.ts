import { Component, Input, OnInit } from '@angular/core';
import { CommonModule } from '@angular/common';
import { MatCardModule } from '@angular/material/card';
import { MatChipsModule } from '@angular/material/chips';
import { MatIconModule } from '@angular/material/icon';
import { MatButtonModule } from '@angular/material/button';
import { MatExpansionModule } from '@angular/material/expansion';
import { MatDividerModule } from '@angular/material/divider';
import { MatBadgeModule } from '@angular/material/badge';
import { MatPaginatorModule, PageEvent } from '@angular/material/paginator';

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
    MatBadgeModule,
    MatPaginatorModule
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

    .alert-success-banner {
      background-color: #d4edda;
      border: 1px solid #c3e6cb;
      border-radius: 8px;
      padding: 16px;
      margin-bottom: 16px;
      display: flex;
      align-items: center;
      gap: 12px;
      animation: slideIn 0.5s ease-out;
    }

    .alert-success-banner mat-icon {
      color: #155724;
    }

    .alert-success-banner span {
      flex: 1;
      color: #155724;
      font-weight: 500;
    }

    @keyframes slideIn {
      from {
        opacity: 0;
        transform: translateY(-10px);
      }
      to {
        opacity: 1;
        transform: translateY(0);
      }
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

    .matches-header {
      display: flex;
      justify-content: space-between;
      align-items: center;
      margin-bottom: 16px;
      flex-wrap: wrap;
      gap: 16px;
    }

    .matches-header h3 {
      display: flex;
      align-items: center;
      gap: 8px;
      margin: 0;
      color: #f57c00;
      font-weight: 600;
    }

    .pagination-controls {
      display: flex;
      align-items: center;
    }

    .pagination-controls mat-paginator {
      background: transparent;
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
      overflow: visible !important;
    }

    ::ng-deep .match-panel .mat-expansion-panel-content {
      overflow: visible !important;
    }

    ::ng-deep .match-panel .mat-expansion-panel-body {
      padding: 16px !important;
      overflow: visible !important;
    }

    .match-header {
      display: flex;
      justify-content: space-between;
      align-items: center;
      width: 100%;
      flex-wrap: wrap;
      gap: 8px;
    }

    .match-name {
      font-weight: 500;
      font-size: 16px;
      flex: 1;
      min-width: 200px;
    }

    .match-score {
      font-weight: bold;
      color: #1976d2;
      white-space: nowrap;
    }

    .match-details {
      padding: 16px 0;
    }

    .detail-grid {
      display: grid;
      grid-template-columns: repeat(auto-fit, minmax(300px, 1fr));
      gap: 20px;
      margin-bottom: 20px;
      padding: 16px;
      background-color: #f8f9fa;
      border-radius: 8px;
      border: 1px solid #e9ecef;
    }

    .detail-item {
      display: flex;
      flex-direction: column;
      gap: 6px;
      padding: 8px;
      background-color: #f8f9fa;
      border-radius: 4px;
      border-left: 3px solid #1976d2;
    }

    .detail-item strong {
      font-size: 11px;
      color: #666;
      text-transform: uppercase;
      font-weight: 600;
      letter-spacing: 0.5px;
    }

    .detail-item span {
      font-size: 14px;
      color: #333;
      word-break: break-word;
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
      gap: 12px;
      flex-wrap: wrap;
    }

    .match-actions button {
      min-width: 140px;
      height: 40px;
      display: flex;
      align-items: center;
      justify-content: center;
      gap: 8px;
    }

    .match-actions button mat-icon {
      margin-right: 4px;
    }

    .match-actions button span {
      font-size: 14px;
      font-weight: 500;
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

    /* Alert Status Styles */
    .alert-created-icon {
      color: #4caf50;
      font-size: 18px;
      margin-left: 8px;
    }

    .alert-chip {
      background-color: #4caf50 !important;
      color: white !important;
      font-size: 11px;
    }

    .alert-chip mat-icon {
      font-size: 14px;
      margin-right: 4px;
    }

    .recent-alert-chip {
      background-color: #ff9800 !important;
      color: white !important;
      font-size: 11px;
      animation: pulse 2s infinite;
    }

    .recent-alert-chip mat-icon {
      font-size: 14px;
      margin-right: 4px;
    }

    @keyframes pulse {
      0% { opacity: 1; }
      50% { opacity: 0.7; }
      100% { opacity: 1; }
    }

    .match-actions button[disabled] {
      opacity: 0.7;
    }

    .match-actions button:not(:last-child) {
      margin-right: 8px;
    }

    /* Global Material Design fixes for button text */
    ::ng-deep .mat-mdc-button .mdc-button__label,
    ::ng-deep .mat-mdc-raised-button .mdc-button__label {
      display: inline-flex !important;
      visibility: visible !important;
      opacity: 1 !important;
      align-items: center !important;
      gap: 8px !important;
      white-space: nowrap !important;
    }

    ::ng-deep .mat-mdc-button span,
    ::ng-deep .mat-mdc-raised-button span {
      display: inline !important;
      visibility: visible !important;
      opacity: 1 !important;
      color: inherit !important;
    }

    /* Fix overlapping issues */
    .match-actions {
      display: flex !important;
      flex-wrap: wrap !important;
      gap: 12px !important;
      margin-top: 16px !important;
      align-items: center !important;
      justify-content: flex-start !important;
    }

    .match-actions button {
      flex-shrink: 0 !important;
      margin: 0 !important;
      white-space: nowrap !important;
      overflow: visible !important;
      min-height: 36px !important;
    }

    /* Button styles */
    .create-alert-btn,
    .alert-created-btn {
      font-weight: 500 !important;
      min-width: 160px !important;
      display: inline-flex !important;
      align-items: center !important;
      justify-content: center !important;
      gap: 8px !important;
      padding: 8px 16px !important;
      text-transform: none !important;
    }

    .create-alert-btn mat-icon,
    .alert-created-btn mat-icon {
      display: inline-flex !important;
      font-size: 18px !important;
      width: 18px !important;
      height: 18px !important;
      margin: 0 !important;
    }

    .alert-created-btn {
      opacity: 0.8 !important;
    }

    .view-alert-btn,
    .export-btn {
      display: inline-flex !important;
      align-items: center !important;
      gap: 8px !important;
      padding: 8px 16px !important;
    }

    .view-alert-btn mat-icon,
    .export-btn mat-icon {
      font-size: 18px !important;
      width: 18px !important;
      height: 18px !important;
      margin: 0 !important;
    }

    /* Additional fixes for text visibility */
    ::ng-deep .mat-mdc-button,
    ::ng-deep .mat-mdc-raised-button {
      font-size: 14px !important;
      line-height: 1.4 !important;
      text-transform: none !important;
      font-weight: 500 !important;
    }

    /* Force button text to be visible */
    ::ng-deep .mat-mdc-button .mdc-button__label,
    ::ng-deep .mat-mdc-raised-button .mdc-button__label {
      color: inherit !important;
      font-size: inherit !important;
      font-weight: inherit !important;
    }

    /* Prevent text overlapping */
    .match-name {
      margin-bottom: 8px !important;
      word-wrap: break-word !important;
      overflow-wrap: break-word !important;
    }

    .match-header {
      flex-direction: column !important;
      align-items: flex-start !important;
      gap: 8px !important;
    }

    @media (min-width: 768px) {
      .match-header {
        flex-direction: row !important;
        align-items: center !important;
        justify-content: space-between !important;
      }
    }

    .match-actions button .mat-button-wrapper {
      display: flex;
      align-items: center;
      justify-content: center;
      gap: 6px;
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
        gap: 12px;
      }

      .match-name {
        min-width: unset;
        width: 100%;
      }

      .detail-grid {
        grid-template-columns: 1fr;
        gap: 12px;
      }

      .detail-item {
        padding: 12px;
      }

      .match-actions {
        flex-direction: column;
        gap: 12px;
      }

      .match-actions button {
        width: 100%;
      }
    }
  `]
})
export class ScreeningResultsComponent implements OnInit {
  @Input() result: any = null;
  @Input() onCreateAlert: ((match: any) => void) | null = null;

  // Pagination properties
  currentPage = 0;
  pageSize = 10;
  pageSizeOptions = [5, 10, 25, 50];

  ngOnInit() {
    console.log('ScreeningResultsComponent - result received:', this.result);
    if (this.result) {
      console.log('Result has matches:', this.result.matches?.length || 0);
      console.log('Result data:', JSON.stringify(this.result, null, 2));
    }
  }

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



  viewAlert(alertId: string): void {
    // Navigate to specific alert details
    console.log('Navigate to alert:', alertId);
    // TODO: Implement navigation to alert details page
    // this.router.navigate(['/alerts', alertId]);
  }

  exportMatch(match: any): void {
    const matchData = {
      matchedName: match.matchedName || match.fullName || match.name,
      source: match.source,
      riskCategory: match.riskCategory || match.listType,
      matchScore: match.matchScore || match.similarityScore || 0,
      country: match.country,
      alternateNames: match.alternateNames,
      positionOrRole: match.positionOrRole,
      sanctionType: match.sanctionType,
      sanctionReason: match.sanctionReason,
      pepCategory: match.pepCategory,
      pepPosition: match.pepPosition,
      dateAdded: match.dateAdded,
      lastUpdated: match.lastUpdated,
      alertCreated: match.alertCreated,
      alertId: match.alertId,
      alertCreatedAt: match.alertCreatedAt,
      alertCreatedBy: match.alertCreatedBy,
      exportedAt: new Date().toISOString()
    };

    const dataStr = JSON.stringify(matchData, null, 2);
    const dataBlob = new Blob([dataStr], { type: 'application/json' });
    const url = URL.createObjectURL(dataBlob);
    const link = document.createElement('a');
    link.href = url;
    link.download = `match-details-${match.matchedName || 'unknown'}-${new Date().toISOString().split('T')[0]}.json`;
    link.click();
    URL.revokeObjectURL(url);
  }

  isRecentlyCreated(alertCreatedAt: Date | string): boolean {
    if (!alertCreatedAt) return false;

    const createdDate = new Date(alertCreatedAt);
    const now = new Date();
    const diffInMinutes = (now.getTime() - createdDate.getTime()) / (1000 * 60);

    // Consider "recently created" if within the last 5 minutes
    return diffInMinutes <= 5;
  }

  hasRecentAlerts(): boolean {
    const result = this.result;
    if (!result?.matches) return false;

    return result.matches.some((match: any) =>
      match.alertCreatedAt && this.isRecentlyCreated(match.alertCreatedAt)
    );
  }

  getRecentAlertsCount(): number {
    const result = this.result;
    if (!result?.matches) return 0;

    return result.matches.filter((match: any) =>
      match.alertCreatedAt && this.isRecentlyCreated(match.alertCreatedAt)
    ).length;
  }

  viewAlerts(): void {
    // Navigate to alerts dashboard or open alerts in a new tab
    window.open('/alerts', '_blank');
  }

  // Pagination methods
  onPageChange(event: PageEvent): void {
    this.currentPage = event.pageIndex;
    this.pageSize = event.pageSize;
    console.log('Page changed to:', this.currentPage, 'Size:', this.pageSize);
  }

  // Track by function for better performance
  trackByMatch(index: number, match: any): any {
    return match.matchId || match.id || index;
  }

  // Get display name for match with fallbacks
  getMatchDisplayName(match: any): string {
    const name = match.matchedName || match.fullName || match.name || match.entityName || match.listName;

    if (!name || name.trim() === '') {
      console.log('Empty match name found:', match);
      return `Match ${this.getPaginatedMatches().indexOf(match) + 1}`;
    }

    return name;
  }

  // Debug helper methods
  getMatchKeys(match: any): string[] {
    return Object.keys(match || {});
  }

  hasMatchData(match: any): boolean {
    if (!match) return false;
    const keys = Object.keys(match);
    return keys.length > 0 && keys.some(key => match[key] !== null && match[key] !== undefined && match[key] !== '');
  }

  getMatchDataAsString(match: any): string {
    try {
      return JSON.stringify(match, null, 2);
    } catch (e) {
      return 'Error serializing match data';
    }
  }

  getMatchSource(match: any): string {
    return match.source || match.dataSource || match.listSource || 'Unknown Source';
  }

  getMatchScore(match: any): number {
    return match.matchScore || match.similarityScore || match.score || 0;
  }

  getMatchField(match: any, fieldName: string): any {
    return match[fieldName] || match[fieldName.toLowerCase()] || match[fieldName.toUpperCase()] || null;
  }

  getPaginatedMatches(): any[] {
    if (!this.result?.matches) {
      console.log('No matches found in result:', this.result);
      return [];
    }

    console.log('Total matches available:', this.result.matches.length);
    console.log('Current page:', this.currentPage, 'Page size:', this.pageSize);
    console.log('All matches:', this.result.matches);

    const startIndex = this.currentPage * this.pageSize;
    const endIndex = startIndex + this.pageSize;
    const paginatedMatches = this.result.matches.slice(startIndex, endIndex);

    console.log('Paginated matches (showing):', paginatedMatches);
    console.log('Start index:', startIndex, 'End index:', endIndex);

    return paginatedMatches;
  }

  // Screening details methods
  getSearchCriteria(): string {
    if (this.result?.screeningDetails?.searchCriteria) {
      return this.result.screeningDetails.searchCriteria;
    }

    // Generate search criteria from available data
    const criteria = [];
    if (this.result?.fullName || this.result?.customerName) {
      criteria.push(`Name: ${this.result.fullName || this.result.customerName}`);
    }
    if (this.result?.nationality) {
      criteria.push(`Nationality: ${this.result.nationality}`);
    }
    if (this.result?.country) {
      criteria.push(`Country: ${this.result.country}`);
    }
    if (this.result?.dateOfBirth) {
      criteria.push(`DOB: ${this.result.dateOfBirth}`);
    }

    return criteria.length > 0 ? criteria.join(', ') : 'Full name and available identifiers';
  }

  getSourcesSearched(): string[] {
    if (this.result?.screeningDetails?.sourcesSearched) {
      return this.result.screeningDetails.sourcesSearched;
    }

    // Extract unique sources from matches
    if (this.result?.matches) {
      const sources = [...new Set(this.result.matches.map((match: any) => match.source).filter(Boolean))] as string[];
      return sources.length > 0 ? sources : ['OFAC', 'EU Sanctions', 'UN Sanctions', 'PEP Lists'];
    }

    return ['OFAC', 'EU Sanctions', 'UN Sanctions', 'PEP Lists'];
  }

  getTotalRecordsSearched(): number {
    if (this.result?.screeningDetails?.totalWatchlistEntries) {
      return this.result.screeningDetails.totalWatchlistEntries;
    }

    // Estimate based on sources and typical database sizes
    const sources = this.getSourcesSearched();
    return sources.length * 50000; // Rough estimate
  }

  getScreeningDuration(): string {
    if (this.result?.screeningDetails?.duration) {
      return this.result.screeningDetails.duration;
    }

    // Calculate or estimate duration
    const matchCount = this.result?.matches?.length || 0;
    const estimatedMs = Math.max(500, matchCount * 100); // Minimum 500ms

    if (estimatedMs < 1000) {
      return `${estimatedMs}ms`;
    } else {
      return `${(estimatedMs / 1000).toFixed(1)}s`;
    }
  }

  getRiskLevel(): string {
    if (this.result?.riskLevel) {
      return this.result.riskLevel;
    }

    // Calculate risk level based on matches
    const matchCount = this.result?.matches?.length || 0;
    const highRiskMatches = this.result?.matches?.filter((match: any) =>
      (match.matchScore || match.similarityScore || 0) > 0.8
    ).length || 0;

    if (matchCount === 0) return 'Low';
    if (highRiskMatches > 0) return 'High';
    if (matchCount > 3) return 'Medium';
    return 'Low';
  }
}
