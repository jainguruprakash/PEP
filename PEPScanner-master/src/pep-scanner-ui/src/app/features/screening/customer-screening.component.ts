import { Component, inject } from '@angular/core';
import { CommonModule } from '@angular/common';
import { ReactiveFormsModule, FormBuilder, Validators } from '@angular/forms';
import { MatCardModule } from '@angular/material/card';
import { MatFormFieldModule } from '@angular/material/form-field';
import { MatInputModule } from '@angular/material/input';
import { MatButtonModule } from '@angular/material/button';
import { MatProgressSpinnerModule } from '@angular/material/progress-spinner';
import { ScreeningService } from '../../services/screening.service';
import { ScreeningResultsComponent } from './screening-results.component';

@Component({
  selector: 'app-customer-screening',
  standalone: true,
  imports: [CommonModule, ReactiveFormsModule, MatCardModule, MatFormFieldModule, MatInputModule, MatButtonModule, MatProgressSpinnerModule, ScreeningResultsComponent],
  template: `
  <mat-card>
    <mat-card-title>Customer Screening</mat-card-title>
    <form [formGroup]="form" (ngSubmit)="submit()">
      <div class="grid">
        <mat-form-field appearance="outline">
          <mat-label>Full Name</mat-label>
          <input matInput formControlName="fullName" required />
        </mat-form-field>
        <mat-form-field appearance="outline">
          <mat-label>Date of Birth</mat-label>
          <input matInput type="date" formControlName="dateOfBirth" />
        </mat-form-field>
        <mat-form-field appearance="outline">
          <mat-label>Nationality</mat-label>
          <input matInput formControlName="nationality" />
        </mat-form-field>
        <mat-form-field appearance="outline">
          <mat-label>Country</mat-label>
          <input matInput formControlName="country" />
        </mat-form-field>
        <mat-form-field appearance="outline">
          <mat-label>ID Number</mat-label>
          <input matInput formControlName="identificationNumber" />
        </mat-form-field>
        <mat-form-field appearance="outline">
          <mat-label>ID Type</mat-label>
          <input matInput formControlName="identificationType" />
        </mat-form-field>
      </div>
      <button mat-raised-button color="primary" type="submit" [disabled]="form.invalid || isLoading">
        <mat-spinner diameter="20" *ngIf="isLoading"></mat-spinner>
        <span *ngIf="!isLoading">Screen</span>
        <span *ngIf="isLoading">Screening...</span>
      </button>
    </form>

    <!-- Loading indicator -->
    <div *ngIf="isLoading" class="loading-section">
      <mat-spinner></mat-spinner>
      <p>Screening against {{ totalSources }} watchlist sources...</p>
    </div>

    <!-- Results -->
    <app-screening-results [result]="result" *ngIf="result && !isLoading"></app-screening-results>
  </mat-card>
  `,
  styles: [`
    .grid {
      display: grid;
      grid-template-columns: repeat(auto-fit, minmax(240px, 1fr));
      gap: 16px;
      margin-bottom: 16px;
    }

    .loading-section {
      text-align: center;
      padding: 32px;
      margin-top: 16px;
    }

    .loading-section p {
      margin-top: 16px;
      color: #666;
    }

    button[disabled] {
      opacity: 0.6;
    }
  `]
})
export class CustomerScreeningComponent {
  private fb = inject(FormBuilder);
  private screeningService = inject(ScreeningService);

  form = this.fb.group({
    fullName: ['', Validators.required],
    dateOfBirth: [''],
    nationality: ['Indian'],
    country: ['India'],
    identificationNumber: [''],
    identificationType: ['PAN']
  });

  result: any = null;
  isLoading = false;
  totalSources = 7; // Based on the sources we know exist

  submit() {
    if (this.form.invalid) return;

    this.isLoading = true;
    this.result = null;

    this.screeningService.screenCustomer(this.form.value).subscribe({
      next: (res) => {
        this.result = res;
        this.isLoading = false;
      },
      error: (error) => {
        console.error('Screening error:', error);
        this.isLoading = false;
        // You could show an error message here
      }
    });
  }
}


