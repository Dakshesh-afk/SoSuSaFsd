# Security Guidelines

## ?? Admin Access Security

### Fixed Vulnerability (January 2025)

**Issue**: Previously, there were **two ways** to obtain admin access:
1. ? Login as `SoSuSaAdmin` (intended)
2. ? Register with username "admin" ? Automatic admin role (security vulnerability)

**Fix**: Removed automatic admin role assignment during registration. Now **only** the seeded `SoSuSaAdmin` account has admin access.

### Current Admin Access

**Single Admin Account** (hardcoded in `DatabaseSeeder.cs`):
```
Username: SoSuSaAdmin
Email: admin@sosusa.com
Password: Admin@123
Role: Admin
```

**?? CRITICAL**: This password is hardcoded for development only. Change before deploying to production.

### Registration Security

All new user registrations now:
- ? Receive "User" role only
- ? Cannot self-elevate to Admin
- ? Require admin approval for restricted category access

**Before Fix** (removed code):
```csharp
// SECURITY RISK - REMOVED
if (username.ToLower() == "admin")
    await _userManager.AddToRoleAsync(newUser, "Admin");
```

**After Fix** (current code):
```csharp
// All registrations get User role
var newUser = new Users { Role = "User" };
await _userManager.AddToRoleAsync(newUser, "User");
```

**Files Modified**:
- ? `SoSuSaFsd/Controllers/AccountController.cs`

---

## ??? Production Security Checklist

### Before Deploying to Production

#### 1. Change Admin Password
Edit `DatabaseSeeder.cs` (line ~60):
```csharp
("SoSuSaAdmin", "admin@sosusa.com", "YourStrongPassword!@#$", "Admin", ...)
```

#### 2. Use Environment Variables (Recommended)
**Best practice approach**:
```csharp
var adminPassword = Environment.GetEnvironmentVariable("ADMIN_PASSWORD") 
    ?? throw new InvalidOperationException("ADMIN_PASSWORD environment variable not set");

("SoSuSaAdmin", "admin@sosusa.com", adminPassword, "Admin", ...)
```

**Set in Azure App Service**:
```
Configuration ? Application Settings ? New application setting
Name: ADMIN_PASSWORD
Value: YourStrongPassword!@#$
```

#### 3. Disable Seeding in Production
Modify `Program.cs`:
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
// Don't seed in production
```

#### 4. Enable HTTPS Only
`Program.cs`:
```csharp
if (!app.Environment.IsDevelopment())
{
    app.UseHsts();
    app.UseHttpsRedirection();
}
```

#### 5. Configure CORS (if API is accessed externally)
Restrict allowed origins:
```csharp
builder.Services.AddCors(options =>
{
    options.AddPolicy("ProductionPolicy", policy =>
    {
        policy.WithOrigins("https://yourdomain.com")
              .AllowAnyHeader()
              .AllowAnyMethod();
    });
});

// Later in the pipeline
app.UseCors("ProductionPolicy");
```

#### 6. Secure Connection String
Move to Azure Key Vault or user secrets:
```bash
dotnet user-secrets set "ConnectionStrings:DefaultConnection" "Server=..."
```

Or use Azure Key Vault:
```csharp
builder.Configuration.AddAzureKeyVault(
    new Uri($"https://{keyVaultName}.vault.azure.net/"),
    new DefaultAzureCredential());
```

---

## ?? Password Requirements

Configured in `Program.cs`:
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

**Minimum Requirements**:
- At least 6 characters
- 1 uppercase letter
- 1 lowercase letter
- 1 digit
- 1 special character

**Valid Examples**:
- ? `Admin@123`
- ? `User@123`
- ? `MyP@ssw0rd`
- ? `Secure!Pass99`

**Invalid Examples**:
- ? `password` (no uppercase, digit, special char)
- ? `Pass1` (too short, no special char)
- ? `PASSWORD123` (no lowercase, no special char)

---

## ?? Security Best Practices

### Authentication
- ? Use ASP.NET Core Identity (already implemented)
- ? Hash passwords (automatic with Identity)
- ? Use HTTPS for all authentication endpoints
- ?? Implement account lockout after failed attempts (recommended)
- ?? Enable two-factor authentication for admin accounts (recommended)

### Authorization
- ? Use `[Authorize]` attribute on protected pages
- ? Role-based access control for admin features
- ? Verify user identity on all data modification operations
- ? Check permissions in services, not just UI

### Data Protection
- ? Validate all user inputs
- ? Use parameterized queries (EF Core does this automatically)
- ? Sanitize HTML content before display
- ?? Implement rate limiting for API endpoints (recommended)
- ?? Add CAPTCHA to registration form (recommended)

### Secrets Management
- ? **NEVER** commit credentials to source control
- ? **NEVER** hardcode API keys or passwords in code
- ? Use environment variables or Azure Key Vault
- ? Add `appsettings.Production.json` to `.gitignore`
- ? Use different credentials for development and production

---

## ?? Security Audit Log (Recommended Implementation)

### Admin Actions to Monitor

The following actions should be logged:

- ? Admin login/logout
- ? User ban/unban
- ? Content deletion
- ? Report dismissal/resolution
- ? Category verification changes
- ? Access request approval/rejection

**Recommended Implementation**:
```csharp
// Add to AdminService methods
_logger.LogWarning($"ADMIN ACTION: User {adminId} banned user {userId} at {DateTime.UtcNow}");
_logger.LogWarning($"ADMIN ACTION: User {adminId} deleted post {postId} at {DateTime.UtcNow}");
_logger.LogWarning($"ADMIN ACTION: User {adminId} dismissed report {reportId} at {DateTime.UtcNow}");
```

**Storage Options**:
- Application Insights (Azure)
- Serilog with file/database sink
- Custom audit table in database

---

## ??? Additional Security Recommendations

### 1. Enable Two-Factor Authentication (Future Enhancement)
```csharp
// Program.cs
builder.Services.AddIdentityCore<Users>(options =>
{
    options.SignIn.RequireConfirmedAccount = true;
    options.Tokens.AuthenticatorTokenProvider = TokenOptions.DefaultAuthenticatorProvider;
});
```

### 2. Implement Account Lockout
```csharp
builder.Services.AddIdentityCore<Users>(options =>
{
    options.Lockout.DefaultLockoutTimeSpan = TimeSpan.FromMinutes(15);
    options.Lockout.MaxFailedAccessAttempts = 5;
    options.Lockout.AllowedForNewUsers = true;
});
```

### 3. Add CAPTCHA for Registration
Use **reCAPTCHA v3** for invisible bot protection:
```bash
dotnet add package reCAPTCHA.AspNetCore
```

### 4. Implement Content Security Policy (CSP)
```csharp
app.Use(async (context, next) =>
{
    context.Response.Headers.Add("Content-Security-Policy", 
        "default-src 'self'; script-src 'self' 'unsafe-inline' cdn.tailwindcss.com; style-src 'self' 'unsafe-inline';");
    context.Response.Headers.Add("X-Content-Type-Options", "nosniff");
    context.Response.Headers.Add("X-Frame-Options", "DENY");
    context.Response.Headers.Add("X-XSS-Protection", "1; mode=block");
    await next();
});
```

### 5. Rate Limiting (Future Enhancement)
```bash
dotnet add package AspNetCoreRateLimit
```

```csharp
// Program.cs
builder.Services.AddMemoryCache();
builder.Services.Configure<IpRateLimitOptions>(builder.Configuration.GetSection("IpRateLimiting"));
builder.Services.AddInMemoryRateLimiting();
builder.Services.AddSingleton<IRateLimitConfiguration, RateLimitConfiguration>();
```

### 6. Input Validation
Always validate user input:
```csharp
// Example in AccountController
if (string.IsNullOrWhiteSpace(username) || username.Length < 3 || username.Length > 50)
{
    return BadRequest("Invalid username");
}

if (!Regex.IsMatch(email, @"^[^@\s]+@[^@\s]+\.[^@\s]+$"))
{
    return BadRequest("Invalid email format");
}
```

### 7. SQL Injection Prevention
EF Core protects against SQL injection by default. If using raw SQL:
```csharp
// ? Safe (parameterized)
var user = await _context.Users
    .FromSqlRaw("SELECT * FROM Users WHERE Email = {0}", email)
    .FirstOrDefaultAsync();

// ? Unsafe (never do this)
var user = await _context.Users
    .FromSqlRaw($"SELECT * FROM Users WHERE Email = '{email}'")
    .FirstOrDefaultAsync();
```

---

## ?? Vulnerability Status

| Issue | Status | Date Fixed | Priority |
|-------|--------|------------|----------|
| Automatic admin creation via registration | ? Fixed | January 2025 | Critical |
| Hardcoded admin credentials | ?? Documented | January 2025 | High |
| Missing HTTPS redirect in production | ?? Recommended | - | High |
| No 2FA implementation | ?? Planned | - | Medium |
| No account lockout | ?? Planned | - | Medium |
| No CAPTCHA on registration | ?? Planned | - | Low |
| No rate limiting | ?? Planned | - | Low |
| No audit logging | ?? Planned | - | Medium |

**Legend**:
- ? Fixed
- ?? Documented (action required before production)
- ?? Planned enhancement

---

## ?? Security Testing

### Test 1: Verify Admin Registration is Blocked
```bash
# Try to create admin via registration
1. Navigate to /Account/Register
2. Username: admin
3. Email: test@test.com
4. Password: Test@123
5. Register

# Expected: User role assigned, NOT admin
```

### Test 2: Verify Only SoSuSaAdmin Has Admin Access
```bash
# Check database
SELECT u.UserName, r.Name as RoleName
FROM AspNetUsers u
JOIN AspNetUserRoles ur ON u.Id = ur.UserId
JOIN AspNetRoles r ON ur.RoleId = r.Id
WHERE r.Name = 'Admin';

# Expected: Only SoSuSaAdmin
```

### Test 3: Verify Admin Panel Access
```bash
# Login as regular user
1. Login with louis@example.com / User@123
2. Navigate to /admin
3. Expected: Access denied or redirect

# Login as admin
1. Login with admin@sosusa.com / Admin@123
2. Navigate to /admin
3. Expected: Admin panel loads
```

### Test 4: SQL Injection Test
```bash
# Try SQL injection in login
Email: ' OR '1'='1
Password: anything

# Expected: Login fails (EF Core prevents injection)
```

---

## ?? Report a Security Issue

If you discover a security vulnerability:

1. **Do NOT** create a public GitHub issue
2. Email the development team directly
3. Provide detailed steps to reproduce
4. Include potential impact assessment
5. Wait for confirmation before public disclosure

**Responsible Disclosure Timeline**:
- Report received: Acknowledge within 24 hours
- Initial assessment: 3-5 business days
- Fix developed: 7-14 days (depending on severity)
- Fix deployed: Within 24 hours of completion
- Public disclosure: After fix is deployed and verified

---

## ?? Security Resources

### Microsoft Documentation
- [ASP.NET Core Security](https://docs.microsoft.com/en-us/aspnet/core/security/)
- [Identity Management](https://docs.microsoft.com/en-us/aspnet/core/security/authentication/identity)
- [Data Protection](https://docs.microsoft.com/en-us/aspnet/core/security/data-protection/)

### OWASP Resources
- [OWASP Top 10](https://owasp.org/www-project-top-ten/)
- [OWASP Cheat Sheet Series](https://cheatsheetseries.owasp.org/)

### Tools
- [OWASP ZAP](https://www.zaproxy.org/) - Security testing
- [Snyk](https://snyk.io/) - Dependency vulnerability scanning
- [SonarQube](https://www.sonarqube.org/) - Code quality and security

---

**Last Updated**: January 2025  
**Security Level**: Development (NOT production-ready without implementing checklist)  
**Next Review**: Before production deployment  
**Classification**: Internal Documentation
