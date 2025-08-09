import { Component, inject } from '@angular/core';
import { CommonModule } from '@angular/common';
import { ReactiveFormsModule, FormBuilder } from '@angular/forms';
import { MatFormFieldModule } from '@angular/material/form-field';
import { MatInputModule } from '@angular/material/input';
import { MatButtonModule } from '@angular/material/button';
import { MatTableModule } from '@angular/material/table';
import { ScreeningService } from '../../services/screening.service';

@Component({
  selector: 'app-search',
  standalone: true,
  imports: [CommonModule, ReactiveFormsModule, MatFormFieldModule, MatInputModule, MatButtonModule, MatTableModule],
  template: `
  <form [formGroup]="form" (ngSubmit)="submit()" class="row">
    <mat-form-field appearance="outline">
      <mat-label>Name</mat-label>
      <input matInput formControlName="name" />
    </mat-form-field>
    <mat-form-field appearance="outline">
      <mat-label>Country</mat-label>
      <input matInput formControlName="country" />
    </mat-form-field>
    <mat-form-field appearance="outline">
      <mat-label>Threshold</mat-label>
      <input matInput type="number" step="0.01" formControlName="threshold" />
    </mat-form-field>
    <button mat-raised-button color="primary">Search</button>
  </form>

  <table mat-table [dataSource]="results" class="mat-elevation-z2" *ngIf="results">
    <ng-container matColumnDef="name">
      <th mat-header-cell *matHeaderCellDef>Name</th>
      <td mat-cell *matCellDef="let row">{{ row.name }}</td>
    </ng-container>
    <ng-container matColumnDef="risk">
      <th mat-header-cell *matHeaderCellDef>Risk</th>
      <td mat-cell *matCellDef="let row">{{ row.riskLevel || row.similarityScore }}</td>
    </ng-container>
    <tr mat-header-row *matHeaderRowDef="displayedColumns"></tr>
    <tr mat-row *matRowDef="let row; columns: displayedColumns;"></tr>
  </table>
  `,
  styles: [`.row{display:grid;grid-template-columns:repeat(auto-fit,minmax(220px,1fr));gap:16px;margin-bottom:16px}`]
})
export class SearchComponent {
  private fb = inject(FormBuilder);
  private screeningService = inject(ScreeningService);

  form = this.fb.group({ name: [''], country: ['India'], threshold: [0.7], maxResults: [50] });
  results: any[] | null = null;
  displayedColumns = ['name', 'risk'];

  submit() {
    this.screeningService.search(this.form.value).subscribe(res => {
      this.results = Array.isArray(res) ? res : (res?.matches ?? []);
    });
  }
}


