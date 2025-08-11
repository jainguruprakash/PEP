# 🔌 Pepify API Documentation

## Overview
The Pepify API provides comprehensive endpoints for PEP screening, compliance management, and system administration. All endpoints use RESTful conventions and return JSON responses.

## Base URL
- **Development**: `http://localhost:5098/api`
- **Production**: `https://api.pepify.com/api`

## Authentication
All API endpoints require JWT authentication except for login and public health checks.

### Headers Required
```
Authorization: Bearer <jwt_token>
Content-Type: application/json
```

## 🔐 Authentication Endpoints

### POST /auth/login
Authenticate user and receive JWT token.

**Request:**
```json
{
  "username": "string",
  "password": "string"
}
```

**Response:**
```json
{
  "accessToken": "string",
  "refreshToken": "string",
  "tokenType": "Bearer",
  "expiresIn": 3600,
  "user": {
    "id": "guid",
    "username": "string",
    "email": "string",
    "firstName": "string",
    "lastName": "string",
    "role": "string",
    "organizationId": "guid"
  }
}
```

### POST /auth/refresh
Refresh expired JWT token.

**Request:**
```json
{
  "refreshToken": "string"
}
```

## 👥 Customer Management

### GET /customers
Retrieve all customers with pagination.

**Query Parameters:**
- `page` (int): Page number (default: 1)
- `pageSize` (int): Items per page (default: 50)
- `search` (string): Search term
- `riskLevel` (string): Filter by risk level

**Response:**
```json
{
  "data": [
    {
      "id": "guid",
      "fullName": "string",
      "email": "string",
      "phone": "string",
      "customerType": "string",
      "riskLevel": "string",
      "status": "string",
      "onboardingDate": "datetime",
      "lastScreeningDate": "datetime"
    }
  ],
  "totalCount": 0,
  "page": 1,
  "pageSize": 50
}
```

### POST /customers
Create new customer.

**Request:**
```json
{
  "fullName": "string",
  "email": "string",
  "phone": "string",
  "dateOfBirth": "date",
  "nationality": "string",
  "country": "string",
  "customerType": "Individual|Corporate",
  "identificationNumber": "string",
  "identificationType": "string"
}
```

### GET /customers/{id}
Get customer by ID.

### PUT /customers/{id}
Update customer information.

### DELETE /customers/{id}
Soft delete customer.

## 🔍 Screening Endpoints

### POST /screening/customer
Screen individual customer.

**Request:**
```json
{
  "customerId": "guid",
  "fullName": "string",
  "dateOfBirth": "date",
  "nationality": "string",
  "threshold": 75,
  "databases": ["PEP", "Sanctions", "AdverseMedia"]
}
```

**Response:**
```json
{
  "screeningId": "guid",
  "customerId": "guid",
  "screeningDate": "datetime",
  "overallRiskLevel": "Low|Medium|High|Critical",
  "totalMatches": 0,
  "highRiskMatches": 0,
  "results": [
    {
      "id": "guid",
      "database": "string",
      "matchedName": "string",
      "similarityScore": 85.5,
      "riskLevel": "string",
      "category": "string",
      "source": "string",
      "details": "object"
    }
  ],
  "recommendedAction": "string"
}
```

### POST /screening/bulk
Bulk screen multiple customers.

**Request:**
```json
{
  "customers": [
    {
      "customerId": "guid",
      "fullName": "string",
      "dateOfBirth": "date",
      "nationality": "string"
    }
  ],
  "threshold": 75,
  "databases": ["PEP", "Sanctions", "AdverseMedia"]
}
```

## 🚨 Alert Management

### GET /alerts
Retrieve alerts with filtering.

**Query Parameters:**
- `status` (string): Alert status
- `workflowStatus` (string): Workflow status
- `priority` (string): Priority level
- `assignedTo` (string): Assigned user ID
- `page` (int): Page number
- `pageSize` (int): Items per page

**Response:**
```json
{
  "data": [
    {
      "id": "guid",
      "alertType": "string",
      "status": "string",
      "workflowStatus": "string",
      "priority": "string",
      "riskLevel": "string",
      "customerId": "guid",
      "customerName": "string",
      "createdAtUtc": "datetime",
      "dueDate": "datetime",
      "assignedTo": "string",
      "similarityScore": 85.5
    }
  ],
  "totalCount": 0
}
```

### GET /alerts/{id}
Get detailed alert information.

### POST /alerts/{id}/approve
Approve an alert.

**Request:**
```json
{
  "approvedBy": "string",
  "comments": "string",
  "outcome": "string"
}
```

### POST /alerts/{id}/reject
Reject an alert.

**Request:**
```json
{
  "rejectedBy": "string",
  "reason": "string"
}
```

### POST /alerts/{id}/assign
Assign alert to user.

**Request:**
```json
{
  "assignedTo": "string",
  "assignedBy": "string",
  "comments": "string"
}
```

## 📊 Dashboard & Analytics

### GET /dashboard/overview
Get dashboard overview data.

**Response:**
```json
{
  "totalCustomers": 0,
  "totalAlerts": 0,
  "pendingAlerts": 0,
  "highRiskAlerts": 0,
  "totalSars": 0,
  "totalStrs": 0,
  "complianceScore": 95.5,
  "alertsTrend": 5.2,
  "lastUpdated": "datetime"
}
```

### GET /dashboard/kpis
Get key performance indicators.

### GET /dashboard/alert-trends
Get alert trend data.

**Query Parameters:**
- `days` (int): Number of days (default: 30)

### GET /dashboard/recent-activities
Get recent system activities.

## 📰 Adverse Media

### POST /adverse-media/screen
Screen for adverse media.

**Request:**
```json
{
  "fullName": "string",
  "country": "string",
  "dateOfBirth": "string",
  "nationality": "string",
  "threshold": 75,
  "sources": ["Reuters", "BBC", "Bloomberg"],
  "categories": ["Financial Crime", "Corruption"]
}
```

### POST /adverse-media/create-alert
Create adverse media alert.

**Request:**
```json
{
  "customerName": "string",
  "source": "string",
  "category": "string",
  "severity": "string",
  "similarityScore": 85.5,
  "headline": "string",
  "summary": "string",
  "url": "string",
  "publicationDate": "datetime",
  "createdBy": "string"
}
```

## 📋 Reports

### GET /reports/sar
Get SAR reports.

### GET /reports/str
Get STR reports.

### POST /reports/generate
Generate compliance report.

**Request:**
```json
{
  "reportType": "SAR|STR|Compliance",
  "dateFrom": "date",
  "dateTo": "date",
  "includeDetails": true,
  "format": "PDF|Excel|CSV"
}
```

## 🏢 Organization Management

### GET /organizations
Get organizations (Admin only).

### POST /organizations
Create new organization (Admin only).

### GET /organizations/{id}/users
Get organization users.

### POST /organizations/{id}/users/invite
Invite user to organization.

## 🔧 System Administration

### GET /health
System health check (public endpoint).

**Response:**
```json
{
  "status": "Healthy",
  "timestamp": "datetime",
  "version": "1.0.0",
  "services": {
    "database": "Healthy",
    "cache": "Healthy",
    "externalApis": "Healthy"
  }
}
```

### GET /system/info
Get system information.

### POST /system/maintenance
Enable/disable maintenance mode (Admin only).

## 📊 Watchlist Management

### GET /watchlists
Get available watchlists.

### POST /watchlists/update
Update watchlist data.

### GET /watchlists/{type}/entries
Get watchlist entries by type.

## 🔍 Search

### GET /search/customers
Search customers.

### GET /search/alerts
Search alerts.

### GET /search/global
Global search across all entities.

## Error Responses

All endpoints return consistent error responses:

```json
{
  "error": "string",
  "message": "string",
  "details": "string",
  "timestamp": "datetime",
  "traceId": "string"
}
```

### HTTP Status Codes
- `200` - Success
- `201` - Created
- `400` - Bad Request
- `401` - Unauthorized
- `403` - Forbidden
- `404` - Not Found
- `409` - Conflict
- `422` - Validation Error
- `500` - Internal Server Error

## Rate Limiting
- **Standard Users**: 1000 requests/hour
- **Premium Users**: 5000 requests/hour
- **Enterprise**: Unlimited

## Pagination
All list endpoints support pagination:
- `page`: Page number (1-based)
- `pageSize`: Items per page (max 100)
- `totalCount`: Total items available

## Filtering & Sorting
Most list endpoints support:
- `search`: Text search
- `sortBy`: Field to sort by
- `sortOrder`: `asc` or `desc`
- Various field-specific filters

## Webhooks
Configure webhooks for real-time notifications:
- Alert creation
- Screening completion
- Compliance events
- System status changes

## SDK & Libraries
Official SDKs available for:
- JavaScript/TypeScript
- Python
- C#/.NET
- Java
- PHP

## Support
- **Documentation**: Available at `/swagger` endpoint
- **Support Email**: support@pepify.com
- **Status Page**: https://status.pepify.com