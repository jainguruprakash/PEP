import { Component } from '@angular/core';
import { Router } from '@angular/router';
import { CommonModule } from '@angular/common';

@Component({
  selector: 'app-home',
  standalone: true,
  imports: [CommonModule],
  template: `
    <style>
      :host {
        --pepify-primary: #667eea;
        --pepify-secondary: #764ba2;
        --pepify-accent: #f093fb;
        --pepify-dark: #1a202c;
        --pepify-light: #f7fafc;
        
        font-family: "Inter", -apple-system, BlinkMacSystemFont, "Segoe UI", Roboto, sans-serif;
        box-sizing: border-box;
        -webkit-font-smoothing: antialiased;
        -moz-osx-font-smoothing: grayscale;
      }

      .pepify-landing {
        min-height: 100vh;
        background: linear-gradient(135deg, var(--pepify-primary) 0%, var(--pepify-secondary) 100%);
        display: flex;
        align-items: center;
        justify-content: center;
        padding: 2rem;
      }

      .pepify-container {
        text-align: center;
        color: white;
        max-width: 800px;
      }

      .pepify-logo {
        width: 120px;
        height: 120px;
        background: rgba(255, 255, 255, 0.1);
        border-radius: 50%;
        display: flex;
        align-items: center;
        justify-content: center;
        margin: 0 auto 2rem;
        backdrop-filter: blur(10px);
        border: 2px solid rgba(255, 255, 255, 0.2);
      }

      .pepify-title {
        font-size: 4rem;
        font-weight: 700;
        margin: 0 0 1rem 0;
        background: linear-gradient(45deg, #fff, #e0e7ff);
        -webkit-background-clip: text;
        -webkit-text-fill-color: transparent;
        background-clip: text;
      }

      .pepify-tagline {
        font-size: 1.5rem;
        font-weight: 500;
        margin: 0 0 2rem 0;
        opacity: 0.9;
      }

      .pepify-description {
        font-size: 1.1rem;
        line-height: 1.6;
        opacity: 0.8;
        margin: 0 0 3rem 0;
      }

      .pepify-features {
        display: grid;
        grid-template-columns: repeat(auto-fit, minmax(250px, 1fr));
        gap: 2rem;
        margin: 3rem 0;
      }

      .feature-card {
        background: rgba(255, 255, 255, 0.1);
        padding: 2rem;
        border-radius: 16px;
        backdrop-filter: blur(10px);
        border: 1px solid rgba(255, 255, 255, 0.2);
      }

      .feature-icon {
        font-size: 3rem;
        margin-bottom: 1rem;
      }

      .feature-title {
        font-size: 1.25rem;
        font-weight: 600;
        margin-bottom: 0.5rem;
      }

      .pepify-cta {
        background: rgba(255, 255, 255, 0.2);
        color: white;
        border: none;
        padding: 1rem 2rem;
        font-size: 1.1rem;
        font-weight: 600;
        border-radius: 50px;
        cursor: pointer;
        transition: all 0.3s ease;
        backdrop-filter: blur(10px);
      }

      .pepify-cta:hover {
        background: rgba(255, 255, 255, 0.3);
        transform: translateY(-2px);
      }

      @media (max-width: 768px) {
        .pepify-title {
          font-size: 2.5rem;
        }
        .pepify-tagline {
          font-size: 1.2rem;
        }
        .pepify-features {
          grid-template-columns: 1fr;
        }
      }
    </style>

    <div class="pepify-landing">
      <div class="pepify-container">
        <div class="pepify-logo">
          <svg width="60" height="60" viewBox="0 0 24 24" fill="white">
            <path d="M12 2L2 7v10c0 5.55 3.84 9.74 9 11 5.16-1.26 9-5.45 9-11V7l-10-5z"/>
          </svg>
        </div>
        
        <h1 class="pepify-title">Pepify</h1>
        <p class="pepify-tagline">AI-Powered PEP Screening from India to Beyond</p>
        <p class="pepify-description">
          Advanced compliance screening solution powered by artificial intelligence for comprehensive 
          risk assessment and regulatory compliance across global markets.
        </p>
        
        <div class="pepify-features">
          <div class="feature-card">
            <div class="feature-icon">🤖</div>
            <div class="feature-title">AI-Powered</div>
            <div>Advanced machine learning algorithms for accurate screening</div>
          </div>
          <div class="feature-card">
            <div class="feature-icon">🌍</div>
            <div class="feature-title">Global Coverage</div>
            <div>From India to international markets with comprehensive data</div>
          </div>
          <div class="feature-card">
            <div class="feature-icon">⚡</div>
            <div class="feature-title">Real-time</div>
            <div>Instant screening results with continuous monitoring</div>
          </div>
          <div class="feature-card">
            <div class="feature-icon">🛡️</div>
            <div class="feature-title">Compliance</div>
            <div>Meet regulatory requirements with confidence</div>
          </div>
        </div>
        
        <div style="display: flex; gap: 1rem; justify-content: center; flex-wrap: wrap;">
          <button class="pepify-cta" (click)="navigateToLogin()">
            Login to Dashboard
          </button>
          <button class="pepify-cta" (click)="navigateToSignup()" style="background: rgba(255, 255, 255, 0.1);">
            Sign Up
          </button>
        </div>
      </div>
    </div>
  `
})
export class HomeComponent {
  constructor(private router: Router) {}

  navigateToLogin() {
    this.router.navigate(['/login']);
  }

  navigateToSignup() {
    this.router.navigate(['/signup']);
  }

  navigateToDashboard() {
    this.router.navigate(['/dashboard']);
  }
}