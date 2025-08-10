# Customer Saving & Bulk Upload Troubleshooting Guide

## Issues Fixed

### 1. **Customer Creation Not Working**
**Problem**: CustomersController was using mock data instead of saving to database
**Solution**: Updated controller to use actual database operations

```csharp
// Before (Mock)
var customer = new { Id = Guid.NewGuid(), FullName = "..." };
return Ok(new { message = "Customer created successfully", customer });

// After (Real Database)
var customer = new Customer { ... };
_context.Customers.Add(customer);
await _context.SaveChangesAsync();
return Ok(new { message = "Customer created successfully", customerId = customer.Id });
```

### 2. **Bulk Upload Not Saving**
**Problem**: Bulk upload was only validating data, not saving to database
**Solution**: Implemented actual bulk insert with proper error handling

```csharp
// Added actual database operations
var customersToAdd = new List<Customer>();
// ... process CSV data
_context.Customers.AddRange(customersToAdd);
await _context.SaveChangesAsync();
```

### 3. **Bulk Screening Not Working**
**Problem**: Batch screening endpoint was returning empty results
**Solution**: Implemented CSV processing with actual screening logic

## Testing Endpoints

### 1. Test Customer Creation
```http
POST /api/customers/test-create
```
This endpoint creates a test customer to verify database connectivity.

### 2. Create Customer
```http
POST /api/customers
Content-Type: application/json

{
  "firstName": "John",
  "lastName": "Doe",
  "email": "john.doe@example.com",
  "country": "India",
  "phoneNumber": "9876543210",
  "nationality": "Indian"
}
```

### 3. Bulk Upload Customers
```http
POST /api/customers/bulk-upload
Content-Type: multipart/form-data

CSV Format:
FullName,Email,Country,PhoneNumber,Nationality
John Doe,john@example.com,India,9876543210,Indian
Jane Smith,jane@example.com,USA,1234567890,American
```

### 4. Bulk Screening
```http
POST /api/screening/batch-file
Content-Type: multipart/form-data

CSV Format:
FullName,Country
John Doe,India
Jane Smith,USA
```

## Database Schema Verification

### Customer Table Structure
```sql
-- Key fields for Customer entity
Id (GUID, Primary Key)
OrganizationId (GUID, Foreign Key)
FullName (NVARCHAR(300), Required)
EmailAddress (NVARCHAR(100))
Country (NVARCHAR(100))
RiskLevel (NVARCHAR(50), Default: 'Low')
IsActive (BIT, Default: 1)
IsDeleted (BIT, Default: 0)
CreatedAtUtc (DATETIME2)
UpdatedAtUtc (DATETIME2)
```

## Common Issues & Solutions

### 1. Database Connection Issues
**Symptoms**: 
- "Internal server error" when creating customers
- Database timeout errors

**Solutions**:
- Check connection string in `appsettings.json`
- Verify database exists and is accessible
- Run database migrations if needed

```bash
# Run migrations
dotnet ef database update
```

### 2. Validation Errors
**Symptoms**:
- "Missing required fields" errors
- Data format validation failures

**Solutions**:
- Ensure required fields (FullName, Email) are provided
- Validate email format
- Check CSV file format matches expected structure

### 3. Bulk Upload Issues
**Symptoms**:
- File upload fails
- CSV parsing errors
- Partial data insertion

**Solutions**:
- Verify CSV format matches expected structure
- Check file encoding (UTF-8 recommended)
- Ensure proper headers in CSV file
- Handle duplicate email addresses

### 4. Performance Issues
**Symptoms**:
- Slow bulk operations
- Timeout errors on large files

**Solutions**:
- Process files in batches (current: 1000 records per batch)
- Add database indexes on frequently queried fields
- Use bulk insert operations

## CSV File Formats

### Customer Bulk Upload
```csv
FullName,Email,Country,PhoneNumber,Nationality
John Doe,john@example.com,India,9876543210,Indian
Jane Smith,jane@example.com,USA,1234567890,American
```

### Bulk Screening
```csv
FullName,Country
John Doe,India
Jane Smith,USA
Amit Shah,India
```

## API Response Examples

### Successful Customer Creation
```json
{
  "message": "Customer created successfully",
  "customerId": "123e4567-e89b-12d3-a456-426614174000"
}
```

### Successful Bulk Upload
```json
{
  "totalRecords": 100,
  "successCount": 95,
  "failedCount": 5,
  "errors": [
    "Line 10: Missing required fields (FullName, Email)",
    "Line 25: Customer with email john@example.com already exists"
  ]
}
```

### Bulk Screening Results
```json
[
  {
    "customerName": "John Doe",
    "country": "India",
    "riskScore": 75,
    "matchCount": 2,
    "status": "Medium Risk",
    "matches": [
      {
        "matchedName": "John Doe",
        "source": "OFAC",
        "listType": "Sanctions",
        "matchScore": 0.95
      }
    ],
    "screenedAt": "2024-01-15T10:30:00Z"
  }
]
```

## Monitoring & Logging

### Key Log Messages
- `Customer created: {Name} with ID: {Id}` - Successful creation
- `Bulk inserted {Count} customers` - Successful bulk insert
- `Batch screening completed: {Count} customers processed` - Successful screening

### Error Monitoring
- Check application logs for database connection errors
- Monitor API response times for performance issues
- Track failed bulk upload records for data quality issues

## Next Steps

1. **Test Database Connection**: Use `/api/customers/test-create` endpoint
2. **Verify Single Customer Creation**: Test with `/api/customers` POST
3. **Test Bulk Upload**: Upload small CSV file (5-10 records)
4. **Test Bulk Screening**: Screen small batch of customers
5. **Monitor Logs**: Check for any error messages or warnings

The customer saving and bulk upload functionality should now work correctly with actual database persistence.