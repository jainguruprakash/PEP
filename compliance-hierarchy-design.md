# Compliance Hierarchy Design

## Current vs Ideal Structure

### Current Issues:
- No manager-subordinate mapping
- All compliance officers get notifications
- No team/territory assignment
- Missing escalation matrix

### Ideal Banking Structure:

## 1. Enhanced User Entity
```sql
-- Add to OrganizationUser table
ALTER TABLE OrganizationUsers ADD COLUMN ManagerId UUID REFERENCES OrganizationUsers(Id);
ALTER TABLE OrganizationUsers ADD COLUMN TeamId UUID;
ALTER TABLE OrganizationUsers ADD COLUMN Territory VARCHAR(100);
ALTER TABLE OrganizationUsers ADD COLUMN EscalationLevel INT DEFAULT 0;
```

## 2. Team Structure
```sql
CREATE TABLE Teams (
    Id UUID PRIMARY KEY,
    OrganizationId UUID NOT NULL,
    Name VARCHAR(100) NOT NULL,
    Department VARCHAR(100) NOT NULL,
    TeamLeadId UUID REFERENCES OrganizationUsers(Id),
    Territory VARCHAR(100),
    IsActive BOOLEAN DEFAULT TRUE
);
```

## 3. Notification Rules
```sql
CREATE TABLE NotificationRules (
    Id UUID PRIMARY KEY,
    OrganizationId UUID NOT NULL,
    AlertType VARCHAR(50) NOT NULL,
    RiskLevel VARCHAR(50) NOT NULL,
    NotifyTeamLead BOOLEAN DEFAULT TRUE,
    NotifyManager BOOLEAN DEFAULT FALSE,
    EscalationTimeHours INT DEFAULT 24,
    AutoAssignToTeamLead BOOLEAN DEFAULT TRUE
);
```

## 4. Workflow Logic

### Alert Assignment:
1. **System creates alert** → Auto-assign to appropriate team analyst
2. **Analyst reviews** → Escalate to Team Lead (Compliance Officer)
3. **Team Lead approves/rejects** → Close or escalate to Manager
4. **Manager final decision** → Close or escalate to Risk Department

### Notification Strategy:
- **Level 1**: Notify assigned analyst only
- **Level 2**: Notify team lead if analyst doesn't act within SLA
- **Level 3**: Notify department manager if team lead doesn't act
- **Level 4**: Notify risk department for critical escalations

## 5. Territory/Product Assignment:
- **Corporate Banking Team**: Large corporate clients
- **Retail Banking Team**: Individual customers
- **Trade Finance Team**: Import/export transactions
- **Private Banking Team**: HNI clients

## 6. SLA Matrix:
| Risk Level | Analyst SLA | Team Lead SLA | Manager SLA |
|------------|-------------|---------------|-------------|
| Low        | 48 hours    | 24 hours      | 72 hours    |
| Medium     | 24 hours    | 12 hours      | 48 hours    |
| High       | 12 hours    | 6 hours       | 24 hours    |
| Critical   | 4 hours     | 2 hours       | 8 hours     |

## 7. Benefits:
- **Targeted notifications** (no spam)
- **Clear accountability** chain
- **Workload distribution** across teams
- **Proper escalation** matrix
- **Audit trail** of decisions
- **Performance metrics** by team/individual