import { Component, inject } from '@angular/core';
import { CommonModule } from '@angular/common';
import { MatTableModule } from '@angular/material/table';
import { MatButtonModule } from '@angular/material/button';
import { CustomersService } from '../../services/customers.service';

@Component({
  selector: 'app-customers',
  standalone: true,
  imports: [CommonModule, MatTableModule, MatButtonModule],
  template: `
    <h2>Customers</h2>
    <table mat-table [dataSource]="customers" class="mat-elevation-z2" *ngIf="customers">
      <ng-container matColumnDef="name">
        <th mat-header-cell *matHeaderCellDef>Name</th>
        <td mat-cell *matCellDef="let row">{{ row.name || row.fullName }}</td>
      </ng-container>
      <ng-container matColumnDef="riskLevel">
        <th mat-header-cell *matHeaderCellDef>Risk</th>
        <td mat-cell *matCellDef="let row">{{ row.riskLevel }}</td>
      </ng-container>
      <tr mat-header-row *matHeaderRowDef="displayedColumns"></tr>
      <tr mat-row *matRowDef="let row; columns: displayedColumns;"></tr>
    </table>
  `
})
export class CustomersComponent {
  private customersService = inject(CustomersService);
  customers: any[] = [];
  displayedColumns = ['name', 'riskLevel'];

  constructor(){
    this.customersService.getAll().subscribe(c => this.customers = c);
  }
}


