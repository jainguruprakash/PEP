import { Injectable } from '@angular/core';

export interface ScreeningOption {
  value: string;
  label: string;
  selected: boolean;
  description?: string;
  category?: string;
  payloadKey?: string; // Key to use in API payload
  dataType?: 'boolean' | 'string' | 'number' | 'array';
  required?: boolean;
  group?: string;
}

export interface ScreeningFieldConfig {
  fieldName: string;
  label: string;
  type: 'checkbox-group' | 'select' | 'input' | 'toggle' | 'slider' | 'radio-group';
  payloadKey: string;
  options?: ScreeningOption[];
  defaultValue?: any;
  required?: boolean;
  validation?: any;
  description?: string;
  category?: string;
  order?: number;
  visible?: boolean;
  disabled?: boolean;
}

@Injectable({
  providedIn: 'root'
})
export class ScreeningConfigService {

  // Main configuration for all screening options
  private screeningConfig: ScreeningFieldConfig[] = [
    // Data Sources Configuration
    {
      fieldName: 'sources',
      label: 'Data Sources',
      type: 'checkbox-group',
      payloadKey: 'sources',
      category: 'sources',
      order: 1,
      required: true,
      description: 'Select watchlists and databases to search',
      options: [
        { value: 'OFAC', label: 'OFAC (US Treasury)', selected: true, description: 'US Office of Foreign Assets Control' },
        { value: 'UN', label: 'UN Sanctions', selected: true, description: 'United Nations Sanctions List' },
        { value: 'EU', label: 'EU Sanctions', selected: true, description: 'European Union Sanctions' },
        { value: 'RBI', label: 'RBI (India)', selected: true, description: 'Reserve Bank of India' },
        { value: 'FIU-IND', label: 'FIU-IND', selected: true, description: 'Financial Intelligence Unit India' },
        { value: 'SEBI', label: 'SEBI (India)', selected: true, description: 'Securities and Exchange Board of India' },
        { value: 'MCA', label: 'MCA Directors (India)', selected: true, description: 'Ministry of Corporate Affairs' },
        { value: 'LOCAL', label: 'Local Lists', selected: false, description: 'Custom local watchlists' },
        { value: 'PEP', label: 'PEP Lists', selected: true, description: 'Politically Exposed Persons' },
        { value: 'INTERPOL', label: 'Interpol', selected: false, description: 'International Criminal Police Organization' },
        { value: 'ECI', label: 'Election Commission (India)', selected: true, description: 'Election Commission of India' }
      ]
    },

    // Matching Options Configuration
    {
      fieldName: 'threshold',
      label: 'Match Threshold',
      type: 'slider',
      payloadKey: 'threshold',
      category: 'matching',
      order: 2,
      defaultValue: 0.8,
      description: 'Minimum similarity score for matches (0.0 - 1.0)'
    },

    {
      fieldName: 'includeFuzzyMatching',
      label: 'Fuzzy Matching',
      type: 'toggle',
      payloadKey: 'includeFuzzyMatching',
      category: 'matching',
      order: 3,
      defaultValue: true,
      description: 'Enable approximate string matching for better results'
    },

    {
      fieldName: 'includePhoneticMatching',
      label: 'Phonetic Matching',
      type: 'toggle',
      payloadKey: 'includePhoneticMatching',
      category: 'matching',
      order: 4,
      defaultValue: false,
      description: 'Match names that sound similar'
    },

    {
      fieldName: 'includeAliasMatching',
      label: 'Alias Matching',
      type: 'toggle',
      payloadKey: 'includeAliasMatching',
      category: 'matching',
      order: 5,
      defaultValue: true,
      description: 'Search alternative names and aliases'
    },

    // Search Scope Configuration
    {
      fieldName: 'searchScope',
      label: 'Search Scope',
      type: 'radio-group',
      payloadKey: 'searchScope',
      category: 'scope',
      order: 6,
      defaultValue: 'comprehensive',
      options: [
        { value: 'basic', label: 'Basic Search', selected: false, description: 'Name-only search' },
        { value: 'standard', label: 'Standard Search', selected: false, description: 'Name + basic identifiers' },
        { value: 'comprehensive', label: 'Comprehensive Search', selected: true, description: 'All available fields' },
        { value: 'deep', label: 'Deep Search', selected: false, description: 'Extended search with relationships' }
      ]
    },

    // Risk Assessment Configuration
    {
      fieldName: 'riskAssessment',
      label: 'Risk Assessment Level',
      type: 'select',
      payloadKey: 'riskAssessmentLevel',
      category: 'risk',
      order: 7,
      defaultValue: 'standard',
      options: [
        { value: 'basic', label: 'Basic Risk Assessment', selected: false },
        { value: 'standard', label: 'Standard Risk Assessment', selected: true },
        { value: 'enhanced', label: 'Enhanced Risk Assessment', selected: false },
        { value: 'comprehensive', label: 'Comprehensive Risk Assessment', selected: false }
      ]
    },

    // Geographic Filters
    {
      fieldName: 'geographicFilters',
      label: 'Geographic Filters',
      type: 'checkbox-group',
      payloadKey: 'geographicFilters',
      category: 'filters',
      order: 8,
      required: false,
      description: 'Filter results by geographic regions',
      options: [
        { value: 'ASIA', label: 'Asia Pacific', selected: false },
        { value: 'EUROPE', label: 'Europe', selected: false },
        { value: 'AMERICAS', label: 'Americas', selected: false },
        { value: 'AFRICA', label: 'Africa', selected: false },
        { value: 'MIDDLE_EAST', label: 'Middle East', selected: false }
      ]
    },

    // Date Range Filters
    {
      fieldName: 'dateRangeFilter',
      label: 'Date Range Filter',
      type: 'toggle',
      payloadKey: 'enableDateFilter',
      category: 'filters',
      order: 9,
      defaultValue: false,
      description: 'Filter results by date ranges'
    },

    // Advanced Options
    {
      fieldName: 'maxResults',
      label: 'Maximum Results',
      type: 'select',
      payloadKey: 'maxResults',
      category: 'advanced',
      order: 10,
      defaultValue: 100,
      options: [
        { value: '50', label: '50 Results', selected: false },
        { value: '100', label: '100 Results', selected: true },
        { value: '250', label: '250 Results', selected: false },
        { value: '500', label: '500 Results', selected: false },
        { value: '1000', label: '1000 Results', selected: false }
      ]
    },

    {
      fieldName: 'enableParallelProcessing',
      label: 'Parallel Processing',
      type: 'toggle',
      payloadKey: 'enableParallelProcessing',
      category: 'advanced',
      order: 11,
      defaultValue: true,
      description: 'Process multiple sources simultaneously for faster results'
    },

    {
      fieldName: 'cacheResults',
      label: 'Cache Results',
      type: 'toggle',
      payloadKey: 'cacheResults',
      category: 'advanced',
      order: 12,
      defaultValue: true,
      description: 'Cache results for faster subsequent searches'
    }
  ];

  // Get all screening configuration
  getScreeningConfig(): ScreeningFieldConfig[] {
    return [...this.screeningConfig].sort((a, b) => (a.order || 0) - (b.order || 0));
  }

  // Get configuration by category
  getConfigByCategory(category: string): ScreeningFieldConfig[] {
    return this.screeningConfig
      .filter(config => config.category === category)
      .sort((a, b) => (a.order || 0) - (b.order || 0));
  }

  // Get all available categories
  getCategories(): string[] {
    const categories = [...new Set(this.screeningConfig.map(config => config.category).filter(Boolean))] as string[];
    return categories;
  }

  // Add new screening option
  addScreeningOption(config: ScreeningFieldConfig): void {
    this.screeningConfig.push(config);
  }

  // Update existing screening option
  updateScreeningOption(fieldName: string, updates: Partial<ScreeningFieldConfig>): void {
    const index = this.screeningConfig.findIndex(config => config.fieldName === fieldName);
    if (index >= 0) {
      this.screeningConfig[index] = { ...this.screeningConfig[index], ...updates };
    }
  }

  // Remove screening option
  removeScreeningOption(fieldName: string): void {
    this.screeningConfig = this.screeningConfig.filter(config => config.fieldName !== fieldName);
  }

  // Build API payload from form values
  buildApiPayload(formValues: any, basePayload: any = {}): any {
    const payload = { ...basePayload };

    this.screeningConfig.forEach(config => {
      const formValue = formValues[config.fieldName];

      // Skip undefined, null, empty strings, and empty arrays
      if (this.isValidValue(formValue)) {
        if (config.type === 'checkbox-group' && Array.isArray(formValue)) {
          // Handle checkbox groups - convert boolean array to selected values
          const selectedOptions = config.options
            ?.filter((_, index) => formValue[index] === true)
            .map(option => option.value) || [];

          // Always include the array, even if empty, so we know user made a selection
          payload[config.payloadKey] = selectedOptions;

          console.log(`Checkbox group ${config.fieldName}:`, {
            formValue,
            selectedOptions,
            payloadKey: config.payloadKey
          });
        } else if (config.type === 'select' && config.options) {
          // Handle select options - use the actual value
          const selectedOption = config.options.find(option => option.value === formValue);
          if (selectedOption) {
            payload[config.payloadKey] = selectedOption.value;
          }
        } else if (config.type === 'radio-group' && config.options) {
          // Handle radio groups - use the actual value
          const selectedOption = config.options.find(option => option.value === formValue);
          if (selectedOption) {
            payload[config.payloadKey] = selectedOption.value;
          }
        } else if (config.type === 'toggle') {
          // Handle toggles - only include if true or explicitly set
          if (formValue === true || formValue === false) {
            payload[config.payloadKey] = formValue;
          }
        } else if (config.type === 'slider') {
          // Handle sliders - include numeric values
          if (typeof formValue === 'number' && !isNaN(formValue)) {
            payload[config.payloadKey] = formValue;
          }
        } else {
          // Handle direct values (inputs, etc.) - only if not empty
          if (typeof formValue === 'string' && formValue.trim() !== '') {
            payload[config.payloadKey] = formValue.trim();
          } else if (typeof formValue !== 'string') {
            payload[config.payloadKey] = formValue;
          }
        }
      }
    });

    return payload;
  }

  // Helper method to check if a value is valid for inclusion in payload
  private isValidValue(value: any): boolean {
    if (value === undefined || value === null) return false;
    if (typeof value === 'string' && value.trim() === '') return false;
    if (Array.isArray(value) && value.length === 0) return false;
    return true;
  }

  // Get default form values
  getDefaultFormValues(): any {
    const defaults: any = {};

    this.screeningConfig.forEach(config => {
      if (config.type === 'checkbox-group' && config.options) {
        defaults[config.fieldName] = config.options.map(option => option.selected);
      } else if (config.defaultValue !== undefined) {
        defaults[config.fieldName] = config.defaultValue;
      }
    });

    return defaults;
  }
}
