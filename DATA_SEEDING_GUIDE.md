# Data Seeding Guide - SoSuSaFsd Application

## Overview
This guide explains the comprehensive data seeding implementation for the SoSuSaFsd Blazor application. The seeding process automatically populates the database with test data on application startup.

## Files Created/Modified

### 1. **DatabaseSeeder.cs** (`SoSuSaFsd/Data/DatabaseSeeder.cs`)
- **Purpose**: Centralized class for all database seeding operations
- **Location**: `SoSuSaFsd/Data/`
- **Responsibilities**:
  - Seed roles (Admin, User)
  - Seed users with different roles
  - Seed categories (verified and unverified)
  - Seed posts with realistic content
  - Seed comments and replies
  - Seed likes for posts
  - Seed category follows

### 2. **Program.cs** (Modified)
- **Changes**: Updated database seeding section to use the new `DatabaseSeeder` class
- **Benefits**: Cleaner, more maintainable code

## Seeded Data

### ?? Users (6 Test Users)
| Username | Email | Password | Role | Verified |
|----------|-------|----------|------|----------|
| SoSuSaAdmin | admin@sosusa.com | Admin@123 | Admin | ? |
| john_doe | john@example.com | User@123 | User | ? |
| jane_smith | jane@example.com | User@123 | User | ? |
| mike_wilson | mike@example.com | User@123 | User | ? |
| sarah_jones | sarah@example.com | User@123 | User | ? |
| alex_brown | alex@example.com | User@123 | User | ? |

### ?? Categories (10 Categories)
1. **Technology** (Verified) - Latest in tech, gadgets, and innovation
2. **Gaming** (Verified) - Gaming experiences, tips, and reviews
3. **Programming** (Verified) - Code, debug, and learn together
4. **Design** (Verified) - UI/UX, graphics, and creative design
5. **Photography** (Unverified) - Best shots and photography tips
6. **Music** (Verified) - Music, playlists, and new artists
7. **Sports** (Unverified) - Favorite sports and teams
8. **Movies & TV** (Verified) - Film and television discussions
9. **VIP Lounge** (Verified, **Restricted**) - Exclusive for verified members
10. **General** (Verified) - Everything else

### ?? Posts (10 Sample Posts)
- Posts distributed across different categories
- Realistic content related to each category
- Varying creation dates (1-10 days old)
- Each post created by different users

### ?? Comments (8 Comments including 1 Reply)
- Parent comments on various posts
- One nested reply demonstrating the reply system
- Comments from different users
- Realistic comment content

### ?? Likes (Random Distribution)
- 2-3 random users like each of the first 5 posts
- Realistic like timestamps

### ?? Category Follows (Random Distribution)
- Each user follows 2-4 random categories
- Demonstrates the follow system

## How It Works

### Startup Process
1. **Application starts** ? `Program.cs` executes
2. **Database context is created** via `IDbContextFactory`
3. **DatabaseSeeder is instantiated** with required services
4. **SeedAsync() method executes** in sequence:
   ```
   SeedRolesAsync()
   ?
   SeedUsersAsync()
   ?
   SeedCategoriesAsync()
   ?
   SeedPostsAsync()
   ?
   SeedCommentsAsync()
   ?
   SeedLikesAsync()
   ?
   SeedCategoryFollowsAsync()
   ```

### Smart Seeding Logic
- **Checks for existing data** before seeding each entity type
- **Skips seeding** if data already exists
- **Logs all operations** for debugging
- **Handles errors gracefully** without crashing the application

## Usage

### Running the Application
1. **Ensure your database connection string is configured** in `appsettings.json`
2. **Run migrations** (if not already done):
   ```bash
   dotnet ef database update
   ```
3. **Start the application**:
   ```bash
   dotnet run
   ```
4. **Data seeding runs automatically** on startup

### Accessing Seeded Data

#### Login Credentials
Use any of these accounts to test:

**Admin Account:**
```
Email: admin@sosusa.com
Password: Admin@123
```

**Regular User:**
```
Email: john@example.com
Password: User@123
```

### Viewing Seeded Content
1. **Home Feed** - See all seeded posts
2. **Categories** - Browse the 10 seeded categories
3. **Posts** - View posts with comments and likes
4. **User Profiles** - Check user information and their posts

## Customization

### Adding More Users
Edit `DatabaseSeeder.cs` ? `SeedUsersAsync()` method:

```csharp
var users = new List<(string Username, string Email, string Password, string Role, string FirstName, string LastName, string? Bio, bool IsVerified)>
{
    // Add new users here
    ("new_user", "new@example.com", "User@123", "User", "New", "User", "Bio text", true),
    // ...existing users
};
```

### Adding More Categories
Edit `DatabaseSeeder.cs` ? `SeedCategoriesAsync()` method:

```csharp
var categories = new List<Categories>
{
    new Categories
    {
        CategoryName = "Your Category",
        CategoryDescription = "Description here",
        CategoryIsRestricted = false,
        IsVerified = true,
        CreatedBy = adminUser?.Id,
        DateCreated = DateTime.Now,
        DateUpdated = DateTime.Now
    },
    // ...existing categories
};
```

### Adding More Posts
Edit `DatabaseSeeder.cs` ? `SeedPostsAsync()` method:

```csharp
var posts = new List<Posts>
{
    new Posts
    {
        Content = "Your post content here",
        PostStatus = "Published",
        UserId = users[0].Id,
        CategoryId = techCategory?.Id ?? 1,
        DateCreated = DateTime.Now,
        DateUpdated = DateTime.Now,
        CreatedBy = users[0].Id
    },
    // ...existing posts
};
```

## Resetting the Database

### To Re-seed the Database:
1. **Drop the database**:
   ```bash
   dotnet ef database drop
   ```
2. **Run migrations again**:
   ```bash
   dotnet ef database update
   ```
3. **Restart the application** - seeding will run automatically

### Alternative: Selective Reset
To reset only specific data (e.g., posts), you can:
1. Delete records from the specific table manually
2. Restart the application
3. The seeder will detect missing data and re-seed

## Troubleshooting

### Issue: Seeding Not Running
**Solution**: Check the logs in the console output. Look for messages starting with:
- `"Created role: ..."`
- `"Created user: ..."`
- `"Seeded X categories."`

### Issue: Duplicate Key Errors
**Cause**: Database already has data
**Solution**: The seeder checks for existing data. If you're seeing errors, your database might be in an inconsistent state. Drop and recreate it.

### Issue: Users Not Created
**Check**:
1. Password requirements in `Program.cs` ? `AddIdentityCore` options
2. User email format
3. Console logs for specific error messages

### Issue: Foreign Key Violations
**Cause**: Seeding order is important
**Solution**: The seeder runs in the correct order (Roles ? Users ? Categories ? Posts ? Comments ? Likes ? Follows). Don't modify this order unless necessary.

## Benefits of This Implementation

? **Automatic** - No manual data entry needed
? **Consistent** - Always the same test data
? **Fast** - Seeds in seconds on startup
? **Safe** - Checks for existing data before seeding
? **Maintainable** - Easy to modify and extend
? **Logged** - All operations are logged for debugging
? **Error Handling** - Graceful failure doesn't crash the app

## Development Workflow

### For Development:
1. Start with a fresh database
2. Let the seeder populate test data
3. Develop and test features
4. Drop database when you need fresh data
5. Repeat

### For Production:
**?? Important**: Modify `Program.cs` to disable seeding in production:

```csharp
// ========== DATABASE SEEDING ==========
if (app.Environment.IsDevelopment())
{
    using (var scope = app.Services.CreateScope())
    {
        // ...seeding code
    }
}
```

## Advanced Customization

### Conditional Seeding
You can add conditions to seed different data based on environment:

```csharp
if (app.Environment.IsDevelopment())
{
    await seeder.SeedAsync(); // Full test data
}
else if (app.Environment.IsStaging())
{
    await seeder.SeedMinimalAsync(); // Minimal data
}
// Don't seed in production
```

### Seed from External Files
You can load data from JSON files:

```csharp
private async Task SeedPostsFromFileAsync()
{
    var json = await File.ReadAllTextAsync("SeedData/posts.json");
    var posts = JsonSerializer.Deserialize<List<Posts>>(json);
    await _context.Posts.AddRangeAsync(posts);
    await _context.SaveChangesAsync();
}
```

## Summary

The data seeding implementation provides:
- 6 test users with different roles
- 10 diverse categories
- 10 realistic posts
- 8 comments with nested replies
- Random likes and follows
- Automatic execution on startup
- Comprehensive logging
- Error handling

This setup allows you to immediately start testing and developing features without manually creating test data! ??
