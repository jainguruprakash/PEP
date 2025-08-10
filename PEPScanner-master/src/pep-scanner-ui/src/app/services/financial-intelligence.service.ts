import { Injectable, inject } from '@angular/core';
import { HttpClient } from '@angular/common/http';
import { Observable } from 'rxjs';
import { environment } from '../../environments/environment';

export interface CibilCreditReport {
  pan: string;
  name: string;
  cibilScore: number;
  scoreDate: Date;
  creditHistory: CreditHistoryItem[];
  activeLoans: ActiveLoan[];
  creditCards: CreditCard[];
  paymentHistory: PaymentHistoryData;
  creditUtilization: CreditUtilizationData;
  creditInquiries: CreditInquiry[];
  publicRecords: PublicRecord[];
  riskFactors: string[];
  reportGeneratedAt: Date;
}

export interface CreditHistoryItem {
  lenderName: string;
  accountType: string;
  creditLimit: number;
  currentBalance: number;
  paymentStatus: string;
  lastUpdated: Date;
  daysOverdue: number;
}

export interface ActiveLoan {
  lenderName: string;
  loanType: string;
  loanAmount: number;
  outstandingAmount: number;
  monthlyEmi: number;
  startDate: Date;
  maturityDate: Date;
  status: string;
}

export interface CreditCard {
  bankName: string;
  cardType: string;
  creditLimit: number;
  currentBalance: number;
  availableCredit: number;
  lastPaymentDate: Date;
  minimumDue: number;
  status: string;
}

export interface PaymentHistoryData {
  totalAccounts: number;
  onTimePayments: number;
  delayedPayments: number;
  missedPayments: number;
  paymentReliability: number;
  recentHistory: PaymentHistoryItem[];
}

export interface PaymentHistoryItem {
  date: Date;
  status: string;
  daysLate: number;
  amount: number;
}

export interface CreditUtilizationData {
  totalCreditLimit: number;
  totalUtilized: number;
  utilizationPercentage: number;
  cardWiseUtilization: CardUtilization[];
}

export interface CardUtilization {
  cardName: string;
  limit: number;
  used: number;
  percentage: number;
}

export interface CreditInquiry {
  inquiryDate: Date;
  inquiryType: string;
  lenderName: string;
  purpose: string;
  amountRequested: number;
}

export interface PublicRecord {
  recordType: string;
  date: Date;
  description: string;
  amount: number;
  status: string;
  court: string;
}

export interface CrisilRating {
  companyName: string;
  cin: string;
  longTermRating: string;
  shortTermRating: string;
  outlook: string;
  ratingDate: Date;
  industryRank: number;
  financialStrength: string;
  businessRisk: string;
  managementQuality: string;
  ratingHistory: RatingHistoryItem[];
  keyRationales: string[];
  peerComparison: PeerComparisonData;
  reportGeneratedAt: Date;
}

export interface RatingHistoryItem {
  date: Date;
  rating: string;
  outlook: string;
  action: string;
  reason: string;
}

export interface PeerComparisonData {
  industry: string;
  industryAverageRating: number;
  companyRank: number;
  totalCompanies: number;
  topPeers: PeerCompany[];
}

export interface PeerCompany {
  name: string;
  rating: string;
  revenue: number;
  marketShare: number;
}

export interface McaCompanyProfile {
  cin: string;
  companyName: string;
  companyStatus: string;
  companyCategory: string;
  companySubCategory: string;
  classOfCompany: string;
  dateOfIncorporation: Date;
  registeredOffice: string;
  paidUpCapital: number;
  authorizedCapital: number;
  listingStatus: string;
  directors: DirectorInfo[];
  charges: ChargeInfo[];
  annualReturns: AnnualReturnInfo[];
  financialStatements: FinancialStatementInfo[];
  complianceStatus: ComplianceStatusInfo;
  legalProceedings: LegalProceedingInfo[];
  reportGeneratedAt: Date;
}

export interface DirectorInfo {
  name: string;
  din: string;
  designation: string;
  appointmentDate: Date;
  cessationDate?: Date;
  status: string;
  otherCompanies: string[];
}

export interface ChargeInfo {
  chargeId: string;
  creationDate: Date;
  amount: number;
  status: string;
  chargeeDetails: string;
  assetDescription: string;
}

export interface AnnualReturnInfo {
  financialYear: number;
  filingDate: Date;
  status: string;
  isDelayed: boolean;
  delayDays: number;
}

export interface FinancialStatementInfo {
  financialYear: number;
  filingDate: Date;
  revenue: number;
  profit: number;
  totalAssets: number;
  totalLiabilities: number;
  auditorName: string;
  auditorOpinion: string;
}

export interface ComplianceStatusInfo {
  annualReturnFiled: boolean;
  financialStatementFiled: boolean;
  directorKycCompleted: boolean;
  complianceScore: number;
  pendingCompliances: string[];
  violations: string[];
}

export interface LegalProceedingInfo {
  caseType: string;
  court: string;
  filingDate: Date;
  status: string;
  claimAmount: number;
  description: string;
}

export interface GstProfile {
  gstin: string;
  legalName: string;
  tradeName: string;
  registrationDate: Date;
  gstStatus: string;
  taxpayerType: string;
  businessNature: string;
  principalPlace: string;
  additionalPlaces: string[];
  filingFrequency: string;
  lastReturnFiled: Date;
  complianceRating: string;
  turnoverRange: string;
  filingHistory: FilingHistoryItem[];
  taxLiabilities: TaxLiabilityItem[];
  refundStatus: RefundStatusInfo;
  reportGeneratedAt: Date;
}

export interface FilingHistoryItem {
  returnType: string;
  period: string;
  dueDate: Date;
  filingDate?: Date;
  status: string;
  delayDays: number;
  lateFee: number;
}

export interface TaxLiabilityItem {
  taxType: string;
  period: string;
  taxDue: number;
  taxPaid: number;
  outstanding: number;
  dueDate: Date;
  status: string;
}

export interface RefundStatusInfo {
  totalRefundClaimed: number;
  refundReceived: number;
  refundPending: number;
  refundHistory: RefundItem[];
}

export interface RefundItem {
  period: string;
  amount: number;
  claimDate: Date;
  processedDate?: Date;
  status: string;
}

export interface FinancialRiskAssessment {
  customerId: string;
  customerName: string;
  assessmentDate: Date;
  overallRiskScore: number;
  creditRisk: number;
  complianceRisk: number;
  liquidityRisk: number;
  operationalRisk: number;
  riskFactors: FinancialRiskFactor[];
  recommendations: FinancialRecommendation[];
  monitoringAlerts: MonitoringAlert[];
  complianceScore: number;
  trendAnalysis: FinancialTrendAnalysis;
}

export interface FinancialRiskFactor {
  category: string;
  description: string;
  impact: number;
  severity: string;
  source: string;
}

export interface FinancialRecommendation {
  priority: string;
  action: string;
  rationale: string;
  timeline: string;
  owner: string;
}

export interface MonitoringAlert {
  type: string;
  message: string;
  severity: string;
  triggerDate: Date;
  requiresAction: boolean;
}

export interface FinancialAlert {
  id: string;
  customerId: string;
  alertType: string;
  title: string;
  description: string;
  severity: string;
  detectedAt: Date;
  source: string;
  status: string;
  metadata: { [key: string]: any };
}

export interface FinancialTrendAnalysis {
  customerId: string;
  analysisPeriod: number;
  creditScoreTrend: TrendDataPoint[];
  paymentBehaviorTrend: TrendDataPoint[];
  creditUtilizationTrend: TrendDataPoint[];
  incomeStabilityTrend: TrendDataPoint[];
  debtToIncomeRatio: TrendDataPoint[];
  financialStressSigns: string[];
  predictiveInsights: PredictiveFinancialInsights;
  recommendedActions: string[];
}

export interface TrendDataPoint {
  date: Date;
  value: number;
  label: string;
}

export interface PredictiveFinancialInsights {
  predictedCreditScore30Days: number;
  predictedCreditScore90Days: number;
  defaultProbability: number;
  riskTrend: string;
  earlyWarningSignals: string[];
  recommendedMonitoringFrequency: string;
}

@Injectable({
  providedIn: 'root'
})
export class FinancialIntelligenceService {
  private http = inject(HttpClient);
  private baseUrl = `${environment.apiBaseUrl}/financial-intelligence`;

  // CIBIL Credit Report
  getCibilReport(pan: string, name: string): Observable<any> {
    return this.http.post(`${this.baseUrl}/cibil-report`, { pan, name });
  }

  // CRISIL Rating
  getCrisilRating(companyName: string, cin?: string): Observable<any> {
    return this.http.post(`${this.baseUrl}/crisil-rating`, { companyName, cin });
  }

  // MCA Company Profile
  getMcaProfile(cin: string): Observable<any> {
    return this.http.post(`${this.baseUrl}/mca-profile`, { cin });
  }

  // GST Profile
  getGstProfile(gstin: string): Observable<any> {
    return this.http.post(`${this.baseUrl}/gst-profile`, { gstin });
  }

  // Banking Profile
  getBankingProfile(accountNumber: string, ifsc: string): Observable<any> {
    return this.http.post(`${this.baseUrl}/banking-profile`, { accountNumber, ifsc });
  }

  // Comprehensive Financial Risk Assessment
  getComprehensiveFinancialRisk(customerId: string): Observable<any> {
    return this.http.get(`${this.baseUrl}/comprehensive-risk/${customerId}`);
  }

  // Financial Anomalies
  scanFinancialAnomalies(customerId: string): Observable<any> {
    return this.http.get(`${this.baseUrl}/financial-anomalies/${customerId}`);
  }

  // Financial Trends
  getFinancialTrends(customerId: string, months: number = 12): Observable<any> {
    return this.http.get(`${this.baseUrl}/financial-trends/${customerId}?months=${months}`);
  }

  // Batch Financial Assessment
  batchFinancialAssessment(customerIds: string[]): Observable<any> {
    return this.http.post(`${this.baseUrl}/batch-financial-assessment`, { customerIds });
  }

  // Financial Dashboard
  getFinancialDashboard(customerId: string): Observable<any> {
    return this.http.get(`${this.baseUrl}/financial-dashboard/${customerId}`);
  }

  // Utility methods
  getCibilScoreColor(score: number): string {
    if (score >= 750) return '#4caf50'; // Excellent - Green
    if (score >= 650) return '#8bc34a'; // Good - Light Green
    if (score >= 550) return '#ff9800'; // Fair - Orange
    return '#f44336'; // Poor - Red
  }

  getCibilScoreLabel(score: number): string {
    if (score >= 750) return 'Excellent';
    if (score >= 650) return 'Good';
    if (score >= 550) return 'Fair';
    return 'Poor';
  }

  getCrisilRatingColor(rating: string): string {
    if (rating.startsWith('AAA') || rating.startsWith('AA')) return '#4caf50';
    if (rating.startsWith('A')) return '#8bc34a';
    if (rating.startsWith('BBB') || rating.startsWith('BB')) return '#ff9800';
    return '#f44336';
  }

  getComplianceScoreColor(score: number): string {
    if (score >= 80) return '#4caf50';
    if (score >= 60) return '#8bc34a';
    if (score >= 40) return '#ff9800';
    return '#f44336';
  }

  getFinancialRiskColor(riskScore: number): string {
    if (riskScore >= 75) return '#f44336'; // High Risk - Red
    if (riskScore >= 50) return '#ff9800'; // Medium Risk - Orange
    if (riskScore >= 25) return '#8bc34a'; // Low Risk - Light Green
    return '#4caf50'; // Very Low Risk - Green
  }

  getFinancialRiskLevel(riskScore: number): string {
    if (riskScore >= 75) return 'High Risk';
    if (riskScore >= 50) return 'Medium Risk';
    if (riskScore >= 25) return 'Low Risk';
    return 'Very Low Risk';
  }

  formatCurrency(amount: number): string {
    return new Intl.NumberFormat('en-IN', {
      style: 'currency',
      currency: 'INR',
      minimumFractionDigits: 0,
      maximumFractionDigits: 0
    }).format(amount);
  }

  formatLargeNumber(num: number): string {
    if (num >= 10000000) { // 1 crore
      return (num / 10000000).toFixed(1) + ' Cr';
    } else if (num >= 100000) { // 1 lakh
      return (num / 100000).toFixed(1) + ' L';
    } else if (num >= 1000) { // 1 thousand
      return (num / 1000).toFixed(1) + 'K';
    }
    return num.toString();
  }

  getPaymentStatusColor(status: string): string {
    switch (status.toLowerCase()) {
      case 'current':
      case 'paid':
      case 'closed': return '#4caf50';
      case 'dpd1':
      case 'dpd2': return '#ff9800';
      case 'dpd3':
      case 'dpd4':
      case 'dpd5': return '#f44336';
      default: return '#757575';
    }
  }

  getGstStatusColor(status: string): string {
    switch (status.toLowerCase()) {
      case 'active': return '#4caf50';
      case 'suspended': return '#ff9800';
      case 'cancelled': return '#f44336';
      default: return '#757575';
    }
  }

  getCompanyStatusColor(status: string): string {
    switch (status.toLowerCase()) {
      case 'active': return '#4caf50';
      case 'strike off': return '#f44336';
      case 'under liquidation': return '#ff5722';
      default: return '#757575';
    }
  }
}
