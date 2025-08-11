import { Injectable, inject } from '@angular/core';
import { HttpClient } from '@angular/common/http';
import { Observable, of } from 'rxjs';
import { environment } from '../../environments/environment';

@Injectable({ providedIn: 'root' })
export class AiSuggestionsService {
  private readonly http = inject(HttpClient);
  private readonly baseUrl = `${environment.apiBaseUrl}/ai`;

  getScreeningSuggestions(screeningData: any): Observable<any> {
    // Mock AI suggestions for demo
    const mockSuggestions = {
      recommendedAction: this.getRecommendedAction(screeningData),
      confidence: Math.random() * 0.3 + 0.7, // 70-100% confidence
      riskFactors: this.getRiskFactors(screeningData),
      similarCases: this.getSimilarCases(screeningData)
    };

    return of(mockSuggestions);
  }

  private getRecommendedAction(data: any): string {
    const name = data.fullName?.toLowerCase() || '';
    
    if (name.includes('shah') || name.includes('modi')) {
      return 'Proceed with Enhanced Due Diligence (EDD) - High-profile individual detected';
    }
    if (name.includes('patel') || name.includes('kumar')) {
      return 'Standard screening sufficient - Common name with low risk indicators';
    }
    return 'Continue with regular onboarding process - No significant risk indicators found';
  }

  private getRiskFactors(data: any): string[] {
    const factors = [];
    const name = data.fullName?.toLowerCase() || '';
    
    if (data.country === 'India') factors.push('High-risk jurisdiction');
    if (name.length < 5) factors.push('Short name - potential alias');
    if (data.threshold && data.threshold > 80) factors.push('High matching threshold');
    if (!data.dateOfBirth) factors.push('Missing date of birth');
    
    return factors.length > 0 ? factors : ['No significant risk factors identified'];
  }

  private getSimilarCases(data: any): any[] {
    return [
      {
        name: 'Similar Customer A',
        riskScore: Math.random() * 0.4 + 0.3,
        outcome: 'Approved after EDD'
      },
      {
        name: 'Similar Customer B', 
        riskScore: Math.random() * 0.3 + 0.2,
        outcome: 'Standard approval'
      }
    ];
  }
}