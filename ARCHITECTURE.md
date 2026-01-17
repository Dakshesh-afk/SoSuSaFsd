# SoSuSaFsd - System Architecture

## Overview

SoSuSaFsd is a Blazor Server application built on .NET 8, implementing a social media platform with categories, posts, comments, and administrative features.

## Technology Stack

| Layer | Technology | Version |
|-------|-----------|---------|
| **Framework** | .NET | 8.0 |
| **UI Framework** | Blazor Server | 8.0 |
| **ORM** | Entity Framework Core | 8.0 |
| **Database** | SQL Server LocalDB | - |
| **Authentication** | ASP.NET Core Identity | 8.0 |
| **Styling** | Tailwind CSS | 3.x (CDN) |
| **Icons** | Emoji | - |

---

## Project Structure

### High-Level Architecture

```
???????????????????????????????????????????????
?            Blazor Server App                 ?
???????????????????????????????????????????????
?                                              ?
?  ????????????????      ????????????????    ?
?  ?   Components ????????  Controllers ?    ?
?  ?   (UI Layer) ?      ?  (API Layer) ?    ?
?  ????????????????      ????????????????    ?
?         ?                     ?             ?
?         ?                     ?             ?
?  ????????????????????????????????????      ?
?  ?         Services Layer            ?      ?
?  ?  (Business Logic & Data Access)   ?      ?
?  ?????????????????????????????????????      ?
?                 ?                            ?
?                 ?                            ?
?  ????????????????????????????????????      ?
?  ?       Domain Models               ?      ?
?  ?  (Entity Classes / Database)      ?      ?
?  ????????????????????????????????????      ?
?                                              ?
???????????????????????????????????????????????
```

### Directory Structure

```
SoSuSaFsd/
??? Components/
?   ??? Pages/              # Full pages with routing
?   ?   ??? Home.razor      # Main feed (posts from followed categories)
?   ?   ??? CategoryDetails.razor  # Category-specific feed
?   ?   ??? Profile.razor   # User profile page
?   ?   ??? Admin.razor     # Admin dashboard
?   ??? Common/             # Reusable components
?   ?   ??? PostCard.razor          # Post display component
?   ?   ??? CommentSection.razor    # Comment thread component
?   ?   ??? CategoryHeader.razor    # Category page header
?   ?   ??? CarouselComponent.razor # Media carousel
?   ??? Layout/             # Layout components
?   ?   ??? MainLayout.razor   # Default layout (with sidebar)
?   ?   ??? EmptyLayout.razor  # Minimal layout (auth pages)
?   ?   ??? NavMenu.razor      # Sidebar navigation
?   ??? Account/            # Authentication pages
?   ?   ??? Pages/
?   ?       ??? Login.razor
?   ?       ??? Register.razor
?   ??? AuthModal.razor     # Authentication modal
??? Controllers/            # API endpoints
?   ??? AccountController.cs  # Authentication API
??? Services/               # Business logic layer
?   ??? PostService.cs      # Post-related operations
?   ??? CategoryService.cs  # Category-related operations
?   ??? AdminService.cs     # Admin-related operations
??? Domain/                 # Entity models (database tables)
?   ??? Users.cs
?   ??? Posts.cs
?   ??? Categories.cs
?   ??? Comments.cs
?   ??? PostLikes.cs
?   ??? PostMedia.cs
?   ??? Report.cs
?   ??? CategoryAccessRequests.cs
??? Data/                   # Data access layer
?   ??? SoSuSaFsdContext.cs    # EF Core DbContext
?   ??? DatabaseSeeder.cs      # Seed data logic
??? Migrations/             # EF Core migrations
??? wwwroot/                # Static files
?   ??? styles/             # CSS files
?   ?   ??? layout.css      # Layout styles
?   ?   ??? shared.css      # Shared components
?   ?   ??? feed.css        # Post feed styles
?   ?   ??? admin.css       # Admin panel styles
?   ?   ??? header.css      # Header styles
?   ?   ??? modals.css      # Modal styles
?   ?   ??? category.css    # Category styles
?   ??? app.css             # Global styles
??? Program.cs              # Application startup
```

---

## Architecture Layers

### 1. Presentation Layer (Components)

**Responsibility**: User interface and user interactions

#### Pages (`Components/Pages/`)
- **Home.razor**: Main feed showing posts from followed categories
- **CategoryDetails.razor**: Category-specific posts and follow button
- **Profile.razor**: User profile with posts and bio
- **Admin.razor**: Admin dashboard with moderation tools

#### Reusable Components (`Components/Common/`)
- **PostCard.razor**: Displays a single post with like/comment actions
- **CommentSection.razor**: Displays comment thread with nested replies
- **CategoryHeader.razor**: Category page header with follow/unfollow
- **CarouselComponent.razor**: Image/video carousel for post media

#### Layout Components (`Components/Layout/`)
- **MainLayout.razor**: Default layout with header and sidebar
- **EmptyLayout.razor**: Minimal layout for authentication pages
- **NavMenu.razor**: Sidebar with categories and navigation

### 2. Service Layer (Services)

**Responsibility**: Business logic and data access abstraction

#### PostService.cs
```csharp
public interface IPostService
{
    Task<List<Posts>> GetPostsForUserFeedAsync(string userId);
    Task<List<Posts>> GetPostsByCategoryAsync(int categoryId);
    Task<Posts?> GetPostByIdAsync(int postId);
    Task<bool> CreatePostAsync(Posts post);
    Task<bool> DeletePostAsync(int postId);
    Task<bool> ToggleLikeAsync(int postId, string userId);
    Task<bool> AddCommentAsync(Comments comment);
}
```

**Used by**: Home.razor, CategoryDetails.razor, Profile.razor

#### CategoryService.cs
```csharp
public interface ICategoryService
{
    Task<List<Categories>> GetAllCategoriesAsync();
    Task<Categories?> GetCategoryByIdAsync(int categoryId);
    Task<List<Categories>> GetFollowedCategoriesAsync(string userId);
    Task<bool> ToggleFollowAsync(int categoryId, string userId);
    Task<bool> CreateCategoryAsync(Categories category);
}
```

**Used by**: NavMenu.razor, CategoryDetails.razor, Home.razor

#### AdminService.cs
```csharp
public interface IAdminService
{
    Task<List<Report>> GetReportsAsync();
    Task<bool> ResolveReportAsync(int reportId, string action);
    Task<List<CategoryAccessRequests>> GetAccessRequestsAsync();
    Task<bool> ProcessAccessRequestAsync(int requestId, bool approve);
    Task<bool> BanUserAsync(string userId);
    Task<bool> DeletePostAsync(int postId);
    Task<bool> VerifyCategoryAsync(int categoryId);
}
```

**Used by**: Admin.razor

### 3. Data Layer (Domain + Data)

**Responsibility**: Database entities and data persistence

#### Entity Models (Domain/)
- **Users.cs**: User accounts (extends IdentityUser)
- **Posts.cs**: User-generated posts
- **Categories.cs**: Discussion categories
- **Comments.cs**: Post comments (supports nesting)
- **PostLikes.cs**: Post likes (many-to-many)
- **PostMedia.cs**: Post attachments (images/videos)
- **Report.cs**: Content reports
- **CategoryAccessRequests.cs**: Verification requests

#### DbContext (Data/SoSuSaFsdContext.cs)
- Manages all entity sets
- Configures relationships
- Handles migrations

### 4. API Layer (Controllers)

**Responsibility**: HTTP endpoints for authentication

#### AccountController.cs
- `POST /Account/Login`: User authentication
- `POST /Account/Register`: User registration
- `POST /Account/Logout`: User logout

---

## Data Model

### Entity Relationships

```
Users (IdentityUser)
  ??? Posts (1:Many) ? Created posts
  ??? Comments (1:Many) ? Created comments
  ??? PostLikes (1:Many) ? Liked posts
  ??? CategoryFollows (1:Many) ? Followed categories
  ??? Reports (1:Many) ? Submitted reports
  ??? CategoryAccessRequests (1:Many) ? Verification requests

Categories
  ??? Posts (1:Many) ? Posts in category
  ??? CategoryFollows (1:Many) ? Followers
  ??? CategoryAccessRequests (1:Many) ? Access requests

Posts
  ??? Comments (1:Many) ? Post comments
  ??? PostLikes (1:Many) ? Post likes
  ??? PostMedia (1:Many) ? Attached media
  ??? Reports (1:Many) ? Content reports

Comments
  ??? User (Many:1) ? Comment author
  ??? Post (Many:1) ? Parent post
  ??? ParentComment (Many:1) ? Nested replies (self-referencing)
```

### Key Entity Properties

#### Users
- `Id` (PK): Unique identifier
- `UserName`: Display name
- `Email`: Email address
- `Role`: "Admin" or "User"
- `Bio`: User biography
- `ProfilePictureUrl`: Profile image
- `IsVerified`: Verified status
- `IsBanned`: Ban status

#### Posts
- `PostId` (PK): Unique identifier
- `Content`: Post text
- `UserId` (FK): Author
- `CategoryId` (FK): Category
- `PostStatus`: "Published", "Draft", "Deleted"
- `DateCreated`: Creation timestamp
- `DateUpdated`: Last update timestamp

#### Categories
- `CategoryId` (PK): Unique identifier
- `CategoryName`: Display name
- `CategoryDescription`: Description
- `IsVerified`: Official category
- `CategoryIsRestricted`: Requires verification
- `DateCreated`: Creation timestamp

#### Comments
- `CommentId` (PK): Unique identifier
- `CommentContent`: Comment text
- `PostId` (FK): Parent post
- `UserId` (FK): Comment author
- `ParentCommentId` (FK, nullable): Parent comment (for nesting)
- `DateCreated`: Creation timestamp

---

## Authentication & Authorization

### Authentication Flow

```
1. User accesses protected page
   ?
2. Blazor checks authentication state
   ?
3. If not authenticated ? Redirect to Login
   ?
4. User submits credentials
   ?
5. AccountController.Login validates credentials
   ?
6. ASP.NET Core Identity creates auth cookie
   ?
7. User redirected to original page
```

### Authorization Levels

| Role | Access |
|------|--------|
| **Anonymous** | Login, Register, View public categories |
| **User** | Create posts, Comment, Like, Follow categories |
| **Admin** | All user permissions + Moderate content, Manage users, Verify categories |

### Role-Based Access

```csharp
// In Blazor components
<AuthorizeView Roles="Admin">
    <Authorized>
        <!-- Admin-only content -->
    </Authorized>
</AuthorizeView>

// In services/controllers
[Authorize(Roles = "Admin")]
public async Task<IActionResult> AdminAction() { }
```

---

## Key Features

### 1. Personalized Feed (Home.razor)

**How it works**:
1. Get user's followed categories
2. Query posts from those categories
3. Order by date (newest first)
4. Display using PostCard components

**Design Decision**: Feed only shows posts from followed categories (like Twitter/Reddit) to give users control over their content.

### 2. Category System

**Types**:
- **Verified Categories**: Official categories (badge shown)
- **Community Categories**: User-created categories
- **Restricted Categories**: Require access approval (VIP Lounge)

**Follow/Unfollow**:
- Users can follow any non-restricted category
- Restricted categories require admin approval
- Following determines home feed content

### 3. Comment System

**Features**:
- Nested comments (2 levels deep)
- Real-time updates via Blazor
- Like/unlike functionality
- Delete own comments

**Implementation**:
```csharp
// Comments.cs
public int? ParentCommentId { get; set; }
public Comments? ParentComment { get; set; }
public ICollection<Comments> Replies { get; set; }
```

### 4. Admin Panel

**Capabilities**:
- View all reports (Pending/Reviewed/Dismissed)
- Resolve/dismiss reports
- Ban/unban users
- Delete posts
- Verify categories
- Approve access requests

**Access Control**: Only users with "Admin" role can access `/admin` route

### 5. Search Functionality

**Page-Specific Search**:
- **Home**: Search posts from followed categories
- **CategoryDetails**: Search posts in current category
- **Profile**: Search user's posts
- **Admin**: Search reports/users

**Conditional Display**: Search bar only appears on pages that support search

---

## Code Cleanup History

### Phase 2: Service Extraction
**Goal**: Eliminate duplicate data access logic

**Changes**:
- Created `PostService.cs` (500+ lines)
- Created `CategoryService.cs` (300+ lines)
- Refactored Home.razor to use PostService
- Refactored CategoryDetails.razor to use CategoryService

**Impact**: ~800 lines of duplicate code eliminated

### Phase 3: Component Reusability
**Goal**: Extract reusable UI components

**Changes**:
- Created `PostCard.razor` (reusable post display)
- Created `CommentSection.razor` (reusable comment thread)
- Created `CategoryHeader.razor` (category page header)
- Created `CarouselComponent.razor` (media display)

**Impact**: 
- Home.razor: 800 ? 520 lines (35% reduction)
- CategoryDetails.razor: 600 ? 400 lines (33% reduction)
- Admin.razor: 800 ? 350 lines (56% reduction)

**Total**: ~1,500 lines eliminated across all pages

---

## CSS Organization

### Structure

```
wwwroot/
??? app.css              # Global styles and Tailwind imports
??? styles/
    ??? layout.css       # MainLayout, grid system
    ??? shared.css       # Buttons, cards, form elements
    ??? feed.css         # PostCard, feed layout
    ??? admin.css        # Admin panel styles
    ??? header.css       # Header, search bar
    ??? modals.css       # Modal overlays
    ??? category.css     # Category pages
```

### Style Guidelines

**Naming Convention**: BEM-inspired
```css
/* Block */
.post-card { }

/* Element */
.post-card__header { }
.post-card__content { }

/* Modifier */
.post-card--highlighted { }
```

**Tailwind Usage**: Mixed approach
- Tailwind classes for utilities (margin, padding, flex)
- Custom CSS for complex components
- Loaded via CDN in `App.razor`

---

## Performance Considerations

### Database Queries

**Eager Loading**: Use `.Include()` to prevent N+1 queries
```csharp
var posts = await _context.Posts
    .Include(p => p.User)
    .Include(p => p.Category)
    .Include(p => p.PostLikes)
    .Include(p => p.Comments)
    .ToListAsync();
```

**Pagination**: Not yet implemented (recommended for large datasets)

### Blazor Rendering

**StateHasChanged()**: Called when data changes to trigger re-render
```csharp
await PostService.ToggleLikeAsync(postId, userId);
StateHasChanged(); // Update UI
```

**SignalR**: Not used (Blazor Server provides automatic updates)

---

## Security

### Authentication
- ASP.NET Core Identity for user management
- Cookie-based authentication
- Password requirements: 6+ chars, uppercase, lowercase, digit, special char

### Authorization
- Role-based access control (Admin, User)
- `[Authorize]` attribute on protected pages
- Service-level permission checks

### Security Fixes
- **January 2025**: Removed automatic admin role assignment during registration
- Only seeded `SoSuSaAdmin` account has admin access
- See `SECURITY.md` for details

---

## Development Workflow

### Adding a New Feature

1. **Create Domain Entity** (if needed)
   ```bash
   # Add entity class in Domain/
   dotnet ef migrations add AddYourEntity
   dotnet ef database update
   ```

2. **Create/Update Service** (if needed)
   ```csharp
   // Add methods to existing service or create new one
   public interface IYourService { }
   public class YourService : IYourService { }
   
   // Register in Program.cs
   builder.Services.AddScoped<IYourService, YourService>();
   ```

3. **Create Blazor Component**
   ```razor
   @page "/your-route"
   @inject IYourService YourService
   
   <h1>Your Page</h1>
   ```

4. **Add Styles** (optional)
   ```css
   /* wwwroot/styles/your-component.css */
   .your-component { }
   ```

5. **Test**
   - Build: `dotnet build`
   - Run: `dotnet run`
   - Test manually

### Database Changes

**Add Migration**:
```bash
dotnet ef migrations add YourMigrationName
```

**Apply Migration**:
```bash
dotnet ef database update
```

**Reset Database**:
```bash
dotnet ef database drop --force
dotnet ef database update
dotnet run  # Triggers seeding
```

---

## Testing Strategy

### Current Testing
- **Manual testing** using seeded data
- **Visual inspection** of UI
- **Database verification** via SQL Server Object Explorer

### Recommended Additions
- **Unit tests** for services (xUnit)
- **Integration tests** for data access
- **Component tests** for Blazor pages (bUnit)
- **End-to-end tests** (Selenium)

---

## Deployment Considerations

### Development
- LocalDB for database
- Seeding enabled
- Hardcoded admin password (`Admin@123`)
- Detailed error pages

### Production (Checklist)
- [ ] Change admin password (see `SECURITY.md`)
- [ ] Use environment variables for secrets
- [ ] Disable seeding
- [ ] Use production SQL Server
- [ ] Enable HTTPS redirect
- [ ] Configure CORS (if API accessed externally)
- [ ] Set up logging (Application Insights, Serilog)
- [ ] Enable error handling middleware
- [ ] Configure rate limiting
- [ ] Add CAPTCHA to registration

---

## Future Enhancements

### Planned Features
- **Pagination**: For posts and comments
- **Real-time notifications**: SignalR for likes/comments
- **File uploads**: User-uploaded images for posts
- **Two-factor authentication**: For admin accounts
- **Account lockout**: After failed login attempts
- **CAPTCHA**: On registration form
- **Rate limiting**: Prevent abuse
- **Audit logging**: Track admin actions

### Technical Improvements
- **Caching**: Redis for frequently accessed data
- **CDN**: For static assets
- **Separate API**: Extract API layer for mobile apps
- **Microservices**: Split admin panel into separate service
- **Message queue**: For background tasks (email, notifications)

---

## Dependencies

### NuGet Packages

```xml
<PackageReference Include="Microsoft.AspNetCore.Components.Authorization" Version="8.0.0" />
<PackageReference Include="Microsoft.AspNetCore.Identity.EntityFrameworkCore" Version="8.0.0" />
<PackageReference Include="Microsoft.EntityFrameworkCore" Version="8.0.0" />
<PackageReference Include="Microsoft.EntityFrameworkCore.SqlServer" Version="8.0.0" />
<PackageReference Include="Microsoft.EntityFrameworkCore.Tools" Version="8.0.0" />
```

### External Dependencies
- **Tailwind CSS**: Loaded via CDN (`https://cdn.tailwindcss.com`)
- **SQL Server LocalDB**: Development database

---

## Troubleshooting

### Common Issues

**Issue**: Empty home feed after seeding
- **Cause**: Feed only shows posts from followed categories
- **Solution**: Follow categories via sidebar

**Issue**: Build errors after pulling latest code
- **Solution**: `dotnet clean && dotnet restore && dotnet build`

**Issue**: Migration errors
- **Solution**: `dotnet ef database drop --force && dotnet ef database update`

**Issue**: Admin panel not accessible
- **Cause**: Not logged in as admin
- **Solution**: Login with `admin@sosusa.com` / `Admin@123`

---

## References

- **[SEEDING.md](SEEDING.md)** - Database seeding guide
- **[SECURITY.md](SECURITY.md)** - Security policies and fixes
- **[QUICKSTART.md](QUICKSTART.md)** - Developer onboarding guide
- **[BEST_PRACTICES.md](BEST_PRACTICES.md)** - Coding standards

---

**Last Updated**: January 2025  
**Version**: 1.0  
**Maintainers**: Development Team  
**License**: See repository
