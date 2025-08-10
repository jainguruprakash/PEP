import { Component, inject } from '@angular/core';
import { RouterLink, RouterLinkActive, RouterOutlet } from '@angular/router';
import { NgIf, AsyncPipe, DatePipe } from '@angular/common';
import { MatToolbarModule } from '@angular/material/toolbar';
import { MatSidenavModule } from '@angular/material/sidenav';
import { MatIconModule } from '@angular/material/icon';
import { MatListModule } from '@angular/material/list';
import { MatButtonModule } from '@angular/material/button';
import { MatMenuModule } from '@angular/material/menu';
import { MatBadgeModule } from '@angular/material/badge';
import { MatDividerModule } from '@angular/material/divider';
import { AuthService } from '../services/auth.service';
import { NotificationService } from '../services/notification.service';
import { Router } from '@angular/router';

@Component({
  selector: 'app-shell',
  standalone: true,
  imports: [
    RouterOutlet,
    RouterLink,
    RouterLinkActive,
    NgIf,
    AsyncPipe,
    DatePipe,
    MatToolbarModule,
    MatSidenavModule,
    MatIconModule,
    MatListModule,
    MatButtonModule,
    MatMenuModule,
    MatBadgeModule,
    MatDividerModule
  ],
  template: `
  <mat-sidenav-container class="container">
    <mat-sidenav mode="side" opened>
      <mat-toolbar color="primary">
        <mat-icon>security</mat-icon>
        <span class="app-title">PEP Scanner</span>
      </mat-toolbar>
      <mat-nav-list>
        <a mat-list-item routerLink="/dashboard" routerLinkActive="active">
          <mat-icon matListItemIcon>dashboard</mat-icon>
          <span matListItemTitle>Dashboard</span>
        </a>
        <a mat-list-item routerLink="/screening/customer" routerLinkActive="active">
          <mat-icon matListItemIcon>person_search</mat-icon>
          <span matListItemTitle>Customer Screening</span>
        </a>
        <a mat-list-item routerLink="/screening/transaction" routerLinkActive="active">
          <mat-icon matListItemIcon>account_balance</mat-icon>
          <span matListItemTitle>Transaction Screening</span>
        </a>
        <a mat-list-item routerLink="/search" routerLinkActive="active">
          <mat-icon matListItemIcon>search</mat-icon>
          <span matListItemTitle>Search</span>
        </a>
        <a mat-list-item routerLink="/watchlists" routerLinkActive="active">
          <mat-icon matListItemIcon>list</mat-icon>
          <span matListItemTitle>Watchlists</span>
        </a>
        <a mat-list-item routerLink="/sanctions" routerLinkActive="active">
          <mat-icon matListItemIcon>gavel</mat-icon>
          <span matListItemTitle>Sanctions</span>
        </a>
        <a mat-list-item routerLink="/alerts" routerLinkActive="active">
          <mat-icon matListItemIcon>warning</mat-icon>
          <span matListItemTitle>Alerts</span>
        </a>
        <a mat-list-item routerLink="/customers" routerLinkActive="active">
          <mat-icon matListItemIcon>people</mat-icon>
          <span matListItemTitle>Customers</span>
        </a>
        @if (authService.hasAnyRole(['Admin'])) {
          <a mat-list-item routerLink="/organizations" routerLinkActive="active">
            <mat-icon matListItemIcon>business</mat-icon>
            <span matListItemTitle>Organizations</span>
          </a>
        }
        @if (authService.hasAnyRole(['Admin', 'Manager'])) {
          <a mat-list-item routerLink="/settings" routerLinkActive="active">
            <mat-icon matListItemIcon>settings</mat-icon>
            <span matListItemTitle>Settings</span>
          </a>
        }
      </mat-nav-list>
    </mat-sidenav>
    <mat-sidenav-content>
      <mat-toolbar color="primary">
        <span class="toolbar-spacer"></span>

        <!-- Notifications -->
        <button mat-icon-button [matMenuTriggerFor]="notificationMenu">
          <mat-icon [matBadge]="notificationService.unreadCount$ | async" matBadgeColor="warn" [matBadgeHidden]="(notificationService.unreadCount$ | async) === 0">notifications</mat-icon>
        </button>
        <mat-menu #notificationMenu="matMenu" class="notification-menu">
          <div class="notification-header">
            <h3>Notifications</h3>
            <button mat-button (click)="markAllAsRead()">Mark all as read</button>
          </div>
          <mat-divider></mat-divider>
          @if ((notificationService.notifications$ | async)?.length === 0) {
            <div class="no-notifications">
              <mat-icon>notifications_none</mat-icon>
              <p>No new notifications</p>
            </div>
          } @else {
            @for (notification of (notificationService.notifications$ | async); track notification.id) {
              <button mat-menu-item class="notification-item" [class.unread]="!notification.read" (click)="handleNotificationClick(notification)">
                <mat-icon [color]="notificationService.getNotificationColor(notification.type)">{{ notificationService.getNotificationIcon(notification.type) }}</mat-icon>
                <div class="notification-content">
                  <div class="notification-title">{{ notification.title }}</div>
                  <div class="notification-message">{{ notification.message }}</div>
                  <div class="notification-time">{{ notification.createdAt | date:'short' }}</div>
                </div>
              </button>
              <mat-divider></mat-divider>
            }
          }
        </mat-menu>

        <!-- User Profile -->
        <button mat-icon-button [matMenuTriggerFor]="userMenu">
          <mat-icon>account_circle</mat-icon>
        </button>
        <mat-menu #userMenu="matMenu">
          <div class="user-info">
            <div class="user-avatar">
              <mat-icon>account_circle</mat-icon>
            </div>
            <div class="user-details">
              <div class="user-name">{{ (authService.currentUser$ | async)?.firstName }} {{ (authService.currentUser$ | async)?.lastName }}</div>
              <div class="user-email">{{ (authService.currentUser$ | async)?.email }}</div>
              <div class="user-role">{{ (authService.currentUser$ | async)?.role }}</div>
            </div>
          </div>
          <mat-divider></mat-divider>
          <button mat-menu-item routerLink="/profile">
            <mat-icon>person</mat-icon>
            <span>Profile</span>
          </button>
          <button mat-menu-item routerLink="/settings">
            <mat-icon>settings</mat-icon>
            <span>Settings</span>
          </button>
          <button mat-menu-item (click)="logout()">
            <mat-icon>logout</mat-icon>
            <span>Logout</span>
          </button>
        </mat-menu>
      </mat-toolbar>
      <div class="content">
        <router-outlet />
      </div>
    </mat-sidenav-content>
  </mat-sidenav-container>
  `,
  styles: [`
    .container {
      height: 100vh;
    }
    .content {
      padding: 16px;
    }
    .toolbar-spacer {
      flex: 1 1 auto;
    }
    .app-title {
      margin-left: 8px;
      font-weight: 500;
    }
    .user-info {
      display: flex;
      align-items: center;
      padding: 16px;
      min-width: 280px;
    }
    .user-avatar {
      margin-right: 12px;
    }
    .user-avatar mat-icon {
      font-size: 40px;
      width: 40px;
      height: 40px;
      color: #666;
    }
    .user-details {
      flex: 1;
    }
    .user-name {
      font-weight: 500;
      font-size: 16px;
      margin-bottom: 4px;
    }
    .user-email {
      color: #666;
      font-size: 14px;
      margin-bottom: 2px;
    }
    .user-role {
      color: #999;
      font-size: 12px;
      text-transform: uppercase;
      font-weight: 500;
    }
    .notification-menu {
      min-width: 350px;
      max-width: 400px;
    }
    .notification-header {
      display: flex;
      justify-content: space-between;
      align-items: center;
      padding: 16px;
    }
    .notification-header h3 {
      margin: 0;
      font-size: 18px;
      font-weight: 500;
    }
    .notification-item {
      display: flex !important;
      align-items: flex-start !important;
      padding: 12px 16px !important;
      min-height: auto !important;
      height: auto !important;
      white-space: normal !important;
    }
    .notification-item.unread {
      background-color: #f5f5f5;
    }
    .notification-item mat-icon {
      margin-right: 12px;
      margin-top: 2px;
    }
    .notification-content {
      flex: 1;
      text-align: left;
    }
    .notification-title {
      font-weight: 500;
      font-size: 14px;
      margin-bottom: 4px;
    }
    .notification-message {
      font-size: 13px;
      color: #666;
      margin-bottom: 4px;
      line-height: 1.3;
    }
    .notification-time {
      font-size: 11px;
      color: #999;
    }
    .no-notifications {
      text-align: center;
      padding: 32px 16px;
      color: #999;
    }
    .no-notifications mat-icon {
      font-size: 48px;
      width: 48px;
      height: 48px;
      margin-bottom: 8px;
    }
    .no-notifications p {
      margin: 0;
      font-size: 14px;
    }
  `]
})
export class AppShellComponent {
  authService = inject(AuthService);
  notificationService = inject(NotificationService);
  router = inject(Router);

  constructor() {
    // Simulate real-time notifications
    this.notificationService.simulateRealTimeNotification();
  }

  logout() {
    this.authService.logout();
  }

  handleNotificationClick(notification: any) {
    // Mark as read
    this.notificationService.markAsRead(notification.id).subscribe();

    // Navigate to action URL if available
    if (notification.actionUrl) {
      this.router.navigateByUrl(notification.actionUrl);
    }
  }

  markAllAsRead() {
    this.notificationService.markAllAsRead().subscribe();
  }
}


