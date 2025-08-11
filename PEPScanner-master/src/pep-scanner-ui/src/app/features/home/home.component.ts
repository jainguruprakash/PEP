import { Component } from '@angular/core';
import { RouterLink } from '@angular/router';
import { MatButtonModule } from '@angular/material/button';
import { MatCardModule } from '@angular/material/card';

@Component({
  selector: 'app-home',
  standalone: true,
  imports: [RouterLink, MatButtonModule, MatCardModule],
  template: `
    <div style="padding: 2rem; text-align: center;">
      <h1>PEP Scanner</h1>
      <p>Compliance Management System</p>
      <div style="margin-top: 2rem;">
        <a mat-raised-button color="primary" routerLink="/login">Login</a>
        <a mat-raised-button routerLink="/signup" style="margin-left: 1rem;">Sign Up</a>
      </div>
    </div>
  `
})
export class HomeComponent {}