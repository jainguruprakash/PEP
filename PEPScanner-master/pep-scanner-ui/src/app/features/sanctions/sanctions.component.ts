import { Component, inject } from '@angular/core';
import { CommonModule } from '@angular/common';
import { ReactiveFormsModule, FormBuilder } from '@angular/forms';
import { MatButtonModule } from '@angular/material/button';
import { MatFormFieldModule } from '@angular/material/form-field';
import { MatInputModule } from '@angular/material/input';
import { MatTabsModule } from '@angular/material/tabs';
import { MatTableModule } from '@angular/material/table';
import { WatchlistService } from '../../services/watchlist.service';

@Component({
  selector: 'app-sanctions',
  standalone: true,
  imports: [CommonModule, ReactiveFormsModule, MatButtonModule, MatFormFieldModule, MatInputModule, MatTabsModule, MatTableModule],
  template: `
    <h2>Sanctions & Regulatory Lists</h2>
    <mat-tab-group>
      <mat-tab label="OFAC">
        <div class="row">
          <form [formGroup]="ofacForm" (ngSubmit)="searchOfac()">
            <mat-form-field appearance="outline"><mat-label>Name</mat-label><input matInput formControlName="name" /></mat-form-field>
            <button mat-raised-button color="primary">Search</button>
            <button mat-stroked-button color="accent" type="button" (click)="updateOfac()">Update</button>
          </form>
          <table mat-table [dataSource]="ofacResults" *ngIf="ofacResults">
            <ng-container matColumnDef="name"><th mat-header-cell *matHeaderCellDef>Name</th><td mat-cell *matCellDef="let r">{{ r.name }}</td></ng-container>
            <ng-container matColumnDef="program"><th mat-header-cell *matHeaderCellDef>Program</th><td mat-cell *matCellDef="let r">{{ r.program }}</td></ng-container>
            <tr mat-header-row *matHeaderRowDef="ofacCols"></tr>
            <tr mat-row *matRowDef="let row; columns: ofacCols;"></tr>
          </table>
        </div>
      </mat-tab>

      <mat-tab label="UN">
        <div class="row">
          <form [formGroup]="unForm" (ngSubmit)="searchUn()">
            <mat-form-field appearance="outline"><mat-label>Name</mat-label><input matInput formControlName="name" /></mat-form-field>
            <button mat-raised-button color="primary">Search</button>
            <button mat-stroked-button color="accent" type="button" (click)="updateUn()">Update</button>
          </form>
          <table mat-table [dataSource]="unResults" *ngIf="unResults">
            <ng-container matColumnDef="name"><th mat-header-cell *matHeaderCellDef>Name</th><td mat-cell *matCellDef="let r">{{ r.name }}</td></ng-container>
            <tr mat-header-row *matHeaderRowDef="unCols"></tr>
            <tr mat-row *matRowDef="let row; columns: unCols;"></tr>
          </table>
        </div>
      </mat-tab>

      <mat-tab label="RBI">
        <div class="row">
          <form [formGroup]="rbiForm" (ngSubmit)="searchRbi()">
            <mat-form-field appearance="outline"><mat-label>Name</mat-label><input matInput formControlName="name" /></mat-form-field>
            <button mat-raised-button color="primary">Search</button>
            <button mat-stroked-button color="accent" type="button" (click)="updateRbi()">Update</button>
            <button mat-stroked-button type="button" (click)="scrapeRbi()">Scrape Advanced</button>
          </form>
          <table mat-table [dataSource]="rbiResults" *ngIf="rbiResults">
            <ng-container matColumnDef="name"><th mat-header-cell *matHeaderCellDef>Name</th><td mat-cell *matCellDef="let r">{{ r.name }}</td></ng-container>
            <ng-container matColumnDef="category"><th mat-header-cell *matHeaderCellDef>Category</th><td mat-cell *matCellDef="let r">{{ r.category }}</td></ng-container>
            <tr mat-header-row *matHeaderRowDef="rbiCols"></tr>
            <tr mat-row *matRowDef="let row; columns: rbiCols;"></tr>
          </table>
        </div>
      </mat-tab>

      <mat-tab label="SEBI">
        <div class="row">
          <form [formGroup]="sebiForm" (ngSubmit)="searchSebi()">
            <mat-form-field appearance="outline"><mat-label>Name</mat-label><input matInput formControlName="name" /></mat-form-field>
            <button mat-raised-button color="primary">Search</button>
            <button mat-stroked-button color="accent" type="button" (click)="updateSebi()">Update</button>
            <button mat-stroked-button type="button" (click)="scrapeSebi()">Scrape Advanced</button>
          </form>
          <table mat-table [dataSource]="sebiResults" *ngIf="sebiResults">
            <ng-container matColumnDef="name"><th mat-header-cell *matHeaderCellDef>Name</th><td mat-cell *matCellDef="let r">{{ r.name || r.entityName }}</td></ng-container>
            <ng-container matColumnDef="category"><th mat-header-cell *matHeaderCellDef>Category</th><td mat-cell *matCellDef="let r">{{ r.category }}</td></ng-container>
            <tr mat-header-row *matHeaderRowDef="sebiCols"></tr>
            <tr mat-row *matRowDef="let row; columns: sebiCols;"></tr>
          </table>
        </div>
      </mat-tab>

      <mat-tab label="Parliament">
        <div class="row">
          <form [formGroup]="parForm" (ngSubmit)="searchPar()">
            <mat-form-field appearance="outline"><mat-label>Name</mat-label><input matInput formControlName="name" /></mat-form-field>
            <button mat-raised-button color="primary">Search</button>
            <button mat-stroked-button color="accent" type="button" (click)="updatePar()">Update</button>
            <button mat-stroked-button type="button" (click)="scrapePar()">Scrape Advanced</button>
          </form>
          <table mat-table [dataSource]="parResults" *ngIf="parResults">
            <ng-container matColumnDef="name"><th mat-header-cell *matHeaderCellDef>Name</th><td mat-cell *matCellDef="let r">{{ r.name }}</td></ng-container>
            <ng-container matColumnDef="role"><th mat-header-cell *matHeaderCellDef>Role</th><td mat-cell *matCellDef="let r">{{ r.role || r.category }}</td></ng-container>
            <tr mat-header-row *matHeaderRowDef="parCols"></tr>
            <tr mat-row *matRowDef="let row; columns: parCols;"></tr>
          </table>
        </div>
      </mat-tab>
    </mat-tab-group>
  `,
  styles: [`.row{display:grid;grid-template-columns:repeat(auto-fit,minmax(220px,1fr));gap:16px}`]
})
export class SanctionsComponent {
  private fb = inject(FormBuilder);
  private svc = inject(WatchlistService);

  ofacForm = this.fb.group({ name: [''] });
  unForm = this.fb.group({ name: [''] });
  rbiForm = this.fb.group({ name: [''] });
  sebiForm = this.fb.group({ name: [''] });
  parForm = this.fb.group({ name: [''] });

  ofacResults: any[] | null = null; ofacCols = ['name','program'];
  unResults: any[] | null = null; unCols = ['name'];
  rbiResults: any[] | null = null; rbiCols = ['name','category'];
  sebiResults: any[] | null = null; sebiCols = ['name','category'];
  parResults: any[] | null = null; parCols = ['name','role'];

  searchOfac(){ this.svc.searchOfac(this.ofacForm.value.name || '').subscribe(r => this.ofacResults = r); }
  updateOfac(){ this.svc.updateOfac().subscribe(); }

  searchUn(){ this.svc.searchUn(this.unForm.value.name || '').subscribe(r => this.unResults = r); }
  updateUn(){ this.svc.updateUn().subscribe(); }

  searchRbi(){ this.svc.searchRbi(this.rbiForm.value.name || '').subscribe(r => this.rbiResults = r); }
  updateRbi(){ this.svc.updateRbi().subscribe(); }
  scrapeRbi(){ this.svc.scrapeAdvancedRbi().subscribe(); }

  searchSebi(){ this.svc.searchSebi(this.sebiForm.value.name || '').subscribe(r => this.sebiResults = r); }
  updateSebi(){ this.svc.updateSebi().subscribe(); }
  scrapeSebi(){ this.svc.scrapeAdvancedSebi().subscribe(); }

  searchPar(){ this.svc.searchParliament(this.parForm.value.name || '').subscribe(r => this.parResults = r); }
  updatePar(){ this.svc.updateParliament().subscribe(); }
  scrapePar(){ this.svc.scrapeAdvancedParliament().subscribe(); }
}


