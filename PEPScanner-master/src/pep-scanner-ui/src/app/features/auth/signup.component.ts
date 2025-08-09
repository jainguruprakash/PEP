import { Component, inject } from '@angular/core';
import { CommonModule } from '@angular/common';
import { ReactiveFormsModule, FormBuilder, Validators } from '@angular/forms';
import { MatCardModule } from '@angular/material/card';
import { MatFormFieldModule } from '@angular/material/form-field';
import { MatInputModule } from '@angular/material/input';
import { MatButtonModule } from '@angular/material/button';
import { OrganizationsService } from '../../services/organizations.service';
import { Router } from '@angular/router';

@Component({
  selector: 'app-signup',
  standalone: true,
  imports: [CommonModule, ReactiveFormsModule, MatCardModule, MatFormFieldModule, MatInputModule, MatButtonModule],
  template: `
  <div class="center">
    <mat-card>
      <mat-card-title>Sign up your Bank</mat-card-title>
      <form [formGroup]="form" (ngSubmit)="submit()" class="grid">
        <mat-form-field appearance="outline"><mat-label>Bank Name</mat-label><input matInput formControlName="name" required /></mat-form-field>
        <mat-form-field appearance="outline"><mat-label>Code</mat-label><input matInput formControlName="code" /></mat-form-field>
        <mat-form-field appearance="outline"><mat-label>Country</mat-label><input matInput formControlName="country" /></mat-form-field>
        <mat-form-field appearance="outline"><mat-label>Contact Person</mat-label><input matInput formControlName="contactPerson" /></mat-form-field>
        <mat-form-field appearance="outline"><mat-label>Contact Email</mat-label><input matInput type="email" formControlName="contactEmail" /></mat-form-field>
        <mat-form-field appearance="outline"><mat-label>Contact Phone</mat-label><input matInput formControlName="contactPhone" /></mat-form-field>
        <mat-form-field appearance="outline" class="col-span"><mat-label>Description</mat-label><textarea matInput rows="3" formControlName="description"></textarea></mat-form-field>
        <button mat-raised-button color="primary" type="submit" [disabled]="form.invalid">Sign Up</button>
      </form>
    </mat-card>
  </div>
  `,
  styles: [`.center{display:flex;align-items:center;justify-content:center;padding:24px}.grid{display:grid;grid-template-columns:repeat(auto-fit,minmax(240px,1fr));gap:16px;width:min(900px,90vw)}.col-span{grid-column:1/-1}`]
})
export class SignupComponent {
  private fb = inject(FormBuilder);
  private orgs = inject(OrganizationsService);
  private router = inject(Router);

  form = this.fb.group({
    name: ['', Validators.required],
    code: [''],
    description: [''],
    type: ['Bank'],
    industry: ['Financial Services'],
    country: ['India'],
    contactPerson: [''],
    contactEmail: ['', Validators.email],
    contactPhone: ['']
  });

  submit(){
    if (this.form.invalid) return;
    this.orgs.create(this.form.value).subscribe(() => {
      // Dev: mint a placeholder token to pass API auth until real auth exists
      localStorage.setItem('access_token', 'bank-onboarding-dev-token');
      this.router.navigateByUrl('/organizations');
    });
  }
}


