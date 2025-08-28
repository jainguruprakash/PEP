import { Component, OnInit } from '@angular/core';
import { CommonModule } from '@angular/common';
import { ReactiveFormsModule, FormBuilder, FormGroup, Validators, FormArray } from '@angular/forms';
import { MatCardModule } from '@angular/material/card';
import { MatFormFieldModule } from '@angular/material/form-field';
import { MatInputModule } from '@angular/material/input';
import { MatButtonModule } from '@angular/material/button';
import { MatSelectModule } from '@angular/material/select';
import { MatCheckboxModule } from '@angular/material/checkbox';
import { MatSlideToggleModule } from '@angular/material/slide-toggle';
import { MatIconModule } from '@angular/material/icon';
import { MatTableModule } from '@angular/material/table';
import { MatDialogModule, MatDialog } from '@angular/material/dialog';
import { MatSnackBarModule } from '@angular/material/snack-bar';
import { ScreeningConfigService, ScreeningFieldConfig, ScreeningOption } from '../../services/screening-config.service';
import { ToastService } from '../../services/toast.service';

@Component({
  selector: 'app-screening-config-manager',
  standalone: true,
  imports: [
    CommonModule,
    ReactiveFormsModule,
    MatCardModule,
    MatFormFieldModule,
    MatInputModule,
    MatButtonModule,
    MatSelectModule,
    MatCheckboxModule,
    MatSlideToggleModule,
    MatIconModule,
    MatTableModule,
    MatDialogModule,
    MatSnackBarModule
  ],
  template: `
    <mat-card class="config-manager-card">
      <mat-card-header>
        <mat-card-title>
          <mat-icon>settings</mat-icon>
          Screening Configuration Manager
        </mat-card-title>
        <mat-card-subtitle>
          Manage screening options and data sources
        </mat-card-subtitle>
      </mat-card-header>

      <mat-card-content>
        <!-- Current Configuration Display -->
        <div class="current-config">
          <h3>Current Configuration</h3>
          <div class="config-table">
            <table mat-table [dataSource]="screeningConfig" class="config-table">
              <ng-container matColumnDef="fieldName">
                <th mat-header-cell *matHeaderCellDef>Field Name</th>
                <td mat-cell *matCellDef="let config">{{ config.fieldName }}</td>
              </ng-container>

              <ng-container matColumnDef="label">
                <th mat-header-cell *matHeaderCellDef>Label</th>
                <td mat-cell *matCellDef="let config">{{ config.label }}</td>
              </ng-container>

              <ng-container matColumnDef="type">
                <th mat-header-cell *matHeaderCellDef>Type</th>
                <td mat-cell *matCellDef="let config">{{ config.type }}</td>
              </ng-container>

              <ng-container matColumnDef="category">
                <th mat-header-cell *matHeaderCellDef>Category</th>
                <td mat-cell *matCellDef="let config">{{ config.category }}</td>
              </ng-container>

              <ng-container matColumnDef="payloadKey">
                <th mat-header-cell *matHeaderCellDef>Payload Key</th>
                <td mat-cell *matCellDef="let config">{{ config.payloadKey }}</td>
              </ng-container>

              <ng-container matColumnDef="actions">
                <th mat-header-cell *matHeaderCellDef>Actions</th>
                <td mat-cell *matCellDef="let config">
                  <button mat-icon-button (click)="editConfig(config)">
                    <mat-icon>edit</mat-icon>
                  </button>
                  <button mat-icon-button color="warn" (click)="deleteConfig(config.fieldName)">
                    <mat-icon>delete</mat-icon>
                  </button>
                </td>
              </ng-container>

              <tr mat-header-row *matHeaderRowDef="displayedColumns"></tr>
              <tr mat-row *matRowDef="let row; columns: displayedColumns;"></tr>
            </table>
          </div>
        </div>

        <!-- Add New Configuration -->
        <div class="add-config">
          <h3>Add New Screening Option</h3>
          <form [formGroup]="configForm" (ngSubmit)="addNewConfig()">
            <div class="form-row">
              <mat-form-field>
                <mat-label>Field Name</mat-label>
                <input matInput formControlName="fieldName" placeholder="e.g., newSource">
              </mat-form-field>

              <mat-form-field>
                <mat-label>Label</mat-label>
                <input matInput formControlName="label" placeholder="e.g., New Data Source">
              </mat-form-field>

              <mat-form-field>
                <mat-label>Type</mat-label>
                <mat-select formControlName="type">
                  <mat-option value="checkbox-group">Checkbox Group</mat-option>
                  <mat-option value="select">Select Dropdown</mat-option>
                  <mat-option value="toggle">Toggle Switch</mat-option>
                  <mat-option value="slider">Slider</mat-option>
                  <mat-option value="radio-group">Radio Group</mat-option>
                  <mat-option value="input">Text Input</mat-option>
                </mat-select>
              </mat-form-field>
            </div>

            <div class="form-row">
              <mat-form-field>
                <mat-label>Payload Key</mat-label>
                <input matInput formControlName="payloadKey" placeholder="e.g., newSourceList">
              </mat-form-field>

              <mat-form-field>
                <mat-label>Category</mat-label>
                <mat-select formControlName="category">
                  <mat-option value="sources">Data Sources</mat-option>
                  <mat-option value="matching">Matching Options</mat-option>
                  <mat-option value="scope">Search Scope</mat-option>
                  <mat-option value="risk">Risk Assessment</mat-option>
                  <mat-option value="filters">Filters</mat-option>
                  <mat-option value="advanced">Advanced Options</mat-option>
                </mat-select>
              </mat-form-field>

              <mat-form-field>
                <mat-label>Order</mat-label>
                <input matInput type="number" formControlName="order" placeholder="1">
              </mat-form-field>
            </div>

            <div class="form-row">
              <mat-form-field>
                <mat-label>Description</mat-label>
                <textarea matInput formControlName="description" placeholder="Optional description"></textarea>
              </mat-form-field>

              <mat-form-field>
                <mat-label>Default Value</mat-label>
                <input matInput formControlName="defaultValue" placeholder="Optional default value">
              </mat-form-field>
            </div>

            <div class="form-row">
              <mat-slide-toggle formControlName="required">Required Field</mat-slide-toggle>
              <mat-slide-toggle formControlName="visible">Visible</mat-slide-toggle>
            </div>

            <!-- Options for checkbox-group, select, radio-group -->
            <div *ngIf="needsOptions()" class="options-section">
              <h4>Options</h4>
              <div formArrayName="options">
                <div *ngFor="let option of getOptionsArray().controls; let i = index" [formGroupName]="i" class="option-row">
                  <mat-form-field>
                    <mat-label>Value</mat-label>
                    <input matInput formControlName="value">
                  </mat-form-field>
                  <mat-form-field>
                    <mat-label>Label</mat-label>
                    <input matInput formControlName="label">
                  </mat-form-field>
                  <mat-form-field>
                    <mat-label>Description</mat-label>
                    <input matInput formControlName="description">
                  </mat-form-field>
                  <mat-slide-toggle formControlName="selected">Default Selected</mat-slide-toggle>
                  <button mat-icon-button type="button" (click)="removeOption(i)">
                    <mat-icon>remove</mat-icon>
                  </button>
                </div>
              </div>
              <button mat-button type="button" (click)="addOption()">
                <mat-icon>add</mat-icon>
                Add Option
              </button>
            </div>

            <div class="form-actions">
              <button mat-raised-button color="primary" type="submit" [disabled]="!configForm.valid">
                Add Configuration
              </button>
              <button mat-button type="button" (click)="resetForm()">
                Reset
              </button>
            </div>
          </form>
        </div>

        <!-- Export/Import Configuration -->
        <div class="config-actions">
          <h3>Configuration Management</h3>
          <div class="action-buttons">
            <button mat-raised-button (click)="exportConfig()">
              <mat-icon>download</mat-icon>
              Export Configuration
            </button>
            <button mat-raised-button (click)="importConfig()">
              <mat-icon>upload</mat-icon>
              Import Configuration
            </button>
            <button mat-raised-button color="warn" (click)="resetToDefaults()">
              <mat-icon>restore</mat-icon>
              Reset to Defaults
            </button>
          </div>
        </div>
      </mat-card-content>
    </mat-card>
  `,
  styles: [`
    .config-manager-card {
      margin: 20px;
      max-width: 1200px;
    }

    .config-table {
      width: 100%;
      margin: 20px 0;
    }

    .form-row {
      display: flex;
      gap: 16px;
      margin-bottom: 16px;
      flex-wrap: wrap;
    }

    .form-row mat-form-field {
      flex: 1;
      min-width: 200px;
    }

    .options-section {
      border: 1px solid #ddd;
      padding: 16px;
      border-radius: 4px;
      margin: 16px 0;
    }

    .option-row {
      display: flex;
      gap: 12px;
      align-items: center;
      margin-bottom: 12px;
    }

    .option-row mat-form-field {
      flex: 1;
    }

    .form-actions {
      display: flex;
      gap: 12px;
      margin-top: 20px;
    }

    .action-buttons {
      display: flex;
      gap: 12px;
      flex-wrap: wrap;
    }

    .current-config {
      margin-bottom: 40px;
    }

    .add-config {
      margin-bottom: 40px;
    }
  `]
})
export class ScreeningConfigManagerComponent implements OnInit {
  screeningConfig: ScreeningFieldConfig[] = [];
  configForm: FormGroup;
  displayedColumns = ['fieldName', 'label', 'type', 'category', 'payloadKey', 'actions'];

  constructor(
    private fb: FormBuilder,
    private screeningConfigService: ScreeningConfigService,
    private toastService: ToastService,
    private dialog: MatDialog
  ) {
    this.configForm = this.createConfigForm();
  }

  ngOnInit() {
    this.loadConfiguration();
  }

  private createConfigForm(): FormGroup {
    return this.fb.group({
      fieldName: ['', Validators.required],
      label: ['', Validators.required],
      type: ['', Validators.required],
      payloadKey: ['', Validators.required],
      category: [''],
      order: [0],
      description: [''],
      defaultValue: [''],
      required: [false],
      visible: [true],
      options: this.fb.array([])
    });
  }

  private loadConfiguration() {
    this.screeningConfig = this.screeningConfigService.getScreeningConfig();
  }

  needsOptions(): boolean {
    const type = this.configForm.get('type')?.value;
    return ['checkbox-group', 'select', 'radio-group'].includes(type);
  }

  getOptionsArray() {
    return this.configForm.get('options') as FormArray;
  }

  addOption() {
    const optionGroup = this.fb.group({
      value: ['', Validators.required],
      label: ['', Validators.required],
      description: [''],
      selected: [false]
    });
    this.getOptionsArray().push(optionGroup);
  }

  removeOption(index: number) {
    this.getOptionsArray().removeAt(index);
  }

  addNewConfig() {
    if (this.configForm.valid) {
      const formValue = this.configForm.value;
      const newConfig: ScreeningFieldConfig = {
        ...formValue,
        options: this.needsOptions() ? formValue.options : undefined
      };

      this.screeningConfigService.addScreeningOption(newConfig);
      this.loadConfiguration();
      this.resetForm();
      this.toastService.success('New screening option added successfully!');
    }
  }

  editConfig(config: ScreeningFieldConfig) {
    // Implement edit functionality
    this.toastService.info('Edit functionality coming soon!');
  }

  deleteConfig(fieldName: string) {
    this.screeningConfigService.removeScreeningOption(fieldName);
    this.loadConfiguration();
    this.toastService.success('Screening option removed successfully!');
  }

  resetForm() {
    this.configForm.reset();
    this.getOptionsArray().clear();
  }

  exportConfig() {
    const config = this.screeningConfigService.getScreeningConfig();
    const dataStr = JSON.stringify(config, null, 2);
    const dataBlob = new Blob([dataStr], { type: 'application/json' });
    const url = URL.createObjectURL(dataBlob);
    const link = document.createElement('a');
    link.href = url;
    link.download = `screening-config-${new Date().toISOString().split('T')[0]}.json`;
    link.click();
    URL.revokeObjectURL(url);
    this.toastService.success('Configuration exported successfully!');
  }

  importConfig() {
    // Implement import functionality
    this.toastService.info('Import functionality coming soon!');
  }

  resetToDefaults() {
    // Implement reset to defaults
    this.toastService.info('Reset to defaults functionality coming soon!');
  }
}
