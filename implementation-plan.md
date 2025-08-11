# Implementation Plan for Compliance Hierarchy

## Phase 1: Database Schema Updates (High Priority)

### 1. Add Team Structure
```csharp
public class Team
{
    public Guid Id { get; set; }
    public Guid OrganizationId { get; set; }
    public string Name { get; set; } = string.Empty;
    public string Department { get; set; } = string.Empty;
    public Guid? TeamLeadId { get; set; }
    public string? Territory { get; set; }
    public bool IsActive { get; set; } = true;
    
    public virtual OrganizationUser? TeamLead { get; set; }
    public virtual ICollection<OrganizationUser> Members { get; set; } = new List<OrganizationUser>();
}
```

### 2. Update OrganizationUser Entity
```csharp
// Add to OrganizationUser class
public Guid? ManagerId { get; set; }
public Guid? TeamId { get; set; }
public string? Territory { get; set; }
public int EscalationLevel { get; set; } = 0;

public virtual OrganizationUser? Manager { get; set; }
public virtual Team? Team { get; set; }
public virtual ICollection<OrganizationUser> Subordinates { get; set; } = new List<OrganizationUser>();
```

## Phase 2: Smart Assignment Logic (Medium Priority)

### 1. Alert Assignment Service
```csharp
public class SmartAssignmentService
{
    public async Task<OrganizationUser> GetAssigneeForAlert(Alert alert)
    {
        // Logic:
        // 1. Check customer territory/product type
        // 2. Find appropriate team
        // 3. Round-robin assign to available analyst
        // 4. Consider workload balancing
    }
}
```

### 2. Notification Rules Engine
```csharp
public class NotificationRulesEngine
{
    public async Task<List<OrganizationUser>> GetNotificationRecipients(Alert alert)
    {
        // Logic:
        // 1. Primary assignee
        // 2. Team lead (if configured)
        // 3. Manager (for high-risk alerts)
        // 4. Escalation based on SLA breach
    }
}
```

## Phase 3: UI Enhancements (Low Priority)

### 1. Team Management Interface
- Create/manage teams
- Assign team leads
- Territory mapping
- Workload dashboard

### 2. Escalation Dashboard
- SLA monitoring
- Escalation alerts
- Performance metrics
- Team productivity

## Recommended Approach for Banks:

### Small Banks (< 100 employees):
- **Simple hierarchy**: Analyst → Compliance Officer → Manager
- **Territory-based**: Branch-wise assignment
- **Notification**: Team lead + manager for high-risk

### Large Banks (> 1000 employees):
- **Complex hierarchy**: Multiple teams by product/geography
- **Specialized teams**: Corporate, Retail, Trade Finance, Private Banking
- **Notification**: Targeted to specific team only
- **Escalation matrix**: 4-level escalation with time-based auto-escalation

### Regulatory Preference:
- **Clear accountability** chain (who made what decision)
- **Segregation of duties** (analyst cannot approve own work)
- **Audit trail** of all actions and escalations
- **SLA compliance** monitoring and reporting