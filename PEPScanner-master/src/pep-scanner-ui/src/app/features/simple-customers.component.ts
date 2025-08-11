import { Component, inject, OnInit } from '@angular/core';
import { CommonModule } from '@angular/common';
import { MatCardModule } from '@angular/material/card';
import { MatTableModule } from '@angular/material/table';
import { MatButtonModule } from '@angular/material/button';
import { MatIconModule } from '@angular/material/icon';
import { HttpClient } from '@angular/common/http';
import { environment } from '../../environments/environment';

@Component({
  selector: 'app-simple-customers',
  standalone: true,
  imports: [CommonModule, MatCardModule, MatTableModule, MatButtonModule, MatIconModule],
  template: `
    <mat-card>
      <mat-card-header>
        <mat-card-title>
          <mat-icon>people</mat-icon>
          Customer Management
        </mat-card-title>
      </mat-card-header>
      <mat-card-content>
        <div *ngIf="loading">Loading customers...</div>
        <div *ngIf="error" style="color: red;">Error: {{ error }}</div>
        
        <table mat-table [dataSource]="customers" *ngIf="!loading && !error">
          <ng-container matColumnDef="name">
            <th mat-header-cell *matHeaderCellDef>Name</th>
            <td mat-cell *matCellDef="let customer">{{ customer.fullName || customer.name }}</td>
          </ng-container>
          
          <ng-container matColumnDef="email">
            <th mat-header-cell *matHeaderCellDef>Email</th>
            <td mat-cell *matCellDef="let customer">{{ customer.emailAddress || customer.email }}</td>
          </ng-container>
          
          <ng-container matColumnDef="country">
            <th mat-header-cell *matHeaderCellDef>Country</th>
            <td mat-cell *matCellDef="let customer">{{ customer.country }}</td>
          </ng-container>
          
          <ng-container matColumnDef="riskLevel">
            <th mat-header-cell *matHeaderCellDef>Risk Level</th>
            <td mat-cell *matCellDef="let customer">{{ customer.riskLevel }}</td>
          </ng-container>
          
          <tr mat-header-row *matHeaderRowDef="displayedColumns"></tr>
          <tr mat-row *matRowDef="let row; columns: displayedColumns;"></tr>
        </table>
        
        <div *ngIf="!loading && !error && customers.length === 0">
          No customers found. <button mat-button (click)="createSampleCustomer()">Create Sample Customer</button>
        </div>
      </mat-card-content>
    </mat-card>
  `,
  styles: [`
    table { width: 100%; margin-top: 16px; }
  `]
})
export class SimpleCustomersComponent implements OnInit {
  private http = inject(HttpClient);
  
  customers: any[] = [];
  loading = true;
  error = '';
  displayedColumns = ['name', 'email', 'country', 'riskLevel'];

  ngOnInit() {
    this.loadCustomers();
  }

  loadCustomers() {
    this.loading = true;
    this.error = '';
    
    this.http.get<any[]>(`${environment.apiBaseUrl}/customers`).subscribe({
      next: (customers) => {
        this.customers = customers || [];
        this.loading = false;
      },
      error: (error) => {
        this.error = error.message || 'Failed to load customers';
        this.loading = false;
        console.error('Error loading customers:', error);
      }
    });
  }

  createSampleCustomer() {
    const sampleCustomer = {
      FirstName: 'John',
      LastName: 'Doe',
      Email: 'john.doe@example.com',
      Country: 'United States'
    };

    this.http.post(`${environment.apiBaseUrl}/customers`, sampleCustomer).subscribe({
      next: () => {
        this.loadCustomers();
      },
      error: (error) => {
        console.error('Error creating customer:', error);
      }
    });
  }
}