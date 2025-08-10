import { Component, inject, signal } from '@angular/core';
import { CommonModule } from '@angular/common';
import { ReactiveFormsModule, FormBuilder, Validators } from '@angular/forms';
import { MatTableModule } from '@angular/material/table';
import { MatButtonModule } from '@angular/material/button';
import { MatIconModule } from '@angular/material/icon';
import { MatCardModule } from '@angular/material/card';
import { MatFormFieldModule } from '@angular/material/form-field';
import { MatInputModule } from '@angular/material/input';
import { MatSelectModule } from '@angular/material/select';
import { MatChipsModule } from '@angular/material/chips';
import { MatProgressBarModule } from '@angular/material/progress-bar';
import { MatSnackBarModule, MatSnackBar } from '@angular/material/snack-bar';
import { CustomersService } from '../../services/customers.service';
import { ScreeningService } from '../../services/screening.service';

@Component({
  selector: 'app-customers',
  standalone: true,
  imports: [CommonModule, ReactiveFormsModule, MatTableModule, MatButtonModule, MatIconModule, MatCardModule, MatFormFieldModule, MatInputModule, MatSelectModule, MatChipsModule, MatProgressBarModule, MatSnackBarModule],
  template: `
    <mat-card>
      <mat-card-header>
        <mat-card-title>
          <mat-icon>people</mat-icon>
          Customer Management
        </mat-card-title>
        <div class="header-actions">
          <button mat-raised-button color="primary" (click)="showAddForm = !showAddForm">
            <mat-icon>add</mat-icon>
            Add Customer
          </button>
          <button mat-raised-button color="accent" (click)="showUploadForm = !showUploadForm">
            <mat-icon>upload</mat-icon>
            Bulk Upload
          </button>
          <button mat-button (click)="downloadTemplate()">
            <mat-icon>download</mat-icon>
            Download Template
          </button>
        </div>
      </mat-card-header>

      <mat-card-content>
        <!-- Add Customer Form -->
        <mat-card *ngIf="showAddForm" class="add-form">
          <mat-card-title>Add New Customer</mat-card-title>
          <form [formGroup]="customerForm" (ngSubmit)="addCustomer()">
            <div class="form-grid">
              <mat-form-field appearance="outline">
                <mat-label>First Name</mat-label>
                <input matInput formControlName="firstName" required>
              </mat-form-field>
              <mat-form-field appearance="outline">
                <mat-label>Last Name</mat-label>
                <input matInput formControlName="lastName" required>
              </mat-form-field>
              <mat-form-field appearance="outline">
                <mat-label>Email</mat-label>
                <input matInput type="email" formControlName="email" required>
              </mat-form-field>
              <mat-form-field appearance="outline">
                <mat-label>Country</mat-label>
                <mat-select formControlName="country" required>
                  <mat-option *ngFor="let country of countries" [value]="country">{{ country }}</mat-option>
                </mat-select>
              </mat-form-field>
            </div>
            <div class="form-actions">
              <button mat-raised-button color="primary" type="submit" [disabled]="customerForm.invalid">Add Customer</button>
              <button mat-button type="button" (click)="showAddForm = false">Cancel</button>
            </div>
          </form>
        </mat-card>

        <!-- Bulk Upload Form -->
        <mat-card *ngIf="showUploadForm" class="upload-form">
          <mat-card-title>Bulk Customer Upload</mat-card-title>
          <mat-card-content>
            <div class="upload-instructions">
              <p><strong>Instructions:</strong></p>
              <ol>
                <li>Download the template file using the "Download Template" button</li>
                <li>Fill in your customer data in the template</li>
                <li>Upload the completed file (CSV, Excel formats supported)</li>
                <li>Review and confirm the upload</li>
              </ol>
            </div>

            <div class="upload-section">
              <input type="file" #fileInput (change)="onFileSelected($event)"
                     accept=".csv,.xlsx,.xls" style="display: none;">

              <div class="file-drop-zone"
                   (dragover)="onDragOver($event)"
                   (dragleave)="onDragLeave($event)"
                   (drop)="onFileDropped($event)"
                   [class.drag-over]="isDragOver">
                <mat-icon>cloud_upload</mat-icon>
                <p>Drag and drop your file here or</p>
                <button mat-raised-button (click)="fileInput.click()">Choose File</button>
                <p class="file-info">Supported formats: CSV, Excel (.xlsx, .xls)</p>
              </div>

              <div *ngIf="selectedFile" class="selected-file">
                <mat-icon>description</mat-icon>
                <span>{{ selectedFile.name }}</span>
                <span class="file-size">({{ formatFileSize(selectedFile.size) }})</span>
                <button mat-icon-button (click)="removeFile()">
                  <mat-icon>close</mat-icon>
                </button>
              </div>

              <div *ngIf="uploadProgress > 0" class="upload-progress">
                <mat-progress-bar mode="determinate" [value]="uploadProgress"></mat-progress-bar>
                <p>Uploading... {{ uploadProgress }}%</p>
              </div>

              <div *ngIf="uploadResult" class="upload-result">
                <div class="result-summary">
                  <mat-icon [color]="uploadResult.success ? 'primary' : 'warn'">
                    {{ uploadResult.success ? 'check_circle' : 'error' }}
                  </mat-icon>
                  <span>{{ uploadResult.message }}</span>
                </div>
                <div *ngIf="uploadResult.details" class="result-details">
                  <p>Total Records: {{ uploadResult.details.total }}</p>
                  <p>Successfully Imported: {{ uploadResult.details.success }}</p>
                  <p *ngIf="uploadResult.details.failed > 0">Failed: {{ uploadResult.details.failed }}</p>
                  <div *ngIf="uploadResult.details.errors?.length > 0" class="error-list">
                    <p><strong>Errors:</strong></p>
                    <ul>
                      <li *ngFor="let error of uploadResult.details.errors">{{ error }}</li>
                    </ul>
                  </div>
                </div>
              </div>
            </div>

            <div class="upload-actions">
              <button mat-raised-button color="primary"
                      (click)="uploadFile()"
                      [disabled]="!selectedFile || uploadProgress > 0">
                <mat-icon>upload</mat-icon>
                Upload Customers
              </button>
              <button mat-button (click)="cancelUpload()">Cancel</button>
            </div>
          </mat-card-content>
        </mat-card>

        <!-- Customers Table -->
        <table mat-table [dataSource]="customers()" class="mat-elevation-z2">
          <ng-container matColumnDef="name">
            <th mat-header-cell *matHeaderCellDef>Name</th>
            <td mat-cell *matCellDef="let row">{{ row.name || row.fullName }}</td>
          </ng-container>
          <ng-container matColumnDef="email">
            <th mat-header-cell *matHeaderCellDef>Email</th>
            <td mat-cell *matCellDef="let row">{{ row.emailAddress }}</td>
          </ng-container>
          <ng-container matColumnDef="country">
            <th mat-header-cell *matHeaderCellDef>Country</th>
            <td mat-cell *matCellDef="let row">{{ row.country }}</td>
          </ng-container>
          <ng-container matColumnDef="riskLevel">
            <th mat-header-cell *matHeaderCellDef>Risk Level</th>
            <td mat-cell *matCellDef="let row">
              <mat-chip [color]="getRiskColor(row.riskLevel)">{{ row.riskLevel }}</mat-chip>
            </td>
          </ng-container>
          <ng-container matColumnDef="actions">
            <th mat-header-cell *matHeaderCellDef>Actions</th>
            <td mat-cell *matCellDef="let row">
              <button mat-icon-button (click)="screenCustomer(row)">
                <mat-icon>search</mat-icon>
              </button>
              <button mat-icon-button color="warn" (click)="deleteCustomer(row.id)">
                <mat-icon>delete</mat-icon>
              </button>
            </td>
          </ng-container>
          <tr mat-header-row *matHeaderRowDef="displayedColumns"></tr>
          <tr mat-row *matRowDef="let row; columns: displayedColumns;"></tr>
        </table>
      </mat-card-content>
    </mat-card>
  `,
  styles: [`
    .add-form { margin-bottom: 24px; }
    .form-grid { display: grid; grid-template-columns: repeat(auto-fit, minmax(250px, 1fr)); gap: 16px; }
    .form-actions { margin-top: 16px; }
    .form-actions button { margin-right: 8px; }
    table { width: 100%; margin-top: 16px; }
    .mat-mdc-chip { font-size: 0.75rem; }
    mat-card-header { display: flex; justify-content: space-between; align-items: center; }
    .header-actions { display: flex; gap: 8px; }
    .upload-form { margin-bottom: 24px; }
    .upload-instructions { margin-bottom: 16px; background: #f5f5f5; padding: 16px; border-radius: 4px; }
    .file-drop-zone { border: 2px dashed #ccc; padding: 40px; text-align: center; border-radius: 8px; cursor: pointer; }
    .file-drop-zone.drag-over { border-color: #1976d2; background: #e3f2fd; }
    .selected-file { display: flex; align-items: center; gap: 8px; margin: 16px 0; }
    .file-size { color: #666; font-size: 0.875rem; }
    .upload-progress { margin: 16px 0; }
    .upload-result { margin: 16px 0; }
    .result-summary { display: flex; align-items: center; gap: 8px; margin-bottom: 8px; }
    .result-details { background: #f5f5f5; padding: 12px; border-radius: 4px; }
    .error-list { margin-top: 8px; }
    .upload-actions { margin-top: 16px; }
    .upload-actions button { margin-right: 8px; }
  `]
})
export class CustomersComponent {
  private customersService = inject(CustomersService);
  private screeningService = inject(ScreeningService);
  private fb = inject(FormBuilder);
  private snackBar = inject(MatSnackBar);

  customers = signal<any[]>([]);
  showAddForm = false;
  showUploadForm = false;

  // File upload properties
  selectedFile: File | null = null;
  isDragOver = false;
  uploadProgress = 0;
  uploadResult: any = null;

  displayedColumns = ['name', 'email', 'country', 'riskLevel', 'actions'];

  countries = ['India', 'United States', 'United Kingdom', 'Canada', 'Australia'];

  customerForm = this.fb.group({
    firstName: ['', Validators.required],
    lastName: ['', Validators.required],
    email: ['', [Validators.required, Validators.email]],
    country: ['', Validators.required]
  });

  constructor(){
    this.loadCustomers();
  }

  loadCustomers() {
    this.customersService.getAll().subscribe({
      next: (customers) => {
        this.customers.set(customers);
      },
      error: (error) => {
        console.error('Error loading customers:', error);
        this.snackBar.open('Failed to load customers. Please refresh the page.', 'Close', { duration: 5000 });
      }
    });
  }

  addCustomer() {
    if (this.customerForm.valid) {
      const formValue = this.customerForm.value;
      const customerData = {
        FirstName: formValue.firstName,
        LastName: formValue.lastName,
        Email: formValue.email,
        Country: formValue.country
      };

      this.customersService.create(customerData).subscribe({
        next: () => {
          this.loadCustomers();
          this.customerForm.reset();
          this.showAddForm = false;
          this.snackBar.open('Customer created successfully!', 'Close', { duration: 3000 });
        },
        error: (error) => {
          console.error('Error creating customer:', error);
          this.snackBar.open('Failed to create customer. Please try again.', 'Close', { duration: 5000 });
        }
      });
    }
  }

  screenCustomer(customer: any) {
    const screeningRequest = {
      fullName: customer.fullName || customer.name,
      country: customer.country,
      threshold: 75
    };

    this.screeningService.screenCustomer(screeningRequest).subscribe(result => {
      console.log('Screening result:', result);
      this.loadCustomers();
    });
  }

  deleteCustomer(id: string) {
    if (confirm('Are you sure?')) {
      this.customersService.delete(id).subscribe(() => {
        this.loadCustomers();
      });
    }
  }

  getRiskColor(risk: string): string {
    switch(risk?.toLowerCase()) {
      case 'high': return 'warn';
      case 'medium': return 'accent';
      default: return 'primary';
    }
  }

  // File upload methods
  downloadTemplate() {
    const csvContent = 'First Name,Last Name,Email,Country,Phone,Date of Birth,ID Number,ID Type,Address\n' +
                      'John,Doe,john.doe@email.com,United States,+1234567890,1990-01-15,123456789,Passport,"123 Main St"\n' +
                      'Jane,Smith,jane.smith@email.com,United Kingdom,+44123456789,1985-05-20,987654321,National ID,"456 Oak Ave"';

    const blob = new Blob([csvContent], { type: 'text/csv' });
    const url = window.URL.createObjectURL(blob);
    const link = document.createElement('a');
    link.href = url;
    link.download = 'customer_upload_template.csv';
    link.click();
    window.URL.revokeObjectURL(url);

    this.snackBar.open('Template downloaded successfully', 'Close', { duration: 3000 });
  }

  onFileSelected(event: any) {
    const file = event.target.files[0];
    if (file) {
      this.selectedFile = file;
      this.uploadResult = null;
    }
  }

  onDragOver(event: DragEvent) {
    event.preventDefault();
    this.isDragOver = true;
  }

  onDragLeave(event: DragEvent) {
    event.preventDefault();
    this.isDragOver = false;
  }

  onFileDropped(event: DragEvent) {
    event.preventDefault();
    this.isDragOver = false;

    const files = event.dataTransfer?.files;
    if (files && files.length > 0) {
      this.selectedFile = files[0];
      this.uploadResult = null;
    }
  }

  removeFile() {
    this.selectedFile = null;
    this.uploadResult = null;
  }

  formatFileSize(bytes: number): string {
    if (bytes === 0) return '0 Bytes';
    const k = 1024;
    const sizes = ['Bytes', 'KB', 'MB', 'GB'];
    const i = Math.floor(Math.log(bytes) / Math.log(k));
    return parseFloat((bytes / Math.pow(k, i)).toFixed(2)) + ' ' + sizes[i];
  }

  uploadFile() {
    if (!this.selectedFile) return;

    const formData = new FormData();
    formData.append('file', this.selectedFile);

    this.uploadProgress = 0;
    this.uploadResult = null;

    // Simulate upload progress
    const progressInterval = setInterval(() => {
      this.uploadProgress += 10;
      if (this.uploadProgress >= 90) {
        clearInterval(progressInterval);
      }
    }, 200);

    // Call the upload service
    this.customersService.bulkUpload(formData).subscribe({
      next: (response) => {
        clearInterval(progressInterval);
        this.uploadProgress = 100;

        setTimeout(() => {
          this.uploadResult = {
            success: true,
            message: 'Customers uploaded successfully!',
            details: {
              total: response.TotalRecords || 0,
              success: response.SuccessCount || 0,
              failed: response.FailedCount || 0,
              errors: response.Errors || []
            }
          };
          this.uploadProgress = 0;
          this.loadCustomers();

          this.snackBar.open(
            `Successfully uploaded ${response.SuccessCount} customers`,
            'Close',
            { duration: 5000 }
          );
        }, 500);
      },
      error: (error) => {
        clearInterval(progressInterval);
        this.uploadProgress = 0;
        this.uploadResult = {
          success: false,
          message: 'Upload failed: ' + (error.error?.message || 'Unknown error'),
          details: null
        };

        this.snackBar.open('Upload failed. Please try again.', 'Close', { duration: 5000 });
      }
    });
  }

  cancelUpload() {
    this.showUploadForm = false;
    this.selectedFile = null;
    this.uploadResult = null;
    this.uploadProgress = 0;
  }
}


