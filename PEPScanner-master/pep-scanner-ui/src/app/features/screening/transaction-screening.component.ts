import { Component, inject } from '@angular/core';
import { CommonModule } from '@angular/common';
import { ReactiveFormsModule, FormBuilder, Validators } from '@angular/forms';
import { MatCardModule } from '@angular/material/card';
import { MatFormFieldModule } from '@angular/material/form-field';
import { MatInputModule } from '@angular/material/input';
import { MatButtonModule } from '@angular/material/button';
import { ScreeningService } from '../../services/screening.service';

@Component({
  selector: 'app-transaction-screening',
  standalone: true,
  imports: [CommonModule, ReactiveFormsModule, MatCardModule, MatFormFieldModule, MatInputModule, MatButtonModule],
  template: `
  <mat-card>
    <mat-card-title>Transaction Screening</mat-card-title>
    <form [formGroup]="form" (ngSubmit)="submit()">
      <div class="grid">
        <mat-form-field appearance="outline">
          <mat-label>Transaction ID</mat-label>
          <input matInput formControlName="transactionId" required />
        </mat-form-field>
        <mat-form-field appearance="outline">
          <mat-label>Amount</mat-label>
          <input matInput type="number" step="0.01" formControlName="amount" required />
        </mat-form-field>
        <mat-form-field appearance="outline">
          <mat-label>Transaction Type</mat-label>
          <input matInput formControlName="transactionType" />
        </mat-form-field>
        <mat-form-field appearance="outline">
          <mat-label>Sender Name</mat-label>
          <input matInput formControlName="senderName" />
        </mat-form-field>
        <mat-form-field appearance="outline">
          <mat-label>Beneficiary Name</mat-label>
          <input matInput formControlName="beneficiaryName" />
        </mat-form-field>
        <mat-form-field appearance="outline">
          <mat-label>Source Country</mat-label>
          <input matInput formControlName="sourceCountry" />
        </mat-form-field>
        <mat-form-field appearance="outline">
          <mat-label>Destination Country</mat-label>
          <input matInput formControlName="destinationCountry" />
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
export class TransactionScreeningComponent {
  private fb = inject(FormBuilder);
  private screeningService = inject(ScreeningService);

  form = this.fb.group({
    transactionId: ['', Validators.required],
    amount: [0, Validators.required],
    transactionType: ['Transfer'],
    senderName: [''],
    beneficiaryName: [''],
    sourceCountry: ['India'],
    destinationCountry: ['India']
  });

  result: any = null;

  submit() {
    if (this.form.invalid) return;
    this.screeningService.screenTransaction(this.form.value).subscribe(res => this.result = res);
  }
}


