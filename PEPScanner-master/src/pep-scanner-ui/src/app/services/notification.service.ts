import { Injectable, inject } from '@angular/core';
import { HttpClient } from '@angular/common/http';
import { BehaviorSubject, Observable, of } from 'rxjs';

export interface Notification {
  id: number;
  type: 'alert' | 'info' | 'warning' | 'success';
  title: string;
  message: string;
  createdAt: Date;
  read: boolean;
  userId?: string;
  actionUrl?: string;
}

@Injectable({
  providedIn: 'root'
})
export class NotificationService {
  private http = inject(HttpClient);
  private readonly API_BASE = 'http://localhost:5097/api';

  private notificationsSubject = new BehaviorSubject<Notification[]>([]);
  public notifications$ = this.notificationsSubject.asObservable();

  private unreadCountSubject = new BehaviorSubject<number>(0);
  public unreadCount$ = this.unreadCountSubject.asObservable();

  constructor() {
    this.loadNotifications();
  }

  private loadNotifications() {
    // Mock notifications for now - replace with real API call
    const mockNotifications: Notification[] = [
      {
        id: 1,
        type: 'alert',
        title: 'High Risk Alert',
        message: 'Customer John Smith matched against sanctions list',
        createdAt: new Date(Date.now() - 1000 * 60 * 30), // 30 minutes ago
        read: false,
        actionUrl: '/alerts/1'
      },
      {
        id: 2,
        type: 'info',
        title: 'System Update',
        message: 'Watchlist data has been updated with latest entries',
        createdAt: new Date(Date.now() - 1000 * 60 * 60 * 2), // 2 hours ago
        read: false
      },
      {
        id: 3,
        type: 'warning',
        title: 'Review Required',
        message: 'Transaction screening requires manual review',
        createdAt: new Date(Date.now() - 1000 * 60 * 60 * 4), // 4 hours ago
        read: false,
        actionUrl: '/screening/transaction'
      },
      {
        id: 4,
        type: 'success',
        title: 'Screening Complete',
        message: 'Batch screening of 150 customers completed successfully',
        createdAt: new Date(Date.now() - 1000 * 60 * 60 * 6), // 6 hours ago
        read: true
      },
      {
        id: 5,
        type: 'alert',
        title: 'PEP Match Found',
        message: 'Politically Exposed Person detected in customer database',
        createdAt: new Date(Date.now() - 1000 * 60 * 60 * 8), // 8 hours ago
        read: true,
        actionUrl: '/customers'
      }
    ];

    this.notificationsSubject.next(mockNotifications);
    this.updateUnreadCount();
  }

  getNotifications(): Observable<Notification[]> {
    // In a real app, this would make an HTTP request
    // return this.http.get<Notification[]>(`${this.API_BASE}/notifications`);
    return this.notifications$;
  }

  markAsRead(notificationId: number): Observable<void> {
    // In a real app, this would make an HTTP request
    // return this.http.put<void>(`${this.API_BASE}/notifications/${notificationId}/read`, {});
    
    const notifications = this.notificationsSubject.value;
    const notification = notifications.find(n => n.id === notificationId);
    if (notification) {
      notification.read = true;
      this.notificationsSubject.next([...notifications]);
      this.updateUnreadCount();
    }
    return of();
  }

  markAllAsRead(): Observable<void> {
    // In a real app, this would make an HTTP request
    // return this.http.put<void>(`${this.API_BASE}/notifications/mark-all-read`, {});
    
    const notifications = this.notificationsSubject.value.map(n => ({ ...n, read: true }));
    this.notificationsSubject.next(notifications);
    this.updateUnreadCount();
    return of();
  }

  deleteNotification(notificationId: number): Observable<void> {
    // In a real app, this would make an HTTP request
    // return this.http.delete<void>(`${this.API_BASE}/notifications/${notificationId}`);
    
    const notifications = this.notificationsSubject.value.filter(n => n.id !== notificationId);
    this.notificationsSubject.next(notifications);
    this.updateUnreadCount();
    return of();
  }

  addNotification(notification: Omit<Notification, 'id' | 'createdAt'>): void {
    const newNotification: Notification = {
      ...notification,
      id: Date.now(), // Simple ID generation
      createdAt: new Date()
    };
    
    const notifications = [newNotification, ...this.notificationsSubject.value];
    this.notificationsSubject.next(notifications);
    this.updateUnreadCount();
  }

  getUnreadCount(): number {
    return this.unreadCountSubject.value;
  }

  private updateUnreadCount(): void {
    const unreadCount = this.notificationsSubject.value.filter(n => !n.read).length;
    this.unreadCountSubject.next(unreadCount);
  }

  // Utility methods for notification icons and colors
  getNotificationIcon(type: string): string {
    switch (type) {
      case 'alert': return 'warning';
      case 'info': return 'info';
      case 'warning': return 'error_outline';
      case 'success': return 'check_circle';
      default: return 'notifications';
    }
  }

  getNotificationColor(type: string): string {
    switch (type) {
      case 'alert': return 'warn';
      case 'info': return 'primary';
      case 'warning': return 'accent';
      case 'success': return 'primary';
      default: return '';
    }
  }

  // Real-time notification simulation (in a real app, this would be WebSocket or SSE)
  simulateRealTimeNotification(): void {
    setTimeout(() => {
      this.addNotification({
        type: 'alert',
        title: 'New Alert Generated',
        message: 'A new high-risk transaction has been detected',
        read: false,
        actionUrl: '/alerts'
      });
    }, 5000); // Add a notification after 5 seconds
  }
}
