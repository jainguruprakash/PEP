import { Component, inject } from '@angular/core';
import { CommonModule } from '@angular/common';
import { ReactiveFormsModule, FormBuilder, Validators } from '@angular/forms';
import { MatCardModule } from '@angular/material/card';
import { MatFormFieldModule } from '@angular/material/form-field';
import { MatInputModule } from '@angular/material/input';
import { MatButtonModule } from '@angular/material/button';
import { ScreeningService } from '../../services/screening.service';

@Component({
  selector: 'app-customer-screening',
  standalone: true,
  imports: [CommonModule, ReactiveFormsModule, MatCardModule, MatFormFieldModule, MatInputModule, MatButtonModule],
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
      <button mat-raised-button color="primary" type="submit" [disabled]="form.invalid">Screen</button>
    </form>
    <div *ngIf="result" class="result">
      <h3>Result</h3>
      <pre>{{ result | json }}</pre>
    </div>
  </mat-card>
  `,
  styles: [`
    .grid { display: grid; grid-template-columns: repeat(auto-fit, minmax(240px, 1fr)); gap: 16px; }
    .result { margin-top: 16px; }
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

  submit() {
    if (this.form.invalid) return;
    this.screeningService.screenCustomer(this.form.value).subscribe(res => this.result = res);
  }
}


