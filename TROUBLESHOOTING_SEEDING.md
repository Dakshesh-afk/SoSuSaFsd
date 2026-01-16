# ?? Troubleshooting: Categories and Posts Not Showing

## Issue
The data seeding has been implemented, but categories and posts are not visible in the application.

## Step-by-Step Fix Guide

### Step 1: Check Console Logs When Starting the App

When you run `dotnet run`, look for these messages in the console:

? **Good Output (Seeding Working):**
```
Created role: Admin
Created role: User
Created user: SoSuSaAdmin with role: Admin
Created user: john_doe with role: User
Created user: jane_smith with role: User
...
Seeded 10 categories.
Seeded 10 posts.
Seeded 8 comments (including replies).
Seeded 15 likes.
Seeded 20 category follows.
Database seeding completed successfully.
```

? **Bad Output (Seeding Skipped):**
```
Categories already exist. Skipping category seeding.
Posts already exist. Skipping post seeding.
```

### Step 2: Reset Your Database (PowerShell Commands)

If you see "already exist" messages, your database has data but it's not showing up. Let's reset it:

```powershell
# Navigate to your project directory
cd C:\Users\louis\source\repos\SoSuSaFsdd\SoSuSaFsd

# Stop the application if it's running (Ctrl+C)

# Drop the database
dotnet ef database drop --force

# Recreate the database with migrations
dotnet ef database update

# Start the application
dotnet run
```

### Step 3: Login and Follow Categories

**Important:** The home feed only shows posts from categories you follow!

1. **Start the app** and navigate to your browser
2. **Login** with: `john@example.com` / `User@123`
3. **Look at the RIGHT sidebar** - You should see "Verified Categories"
4. **Click on a category** (e.g., #Technology, #Gaming)
5. **On the category page**, click the "Follow" button
6. **Go back to Home** - You should now see posts from that category!

### Step 4: Verify Data in Database (Optional)

If you want to confirm the data exists, you can check your database:

**Using SQL Server Management Studio or Azure Data Studio:**

```sql
-- Check categories
SELECT * FROM Categories;

-- Check posts
SELECT * FROM Posts;

-- Check users
SELECT * FROM AspNetUsers;

-- Check category follows
SELECT * FROM CategoryFollows;
```

### Step 5: Check Your Feed Logic

The Home feed shows posts ONLY from followed categories. Here's how it works:

```
1. User follows categories ? Stored in CategoryFollows table
2. Home page loads ? Queries posts where CategoryId IN (followed categories)
3. If no categories followed ? Empty feed
```

**To see posts immediately:**
- Follow at least one category from the right sidebar
- The seeder automatically creates follows, but only if it's a fresh database

## Common Issues and Solutions

### Issue 1: "Your feed is empty"
**Cause:** You haven't followed any categories
**Solution:** 
- Check right sidebar for "Verified Categories"
- Click any category, then click "Follow" button
- Return to Home to see posts

### Issue 2: Right sidebar shows empty categories
**Cause:** Seeding didn't run or failed
**Solution:**
```powershell
# Reset database completely
cd C:\Users\louis\source\repos\SoSuSaFsdd\SoSuSaFsd
dotnet ef database drop --force
dotnet ef database update
dotnet run
```

### Issue 3: Categories exist but no posts
**Cause:** Posts seeding failed due to missing users/categories
**Solution:**
- Check console for error messages
- Reset database (see Step 2)
- Make sure seeding runs in correct order (it should automatically)

### Issue 4: Can't login with test accounts
**Cause:** Users weren't created
**Solution:**
- Reset database
- Check console for user creation errors
- Verify password requirements in Program.cs are relaxed (they should be)

## Quick Test Checklist

Run through this checklist to verify everything works:

- [ ] Drop and recreate database
- [ ] Start application with `dotnet run`
- [ ] Check console shows "Database seeding completed successfully"
- [ ] Open browser and go to application
- [ ] Login with `john@example.com` / `User@123`
- [ ] See "Verified Categories" in right sidebar (should show 8 categories)
- [ ] See "Recently Created" in right sidebar (should show some categories)
- [ ] Click on "#Technology" category
- [ ] Click "Follow" button on category page
- [ ] Go back to Home
- [ ] See posts in feed

## Expected Behavior After Fresh Seeding

### Right Sidebar (Before Following Anything)
**Verified Categories** section should show:
- #Technology ?
- #Gaming ?
- #Programming ?
- #Design ?
- #Music ?
- (and more...)

**Recently Created** section should show:
- Recent categories (may be empty if using admin account)

### Left Sidebar (Before Following Anything)
**Following** section:
- "You are not following any categories"
- Or: Shows 2-4 random categories (if seeding created follows)

### Home Feed (Before Following)
- "Your feed is empty"
- "Follow categories to see posts here!"

### Home Feed (After Following Categories)
- Should show posts from followed categories
- Posts with usernames, dates, content
- Like buttons, comment buttons
- Category tags

## PowerShell Commands Reference

```powershell
# Navigate to project
cd C:\Users\louis\source\repos\SoSuSaFsdd\SoSuSaFsd

# Drop database (removes all data)
dotnet ef database drop --force

# Apply migrations (creates fresh database)
dotnet ef database update

# Run application (seeds data automatically)
dotnet run

# If you need to add a new migration
dotnet ef migrations add MigrationName

# Remove last migration (if needed)
dotnet ef migrations remove
```

## Debugging Output

Add this to your console to see what's happening:

When you run `dotnet run`, you should see output like:

```
info: SoSuSaFsd.Data.DatabaseSeeder[0]
      Created role: Admin
info: SoSuSaFsd.Data.DatabaseSeeder[0]
      Created role: User
info: SoSuSaFsd.Data.DatabaseSeeder[0]
      Created user: SoSuSaAdmin with role: Admin
info: SoSuSaFsd.Data.DatabaseSeeder[0]
      Created user: john_doe with role: User
info: SoSuSaFsd.Data.DatabaseSeeder[0]
      Created user: jane_smith with role: User
info: SoSuSaFsd.Data.DatabaseSeeder[0]
      Created user: mike_wilson with role: User
info: SoSuSaFsd.Data.DatabaseSeeder[0]
      Created user: sarah_jones with role: User
info: SoSuSaFsd.Data.DatabaseSeeder[0]
      Created user: alex_brown with role: User
info: SoSuSaFsd.Data.DatabaseSeeder[0]
      Seeded 10 categories.
info: SoSuSaFsd.Data.DatabaseSeeder[0]
      Seeded 10 posts.
info: SoSuSaFsd.Data.DatabaseSeeder[0]
      Seeded 8 comments (including replies).
info: SoSuSaFsd.Data.DatabaseSeeder[0]
      Seeded 15 likes.
info: SoSuSaFsd.Data.DatabaseSeeder[0]
      Seeded 20 category follows.
info: SoSuSaFsd.Data.DatabaseSeeder[0]
      Database seeding completed successfully.
```

If you DON'T see these messages, the seeding isn't running!

## Still Not Working?

If after following all steps above, you still don't see data:

### Check 1: Verify Seeding Code is in Program.cs
Look for this section in `Program.cs`:

```csharp
// ========== DATABASE SEEDING ==========
using (var scope = app.Services.CreateScope())
{
    var services = scope.ServiceProvider;
    try
    {
        var contextFactory = services.GetRequiredService<IDbContextFactory<SoSuSaFsdContext>>();
        using var context = contextFactory.CreateDbContext();
        
        var userManager = services.GetRequiredService<UserManager<Users>>();
        var roleManager = services.GetRequiredService<RoleManager<IdentityRole>>();
        var logger = services.GetRequiredService<ILogger<DatabaseSeeder>>();

        var seeder = new DatabaseSeeder(context, userManager, roleManager, logger);
        await seeder.SeedAsync();
    }
    catch (Exception ex)
    {
        var logger = services.GetRequiredService<ILogger<Program>>();
        logger.LogError(ex, "An error occurred while seeding the database.");
    }
}
```

### Check 2: Verify DatabaseSeeder.cs Exists
File location: `SoSuSaFsd/Data/DatabaseSeeder.cs`

### Check 3: Connection String
Check `appsettings.json` has valid connection string:

```json
{
  "ConnectionStrings": {
    "SoSuSaFsdContext": "Server=(localdb)\\mssqllocaldb;Database=SoSuSaFsd;Trusted_Connection=True;MultipleActiveResultSets=true"
  }
}
```

## Summary

**Most Common Solution:**
```powershell
cd C:\Users\louis\source\repos\SoSuSaFsdd\SoSuSaFsd
dotnet ef database drop --force
dotnet ef database update
dotnet run
```

Then:
1. Login with `john@example.com` / `User@123`
2. Click a category in right sidebar
3. Click "Follow" on category page
4. Go back to Home
5. See posts! ??

---

**Need More Help?**
- Check console output for errors
- Share any error messages you see
- Verify you're logged in
- Make sure you've followed at least one category
