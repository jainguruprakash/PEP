import { Injectable } from '@angular/core';
import { Observable, Subject } from 'rxjs';

@Injectable({ providedIn: 'root' })
export class WebSocketService {
  private socket?: WebSocket;
  private messageSubject = new Subject<any>();

  connect(): Observable<any> {
    if (!this.socket) {
      this.socket = new WebSocket('ws://localhost:4200/ws');
      this.socket.onmessage = (event) => {
        this.messageSubject.next(JSON.parse(event.data));
      };
    }
    return this.messageSubject.asObservable();
  }

  disconnect() {
    this.socket?.close();
    this.socket = undefined;
  }
}