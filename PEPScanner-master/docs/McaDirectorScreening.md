# MCA Director Screening Integration

## Overview
Integration of Ministry of Corporate Affairs (MCA) director data into the customer screening process to identify company directors and their associated risks.

## Features Implemented

### 1. **MCA Service Integration**
- Director search by name, DIN, CIN, or company name
- Company director listing
- Director details retrieval

### 2. **Enhanced Screening Process**
- Automatic MCA director screening during customer screening
- Director information included in screening results
- Risk assessment for corporate directors

### 3. **Director Data Points**
- **DIN**: Director Identification Number
- **CIN**: Company Identification Number
- **Company Name**: Associated company
- **Designation**: Director role (Managing Director, Director, etc.)
- **Appointment Date**: When director was appointed
- **Status**: Active/Inactive status
- **Risk Level**: Calculated risk assessment

## API Endpoints

### 1. Search Directors
```http
POST /api/mca/directors/search
Content-Type: application/json

{
  "name": "Amit Shah",
  "cin": "U12345MH2020PTC123456",
  "companyName": "ABC Private Limited",
  "din": "00123456"
}
```

### 2. Get Company Directors
```http
GET /api/mca/company/{cin}/directors
```

### 3. Get Director Details
```http
GET /api/mca/director/{din}
```

## Integration with Screening

### Enhanced Customer Screening
When screening a customer, the system now:

1. **Searches Regular Watchlists**: OFAC, UN, EU, RBI, SEBI, etc.
2. **Searches MCA Directors**: Checks if customer is a company director
3. **Combines Results**: Merges watchlist and director matches
4. **Risk Assessment**: Calculates combined risk score

### Director Match Information
```json
{
  "id": "00123456",
  "matchedName": "Amit Shah",
  "alternateNames": "ABC Private Limited",
  "source": "MCA",
  "listType": "Director",
  "country": "India",
  "positionOrRole": "Managing Director",
  "riskCategory": "Medium",
  "pepCategory": "Corporate Director",
  "pepPosition": "Managing Director",
  "companyName": "ABC Private Limited",
  "cin": "U12345MH2020PTC123456",
  "din": "00123456",
  "appointmentDate": "2020-01-15",
  "matchScore": 0.95
}
```

## Frontend Integration

### Source Selection
MCA is now available as a screening source:
```typescript
availableSources = [
  { value: 'OFAC', label: 'OFAC (US Treasury)', selected: true },
  { value: 'UN', label: 'UN Sanctions', selected: true },
  { value: 'EU', label: 'EU Sanctions', selected: true },
  { value: 'RBI', label: 'RBI (India)', selected: true },
  { value: 'SEBI', label: 'SEBI (India)', selected: true },
  { value: 'MCA', label: 'MCA Directors (India)', selected: true },
  { value: 'LOCAL', label: 'Local Lists', selected: false }
];
```

### Results Display
Director matches are displayed with:
- Company information
- Director designation
- Appointment details
- Risk assessment
- Special "Director" badge

## Risk Assessment for Directors

### Risk Factors
1. **Designation Level**:
   - Managing Director: Higher risk
   - Executive Director: Medium risk
   - Independent Director: Lower risk

2. **Company Status**:
   - Active companies: Standard risk
   - Struck-off companies: Higher risk
   - Under investigation: Critical risk

3. **Multiple Directorships**:
   - Single company: Lower risk
   - Multiple companies: Medium risk
   - Excessive directorships: Higher risk

### Risk Calculation
```csharp
private string CalculateDirectorRisk(McaDirector director)
{
    var riskScore = 0;
    
    // Designation risk
    riskScore += director.Designation switch
    {
        "Managing Director" => 30,
        "Executive Director" => 20,
        "Director" => 15,
        "Independent Director" => 10,
        _ => 5
    };
    
    // Company status risk
    riskScore += director.Status switch
    {
        "Active" => 0,
        "Inactive" => 20,
        "Struck Off" => 40,
        _ => 10
    };
    
    return riskScore switch
    {
        >= 40 => "High",
        >= 25 => "Medium",
        _ => "Low"
    };
}
```

## Mock Data Structure

### Sample Director Response
```json
{
  "din": "00123456",
  "name": "Amit Shah",
  "companyName": "ABC Private Limited",
  "cin": "U12345MH2020PTC123456",
  "designation": "Managing Director",
  "appointmentDate": "2020-01-15",
  "status": "Active",
  "panNumber": "ABCDE1234F",
  "address": "Mumbai, Maharashtra",
  "nationality": "Indian",
  "riskLevel": "Medium"
}
```

### Sample Company Response
```json
{
  "cin": "U12345MH2020PTC123456",
  "companyName": "ABC Private Limited",
  "status": "Active",
  "incorporationDate": "2020-01-15",
  "authorizedCapital": 10000000,
  "paidUpCapital": 5000000,
  "directors": [
    {
      "din": "00123456",
      "name": "Amit Shah",
      "designation": "Managing Director",
      "appointmentDate": "2020-01-15",
      "status": "Active",
      "riskLevel": "Medium"
    }
  ]
}
```

## Real MCA API Integration

### For Production Implementation
Replace mock data with actual MCA API calls:

```csharp
// Example MCA API integration
private async Task<List<McaDirector>> SearchMcaDirectorsAsync(string searchName)
{
    var client = new HttpClient();
    client.DefaultRequestHeaders.Add("Authorization", "Bearer " + _mcaApiKey);
    
    var request = new
    {
        name = searchName,
        limit = 10
    };
    
    var response = await client.PostAsJsonAsync(_mcaApiUrl + "/directors/search", request);
    var directors = await response.Content.ReadFromJsonAsync<List<McaDirector>>();
    
    return directors ?? new List<McaDirector>();
}
```

### MCA API Configuration
```json
{
  "McaApi": {
    "BaseUrl": "https://api.mca.gov.in/v1",
    "ApiKey": "your-mca-api-key",
    "Timeout": 30000,
    "RetryAttempts": 3
  }
}
```

## Benefits

### 1. **Comprehensive Screening**
- Identifies customers who are company directors
- Provides corporate connection information
- Enhances due diligence process

### 2. **Risk Assessment**
- Corporate governance risk evaluation
- Multiple directorship identification
- Company status-based risk scoring

### 3. **Regulatory Compliance**
- Enhanced KYC for corporate directors
- Better understanding of customer's business interests
- Improved AML compliance

### 4. **Business Intelligence**
- Corporate network mapping
- Business relationship identification
- Enhanced customer profiling

## Usage Examples

### 1. Screen Customer with Director Check
```typescript
const screeningRequest = {
  fullName: "Amit Shah",
  sources: ["OFAC", "UN", "RBI", "MCA"],
  threshold: 70,
  includeAliases: true
};

// Results will include both watchlist and director matches
```

### 2. Director-Specific Search
```typescript
const mcaRequest = {
  name: "Amit Shah",
  companyName: "ABC Private Limited"
};

mcaService.searchDirectors(mcaRequest).subscribe(directors => {
  // Process director results
});
```

### 3. Company Director Listing
```typescript
mcaService.getCompanyDirectors("U12345MH2020PTC123456").subscribe(company => {
  // Display all directors of the company
});
```

The MCA director screening integration provides comprehensive coverage of corporate directors, enhancing the overall screening effectiveness and regulatory compliance.