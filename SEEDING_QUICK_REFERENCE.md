# ?? Quick Reference: Seeded Test Data

## ?? Login Credentials

### Admin Account
```
Email: admin@sosusa.com
Password: Admin@123
Role: Admin
```

### Test Users
```
1. john@example.com   | User@123 | User Role | Verified
2. jane@example.com   | User@123 | User Role | Verified
3. mike@example.com   | User@123 | User Role | Not Verified
4. sarah@example.com  | User@123 | User Role | Verified
5. alex@example.com   | User@123 | User Role | Verified
```

## ?? Categories (10 Total)

| Category | Verified | Restricted | Description |
|----------|----------|------------|-------------|
| Technology | ? | ? | Tech, gadgets, innovation |
| Gaming | ? | ? | Gaming tips & reviews |
| Programming | ? | ? | Code & learn together |
| Design | ? | ? | UI/UX & graphics |
| Photography | ? | ? | Photography tips |
| Music | ? | ? | Music & playlists |
| Sports | ? | ? | Sports discussions |
| Movies & TV | ? | ? | Film & television |
| VIP Lounge | ? | ? | Exclusive members only |
| General | ? | ? | Everything else |

## ?? What's Seeded

- ? **6 Users** (1 Admin, 5 Regular Users)
- ? **2 Roles** (Admin, User)
- ? **10 Categories** (8 verified, 1 restricted)
- ? **10 Posts** (across various categories)
- ? **8 Comments** (including nested replies)
- ? **~15 Likes** (randomly distributed)
- ? **~20 Category Follows** (users following categories)

## ?? Quick Start

1. **Reset database**: Run `reset-database.ps1` (PowerShell script)
   OR manually:
   ```powershell
   cd C:\Users\louis\source\repos\SoSuSaFsdd\SoSuSaFsd
   dotnet ef database drop --force
   dotnet ef database update
   ```

2. **Start app**: `dotnet run`

3. **Login with**: `john@example.com` / `User@123`

4. **Follow categories**: Click categories in right sidebar, then click "Follow"

5. **View posts**: Go back to Home to see posts from followed categories

## ?? Reset Data

### PowerShell Script (Easiest)
```powershell
.\reset-database.ps1
```

### Manual Commands
```powershell
cd C:\Users\louis\source\repos\SoSuSaFsdd\SoSuSaFsd
dotnet ef database drop --force
dotnet ef database update
dotnet run
```

## ?? Tips

- All test users have password: `User@123`
- Admin password is: `Admin@123`
- Profile images are auto-generated from user initials
- Posts are dated 1-10 days in the past
- Comments have realistic timestamps
- **IMPORTANT:** Home feed only shows posts from categories you follow!

## ?? Testing Features

### Test User Authentication
- Login with different user accounts
- Check role-based access (Admin vs User)

### Test Posts
- View posts in Home feed (after following categories)
- Like/unlike posts
- Comment on posts

### Test Categories
- Browse categories in right sidebar
- Follow/unfollow categories
- View category-specific posts
- Test restricted category access (VIP Lounge)

### Test Comments
- Add comments to posts
- Reply to comments (nested)
- View comment threads

### Test Admin Features
- Login as admin (`admin@sosusa.com` / `Admin@123`)
- Access admin dashboard
- Moderate content

## ?? Common Issues

### Issue: Can't see categories
**Solution**: Check right sidebar for "Verified Categories"

### Issue: Can't see posts on Home
**Solution**: 
1. Follow at least one category
2. Posts only show from followed categories
3. If no follows exist, feed is empty by design

### Issue: Seeding not running
**Solution**: 
1. Drop and recreate database
2. Check console for "Database seeding completed successfully"
3. See TROUBLESHOOTING_SEEDING.md for details

### Issue: Can't login
**Solution**:
1. Make sure database is seeded
2. Use correct credentials from table above
3. Reset database if needed

## ?? Documentation Files

- **TROUBLESHOOTING_SEEDING.md** - Detailed troubleshooting guide
- **DATA_SEEDING_GUIDE.md** - Complete implementation guide
- **reset-database.ps1** - PowerShell script to reset database

## ? Quick Commands

```powershell
# Reset everything
cd C:\Users\louis\source\repos\SoSuSaFsdd\SoSuSaFsd
dotnet ef database drop --force
dotnet ef database update
dotnet run

# Just run app
dotnet run

# Create new migration
dotnet ef migrations add MigrationName

# Remove last migration
dotnet ef migrations remove
