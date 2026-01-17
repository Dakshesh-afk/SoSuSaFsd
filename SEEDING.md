# Database Seeding Guide

## Overview

SoSuSaFsd automatically populates the database with test data on startup. This eliminates manual data entry and provides a consistent development environment.

## Quick Reference

### Default Admin Account (Hardcoded)
```
Email: admin@sosusa.com
Password: Admin@123
Username: SoSuSaAdmin
Role: Admin
```

**?? IMPORTANT**: This password is hardcoded for development. Change before deploying to production.

### Test User Accounts
| Username | Email | Password | Role | Verified |
|----------|-------|----------|------|----------|
| Louis | louis@example.com | User@123 | User | ? |
| Dakshesh | dakshesh@example.com | User@123 | User | ? |
| gamer_123 | gamer123@example.com | User@123 | User | ? |
| troller67 | troller67@example.com | User@123 | User | ? |
| Mothership | mothership@example.com | User@123 | User | ? |
| temasekteacher | temasekteacher@example.com | User@123 | User | ? |

## What Gets Seeded

| Entity | Count | Details |
|--------|-------|---------|
| **Roles** | 2 | Admin, User |
| **Users** | 7 | 1 admin, 6 regular users |
| **Categories** | 10 | 2 verified (SG News, Temasek Poly News), 8 community |
| **Posts** | 10 | Realistic content across categories |
| **Post Media** | ~15 | Images and videos on select posts |
| **Comments** | 9 | Including nested replies |
| **Likes** | ~20 | Distributed across posts |
| **Category Follows** | ~30 | Users following 3-5 categories each |
| **Reports** | 5 | Mix of Pending/Reviewed/Dismissed |
| **Access Requests** | 5 | VIP Lounge verification requests |

## Category Details

### Verified Categories (Official)
- **SG News** - Latest news from Singapore ????
- **Temasek Poly News** - Official school updates ??

### Community Categories (Unverified)
- Gaming ??
- Technology ??
- Programming ?????
- Memes & Humor ??
- Education ??
- Food & Dining ??
- Sports ??
- General ??

### Restricted Category
- **VIP Lounge** ?? - Verified members only

## How Seeding Works

### Automatic Seeding
Seeding runs automatically when you start the application:

```bash
dotnet run
```

Console output confirms success:
```
Created role: Admin
Created role: User
Created user: SoSuSaAdmin with role: Admin
...
Seeded 10 categories.
Seeded 10 posts.
Database seeding completed successfully.
```

### Reset Database
To start fresh with clean seed data:

#### Option 1: PowerShell Script
```powershell
.\reset-database.ps1
```

#### Option 2: Manual Commands
```bash
cd SoSuSaFsd
dotnet ef database drop --force
dotnet ef database update
dotnet run
```

## Implementation Details

### Location
- **Seeding Logic**: `SoSuSaFsd/Data/DatabaseSeeder.cs`
- **Startup Integration**: `SoSuSaFsd/Program.cs` (lines 150-170)

### Seeding Order (Critical)
```
1. Roles ? 2. Users ? 3. Categories ? 4. Posts ? 
5. Post Media ? 6. Comments ? 7. Likes ? 
8. Category Follows ? 9. Reports ? 10. Access Requests
```

**?? Do not change this order** - it maintains foreign key relationships.

### Smart Detection
The seeder checks if data already exists:
```csharp
if (await _context.Posts.AnyAsync())
{
    _logger.LogInformation("Posts already exist. Skipping post seeding.");
    return;
}
```

## Customization

### Adding More Test Users
Edit `DatabaseSeeder.cs` (line ~54):

```csharp
var users = new List<(string Username, string Email, string Password, string Role, string FirstName, string LastName, string? Bio, bool IsVerified)>
{
    ("SoSuSaAdmin", "admin@sosusa.com", "Admin@123", "Admin", "Admin", "User", "Platform Administrator ???", true),
    ("your_username", "your@email.com", "User@123", "User", "First", "Last", "Your bio", true),
    // Add more here
};
```

### Adding More Categories
Edit `DatabaseSeeder.cs` (line ~120):

```csharp
new Categories
{
    CategoryName = "Your Category",
    CategoryDescription = "Description goes here",
    CategoryIsRestricted = false,
    IsVerified = true,
    CreatedBy = adminUser?.Id,
    DateCreated = DateTime.Now.AddDays(-30),
    DateUpdated = DateTime.Now.AddDays(-30)
}
```

### Adding More Posts
Edit `DatabaseSeeder.cs` (line ~250):

```csharp
new Posts
{
    Content = "Your post content here",
    PostStatus = "Published",
    UserId = user?.Id ?? users[1].Id,
    CategoryId = category?.Id ?? 1,
    DateCreated = DateTime.Now.AddHours(-6),
    DateUpdated = DateTime.Now.AddHours(-6),
    CreatedBy = user?.Id ?? users[1].Id
}
```

## Production Deployment

### Disable Seeding in Production
Modify `Program.cs` to only seed in development:

```csharp
// ========== DATABASE SEEDING ==========
if (app.Environment.IsDevelopment())
{
    using (var scope = app.Services.CreateScope())
    {
        var services = scope.ServiceProvider;
        var contextFactory = services.GetRequiredService<IDbContextFactory<SoSuSaFsdContext>>();
        var userManager = services.GetRequiredService<UserManager<Users>>();
        var roleManager = services.GetRequiredService<RoleManager<IdentityRole>>();
        var logger = services.GetRequiredService<ILogger<DatabaseSeeder>>();

        using (var context = await contextFactory.CreateDbContextAsync())
        {
            var seeder = new DatabaseSeeder(context, userManager, roleManager, logger);
            await seeder.SeedAsync();
        }
    }
}
```

### Change Admin Password
**Before production**, update `DatabaseSeeder.cs` (line ~60):

```csharp
("SoSuSaAdmin", "admin@sosusa.com", "YourStrongPassword!@#", "Admin", ...)
```

Or use environment variables:
```csharp
var adminPassword = Environment.GetEnvironmentVariable("ADMIN_PASSWORD") ?? "Admin@123";
("SoSuSaAdmin", "admin@sosusa.com", adminPassword, "Admin", ...)
```

## Troubleshooting

### Issue: "Data already exists" warnings
**Cause**: Database already contains seeded data.  
**Solution**: Drop and recreate the database (see [Reset Database](#reset-database)).

### Issue: Seeding fails silently
**Cause**: Exception caught but not visible.  
**Solution**: Check console output for error messages. Enable detailed logging in `appsettings.Development.json`:

```json
"Logging": {
  "LogLevel": {
    "Default": "Information",
    "SoSuSaFsd.Data.DatabaseSeeder": "Debug"
  }
}
```

### Issue: Foreign key constraint errors
**Cause**: Seeding order is incorrect or data is partially seeded.  
**Solution**: Drop database completely and restart application.

### Issue: Admin account not working
**Cause**: Password doesn't meet Identity requirements.  
**Solution**: Verify password requirements in `Program.cs`:

```csharp
builder.Services.AddIdentityCore<Users>(options =>
{
    options.Password.RequireDigit = true;
    options.Password.RequireLowercase = true;
    options.Password.RequireUppercase = true;
    options.Password.RequireNonAlphanumeric = true;
    options.Password.RequiredLength = 6;
});
```

### Issue: Empty home feed after seeding
**Cause**: Feed only shows posts from followed categories (by design).  
**Solution**: 
1. Navigate to sidebar
2. Click any category
3. Click "Follow" button
4. Return to home page
5. Posts will now appear

**Note**: This is intentional design (like Twitter/Reddit). Users curate their own feed.

## Verification Checklist

After seeding, verify:

- [ ] Console shows "Database seeding completed successfully"
- [ ] Can login with `admin@sosusa.com` / `Admin@123`
- [ ] Can login with `louis@example.com` / `User@123`
- [ ] Admin Panel accessible (Admin account only)
- [ ] Sidebar shows categories
- [ ] Categories show post counts
- [ ] Can follow/unfollow categories
- [ ] Posts appear in home feed after following categories
- [ ] Posts have likes and comments
- [ ] Reports visible in Admin Panel (Moderation tab)
- [ ] Access requests visible in Admin Panel (Access tab)

## Architecture Notes

### Why Separate Seeder Class?
- **Separation of Concerns**: Seeding logic isolated from startup
- **Testability**: Can unit test seeding independently
- **Maintainability**: Easy to modify without touching `Program.cs`
- **Reusability**: Can call from integration tests

### Why Check Existing Data?
- **Idempotency**: Safe to restart application multiple times
- **Performance**: Skips unnecessary database operations
- **Flexibility**: Allows partial seeding (some entities exist, others don't)

### Why This Seeding Order?
Foreign key dependencies require this sequence:
```
Roles (no dependencies)
  ?
Users (depends on Roles)
  ?
Categories (depends on Users for CreatedBy)
  ?
Posts (depends on Users + Categories)
  ?
Media (depends on Posts)
  ?
Comments (depends on Posts + Users)
  ?
Likes (depends on Posts + Users)
  ?
Follows (depends on Categories + Users)
  ?
Reports (depends on Posts + Users)
  ?
Access Requests (depends on Categories + Users)
```

## Best Practices

? **Do:**
- Always drop and recreate database when testing seeding changes
- Use realistic test data that reflects production scenarios
- Keep admin password strong (even in development)
- Document any custom seed data additions
- Test seed data after major schema changes

? **Don't:**
- Modify seeding order without understanding dependencies
- Hardcode production credentials in seeder
- Seed sensitive or real user data in development
- Commit database files to version control
- Use seeding in production environments

---

**Last Updated**: January 2025  
**Version**: 1.0  
**Compatibility**: .NET 8, EF Core 8, Blazor
