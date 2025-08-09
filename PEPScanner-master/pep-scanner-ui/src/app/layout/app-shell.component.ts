import { Component } from '@angular/core';
import { RouterLink, RouterLinkActive, RouterOutlet } from '@angular/router';
import { NgIf } from '@angular/common';
import { MatToolbarModule } from '@angular/material/toolbar';
import { MatSidenavModule } from '@angular/material/sidenav';
import { MatIconModule } from '@angular/material/icon';
import { MatListModule } from '@angular/material/list';
import { MatButtonModule } from '@angular/material/button';

@Component({
  selector: 'app-shell',
  standalone: true,
  imports: [
    RouterOutlet,
    RouterLink,
    RouterLinkActive,
    NgIf,
    MatToolbarModule,
    MatSidenavModule,
    MatIconModule,
    MatListModule,
    MatButtonModule
  ],
  template: `
  <mat-sidenav-container class="container">
    <mat-sidenav mode="side" opened>
      <mat-toolbar color="primary">PEP Scanner</mat-toolbar>
      <mat-nav-list>
        <a mat-list-item routerLink="/dashboard" routerLinkActive="active">Dashboard</a>
        <a mat-list-item routerLink="/screening/customer" routerLinkActive="active">Customer Screening</a>
        <a mat-list-item routerLink="/screening/transaction" routerLinkActive="active">Transaction Screening</a>
        <a mat-list-item routerLink="/search" routerLinkActive="active">Search</a>
        <a mat-list-item routerLink="/watchlists" routerLinkActive="active">Watchlists</a>
        <a mat-list-item routerLink="/sanctions" routerLinkActive="active">Sanctions</a>
        <a mat-list-item routerLink="/alerts" routerLinkActive="active">Alerts</a>
        <a mat-list-item routerLink="/customers" routerLinkActive="active">Customers</a>
        <a mat-list-item routerLink="/organizations" routerLinkActive="active">Organizations</a>
        <a mat-list-item routerLink="/settings" routerLinkActive="active">Settings</a>
      </mat-nav-list>
    </mat-sidenav>
    <mat-sidenav-content>
      <mat-toolbar color="primary">
        <span class="toolbar-spacer"></span>
        <button mat-button routerLink="/signup">Sign up</button>
        <button mat-button routerLink="/login">Login</button>
      </mat-toolbar>
      <div class="content">
        <router-outlet />
      </div>
    </mat-sidenav-content>
  </mat-sidenav-container>
  `,
  styles: [`
    .container { height: 100vh; }
    .content { padding: 16px; }
    .toolbar-spacer { flex: 1 1 auto; }
  `]
})
export class AppShellComponent {}


