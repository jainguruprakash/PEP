import { Component, inject, signal } from '@angular/core';
import { CommonModule } from '@angular/common';
import { ActivatedRoute } from '@angular/router';
import { ReactiveFormsModule, FormBuilder } from '@angular/forms';
import { MatCardModule } from '@angular/material/card';
import { MatFormFieldModule } from '@angular/material/form-field';
import { MatInputModule } from '@angular/material/input';
import { MatSelectModule } from '@angular/material/select';
import { MatButtonModule } from '@angular/material/button';
import { AlertsService } from '../../services/alerts.service';

@Component({
  selector: 'app-alert-detail',
  standalone: true,
  imports: [CommonModule, ReactiveFormsModule, MatCardModule, MatFormFieldModule, MatInputModule, MatSelectModule, MatButtonModule],
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
          <mat-select formControlName="outcome">
            <mat-option value="Confirmed">Confirmed</mat-option>
            <mat-option value="FalsePositive">FalsePositive</mat-option>
            <mat-option value="RequiresInvestigation">RequiresInvestigation</mat-option>
            <mat-option value="Escalated">Escalated</mat-option>
          </mat-select>
        </mat-form-field>
        <mat-form-field appearance="outline" class="col-span"><mat-label>Notes</mat-label>
          <textarea matInput rows="4" formControlName="outcomeNotes"></textarea>
        </mat-form-field>
        <button mat-raised-button color="primary">Save</button>
      </form>
    </mat-card>
  `,
  styles: [`.grid{display:grid;grid-template-columns:repeat(auto-fit,minmax(220px,1fr));gap:16px}.col-span{grid-column:1/-1}`]
})
export class AlertDetailComponent {
  private route = inject(ActivatedRoute);
  private alertsService = inject(AlertsService);
  private fb = inject(FormBuilder);

  alert = signal<any | null>(null);
  form = this.fb.group({ status: ['Open'], priority: ['Medium'], assignedTo: [''], outcome: [''], outcomeNotes: [''] });

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
}


