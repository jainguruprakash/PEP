import { Component, inject } from '@angular/core';
import { CommonModule } from '@angular/common';
import { MatButtonModule } from '@angular/material/button';
import { MatListModule } from '@angular/material/list';
import { WatchlistService } from '../../services/watchlist.service';

@Component({
  selector: 'app-watchlists',
  standalone: true,
  imports: [CommonModule, MatButtonModule, MatListModule],
  template: `
    <h2>Watchlists</h2>
    <button mat-raised-button color="primary" (click)="updateAll()">Update All</button>
    <mat-nav-list>
      <a mat-list-item *ngFor="let s of sources" (click)="update(s.name)">{{ s.name }} ({{ s.type }} - {{ s.country }})</a>
    </mat-nav-list>
    <pre *ngIf="lastUpdates">{{ lastUpdates | json }}</pre>
  `
})
export class WatchlistsComponent {
  private watchlistService = inject(WatchlistService);
  sources: any[] = [];
  lastUpdates: any;

  constructor(){
    this.watchlistService.getGenericSources().subscribe(s => this.sources = s as any[]);
    this.watchlistService.getLastUpdates().subscribe(l => this.lastUpdates = l);
  }

  updateAll(){ this.watchlistService.updateAll().subscribe(); }
  update(name: string){ this.watchlistService.updateSource(name).subscribe(); }
}


