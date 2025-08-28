// Example: How to add new screening options programmatically

import { ScreeningConfigService, ScreeningFieldConfig } from '../../services/screening-config.service';

export class AddScreeningOptionExample {
  
  constructor(private screeningConfigService: ScreeningConfigService) {}

  // Example 1: Add a new data source
  addNewDataSource() {
    const newDataSource: ScreeningFieldConfig = {
      fieldName: 'newDataSource',
      label: 'New Regulatory Database',
      type: 'checkbox-group',
      payloadKey: 'newDataSources',
      category: 'sources',
      order: 15,
      required: false,
      description: 'Select new regulatory databases to search',
      options: [
        { 
          value: 'FINRA', 
          label: 'FINRA (US)', 
          selected: false, 
          description: 'Financial Industry Regulatory Authority' 
        },
        { 
          value: 'SEC', 
          label: 'SEC (US)', 
          selected: false, 
          description: 'Securities and Exchange Commission' 
        },
        { 
          value: 'FCA', 
          label: 'FCA (UK)', 
          selected: false, 
          description: 'Financial Conduct Authority' 
        }
      ]
    };

    this.screeningConfigService.addScreeningOption(newDataSource);
  }

  // Example 2: Add a new matching algorithm option
  addMatchingAlgorithm() {
    const matchingAlgorithm: ScreeningFieldConfig = {
      fieldName: 'matchingAlgorithm',
      label: 'Matching Algorithm',
      type: 'select',
      payloadKey: 'algorithm',
      category: 'matching',
      order: 6,
      defaultValue: 'standard',
      description: 'Choose the matching algorithm to use',
      options: [
        { value: 'basic', label: 'Basic String Match', selected: false },
        { value: 'standard', label: 'Standard Fuzzy Match', selected: true },
        { value: 'advanced', label: 'Advanced ML Match', selected: false },
        { value: 'ai', label: 'AI-Powered Match', selected: false }
      ]
    };

    this.screeningConfigService.addScreeningOption(matchingAlgorithm);
  }

  // Example 3: Add a new risk scoring option
  addRiskScoring() {
    const riskScoring: ScreeningFieldConfig = {
      fieldName: 'enableRiskScoring',
      label: 'Advanced Risk Scoring',
      type: 'toggle',
      payloadKey: 'enableAdvancedRiskScoring',
      category: 'risk',
      order: 8,
      defaultValue: false,
      description: 'Enable advanced AI-based risk scoring for matches'
    };

    this.screeningConfigService.addScreeningOption(riskScoring);
  }

  // Example 4: Add geographic region filters
  addGeographicFilters() {
    const geoFilters: ScreeningFieldConfig = {
      fieldName: 'specificCountries',
      label: 'Specific Countries',
      type: 'checkbox-group',
      payloadKey: 'countryFilters',
      category: 'filters',
      order: 9,
      required: false,
      description: 'Filter results by specific countries',
      options: [
        { value: 'US', label: 'United States', selected: false },
        { value: 'UK', label: 'United Kingdom', selected: false },
        { value: 'IN', label: 'India', selected: false },
        { value: 'CN', label: 'China', selected: false },
        { value: 'RU', label: 'Russia', selected: false },
        { value: 'IR', label: 'Iran', selected: false },
        { value: 'KP', label: 'North Korea', selected: false }
      ]
    };

    this.screeningConfigService.addScreeningOption(geoFilters);
  }

  // Example 5: Add performance optimization options
  addPerformanceOptions() {
    const performanceOptions: ScreeningFieldConfig = {
      fieldName: 'performanceMode',
      label: 'Performance Mode',
      type: 'radio-group',
      payloadKey: 'performanceMode',
      category: 'advanced',
      order: 13,
      defaultValue: 'balanced',
      description: 'Choose performance vs accuracy trade-off',
      options: [
        { 
          value: 'fast', 
          label: 'Fast Mode', 
          selected: false, 
          description: 'Prioritize speed over accuracy' 
        },
        { 
          value: 'balanced', 
          label: 'Balanced Mode', 
          selected: true, 
          description: 'Balance between speed and accuracy' 
        },
        { 
          value: 'accurate', 
          label: 'Accurate Mode', 
          selected: false, 
          description: 'Prioritize accuracy over speed' 
        }
      ]
    };

    this.screeningConfigService.addScreeningOption(performanceOptions);
  }

  // Example 6: Add custom threshold for different risk categories
  addCustomThresholds() {
    const customThresholds: ScreeningFieldConfig = {
      fieldName: 'pepThreshold',
      label: 'PEP Match Threshold',
      type: 'slider',
      payloadKey: 'pepMatchThreshold',
      category: 'matching',
      order: 7,
      defaultValue: 0.7,
      description: 'Specific threshold for PEP (Politically Exposed Person) matches'
    };

    this.screeningConfigService.addScreeningOption(customThresholds);
  }

  // Example 7: Add compliance reporting options
  addComplianceOptions() {
    const complianceOptions: ScreeningFieldConfig = {
      fieldName: 'complianceReporting',
      label: 'Compliance Reporting',
      type: 'checkbox-group',
      payloadKey: 'complianceFeatures',
      category: 'advanced',
      order: 14,
      description: 'Enable compliance and reporting features',
      options: [
        { 
          value: 'auditTrail', 
          label: 'Audit Trail', 
          selected: true, 
          description: 'Maintain detailed audit logs' 
        },
        { 
          value: 'regulatoryReporting', 
          label: 'Regulatory Reporting', 
          selected: false, 
          description: 'Generate regulatory compliance reports' 
        },
        { 
          value: 'riskMetrics', 
          label: 'Risk Metrics', 
          selected: true, 
          description: 'Calculate and track risk metrics' 
        }
      ]
    };

    this.screeningConfigService.addScreeningOption(complianceOptions);
  }

  // Example usage: Add all example options
  addAllExampleOptions() {
    this.addNewDataSource();
    this.addMatchingAlgorithm();
    this.addRiskScoring();
    this.addGeographicFilters();
    this.addPerformanceOptions();
    this.addCustomThresholds();
    this.addComplianceOptions();
    
    console.log('All example screening options added successfully!');
    console.log('Updated configuration:', this.screeningConfigService.getScreeningConfig());
  }

  // Example: How to update existing options
  updateExistingOption() {
    // Update the OFAC source to include more details
    this.screeningConfigService.updateScreeningOption('sources', {
      description: 'Enhanced OFAC screening with real-time updates and additional sanctions lists'
    });
  }

  // Example: How to remove an option
  removeOption() {
    this.screeningConfigService.removeScreeningOption('oldFieldName');
  }

  // Example: How to get configuration for a specific category
  getSourcesConfiguration() {
    const sourcesConfig = this.screeningConfigService.getConfigByCategory('sources');
    console.log('Sources configuration:', sourcesConfig);
    return sourcesConfig;
  }

  // Example: How to build API payload with custom form values
  buildCustomPayload() {
    const formValues = {
      // Standard fields
      fullName: 'John Doe',
      threshold: 0.8,
      sources: [true, true, false, true], // Array of boolean values for checkbox group
      
      // Custom fields added through configuration
      newDataSource: [false, true, false], // New data sources
      matchingAlgorithm: 'advanced',
      enableRiskScoring: true,
      specificCountries: [true, false, true, false, false, false, false],
      performanceMode: 'balanced',
      pepThreshold: 0.7,
      complianceReporting: [true, false, true]
    };

    const apiPayload = this.screeningConfigService.buildApiPayload(formValues, {
      fullName: formValues.fullName
    });

    console.log('Generated API payload:', apiPayload);
    return apiPayload;
  }
}

// Usage example:
/*
// In your component or service:
const example = new AddScreeningOptionExample(this.screeningConfigService);

// Add a single new option
example.addNewDataSource();

// Add all example options
example.addAllExampleOptions();

// Build payload with new options
const payload = example.buildCustomPayload();

// The payload will automatically include all configured options:
{
  "fullName": "John Doe",
  "sources": ["OFAC", "UN", "RBI"],
  "newDataSources": ["SEC"],
  "algorithm": "advanced",
  "enableAdvancedRiskScoring": true,
  "countryFilters": ["US", "IN"],
  "performanceMode": "balanced",
  "pepMatchThreshold": 0.7,
  "complianceFeatures": ["auditTrail", "riskMetrics"]
}
*/
