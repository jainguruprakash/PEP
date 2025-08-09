import { Component, inject, signal } from '@angular/core';
import { CommonModule } from '@angular/common';
import { ReactiveFormsModule, FormBuilder, Validators } from '@angular/forms';
import { MatTableModule } from '@angular/material/table';
import { MatButtonModule } from '@angular/material/button';
import { MatFormFieldModule } from '@angular/material/form-field';
import { MatInputModule } from '@angular/material/input';
import { OrganizationsService } from '../../services/organizations.service';

@Component({
  selector: 'app-organizations',
  standalone: true,
  imports: [CommonModule, ReactiveFormsModule, MatTableModule, MatButtonModule, MatFormFieldModule, MatInputModule],
  template: `
    <h2>Bank Onboarding</h2>
    <form [formGroup]="form" (ngSubmit)="create()" class="grid">
      <mat-form-field appearance="outline"><mat-label>Name</mat-label><input matInput formControlName="name" required /></mat-form-field>
      <mat-form-field appearance="outline"><mat-label>Code</mat-label><input matInput formControlName="code" /></mat-form-field>
      <mat-form-field appearance="outline"><mat-label>Type</mat-label><input matInput formControlName="type" value="Bank" /></mat-form-field>
      <mat-form-field appearance="outline"><mat-label>Country</mat-label><input matInput formControlName="country" value="India" /></mat-form-field>
      <mat-form-field appearance="outline"><mat-label>Contact Person</mat-label><input matInput formControlName="contactPerson" /></mat-form-field>
      <mat-form-field appearance="outline"><mat-label>Contact Email</mat-label><input matInput formControlName="contactEmail" /></mat-form-field>
      <mat-form-field appearance="outline"><mat-label>Contact Phone</mat-label><input matInput formControlName="contactPhone" /></mat-form-field>
      <mat-form-field appearance="outline" class="col-span"><mat-label>Description</mat-label><textarea matInput rows="3" formControlName="description"></textarea></mat-form-field>
      <button mat-raised-button color="primary" [disabled]="form.invalid">Create Organization</button>
    </form>

    <h3>Existing Organizations</h3>
    <table mat-table [dataSource]="orgs()" class="mat-elevation-z2" *ngIf="orgs() as data">
      <ng-container matColumnDef="name"><th mat-header-cell *matHeaderCellDef>Name</th><td mat-cell *matCellDef="let r">{{ r.name }}</td></ng-container>
      <ng-container matColumnDef="code"><th mat-header-cell *matHeaderCellDef>Code</th><td mat-cell *matCellDef="let r">{{ r.code }}</td></ng-container>
      <ng-container matColumnDef="type"><th mat-header-cell *matHeaderCellDef>Type</th><td mat-cell *matCellDef="let r">{{ r.type }}</td></ng-container>
      <tr mat-header-row *matHeaderRowDef="cols"></tr>
      <tr mat-row *matRowDef="let row; columns: cols;"></tr>
    </table>
  `,
  styles: [`.grid{display:grid;grid-template-columns:repeat(auto-fit,minmax(240px,1fr));gap:16px}.col-span{grid-column:1/-1}`]
})
export class OrganizationsComponent {
  private svc = inject(OrganizationsService);
  private fb = inject(FormBuilder);
  orgs = signal<any[]>([]);

  cols = ['name', 'code', 'type'];

  form = this.fb.group({
    name: ['', Validators.required],
    code: [''],
    description: [''],
    type: ['Bank'],
    industry: ['Financial Services'],
    country: ['India'],
    contactPerson: [''],
    contactEmail: [''],
    contactPhone: ['']
  });

  constructor(){
    this.refresh();
  }

  refresh(){ this.svc.list().subscribe(x => this.orgs.set(x)); }

  create(){
    if (this.form.invalid) return;
    this.svc.create(this.form.value).subscribe(() => {
      this.form.reset({ type: 'Bank', industry: 'Financial Services', country: 'India' });
      this.refresh();
    });
  }
}


