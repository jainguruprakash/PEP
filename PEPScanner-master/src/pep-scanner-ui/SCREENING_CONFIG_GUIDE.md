# Screening Configuration Guide

## Overview
The PEP Scanner now supports fully configurable screening options. You can easily add new data sources, matching algorithms, filters, and other screening parameters without modifying the core component code.

## How It Works

### 1. Configuration Service
All screening options are managed through `ScreeningConfigService` located at:
```
src/app/services/screening-config.service.ts
```

### 2. Adding New Options
To add a new screening option, simply call the service method:

```typescript
// Inject the service
constructor(private screeningConfigService: ScreeningConfigService) {}

// Add a new option
this.screeningConfigService.addScreeningOption({
  fieldName: 'newOption',           // Unique field name
  label: 'New Screening Option',    // Display label
  type: 'toggle',                   // Field type
  payloadKey: 'newOptionEnabled',   // Key in API payload
  category: 'advanced',             // Category for grouping
  order: 15,                        // Display order
  defaultValue: false,              // Default value
  description: 'Enable new screening feature'
});
```

## Supported Field Types

### 1. Toggle Switch
```typescript
{
  fieldName: 'enableFeature',
  label: 'Enable Feature',
  type: 'toggle',
  payloadKey: 'featureEnabled',
  defaultValue: false
}
```

### 2. Checkbox Group (Multiple Selection)
```typescript
{
  fieldName: 'dataSources',
  label: 'Data Sources',
  type: 'checkbox-group',
  payloadKey: 'sources',
  options: [
    { value: 'SOURCE1', label: 'Source 1', selected: true },
    { value: 'SOURCE2', label: 'Source 2', selected: false }
  ]
}
```

### 3. Select Dropdown (Single Selection)
```typescript
{
  fieldName: 'algorithm',
  label: 'Matching Algorithm',
  type: 'select',
  payloadKey: 'matchingAlgorithm',
  defaultValue: 'standard',
  options: [
    { value: 'basic', label: 'Basic Match' },
    { value: 'standard', label: 'Standard Match' },
    { value: 'advanced', label: 'Advanced Match' }
  ]
}
```

### 4. Radio Group (Single Selection)
```typescript
{
  fieldName: 'riskLevel',
  label: 'Risk Assessment Level',
  type: 'radio-group',
  payloadKey: 'riskAssessmentLevel',
  defaultValue: 'medium',
  options: [
    { value: 'low', label: 'Low Risk Assessment' },
    { value: 'medium', label: 'Medium Risk Assessment' },
    { value: 'high', label: 'High Risk Assessment' }
  ]
}
```

### 5. Slider
```typescript
{
  fieldName: 'threshold',
  label: 'Match Threshold',
  type: 'slider',
  payloadKey: 'matchThreshold',
  defaultValue: 0.8
}
```

## Categories
Options are organized into categories:

- **sources**: Data sources and watchlists
- **matching**: Matching algorithms and thresholds
- **scope**: Search scope and coverage
- **risk**: Risk assessment options
- **filters**: Geographic and date filters
- **advanced**: Performance and technical options

## Real-World Examples

### Adding a New Regulatory Database
```typescript
// Add FINRA as a new data source
this.screeningConfigService.addScreeningOption({
  fieldName: 'finraSource',
  label: 'FINRA Database',
  type: 'checkbox-group',
  payloadKey: 'sources',
  category: 'sources',
  order: 16,
  description: 'Financial Industry Regulatory Authority database',
  options: [
    { 
      value: 'FINRA_BROKERS', 
      label: 'FINRA Broker Check', 
      selected: false,
      description: 'Registered brokers and investment advisors'
    },
    { 
      value: 'FINRA_DISCIPLINARY', 
      label: 'FINRA Disciplinary Actions', 
      selected: false,
      description: 'Disciplinary actions and sanctions'
    }
  ]
});
```

### Adding Performance Options
```typescript
// Add caching options
this.screeningConfigService.addScreeningOption({
  fieldName: 'cacheSettings',
  label: 'Cache Settings',
  type: 'select',
  payloadKey: 'cacheMode',
  category: 'advanced',
  order: 20,
  defaultValue: 'smart',
  description: 'Configure result caching behavior',
  options: [
    { value: 'none', label: 'No Caching' },
    { value: 'basic', label: 'Basic Caching (1 hour)' },
    { value: 'smart', label: 'Smart Caching (24 hours)' },
    { value: 'aggressive', label: 'Aggressive Caching (7 days)' }
  ]
});
```

### Adding Geographic Filters
```typescript
// Add specific country filters
this.screeningConfigService.addScreeningOption({
  fieldName: 'countryFilters',
  label: 'Country Filters',
  type: 'checkbox-group',
  payloadKey: 'includeCountries',
  category: 'filters',
  order: 18,
  description: 'Filter results by specific countries',
  options: [
    { value: 'US', label: 'United States', selected: false },
    { value: 'UK', label: 'United Kingdom', selected: false },
    { value: 'IN', label: 'India', selected: false },
    { value: 'CN', label: 'China', selected: false },
    { value: 'RU', label: 'Russia', selected: false }
  ]
});
```

## API Payload Generation

The system automatically builds the API payload based on your configuration:

```typescript
// Form values from UI
const formValues = {
  fullName: 'John Doe',
  sources: [true, false, true],      // Checkbox selections
  algorithm: 'advanced',             // Select value
  enableFeature: true,               // Toggle value
  threshold: 0.85                    // Slider value
};

// Automatically generated payload
const payload = this.screeningConfigService.buildApiPayload(formValues, {
  fullName: formValues.fullName
});

// Result:
{
  "fullName": "John Doe",
  "sources": ["OFAC", "EU"],         // Only selected sources
  "matchingAlgorithm": "advanced",   // Selected algorithm
  "featureEnabled": true,            // Toggle state
  "matchThreshold": 0.85             // Slider value
}
```

## Configuration Management

### Export Configuration
```typescript
// Export current configuration to JSON
const config = this.screeningConfigService.getScreeningConfig();
const json = JSON.stringify(config, null, 2);
// Save to file...
```

### Import Configuration
```typescript
// Load configuration from JSON
const newConfig = JSON.parse(jsonString);
newConfig.forEach(config => {
  this.screeningConfigService.addScreeningOption(config);
});
```

### Update Existing Options
```typescript
// Update an existing option
this.screeningConfigService.updateScreeningOption('sources', {
  description: 'Updated description for data sources'
});
```

### Remove Options
```typescript
// Remove an option
this.screeningConfigService.removeScreeningOption('oldFieldName');
```

## Benefits

1. **Easy Extension**: Add new options without touching component code
2. **Automatic UI**: New options automatically appear in the UI
3. **Automatic API**: Payload generation handles all configured options
4. **Categorization**: Options are automatically grouped by category
5. **Validation**: Built-in validation and type checking
6. **Backward Compatibility**: Existing code continues to work

## Future Enhancements

The system is designed to support:
- Database-driven configuration
- User-specific option sets
- Role-based option visibility
- Dynamic option loading from external APIs
- A/B testing of different option sets

## Quick Start

To add a new screening option right now:

1. Open the browser console
2. Run this code:
```javascript
// Add a new compliance option
window.screeningConfigService.addScreeningOption({
  fieldName: 'complianceMode',
  label: 'Compliance Mode',
  type: 'select',
  payloadKey: 'complianceLevel',
  category: 'advanced',
  order: 25,
  defaultValue: 'standard',
  description: 'Select compliance checking level',
  options: [
    { value: 'basic', label: 'Basic Compliance' },
    { value: 'standard', label: 'Standard Compliance' },
    { value: 'strict', label: 'Strict Compliance' }
  ]
});
```

3. Refresh the page to see your new option in the UI
4. The option will automatically be included in API calls

That's it! The screening system is now fully configurable and extensible.
