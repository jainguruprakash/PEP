# Customer Screening Search Options Fix

## Issue Description
The customer screening page was not properly handling search combinations with various options, particularly when RBI or other specific sources were selected or deselected. The search functionality was not filtering results based on the selected watchlist sources.

## Root Cause Analysis
1. **Backend Controller Issue**: The `ScreeningController.cs` was not processing the `Sources` parameter from the frontend request
2. **Source Filtering Logic**: The watchlist query was not applying source-based filtering
3. **Form Array Handling**: The frontend form array for sources wasn't properly mapped to the backend request
4. **Threshold Handling**: The matching threshold was hardcoded instead of using the user-selected value

## Changes Made

### Backend Changes (`ScreeningController.cs`)

1. **Enhanced CustomerScreeningRequest Model**:
   ```csharp
   public class CustomerScreeningRequest
   {
       // ... existing properties
       public int? Threshold { get; set; } = 70;
       public List<string>? Sources { get; set; }
       public bool? IncludeAliases { get; set; } = true;
       public bool? IncludeFuzzyMatching { get; set; } = true;
       public bool? IncludePhoneticMatching { get; set; } = true;
   }
   ```

2. **Improved Source Filtering**:
   ```csharp
   var query = _context.WatchlistEntries.Where(w => w.IsActive);
   
   // Apply source filtering if sources are specified
   if (request.Sources != null && request.Sources.Any())
   {
       query = query.Where(w => request.Sources.Contains(w.Source));
   }
   ```

3. **Dynamic Threshold Handling**:
   ```csharp
   var threshold = (request.Threshold ?? 70) / 100.0;
   var matchScore = CalculateMatchScore(searchName, match.PrimaryName, match.AlternateNames, request);
   if (matchScore >= threshold) // Use dynamic threshold
   ```

4. **Enhanced Match Scoring**:
   - Added support for fuzzy matching toggle
   - Added basic phonetic matching capability
   - Improved risk scoring based on source criticality

### Frontend Changes (`customer-screening.component.ts` & `.html`)

1. **Fixed Form Array Initialization**:
   ```typescript
   private initializeFormArrays() {
     const sourcesArray = this.singleScreeningForm.get('sources') as FormArray;
     sourcesArray.clear(); // Clear any existing controls
     this.availableSources.forEach(source => {
       sourcesArray.push(this.fb.control(source.selected));
     });
   }
   ```

2. **Improved Source Selection Handling**:
   ```typescript
   private getSelectedOptions(selections: any[] | null | undefined, options: any[]): string[] {
     if (!selections || !Array.isArray(selections)) {
       // If no selections, return all default selected sources
       return options.filter(option => option.selected).map(option => option.value);
     }
     return options
       .filter((_, index) => selections[index] === true)
       .map(option => option.value);
   }
   ```

3. **Enhanced HTML Template**:
   - Added proper `formArrayName="sources"` binding
   - Added help text for source selection
   - Improved styling for better user experience

## Testing

### Integration Tests
Created `ScreeningControllerTests.cs` with test cases for:
- RBI source filtering
- Multiple source selection
- Threshold variations
- Default behavior when no sources specified

### Unit Tests
Created `ScreeningServiceTests.cs` with test cases for:
- Customer screening with matches
- Alert creation
- Source-based filtering

## Usage Examples

### Search with RBI Only
```json
{
  "fullName": "Amit Shah",
  "sources": ["RBI"],
  "threshold": 70,
  "includeAliases": true,
  "includeFuzzyMatching": true,
  "includePhoneticMatching": false
}
```

### Search with Multiple Sources
```json
{
  "fullName": "John Doe",
  "sources": ["OFAC", "UN", "EU"],
  "threshold": 80,
  "includeAliases": true,
  "includeFuzzyMatching": true,
  "includePhoneticMatching": true
}
```

### Search All Sources (Default)
```json
{
  "fullName": "Jane Smith",
  "threshold": 75,
  "includeAliases": true,
  "includeFuzzyMatching": true
}
```

## Benefits

1. **Accurate Filtering**: Users can now select specific watchlist sources and get filtered results
2. **Flexible Thresholds**: Dynamic threshold adjustment for precision vs. recall trade-off
3. **Enhanced Matching**: Support for fuzzy and phonetic matching options
4. **Better Performance**: Reduced database queries by filtering at the source level
5. **Improved UX**: Clear indication of which sources are being searched

## Future Enhancements

1. **Advanced Phonetic Matching**: Implement proper Soundex or Metaphone algorithms
2. **Machine Learning Scoring**: Use ML models for better match scoring
3. **Real-time Source Status**: Show which sources are currently available/updated
4. **Batch Processing**: Support for bulk customer screening with source selection
5. **Custom Source Weights**: Allow users to assign different weights to different sources

## Configuration

The available sources are configured in the frontend component:
```typescript
availableSources = [
  { value: 'OFAC', label: 'OFAC (US Treasury)', selected: true },
  { value: 'UN', label: 'UN Sanctions', selected: true },
  { value: 'EU', label: 'EU Sanctions', selected: true },
  { value: 'RBI', label: 'RBI (India)', selected: true },
  { value: 'FIU-IND', label: 'FIU-IND', selected: true },
  { value: 'SEBI', label: 'SEBI (India)', selected: true },
  { value: 'LOCAL', label: 'Local Lists', selected: false }
];
```

This configuration can be moved to a service or configuration file for better maintainability.