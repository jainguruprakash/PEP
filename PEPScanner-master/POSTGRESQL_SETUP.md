# PostgreSQL Integration Guide for PEP Scanner

## Prerequisites
- PostgreSQL installed and running on your machine
- .NET 8 SDK
- Entity Framework Core tools

## Step-by-Step Setup

### 1. Database Setup
1. Open PostgreSQL command line (psql) as postgres user
2. Run the setup script:
   ```sql
   \i setup-postgres.sql
   ```
   Or manually execute the commands in `setup-postgres.sql`

### 2. Update Connection Strings
Update the connection strings in both configuration files with your PostgreSQL credentials:

**appsettings.json:**
```json
"DefaultConnection": "Host=localhost;Database=pepscanner;Username=postgres;Password=YOUR_PASSWORD"
```

**appsettings.Development.json:**
```json
"DefaultConnection": "Host=localhost;Database=pepscanner_dev;Username=postgres;Password=YOUR_PASSWORD"
```

### 3. Run Migration
Choose one of the following methods:

**Option A: Using Batch Script (Windows)**
```cmd
migrate-to-postgres.bat
```

**Option B: Using PowerShell Script**
```powershell
.\migrate-to-postgres.ps1
```

**Option C: Manual Commands**
```cmd
cd "src\backend\PEPScanner.API"
dotnet ef migrations add InitialPostgreSQL --project ..\PEPScanner.Infrastructure --startup-project .
dotnet ef database update --project ..\PEPScanner.Infrastructure --startup-project .
```

### 4. Verify Setup
1. Start the application:
   ```cmd
   cd src\backend\PEPScanner.API
   dotnet run
   ```
2. Check that the database tables are created in PostgreSQL
3. Verify the application starts without errors

## Configuration Details

### Connection String Parameters
- **Host**: PostgreSQL server address (localhost for local installation)
- **Database**: Database name (pepscanner for production, pepscanner_dev for development)
- **Username**: PostgreSQL username
- **Password**: PostgreSQL password
- **Port**: PostgreSQL port (default: 5432, can be omitted if using default)

### Additional Options
You can add these parameters to your connection string if needed:
- `Port=5432` (if using non-default port)
- `Pooling=true` (enable connection pooling)
- `CommandTimeout=30` (command timeout in seconds)

Example with additional options:
```
Host=localhost;Database=pepscanner;Username=postgres;Password=your_password;Port=5432;Pooling=true;CommandTimeout=30
```

## Troubleshooting

### Common Issues

1. **Connection Failed**
   - Verify PostgreSQL is running
   - Check connection string credentials
   - Ensure database exists

2. **Migration Errors**
   - Delete existing migrations folder and recreate
   - Ensure no other applications are using the database
   - Check PostgreSQL logs for detailed error messages

3. **Permission Errors**
   - Ensure the PostgreSQL user has necessary privileges
   - Run PostgreSQL service with appropriate permissions

### Useful Commands

**Check PostgreSQL Status:**
```cmd
pg_ctl status
```

**Connect to Database:**
```cmd
psql -h localhost -U postgres -d pepscanner
```

**List Databases:**
```sql
\l
```

**List Tables:**
```sql
\dt
```

## Performance Optimization

For better performance with large datasets, consider:

1. **Indexing**: The application already includes necessary indexes
2. **Connection Pooling**: Enabled by default in the connection string
3. **Query Optimization**: Monitor slow queries using PostgreSQL logs
4. **Regular Maintenance**: Set up regular VACUUM and ANALYZE operations

## Backup and Recovery

**Create Backup:**
```cmd
pg_dump -h localhost -U postgres pepscanner > pepscanner_backup.sql
```

**Restore Backup:**
```cmd
psql -h localhost -U postgres pepscanner < pepscanner_backup.sql
```