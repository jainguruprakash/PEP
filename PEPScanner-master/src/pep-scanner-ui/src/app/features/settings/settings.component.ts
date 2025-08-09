import { Component } from '@angular/core';
import { CommonModule } from '@angular/common';
import { ReactiveFormsModule, FormControl } from '@angular/forms';
import { MatFormFieldModule } from '@angular/material/form-field';
import { MatInputModule } from '@angular/material/input';
import { MatButtonModule } from '@angular/material/button';

@Component({
  selector: 'app-settings',
  standalone: true,
  imports: [CommonModule, ReactiveFormsModule, MatFormFieldModule, MatInputModule, MatButtonModule],
  template: `
    <h2>Settings</h2>
    <mat-form-field appearance="outline">
      <mat-label>API Base URL</mat-label>
      <input matInput [formControl]="api" />
    </mat-form-field>
    <button mat-raised-button color="primary" (click)="save()">Save</button>
  `
})
export class SettingsComponent {
  api = new FormControl('/api');

  save(){
    localStorage.setItem('api_base_url', this.api.value || '/api');
  }
}


