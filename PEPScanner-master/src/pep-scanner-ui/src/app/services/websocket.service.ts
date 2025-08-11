import { Injectable } from '@angular/core';
import { Observable, Subject } from 'rxjs';

@Injectable({ providedIn: 'root' })
export class WebSocketService {
  private socket?: WebSocket;
  private messageSubject = new Subject<any>();

  connect(): Observable<any> {
    // Mock WebSocket connection for demo
    // In production, this would connect to actual WebSocket server
    setTimeout(() => {
      this.messageSubject.next({
        type: 'WATCHLIST_UPDATE',
        message: 'Watchlist updated with new entries'
      });
    }, 30000); // Simulate update after 30 seconds

    return this.messageSubject.asObservable();
  }

  disconnect() {
    if (this.socket) {
      this.socket.close();
    }
  }
}