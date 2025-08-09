# PEP Scanner API - Comprehensive Screening Solution for Indian Banks

A comprehensive Politically Exposed Person (PEP) and sanctions screening tool designed specifically for Indian banks to comply with RBI, FIU-IND, and SEBI regulations.

## 🎯 Overview

This system provides robust screening capabilities for:
- **PEP Detection**: Identify politically exposed persons and their associates
- **Sanctions Screening**: Check against global sanctions lists (OFAC, UN, EU, RBI)
- **Adverse Media Monitoring**: Screen for negative media coverage
- **Real-time & Batch Processing**: Support for both immediate and bulk screening
- **Multilingual Support**: Handle Indian names and scripts (Devanagari, etc.)
- **Compliance Reporting**: Generate reports for regulatory authorities

## 🏗️ Architecture

### Technology Stack
- **Backend**: .NET 8 Web API with C#
- **Database**: SQLite (development) / SQL Server (production)
- **Authentication**: JWT Bearer tokens
- **Background Jobs**: Hangfire with SQLite storage
- **Logging**: Serilog with file and console sinks
- **Name Matching**: FuzzySharp for advanced fuzzy matching algorithms
- **Documentation**: Swagger/OpenAPI

### Core Components
- **Name Matching Service**: Advanced fuzzy matching with transliteration support
- **Screening Service**: Orchestrates screening operations
- **Generic Watchlist Service Architecture**: Extensible framework for onboarding new watchlist sources
- **Alert Management**: Comprehensive alert workflow
- **Audit Logging**: Complete audit trail for compliance
- **Batch Processing**: Scalable batch screening capabilities

## 🚀 Features

### 1. Advanced Name Matching
- **Multiple Algorithms**: Levenshtein, Jaro-Winkler, Soundex, Metaphone
- **Indian Script Support**: Devanagari to Latin transliteration
- **Phonetic Variations**: Handle name pronunciation differences
- **Fuzzy Matching**: Configurable similarity thresholds

### 2. Comprehensive Data Sources
- **Global Sanctions**: OFAC, UN, EU, UK, Australia
- **Local Lists**: RBI (with web scraping), SEBI (with web scraping), Indian Parliament (with web scraping)
- **PEP Databases**: Government officials, state enterprise executives, Members of Parliament
- **Adverse Media**: Negative news and media coverage
- **In-House Lists**: Bank's internal watchlists (CSV, Excel, JSON)

### 3. Real-time Screening
- **Customer Onboarding**: Immediate screening during account opening
- **Transaction Monitoring**: Real-time transaction screening
- **API Integration**: RESTful APIs for core banking systems
- **Webhook Support**: Asynchronous notifications

### 4. Batch Processing
- **Scheduled Jobs**: Automated periodic screening
- **File Upload**: CSV/Excel file processing
- **Progress Tracking**: Real-time job status monitoring
- **Parallel Processing**: Optimized for large datasets

### 5. Compliance & Reporting
- **EDD Workflow**: Enhanced Due Diligence automation
- **STR/SAR Generation**: Suspicious Transaction/Activity Reports
- **Audit Trails**: Complete activity logging
- **Regulatory Reports**: RBI and FIU-IND compliance reports

### 6. Security & Access Control
- **Role-based Access**: Compliance Officer, Manager roles
- **JWT Authentication**: Secure API access
- **Data Encryption**: At rest and in transit
- **Audit Logging**: All actions tracked and logged

### 7. Generic Watchlist Service Architecture
- **Extensible Framework**: Easy onboarding of new watchlist sources
- **Base Service Class**: Common functionality for all watchlist services
- **Service Registry**: Centralized management of all watchlist services
- **Standardized Interface**: Consistent API across all watchlist sources
- **Configuration-Driven**: Easy configuration for new sources
- **File Format Support**: CSV, Excel, JSON processing for each source

## 📋 Requirements

### System Requirements
- .NET 8.0 SDK
- SQLite (development) or SQL Server (production)
- 4GB RAM minimum (8GB recommended)
- 10GB disk space

### Development Requirements
- Visual Studio 2022 or VS Code
- Git
- Postman or similar API testing tool

## 🛠️ Installation & Setup

### 1. Clone the Repository
```bash
git clone https://github.com/your-org/pep-scanner.git
cd pep-scanner/PEPScanner.API
```

### 2. Install Dependencies
```bash
dotnet restore
```

### 3. Configure Database
The system uses SQLite by default. For production, update `appsettings.json`:

```json
{
  "ConnectionStrings": {
    "DefaultConnection": "Data Source=pep_scanner.db"
  }
}
```

### 4. Run Database Migrations
```bash
dotnet ef database update
```

### 5. Run the Application
```bash
dotnet run
```

The API will be available at:
- **API**: https://localhost:7000
- **Swagger UI**: https://localhost:7000
- **Hangfire Dashboard**: https://localhost:7000/hangfire

## 📚 API Documentation

### Authentication
All API endpoints require JWT Bearer token authentication:

```http
Authorization: Bearer <your-jwt-token>
```

### Key Endpoints

#### 1. Customer Screening
```http
POST /api/screening/customer
Content-Type: application/json

{
  "fullName": "Rajesh Kumar",
  "dateOfBirth": "1980-05-15",
  "nationality": "Indian",
  "country": "India",
  "identificationNumber": "ABCD123456",
  "identificationType": "PAN"
}
```

#### 2. Transaction Screening
```http
POST /api/screening/transaction
Content-Type: application/json

{
  "transactionId": "TXN123456",
  "amount": 50000.00,
  "transactionType": "Transfer",
  "senderName": "Rajesh Kumar",
  "beneficiaryName": "Priya Sharma",
  "sourceCountry": "India",
  "destinationCountry": "India"
}
```

#### 3. Name Search
```http
POST /api/screening/search
Content-Type: application/json

{
  "name": "Rajesh Kumar",
  "country": "India",
  "threshold": 0.7,
  "maxResults": 50
}
```

#### 4. Get Statistics
```http
GET /api/screening/statistics?startDate=2024-01-01&endDate=2024-12-31
```

#### 5. RBI Watchlist Management
```http
# Update RBI watchlist via web scraping
POST /api/watchlist/rbi/update

# Advanced RBI scraping
POST /api/watchlist/rbi/scrape-advanced

# Search RBI data by category
GET /api/watchlist/rbi/search-by-category?category=Wilful Defaulter

# Search RBI data by name
GET /api/watchlist/rbi/search?name=Rajesh Kumar

# Upload RBI file
POST /api/watchlist/rbi/upload
Content-Type: multipart/form-data
```

#### 6. SEBI Watchlist Management
```http
# Update SEBI watchlist via web scraping
POST /api/watchlist/sebi/update

# Advanced SEBI scraping
POST /api/watchlist/sebi/scrape-advanced

# Search SEBI data by category
GET /api/watchlist/sebi/search-by-category?category=Debarred Entity

# Search SEBI data by name
GET /api/watchlist/sebi/search?name=Company Name

# Upload SEBI file
POST /api/watchlist/sebi/upload
Content-Type: multipart/form-data
```

#### 7. Indian Parliament Watchlist Management
```http
# Update Indian Parliament watchlist via web scraping
POST /api/watchlist/parliament/update

# Advanced Indian Parliament scraping
POST /api/watchlist/parliament/scrape-advanced

# Search Indian Parliament data by category
GET /api/watchlist/parliament/search-by-category?category=Lok Sabha Member

# Search Indian Parliament data by name
GET /api/watchlist/parliament/search?name=Member Name

# Upload Indian Parliament file
POST /api/watchlist/parliament/upload
Content-Type: multipart/form-data
```

#### 8. Generic Watchlist Management
```http
# Get all available watchlist sources
GET /api/genericwatchlist/sources

# Get watchlist sources by type
GET /api/genericwatchlist/sources/by-type/Local

# Get watchlist sources by country
GET /api/genericwatchlist/sources/by-country/India

# Update a specific watchlist
POST /api/genericwatchlist/update/RBI

# Update all watchlists
POST /api/genericwatchlist/update-all

# Search across all watchlists
GET /api/genericwatchlist/search?name=John Doe

# Search a specific watchlist
GET /api/genericwatchlist/search/RBI?name=John Doe

# Upload file for a specific watchlist
POST /api/genericwatchlist/upload/SEBI
Content-Type: multipart/form-data

# Get last update timestamps
GET /api/genericwatchlist/last-updates
```

#### 9. In-House File Processing
```http
# Upload in-house watchlist file
POST /api/watchlist/inhouse/upload
Content-Type: multipart/form-data

# Validate in-house file
POST /api/watchlist/inhouse/validate
Content-Type: multipart/form-data

# Get supported formats
GET /api/watchlist/inhouse/formats
```

### Response Examples

#### Screening Result
```json
{
  "customerId": "123e4567-e89b-12d3-a456-426614174000",
  "customerName": "Rajesh Kumar",
  "hasMatches": true,
  "riskScore": 75,
  "riskLevel": "High",
  "requiresEdd": true,
  "requiresStr": false,
  "requiresSar": false,
  "alerts": [
    {
      "id": "456e7890-e89b-12d3-a456-426614174001",
      "alertType": "PEP",
      "similarityScore": 0.85,
      "matchAlgorithm": "JaroWinkler",
      "status": "Open",
      "priority": "High",
      "riskLevel": "High",
      "sourceList": "UN",
      "matchedFields": "Name: Overall: 0.850, JaroWinkler: 0.850"
    }
  ],
  "processingTime": "00:00:00.150"
}
```

## 🔧 Configuration

### RBI Web Scraping Configuration
The system includes advanced web scraping capabilities for RBI watchlist data:

```json
{
  "RbiScraping": {
    "BaseUrl": "https://www.rbi.org.in",
    "WilfulDefaultersUrl": "https://www.rbi.org.in/Scripts/bs_viewcontent.aspx?Id=2009",
    "FraudMasterUrl": "https://www.rbi.org.in/Scripts/bs_viewcontent.aspx?Id=2010",
    "CautionListUrl": "https://www.rbi.org.in/Scripts/bs_viewcontent.aspx?Id=2011",
    "DefaultersListUrl": "https://www.rbi.org.in/Scripts/bs_viewcontent.aspx?Id=2008",
    "AlertListUrl": "https://www.rbi.org.in/Scripts/bs_viewcontent.aspx?Id=2012",
    "UserAgent": "Mozilla/5.0 (Windows NT 10.0; Win64; x64) AppleWebKit/537.36",
    "RequestTimeoutSeconds": 30,
    "MaxRetries": 3
  }
}
```

**Features:**
- **Automatic Retry Logic**: Configurable retry attempts with exponential backoff
- **Rate Limiting**: Respectful scraping with configurable delays

### SEBI Web Scraping Configuration
The system includes advanced web scraping capabilities for SEBI watchlist data:

```json
{
  "SebiScraping": {
    "BaseUrl": "https://www.sebi.gov.in",
    "DebarredEntitiesUrl": "https://www.sebi.gov.in/sebiweb/other/OtherAction.do?doListing=yes&sid=1&ssid=1&smid=1",
    "DefaultersUrl": "https://www.sebi.gov.in/sebiweb/other/OtherAction.do?doListing=yes&sid=1&ssid=2&smid=1",
    "SuspendedEntitiesUrl": "https://www.sebi.gov.in/sebiweb/other/OtherAction.do?doListing=yes&sid=1&ssid=3&smid=1",
    "PenaltiesUrl": "https://www.sebi.gov.in/sebiweb/other/OtherAction.do?doListing=yes&sid=1&ssid=4&smid=1",
    "UserAgent": "Mozilla/5.0 (Windows NT 10.0; Win64; x64) AppleWebKit/537.36",
    "RequestTimeoutSeconds": 30,
    "MaxRetries": 3
  }
}
```

**Features:**
- **Regulatory Lists**: Debarred entities, defaulters, suspended entities, penalties
- **Risk Assessment**: Automatic risk level assignment based on category
- **Data Quality**: Comprehensive data validation and cleaning
- **Compliance Focus**: SEBI-specific regulatory compliance

### Indian Parliament Web Scraping Configuration
The system includes advanced web scraping capabilities for Indian Parliament data:

```json
{
  "ParliamentScraping": {
    "BaseUrl": "https://sansad.in",
    "LokSabhaMembersUrl": "https://sansad.in/ls/members",
    "RajyaSabhaMembersUrl": "https://sansad.in/rs/members",
    "MinistersUrl": "https://sansad.in/ministers",
    "CommitteesUrl": "https://sansad.in/committees",
    "UserAgent": "Mozilla/5.0 (Windows NT 10.0; Win64; x64) AppleWebKit/537.36",
    "RequestTimeoutSeconds": 30,
    "MaxRetries": 3
  }
}
```

**Features:**
- **PEP Detection**: Members of Parliament, Ministers, Committee members
- **Political Positions**: Current and former political office holders
- **Constituency Data**: Electoral constituency and party information
- **Risk Categorization**: High risk for Ministers, Medium for MPs
- **Error Handling**: Comprehensive error logging and recovery
- **Multiple Data Sources**: Scrapes various RBI lists (Wilful Defaulters, Fraud Master, etc.)
- **HTML Parsing**: Advanced table parsing with HtmlAgilityPack

### appsettings.json
```json
{
  "ConnectionStrings": {
    "DefaultConnection": "Data Source=pep_scanner.db"
  },
  "Jwt": {
    "Authority": "https://your-auth-server.com",
    "Audience": "pep-scanner-api"
  },
  "Serilog": {
    "MinimumLevel": {
      "Default": "Information",
      "Override": {
        "Microsoft": "Warning",
        "System": "Warning"
      }
    }
  },
  "Screening": {
    "DefaultThreshold": 0.7,
    "BatchSize": 50,
    "MaxConcurrentJobs": 5
  }
}
```

## 🧪 Testing

### Unit Tests
```bash
dotnet test
```

### API Testing with Swagger
1. Open https://localhost:7000
2. Use the interactive Swagger UI
3. Test endpoints with sample data

### RBI Scraping Testing
```http
# Test RBI scraping functionality
GET /api/test/rbi-scraping

# Test RBI search functionality
GET /api/test/rbi-search?name=test
```

**Note**: RBI scraping requires internet connectivity and may be subject to rate limiting. The system includes automatic retry logic and error handling.

### SEBI Scraping Testing
```http
# Test SEBI scraping functionality
GET /api/test/sebi-scraping

# Test SEBI search functionality
GET /api/test/sebi-search?name=test
```

**Note**: SEBI scraping requires internet connectivity and may be subject to rate limiting. The system includes automatic retry logic and error handling.

### Indian Parliament Scraping Testing
```http
# Test Indian Parliament scraping functionality
GET /api/test/parliament-scraping

# Test Indian Parliament search functionality
GET /api/test/parliament-search?name=test
```

**Note**: Indian Parliament scraping requires internet connectivity and may be subject to rate limiting. The system includes automatic retry logic and error handling.

### Load Testing
```bash
# Install NBomber
dotnet tool install -g NBomber

# Run load test
nbomber run LoadTests/ScreeningLoadTest.cs
```

## 📊 Monitoring & Logging

### Log Files
- Location: `logs/pep-scanner-YYYY-MM-DD.txt`
- Rotation: Daily
- Retention: 30 days

### Hangfire Dashboard
- URL: https://localhost:7000/hangfire
- Monitor background jobs
- View job history and statistics

### Health Check
```http
GET /health
```

## 🔒 Security Considerations

### Data Protection
- All PII data encrypted at rest
- HTTPS/TLS for all communications
- JWT tokens with appropriate expiration
- Role-based access control

### Compliance
- RBI KYC Master Directions compliance
- FIU-IND reporting capabilities
- SEBI requirements adherence
- Data retention policies

## 🚀 Deployment

### Docker Deployment
```dockerfile
FROM mcr.microsoft.com/dotnet/aspnet:8.0 AS base
WORKDIR /app
EXPOSE 80
EXPOSE 443

FROM mcr.microsoft.com/dotnet/sdk:8.0 AS build
WORKDIR /src
COPY ["PEPScanner.API/PEPScanner.API.csproj", "PEPScanner.API/"]
RUN dotnet restore "PEPScanner.API/PEPScanner.API.csproj"
COPY . .
WORKDIR "/src/PEPScanner.API"
RUN dotnet build "PEPScanner.API.csproj" -c Release -o /app/build

FROM build AS publish
RUN dotnet publish "PEPScanner.API.csproj" -c Release -o /app/publish

FROM base AS final
WORKDIR /app
COPY --from=publish /app/publish .
ENTRYPOINT ["dotnet", "PEPScanner.API.dll"]
```

### Production Checklist
- [ ] Update connection string to production database
- [ ] Configure JWT authentication
- [ ] Set up SSL certificates
- [ ] Configure logging and monitoring
- [ ] Set up backup and disaster recovery
- [ ] Configure firewall rules
- [ ] Set up CI/CD pipeline

## 🤝 Contributing

1. Fork the repository
2. Create a feature branch
3. Make your changes
4. Add tests
5. Submit a pull request

## 📄 License

This project is licensed under the MIT License - see the [LICENSE](LICENSE) file for details.

## 🆘 Support

For support and questions:
- Email: support@pepscanner.com
- Documentation: https://docs.pepscanner.com
- Issues: https://github.com/your-org/pep-scanner/issues

## 🔄 Version History

- **v1.0.0** - Initial release with core screening capabilities
- **v1.1.0** - Added batch processing and Hangfire integration
- **v1.2.0** - Enhanced name matching and transliteration support
- **v1.3.0** - Added comprehensive audit logging and reporting

---

**Note**: This system is designed for Indian banks and complies with RBI, FIU-IND, and SEBI regulations. Ensure proper compliance review before production deployment.
