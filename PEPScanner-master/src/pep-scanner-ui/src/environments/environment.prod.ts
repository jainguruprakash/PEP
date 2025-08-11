export const environment = {
  production: true,
  apiBaseUrl: 'https://api.pepify.com',
  appName: 'Pepify',
  version: '1.0.0',
  features: {
    enableAnalytics: true,
    enableErrorReporting: true,
    enablePerformanceMonitoring: true,
    enableAdvancedScreening: true,
    enableAIRiskScoring: true,
    enableBulkProcessing: true,
    enableRealTimeAlerts: true,
    enableComplianceReporting: true
  },
  security: {
    tokenExpirationMinutes: 60,
    refreshTokenExpirationDays: 7,
    maxLoginAttempts: 5,
    sessionTimeoutMinutes: 30
  },
  screening: {
    defaultThreshold: 75,
    maxBulkSize: 10000,
    timeoutSeconds: 30,
    enableFuzzyMatching: true,
    enableAIEnhancement: true
  },
  compliance: {
    autoGenerateReports: true,
    enableAuditTrail: true,
    retentionPeriodYears: 7,
    enableRegulatorySubmission: true
  }
};