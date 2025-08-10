import { Component, inject, signal } from '@angular/core';
import { CommonModule } from '@angular/common';
import { ActivatedRoute, Router } from '@angular/router';
import { ReactiveFormsModule, FormBuilder } from '@angular/forms';
import { MatCardModule } from '@angular/material/card';
import { MatFormFieldModule } from '@angular/material/form-field';
import { MatInputModule } from '@angular/material/input';
import { MatSelectModule } from '@angular/material/select';
import { MatButtonModule } from '@angular/material/button';
import { MatDialogModule, MatDialog } from '@angular/material/dialog';
import { MatSnackBarModule, MatSnackBar } from '@angular/material/snack-bar';
import { AlertsService } from '../../services/alerts.service';
import { ReportsService, CreateSarRequest, ReportPriority } from '../../services/reports.service';
import { AuthService } from '../../services/auth.service';

@Component({
  selector: 'app-alert-detail',
  standalone: true,
  imports: [CommonModule, ReactiveFormsModule, MatCardModule, MatFormFieldModule, MatInputModule, MatSelectModule, MatButtonModule, MatDialogModule, MatSnackBarModule],
  template: `
    <mat-card *ngIf="alert() as a">
      <mat-card-title>Alert Detail</mat-card-title>
      <div>Type: {{ a.alertType }} | Risk: {{ a.riskLevel }} | Status: {{ a.status }}</div>
      <form [formGroup]="form" (ngSubmit)="save()" class="grid">
        <mat-form-field appearance="outline"><mat-label>Status</mat-label>
          <mat-select formControlName="status">
            <mat-option value="Open">Open</mat-option>
            <mat-option value="UnderReview">UnderReview</mat-option>
            <mat-option value="Escalated">Escalated</mat-option>
            <mat-option value="Closed">Closed</mat-option>
            <mat-option value="FalsePositive">FalsePositive</mat-option>
          </mat-select>
        </mat-form-field>
        <mat-form-field appearance="outline"><mat-label>Priority</mat-label>
          <mat-select formControlName="priority">
            <mat-option value="Low">Low</mat-option>
            <mat-option value="Medium">Medium</mat-option>
            <mat-option value="High">High</mat-option>
            <mat-option value="Critical">Critical</mat-option>
          </mat-select>
        </mat-form-field>
        <mat-form-field appearance="outline"><mat-label>Assigned To</mat-label>
          <input matInput formControlName="assignedTo" />
        </mat-form-field>
        <mat-form-field appearance="outline"><mat-label>Outcome</mat-label>
          <mat-select formControlName="outcome" (selectionChange)="onOutcomeChange($event.value)">
            <mat-option value="Confirmed">Confirmed</mat-option>
            <mat-option value="FalsePositive">FalsePositive</mat-option>
            <mat-option value="RequiresInvestigation">RequiresInvestigation</mat-option>
            <mat-option value="Escalated">Escalated</mat-option>
          </mat-select>
        </mat-form-field>
        <mat-form-field appearance="outline" class="col-span"><mat-label>Notes</mat-label>
          <textarea matInput rows="4" formControlName="outcomeNotes"></textarea>
        </mat-form-field>
        <div class="actions">
          <button mat-raised-button color="primary" type="submit">Save</button>
          <button mat-raised-button color="accent" type="button" (click)="approveAlert()" 
                  *ngIf="canApprove() && form.get('outcome')?.value === 'Confirmed'">
            Approve & Generate SAR
          </button>
        </div>
      </form>
    </mat-card>
  `,
  styles: [`.grid{display:grid;grid-template-columns:repeat(auto-fit,minmax(220px,1fr));gap:16px}.col-span{grid-column:1/-1}.actions{grid-column:1/-1;display:flex;gap:16px}`]
})
export class AlertDetailComponent {
  private route = inject(ActivatedRoute);
  private router = inject(Router);
  private alertsService = inject(AlertsService);
  private reportsService = inject(ReportsService);
  private authService = inject(AuthService);
  private dialog = inject(MatDialog);
  private snackBar = inject(MatSnackBar);
  private fb = inject(FormBuilder);

  alert = signal<any | null>(null);
  form = this.fb.group({ status: ['Open'], priority: ['Medium'], assignedTo: [''], outcome: [''], outcomeNotes: [''] });
  showApproveButton = signal(false);

  constructor(){
    const id = this.route.snapshot.paramMap.get('id')!;
    this.alertsService.getAll().subscribe(list => {
      const found = list.find(x => x.id === id);
      if (found) {
        this.alert.set(found);
        this.form.patchValue({ status: found.status, priority: found.priority, assignedTo: found.assignedTo, outcome: found.outcome, outcomeNotes: found.outcomeNotes });
      }
    });
  }

  save(){
    const current = this.alert();
    if (!current) return;
    const updated = { ...current, ...this.form.value };
    this.alertsService
      .update(current.id, updated)
      .subscribe(() => this.alert.set(updated));
  }

  onOutcomeChange(outcome: string) {
    this.showApproveButton.set(outcome === 'Confirmed' && this.canApprove());
  }

  canApprove(): boolean {
    const user = this.authService.currentUser$;
    // Check if user has senior role (Admin, Manager, or Compliance Officer)
    return this.authService.hasAnyRole(['Admin', 'Manager', 'Compliance Officer']);
  }

  approveAlert() {
    const current = this.alert();
    if (!current) return;

    // Update alert status to approved
    const updated = { ...current, ...this.form.value, status: 'Approved', approvedAt: new Date() };
    
    this.alertsService.update(current.id, updated).subscribe(() => {
      this.alert.set(updated);
      
      // Auto-generate SAR report
      this.generateSarReport(current);
    });
  }

  private generateSarReport(alert: any) {
    const sarRequest: CreateSarRequest = {
      customerId: alert.customerId,
      subjectName: alert.customerName || alert.subjectName || 'Unknown',
      subjectAddress: alert.customerAddress,
      subjectIdentification: alert.customerIdentification,
      subjectDateOfBirth: alert.customerDateOfBirth,
      suspiciousActivity: this.mapAlertTypeToSuspiciousActivity(alert.alertType),
      activityDescription: this.generateActivityDescription(alert),
      transactionAmount: alert.transactionAmount,
      transactionCurrency: alert.transactionCurrency || 'INR',
      transactionDate: alert.transactionDate,
      transactionLocation: alert.transactionLocation,
      priority: this.mapPriorityToReportPriority(alert.priority),
      incidentDate: alert.createdAt,
      discoveryDate: new Date(),
      regulatoryReferences: this.generateRegulatoryReferences(alert),
      internalNotes: `Auto-generated from Alert ID: ${alert.id}\n\nAlert Details:\n- Type: ${alert.alertType}\n- Risk Level: ${alert.riskLevel}\n- Match Score: ${alert.matchScore}%\n\nApproval Notes: ${this.form.get('outcomeNotes')?.value || 'No additional notes'}`
    };

    this.reportsService.createSar(sarRequest).subscribe({
      next: (sarReport) => {
        this.snackBar.open(
          `SAR Report ${sarReport.reportNumber} generated successfully`, 
          'View Report', 
          { duration: 5000 }
        ).onAction().subscribe(() => {
          this.router.navigate(['/reports', sarReport.id]);
        });
      },
      error: (error) => {
        console.error('Error generating SAR report:', error);
        this.snackBar.open('Error generating SAR report', 'Close', { duration: 3000 });
      }
    });
  }

  private mapAlertTypeToSuspiciousActivity(alertType: string): string {
    const mapping: { [key: string]: string } = {
      'PEP_MATCH': 'Politically Exposed Person (PEP) Match',
      'SANCTIONS_MATCH': 'Sanctions List Match',
      'ADVERSE_MEDIA': 'Adverse Media Coverage',
      'HIGH_RISK_TRANSACTION': 'High Risk Transaction Pattern',
      'UNUSUAL_ACTIVITY': 'Unusual Account Activity',
      'AML_VIOLATION': 'Anti-Money Laundering Violation',
      'KYC_DISCREPANCY': 'Know Your Customer Discrepancy'
    };
    return mapping[alertType] || 'Suspicious Activity Detected';
  }

  private generateActivityDescription(alert: any): string {
    let description = `Alert generated due to ${alert.alertType.toLowerCase().replace('_', ' ')} detection.\n\n`;
    
    if (alert.matchDetails) {
      description += `Match Details:\n${alert.matchDetails}\n\n`;
    }
    
    if (alert.riskFactors && alert.riskFactors.length > 0) {
      description += `Risk Factors:\n${alert.riskFactors.map((factor: string) => `- ${factor}`).join('\n')}\n\n`;
    }
    
    description += `Risk Assessment:\n- Risk Level: ${alert.riskLevel}\n- Match Score: ${alert.matchScore}%\n- Confidence Level: ${alert.confidenceLevel || 'Medium'}`;
    
    return description;
  }

  private mapPriorityToReportPriority(priority: string): ReportPriority {
    switch (priority?.toLowerCase()) {
      case 'low': return ReportPriority.Low;
      case 'medium': return ReportPriority.Medium;
      case 'high': return ReportPriority.High;
      case 'critical': return ReportPriority.Critical;
      default: return ReportPriority.Medium;
    }
  }

  private generateRegulatoryReferences(alert: any): string {
    const references = [];
    
    // Add relevant regulatory references based on alert type
    if (alert.alertType === 'PEP_MATCH') {
      references.push('RBI Master Direction on KYC - Section 51');
      references.push('PMLA Rules 2005 - Rule 9');
    }
    
    if (alert.alertType === 'SANCTIONS_MATCH') {
      references.push('UNSC Sanctions List');
      references.push('OFAC SDN List');
      references.push('EU Consolidated List');
    }
    
    if (alert.alertType === 'HIGH_RISK_TRANSACTION') {
      references.push('RBI Master Direction on AML/CFT - Section 7');
      references.push('PMLA Rules 2005 - Rule 3');
    }
    
    return references.join('; ');
  }
}


