import { Component } from '@angular/core';
import { CommonModule } from '@angular/common';
import { ReactiveFormsModule, FormBuilder, Validators } from '@angular/forms';
import { MatCardModule } from '@angular/material/card';
import { MatFormFieldModule } from '@angular/material/form-field';
import { MatInputModule } from '@angular/material/input';
import { MatButtonModule } from '@angular/material/button';

@Component({
  selector: 'app-login',
  standalone: true,
  imports: [CommonModule, ReactiveFormsModule, MatCardModule, MatFormFieldModule, MatInputModule, MatButtonModule],
  template: `
  <div class="center">
    <mat-card>
      <mat-card-title>Login</mat-card-title>
      <form [formGroup]="form" (ngSubmit)="submit()">
        <mat-form-field appearance="outline">
          <mat-label>Username</mat-label>
          <input matInput formControlName="username" />
        </mat-form-field>
        <mat-form-field appearance="outline">
          <mat-label>Password</mat-label>
          <input matInput type="password" formControlName="password" />
        </mat-form-field>
        <button mat-raised-button color="primary" type="submit">Login</button>
      </form>
    </mat-card>
  </div>
  `,
  styles: [`.center{display:flex;align-items:center;justify-content:center;height:100vh}`]
})
export class LoginComponent {
  form = new FormBuilder().group({ username: ['', Validators.required], password: ['', Validators.required] });
  submit(){
    // TODO: replace with real auth flow; for now stub a token
    localStorage.setItem('access_token', 'dev-token');
    window.location.href = '/';
  }
}


