# Bank Organization Onboarding Guide

## Overview
Comprehensive guide for onboarding banks and financial institutions to the PEP Scanner system with proper organization setup and user management.

## Onboarding Models

### 1. **Organization-First Onboarding** (Recommended)
- **Process**: Organization admin signs up and creates the bank organization
- **Benefits**: Centralized control, proper multi-tenancy, organized user management
- **Use Case**: New bank wants to implement PEP scanning system

### 2. **Individual User Signup** (Legacy)
- **Process**: Each banker signs up individually
- **Limitations**: No centralized organization management
- **Use Case**: Quick testing or small teams

## Organization Onboarding Process

### Step 1: Organization Details
**Bank Information Required:**
- Bank Name (e.g., "State Bank of India")
- Bank Type (Public/Private/Foreign/Cooperative/RRB/NBFC)
- RBI License Number
- SWIFT Code
- Head Office Address (Complete address, city, state, PIN)
- Official Contact (Phone, Email)
- Website

### Step 2: Primary Administrator
**Admin User Setup:**
- Personal Details (Name, Email, Phone)
- Professional Details (Employee ID, Department, Designation)
- Login Credentials (Username, Password)
- Role: Automatically set to "Admin"

### Step 3: System Configuration
**Default Settings:**
- Time Zone (Default: Asia/Kolkata)
- Currency (Default: INR)
- Risk Threshold (Default: 70%)
- Alert Retention (Default: 365 days)

## User Roles & Permissions

### 1. **Admin**
- **Permissions**: Full system access
- **Capabilities**:
  - Organization settings management
  - User management (invite, activate, deactivate)
  - System configuration
  - All screening and compliance features
  - Advanced reporting and analytics

### 2. **Manager**
- **Permissions**: Management level access
- **Capabilities**:
  - User management within department
  - Advanced screening features
  - Compliance oversight
  - Detailed reporting
  - Alert management

### 3. **Compliance Officer**
- **Permissions**: Full compliance access
- **Capabilities**:
  - All screening features
  - Alert management and resolution
  - Compliance reporting
  - Risk assessment
  - Audit trail access

### 4. **Analyst**
- **Permissions**: Operational access
- **Capabilities**:
  - Customer screening
  - Basic alert viewing
  - Standard reporting
  - Data entry and processing

## API Endpoints

### Organization Onboarding
```http
POST /api/auth/onboard-organization
Content-Type: application/json

{
  "organization": {
    "name": "State Bank of India",
    "type": "Public Sector Bank",
    "rbiLicenseNumber": "RBI/DPSS/2023/1234",
    "swiftCode": "SBININBB123",
    "address": "Corporate Centre, SION, Mumbai",
    "city": "Mumbai",
    "state": "Maharashtra",
    "pinCode": "400022",
    "phoneNumber": "+91-22-12345678",
    "email": "compliance@sbi.co.in",
    "website": "https://www.sbi.co.in"
  },
  "adminUser": {
    "firstName": "Rajesh",
    "lastName": "Kumar",
    "email": "rajesh.kumar@sbi.co.in",
    "phoneNumber": "+91-9876543210",
    "employeeId": "SBI001",
    "department": "Compliance",
    "designation": "Chief Compliance Officer",
    "username": "rajesh.kumar",
    "password": "SecurePassword123"
  },
  "configuration": {
    "timeZone": "Asia/Kolkata",
    "currency": "INR",
    "riskThreshold": 70,
    "alertRetentionDays": 365
  }
}
```

### User Invitation
```http
POST /api/auth/invite-user
Content-Type: application/json

{
  "organizationId": "123e4567-e89b-12d3-a456-426614174000",
  "firstName": "Priya",
  "lastName": "Sharma",
  "email": "priya.sharma@sbi.co.in",
  "role": "ComplianceOfficer",
  "department": "Risk Management"
}
```

## Frontend Components

### 1. Organization Signup Component
**Route**: `/onboard-organization`
**Features**:
- Multi-step wizard (Organization → Admin → Configuration)
- Form validation and error handling
- Real-time field validation
- Responsive design

### 2. User Invitation Component
**Usage**: Dialog/Modal within admin panel
**Features**:
- Role-based permission display
- Department selection
- Email validation
- Invitation tracking

### 3. Enhanced Signup Component
**Route**: `/signup`
**Features**:
- Individual user registration
- Organization selection/creation
- Role assignment

## Database Schema

### Organizations Table
```sql
CREATE TABLE Organizations (
    Id UNIQUEIDENTIFIER PRIMARY KEY,
    Name NVARCHAR(200) NOT NULL,
    Type NVARCHAR(50) NOT NULL,
    RbiLicenseNumber NVARCHAR(50),
    SwiftCode NVARCHAR(20),
    Address NVARCHAR(500) NOT NULL,
    City NVARCHAR(100) NOT NULL,
    State NVARCHAR(100) NOT NULL,
    PinCode NVARCHAR(10) NOT NULL,
    PhoneNumber NVARCHAR(20) NOT NULL,
    Email NVARCHAR(100) NOT NULL,
    Website NVARCHAR(200),
    IsActive BIT DEFAULT 1,
    CreatedAtUtc DATETIME2 DEFAULT GETUTCDATE(),
    UpdatedAtUtc DATETIME2 DEFAULT GETUTCDATE()
);
```

### OrganizationUsers Table
```sql
CREATE TABLE OrganizationUsers (
    Id UNIQUEIDENTIFIER PRIMARY KEY,
    OrganizationId UNIQUEIDENTIFIER NOT NULL,
    UserId UNIQUEIDENTIFIER NOT NULL,
    Role NVARCHAR(50) NOT NULL,
    IsActive BIT DEFAULT 1,
    JoinedAtUtc DATETIME2 DEFAULT GETUTCDATE(),
    FOREIGN KEY (OrganizationId) REFERENCES Organizations(Id),
    FOREIGN KEY (UserId) REFERENCES Users(Id)
);
```

### OrganizationConfigurations Table
```sql
CREATE TABLE OrganizationConfigurations (
    Id UNIQUEIDENTIFIER PRIMARY KEY,
    OrganizationId UNIQUEIDENTIFIER NOT NULL,
    TimeZone NVARCHAR(50) DEFAULT 'Asia/Kolkata',
    Currency NVARCHAR(10) DEFAULT 'INR',
    RiskThreshold INT DEFAULT 70,
    AlertRetentionDays INT DEFAULT 365,
    CreatedAtUtc DATETIME2 DEFAULT GETUTCDATE(),
    UpdatedAtUtc DATETIME2 DEFAULT GETUTCDATE(),
    FOREIGN KEY (OrganizationId) REFERENCES Organizations(Id)
);
```

## Onboarding Workflows

### New Bank Onboarding
1. **Initial Contact**: Bank reaches out for PEP scanning solution
2. **Demo & Requirements**: System demonstration and requirement gathering
3. **Organization Setup**: Bank admin uses organization onboarding flow
4. **Configuration**: System configuration based on bank's needs
5. **User Invitations**: Admin invites team members
6. **Training**: User training and system walkthrough
7. **Go-Live**: Production deployment and monitoring

### User Addition Workflow
1. **Admin Login**: Organization admin logs into system
2. **User Invitation**: Admin invites new user via email
3. **Email Notification**: User receives invitation email with setup link
4. **Account Setup**: User completes profile and sets password
5. **Role Assignment**: System assigns appropriate permissions
6. **Access Granted**: User can access system based on role

## Security Considerations

### Organization Level
- **Data Isolation**: Complete data separation between organizations
- **Access Control**: Role-based access within organization
- **Audit Logging**: All actions logged with user and organization context

### User Level
- **Strong Authentication**: Password complexity requirements
- **Session Management**: Secure session handling with timeout
- **Permission Validation**: Server-side permission checks

## Configuration Options

### Risk Management
- **Threshold Settings**: Customizable risk thresholds per organization
- **Source Selection**: Enable/disable specific watchlist sources
- **Alert Rules**: Custom alert generation rules

### Compliance
- **Retention Policies**: Configurable data retention periods
- **Reporting Frequency**: Automated report generation schedules
- **Audit Requirements**: Compliance-specific audit trail settings

## Best Practices

### For Banks
1. **Start with Admin**: Always begin with organization onboarding
2. **Role Planning**: Plan user roles and permissions before inviting users
3. **Training**: Ensure proper user training before go-live
4. **Configuration**: Customize settings based on regulatory requirements

### For System Administrators
1. **Validation**: Verify bank credentials during onboarding
2. **Monitoring**: Monitor organization usage and performance
3. **Support**: Provide dedicated support during initial setup
4. **Updates**: Keep organizations informed of system updates

## Troubleshooting

### Common Issues
1. **Email Conflicts**: User email already exists in system
2. **Organization Duplicates**: Bank name or email already registered
3. **Permission Issues**: Users not seeing expected features
4. **Configuration Problems**: Incorrect timezone or currency settings

### Solutions
1. **Email Verification**: Implement email verification process
2. **Duplicate Checking**: Enhanced duplicate detection
3. **Role Validation**: Clear role permission documentation
4. **Configuration Validation**: Input validation and defaults

This comprehensive onboarding system ensures proper bank organization setup with centralized management and appropriate user access controls.