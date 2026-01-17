# SoSuSaFsd - Developer Quick Start

## ?? Get Started in 5 Minutes

### 1. Clone & Restore
```bash
git clone https://github.com/Dakshesh-afk/SoSuSaFsd
cd SoSuSaFsd
dotnet restore
```

### 2. Setup Database
```bash
cd SoSuSaFsd
dotnet ef database update
```

### 3. Run Application
```bash
dotnet run
```

The database will seed automatically on first run.

### 4. Login
Navigate to: `https://localhost:7071` (or check console for URL)

**Admin Account**:
```
Email: admin@sosusa.com
Password: Admin@123
```

**Test User**:
```
Email: louis@example.com
Password: User@123
```

### 5. Explore
- ? **Home**: See posts from followed categories
- ? **Categories**: Follow topics you're interested in
- ? **Admin Panel**: Admin account only (click "Admin Panel" button)
- ? **Profile**: View user information and posts

---

## ?? Project Structure

```
SoSuSaFsd/
??? Components/
?   ??? Pages/          # Blazor pages (Home.razor, Admin.razor, etc.)
?   ??? Layout/         # Layout components (MainLayout, NavMenu)
?   ??? Common/         # Reusable components (PostCard, CommentSection)
?   ??? Account/        # Authentication pages (Login, Register)
??? Controllers/        # API controllers (AccountController)
??? Data/               # Database context and seeder
??? Domain/             # Entity models (Users, Posts, Categories, etc.)
??? Services/           # Business logic (PostService, AdminService, CategoryService)
??? wwwroot/            # Static files (CSS, JS, images)
?   ??? styles/         # CSS files (organized by component)
??? Program.cs          # Application startup
```

---

## ?? Key Features

| Feature | Description | Location |
|---------|-------------|----------|
| **Authentication** | Login/Register/Logout | `/Account/Login`, `/Account/Register` |
| **Posts** | Create, view, like, comment | `Home.razor` |
| **Categories** | Follow topics for personalized feed | Sidebar (all pages) |
| **Admin Panel** | Moderate content, manage users | `/admin` |
| **Reports** | User-generated content reports | Admin Panel ? Moderation |
| **Verification** | Category access requests | Admin Panel ? Access |
| **Search** | Page-specific search functionality | Header (conditional display) |

---

## ??? Common Tasks

### Reset Database
```bash
cd SoSuSaFsd
dotnet ef database drop --force
dotnet ef database update
dotnet run
```

Or use the PowerShell script:
```powershell
.\reset-database.ps1
```

### Add Migration
```bash
dotnet ef migrations add YourMigrationName
dotnet ef database update
```

### View Database
Use **SQL Server Object Explorer** in Visual Studio or **Azure Data Studio**.

**Connection String** (from `appsettings.json`):
```
Server=(localdb)\\mssqllocaldb;Database=SoSuSaFsd;Trusted_Connection=True;MultipleActiveResultSets=true
```

### Build & Run
```bash
dotnet build
dotnet run
```

---

## ?? Troubleshooting

### Can't see posts on home page?
**Cause**: Home feed only shows posts from followed categories (by design).  
**Fix**: 
1. Go to sidebar
2. Click a category
3. Click "Follow"
4. Return to home

### Build errors?
```bash
dotnet clean
dotnet restore
dotnet build
```

### Database errors?
```bash
dotnet ef database drop --force
dotnet ef database update
```

### Port conflicts?
Edit `Properties/launchSettings.json` to change ports:
```json
"applicationUrl": "https://localhost:7071;http://localhost:5071"
```

### Admin panel not accessible?
- Make sure you're logged in with `admin@sosusa.com`
- Check that the "Admin Panel" button appears in the header
- Verify the admin role was assigned during seeding

---

## ?? Documentation

- **[SEEDING.md](SEEDING.md)** - Database seeding details and customization
- **[SECURITY.md](SECURITY.md)** - Security policies and fixes

---

## ?? Development Workflow

### Creating a New Feature

1. **Add Domain Entity** (if needed)
   ```bash
   # Create new entity in Domain/ folder
   dotnet ef migrations add AddYourEntity
   dotnet ef database update
   ```

2. **Create Service** (if needed)
   ```bash
   # Add service class in Services/ folder
   # Register in Program.cs
   builder.Services.AddScoped<IYourService, YourService>();
   ```

3. **Create Blazor Component/Page**
   ```bash
   # Add .razor file in Components/Pages/ or Components/Common/
   # Inject service: @inject IYourService YourService
   ```

4. **Add Styles** (optional)
   ```bash
   # Create CSS file in wwwroot/styles/
   # Reference in component: <link href="styles/your-component.css" rel="stylesheet" />
   ```

### Code Organization

- **Services**: Business logic and data access (use these instead of direct DbContext access)
- **Components/Pages**: Full pages with routing
- **Components/Common**: Reusable components
- **Components/Layout**: Layout wrappers
- **Domain**: Entity models (database tables)

---

## ?? Testing

### Manual Testing
1. Reset database: `dotnet ef database drop --force && dotnet ef database update`
2. Run application: `dotnet run`
3. Test features using seeded accounts
4. Check console logs for errors

### Test Accounts
- Admin: `admin@sosusa.com` / `Admin@123`
- User 1: `louis@example.com` / `User@123`
- User 2: `dakshesh@example.com` / `User@123`

---

## ?? UI/UX Notes

### Styling
- **Tailwind CSS**: Available via CDN (see `App.razor`)
- **Custom CSS**: Organized by component in `wwwroot/styles/`
- **Icons**: Using emoji (??, ??, etc.) - consider icon library if needed

### Components
- **PostCard**: Reusable post display component
- **CommentSection**: Reusable comment thread component
- **CategoryHeader**: Category page header with follow button
- **CarouselComponent**: Media carousel for posts

### Responsive Design
- Mobile-first approach
- Sidebar collapses on mobile
- Touch-friendly buttons and links

---

## ?? Git Workflow

### Before Committing
```bash
# Build to check for errors
dotnet build

# Check for uncommitted changes
git status

# Stage and commit
git add .
git commit -m "Your descriptive message"
git push origin master
```

### Important: Don't Commit
- `bin/` and `obj/` folders (already in .gitignore)
- Database files (`.mdf`, `.ldf`)
- `appsettings.Development.json` with secrets
- User-specific settings

---

## ?? Next Steps

1. ? Explore the admin panel features
2. ? Create test posts and comments
3. ? Follow categories to customize your feed
4. ? Review the codebase structure
5. ? Read `SEEDING.md` for customization options
6. ? Check `SECURITY.md` before deploying to production

---

## ?? Need Help?

- Check documentation files for detailed information
- Review code comments in service classes
- Inspect browser console for client-side errors
- Check terminal output for server-side errors

---

**Last Updated**: January 2025  
**Version**: 1.0  
**Compatibility**: .NET 8, Blazor, Entity Framework Core 8
