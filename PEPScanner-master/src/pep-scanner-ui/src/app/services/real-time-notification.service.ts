import { Injectable, inject } from '@angular/core';
import { BehaviorSubject, Observable, Subject } from 'rxjs';
import { MatSnackBar } from '@angular/material/snack-bar';
import * as signalR from '@microsoft/signalr';
import { environment } from '../../environments/environment';

export interface RealTimeNotification {
  id: string;
  type: string;
  title: string;
  message: string;
  data?: any;
  timestamp: Date;
  priority: string;
  category: string;
  actionRequired?: boolean;
  actionUrl?: string;
  isRead?: boolean;
}

export interface BulkScanProgress {
  totalCount: number;
  completedCount: number;
  successCount: number;
  failedCount: number;
  percentComplete: number;
  estimatedTimeRemaining?: string;
  currentCustomer?: string;
}

@Injectable({
  providedIn: 'root'
})
export class RealTimeNotificationService {
  private snackBar = inject(MatSnackBar);
  
  private hubConnection: signalR.HubConnection;
  private notificationsSubject = new BehaviorSubject<RealTimeNotification[]>([]);
  private connectionStateSubject = new BehaviorSubject<signalR.HubConnectionState>(signalR.HubConnectionState.Disconnected);
  private bulkScanProgressSubject = new Subject<BulkScanProgress>();

  public notifications$ = this.notificationsSubject.asObservable();
  public connectionState$ = this.connectionStateSubject.asObservable();
  public bulkScanProgress$ = this.bulkScanProgressSubject.asObservable();

  private notifications: RealTimeNotification[] = [];
  private maxNotifications = 100; // Keep last 100 notifications

  constructor() {
    this.hubConnection = new signalR.HubConnectionBuilder()
      .withUrl(`${environment.apiBaseUrl}/notificationHub`)
      .withAutomaticReconnect()
      .build();

    this.setupEventHandlers();
    this.startConnection();
  }

  private setupEventHandlers(): void {
    // Handle incoming notifications
    this.hubConnection.on('ReceiveNotification', (notification: RealTimeNotification) => {
      this.handleNotification(notification);
    });

    // Handle connection state changes
    this.hubConnection.onclose(() => {
      this.connectionStateSubject.next(signalR.HubConnectionState.Disconnected);
      console.log('SignalR connection closed');
    });

    this.hubConnection.onreconnecting(() => {
      this.connectionStateSubject.next(signalR.HubConnectionState.Reconnecting);
      console.log('SignalR reconnecting...');
    });

    this.hubConnection.onreconnected(() => {
      this.connectionStateSubject.next(signalR.HubConnectionState.Connected);
      console.log('SignalR reconnected');
    });
  }

  private async startConnection(): Promise<void> {
    try {
      await this.hubConnection.start();
      this.connectionStateSubject.next(this.hubConnection.state);
      console.log('SignalR connection started');
      
      // Join user group for personalized notifications
      await this.joinUserGroup();
    } catch (error) {
      console.error('Error starting SignalR connection:', error);
      // Retry connection after 5 seconds
      setTimeout(() => this.startConnection(), 5000);
    }
  }

  private handleNotification(notification: RealTimeNotification): void {
    // Add to notifications list
    this.notifications.unshift(notification);
    
    // Keep only the latest notifications
    if (this.notifications.length > this.maxNotifications) {
      this.notifications = this.notifications.slice(0, this.maxNotifications);
    }
    
    this.notificationsSubject.next([...this.notifications]);

    // Handle different notification types
    switch (notification.type) {
      case 'scan_complete':
        this.handleScanCompleteNotification(notification);
        break;
      case 'alert_created':
        this.handleAlertCreatedNotification(notification);
        break;
      case 'risk_score_update':
        this.handleRiskScoreUpdateNotification(notification);
        break;
      case 'bulk_scan_progress':
        this.handleBulkScanProgressNotification(notification);
        break;
      case 'system':
        this.handleSystemNotification(notification);
        break;
      default:
        this.showSnackBarNotification(notification);
    }
  }

  private handleScanCompleteNotification(notification: RealTimeNotification): void {
    const data = notification.data;
    let message = `Scan completed for ${data?.customerName || 'customer'}`;
    
    if (data?.highRiskResults > 0) {
      message += ` - ${data.highRiskResults} high-risk results found!`;
    }
    
    if (data?.alertsCreated > 0) {
      message += ` ${data.alertsCreated} alerts created.`;
    }

    this.showSnackBarNotification({
      ...notification,
      message
    }, data?.highRiskResults > 0 ? 'warn' : 'success');
  }

  private handleAlertCreatedNotification(notification: RealTimeNotification): void {
    this.showSnackBarNotification(notification, 'warn');
    
    // Play notification sound for critical alerts
    if (notification.priority === 'Critical') {
      this.playNotificationSound();
    }
  }

  private handleRiskScoreUpdateNotification(notification: RealTimeNotification): void {
    const data = notification.data;
    const riskLevel = this.getRiskLevel(data?.riskScore || 0);
    
    this.showSnackBarNotification({
      ...notification,
      message: `Risk score updated: ${data?.riskScore?.toFixed(1)} (${riskLevel})`
    }, riskLevel === 'High' || riskLevel === 'Critical' ? 'warn' : 'info');
  }

  private handleBulkScanProgressNotification(notification: RealTimeNotification): void {
    const progress: BulkScanProgress = notification.data;
    this.bulkScanProgressSubject.next(progress);
    
    // Don't show snack bar for progress updates to avoid spam
    // Only emit to progress observable
  }

  private handleSystemNotification(notification: RealTimeNotification): void {
    this.showSnackBarNotification(notification, 'info');
  }

  private showSnackBarNotification(notification: RealTimeNotification, type: 'success' | 'warn' | 'info' = 'info'): void {
    const config = {
      duration: this.getSnackBarDuration(notification.priority),
      panelClass: [`snackbar-${type}`],
      action: notification.actionRequired ? 'View' : 'Dismiss'
    };

    const snackBarRef = this.snackBar.open(notification.message, config.action, config);
    
    if (notification.actionRequired && notification.actionUrl) {
      snackBarRef.onAction().subscribe(() => {
        window.open(notification.actionUrl, '_blank');
      });
    }
  }

  private getSnackBarDuration(priority: string): number {
    switch (priority.toLowerCase()) {
      case 'critical': return 10000; // 10 seconds
      case 'high': return 7000;      // 7 seconds
      case 'medium': return 5000;    // 5 seconds
      case 'low': return 3000;       // 3 seconds
      default: return 4000;          // 4 seconds
    }
  }

  private playNotificationSound(): void {
    try {
      const audio = new Audio('/assets/sounds/notification.mp3');
      audio.play().catch(error => {
        console.log('Could not play notification sound:', error);
      });
    } catch (error) {
      console.log('Notification sound not available:', error);
    }
  }

  private getRiskLevel(riskScore: number): string {
    if (riskScore >= 90) return 'Critical';
    if (riskScore >= 75) return 'High';
    if (riskScore >= 50) return 'Medium';
    if (riskScore >= 25) return 'Low';
    return 'Minimal';
  }

  // Public methods
  public async joinCustomerGroup(customerId: string): Promise<void> {
    if (this.hubConnection.state === signalR.HubConnectionState.Connected) {
      try {
        await this.hubConnection.invoke('JoinCustomerGroup', customerId);
        console.log(`Joined customer group: ${customerId}`);
      } catch (error) {
        console.error('Error joining customer group:', error);
      }
    }
  }

  public async leaveCustomerGroup(customerId: string): Promise<void> {
    if (this.hubConnection.state === signalR.HubConnectionState.Connected) {
      try {
        await this.hubConnection.invoke('LeaveCustomerGroup', customerId);
        console.log(`Left customer group: ${customerId}`);
      } catch (error) {
        console.error('Error leaving customer group:', error);
      }
    }
  }

  public async joinUserGroup(): Promise<void> {
    if (this.hubConnection.state === signalR.HubConnectionState.Connected) {
      try {
        // In a real app, get user ID from auth service
        const userId = 'current-user'; // Placeholder
        await this.hubConnection.invoke('JoinUserGroup', userId);
        console.log(`Joined user group: ${userId}`);
      } catch (error) {
        console.error('Error joining user group:', error);
      }
    }
  }

  public markAsRead(notificationId: string): void {
    const notification = this.notifications.find(n => n.id === notificationId);
    if (notification) {
      notification.isRead = true;
      this.notificationsSubject.next([...this.notifications]);
    }
  }

  public markAllAsRead(): void {
    this.notifications.forEach(n => n.isRead = true);
    this.notificationsSubject.next([...this.notifications]);
  }

  public clearNotifications(): void {
    this.notifications = [];
    this.notificationsSubject.next([]);
  }

  public getUnreadCount(): number {
    return this.notifications.filter(n => !n.isRead).length;
  }

  public getNotificationsByCategory(category: string): RealTimeNotification[] {
    return this.notifications.filter(n => n.category === category);
  }

  public getNotificationsByPriority(priority: string): RealTimeNotification[] {
    return this.notifications.filter(n => n.priority === priority);
  }

  public isConnected(): boolean {
    return this.hubConnection.state === signalR.HubConnectionState.Connected;
  }

  public async reconnect(): Promise<void> {
    if (this.hubConnection.state === signalR.HubConnectionState.Disconnected) {
      await this.startConnection();
    }
  }

  public disconnect(): void {
    if (this.hubConnection.state === signalR.HubConnectionState.Connected) {
      this.hubConnection.stop();
    }
  }
}
