# ?? Best Practices Guide for SoSuSaFsd

## Overview
This guide outlines best practices for maintaining and extending your Blazor application going forward.

---

## 1?? Project Structure

### Recommended Folder Organization:
```
SoSuSaFsd/
??? Components/
?   ??? Pages/
?   ?   ??? Home.razor          (Main feed page)
?   ?   ??? CategoryDetails.razor (Category page)
?   ?   ??? Admin.razor         (Admin dashboard)
?   ??? Common/                 ? NEW: Reusable components
?   ?   ??? PostCard.razor
?   ?   ??? CommentSection.razor
?   ?   ??? CarouselComponent.razor
?   ?   ??? ProfileCard.razor
?   ?   ??? Modals/
?   ?       ??? ReportModal.razor
?   ?       ??? NotificationModal.razor
?   ?       ??? ProfileImageCropModal.razor
?   ??? Layout/
?   ??? ...
??? Services/                   ? NEW: Business logic
?   ??? PostService.cs
?   ??? CategoryService.cs
?   ??? CommentService.cs
?   ??? UserService.cs
?   ??? ReportService.cs
?   ??? IServiceBase.cs         (Interface/Base class)
??? Data/
?   ??? SoSuSaFsdContext.cs
??? Domain/
?   ??? *.cs                    (Models)
??? Controllers/
?   ??? AccountController.cs
??? wwwroot/
?   ??? styles/                 ? NEW: Modular CSS
?       ??? shared.css
?       ??? header.css
?       ??? layout.css
?       ??? feed.css
?       ??? modals.css
??? ...
```

---

## 2?? Component Guidelines

### ? DO:
- Keep components under 500 lines (including @code)
- Use `[Parameter]` for props
- Use `[Parameter] EventCallback<T>` for events
- Extract UI logic to separate components
- Name components descriptively

### ? DON'T:
- Mix multiple features in one component
- Put database logic in components
- Have inline styles over 50 lines
- Duplicate UI patterns across pages
- Use magic strings

### ?? Example Component:

```razor
@* Components/Common/PostCard.razor *@
@using SoSuSaFsd.Domain

<div class="post-card">
    <div class="post-header">
        <div class="post-avatar" style="background-image: url('@(Post.User?.ProfileImage ?? "")');"></div>
        <div>
            <b>@(Post.User?.UserName ?? "User")</b>
            <div class="post-meta">
                <span>@Post.DateCreated.ToString("MMM dd, yyyy • HH:mm")</span>
            </div>
        </div>
    </div>
    
    <div class="post-content">@Post.Content</div>

    <div class="post-actions">
        <button class="action-btn" @onclick="() => OnLike.InvokeAsync(Post.Id)">
            <i class="fas fa-heart"></i> Like
        </button>
        <button class="action-btn" @onclick="() => OnComment.InvokeAsync(Post.Id)">
            <i class="fas fa-comment"></i> Comment
        </button>
    </div>
</div>

@code {
    [Parameter] public Posts Post { get; set; } = null!;
    [Parameter] public EventCallback<int> OnLike { get; set; }
    [Parameter] public EventCallback<int> OnComment { get; set; }
    [Parameter] public Users? CurrentUser { get; set; }
}
```

---

## 3?? Service Guidelines

### ? DO:
- Put all database logic in services
- Inject services via constructor
- Use dependency injection
- Make services async-ready
- Handle exceptions gracefully

### ? DON'T:
- Have raw database queries in components
- Create new DbContext instances unnecessarily
- Ignore error handling
- Mix different domains in one service

### ?? Example Service:

```csharp
// Services/PostService.cs
using SoSuSaFsd.Data;
using SoSuSaFsd.Domain;
using Microsoft.EntityFrameworkCore;

public class PostService
{
    private readonly IDbContextFactory<SoSuSaFsdContext> _dbFactory;

    public PostService(IDbContextFactory<SoSuSaFsdContext> dbFactory)
    {
        _dbFactory = dbFactory;
    }

    public async Task<List<Posts>> GetUserFeedAsync(string userId)
    {
        using var context = _dbFactory.CreateDbContext();
        
        var followedCategoryIds = await context.CategoryFollows
            .Where(cf => cf.UserId == userId)
            .Select(cf => cf.CategoryId)
            .ToListAsync();

        return await context.Posts
            .Where(p => followedCategoryIds.Contains(p.CategoryId))
            .Include(p => p.User)
            .Include(p => p.Category)
            .Include(p => p.Media)
            .Include(p => p.Likes)
            .OrderByDescending(p => p.DateCreated)
            .ToListAsync();
    }

    public async Task<bool> ToggleLikeAsync(int postId, string userId)
    {
        try
        {
            using var context = _dbFactory.CreateDbContext();
            
            var existingLike = await context.PostLikes
                .FirstOrDefaultAsync(l => l.PostID == postId && l.UserID == userId);

            if (existingLike != null)
            {
                context.PostLikes.Remove(existingLike);
            }
            else
            {
                context.PostLikes.Add(new PostLikes
                {
                    PostID = postId,
                    UserID = userId,
                    LikedAt = DateTime.Now
                });
            }

            await context.SaveChangesAsync();
            return true;
        }
        catch (Exception ex)
        {
            Console.WriteLine($"Error toggling like: {ex.Message}");
            return false;
        }
    }
}
```

### ?? Register Service in Program.cs:
```csharp
builder.Services.AddScoped<PostService>();
builder.Services.AddScoped<CategoryService>();
// ... other services
```

### ?? Use Service in Component:
```razor
@inject PostService PostService

@code {
    private List<Posts> FeedPosts = new();

    protected override async Task OnInitializedAsync()
    {
        FeedPosts = await PostService.GetUserFeedAsync(currentUserId);
    }

    private async Task ToggleLike(int postId)
    {
        var success = await PostService.ToggleLikeAsync(postId, currentUserId);
        if (success) StateHasChanged();
    }
}
```

---

## 4?? CSS Guidelines

### ? DO:
- Group related styles in same file
- Use semantic class names
- Keep files under 400 lines
- Use CSS variables for colors
- Maintain consistent naming

### ? DON'T:
- Mix different features in one CSS file
- Use inline styles for complex styling
- Use hard-coded colors (use variables)
- Have conflicting style rules

### ?? Example CSS Organization:

```css
/* wwwroot/styles/feed.css */

/* === POST CARDS === */
.post-card { ... }
.post-header { ... }
.post-avatar { ... }
.post-content { ... }
.post-actions { ... }
.action-btn { ... }

/* === CAROUSEL === */
.carousel-container { ... }
.carousel-btn { ... }

/* === COMMENTS === */
.comment-section { ... }
.comment-item { ... }
.comment-input { ... }
```

---

## 5?? Error Handling

### ? DO:
- Always use try-catch for database operations
- Log errors to console (for now)
- Show user-friendly error messages
- Handle null references gracefully

### ? DON'T:
- Ignore exceptions silently
- Show technical error messages to users
- Allow nulls to propagate uncaught

### ?? Example:

```csharp
private async Task DeletePost(int postId)
{
    try
    {
        if (currentUser == null)
        {
            ShowNotification("Please log in first", "error");
            return;
        }

        using var context = _dbFactory.CreateDbContext();
        var post = await context.Posts.FindAsync(postId);
        
        if (post == null)
        {
            ShowNotification("Post not found", "error");
            return;
        }

        if (post.UserId != currentUser.Id)
        {
            ShowNotification("You can only delete your own posts", "error");
            return;
        }

        context.Posts.Remove(post);
        await context.SaveChangesAsync();
        
        ShowNotification("Post deleted successfully", "success");
    }
    catch (Exception ex)
    {
        Console.WriteLine($"Error deleting post: {ex.Message}");
        ShowNotification("An error occurred while deleting the post", "error");
    }
}
```

---

## 6?? State Management

### Current Pattern (OK for small apps):
```csharp
@code {
    private string searchTerm = "";
    private List<Posts> FeedPosts = new();
    private Dictionary<int, bool> _showComments = new();
}
```

### For Larger Apps, Consider:
- **State Container Pattern** - Centralized state
- **Cascading Parameters** - Pass state down
- **Services** - Manage state in services

### ?? Simple State Container Example:

```csharp
// Services/AppState.cs
public class AppState
{
    public string CurrentUserId { get; set; } = "";
    public Users? CurrentUser { get; set; }
    
    public event Action? OnStateChanged;
    
    public void NotifyStateChanged()
    {
        OnStateChanged?.Invoke();
    }
}

// In Program.cs:
builder.Services.AddScoped<AppState>();

// In Component:
@inject AppState AppState

@code {
    protected override void OnInitialized()
    {
        AppState.OnStateChanged += StateHasChanged;
    }
}
```

---

## 7?? Performance Tips

### ? DO:
- Use `@key` directive for lists
- Lazy load data when possible
- Cache computed values
- Use `OnParametersSet` for parent changes
- Implement `IAsyncDisposable` for cleanup

### ? DON'T:
- Load all data at once
- Requery the same data repeatedly
- Ignore rendering performance
- Create unnecessary components

### ?? Example with @key:

```razor
@foreach (var post in Posts)
{
    @* @key ensures component state persists correctly *@
    <PostCard @key="post.Id" Post="post" />
}
```

---

## 8?? Common Patterns

### Loading State:
```csharp
private bool isLoading = false;

protected override async Task OnInitializedAsync()
{
    isLoading = true;
    try
    {
        // Load data
    }
    finally
    {
        isLoading = false;
    }
}
```

### Modal Management:
```csharp
private bool showModal = false;
private void OpenModal() => showModal = true;
private void CloseModal() => showModal = false;
```

### Notification System:
```csharp
private string notificationMessage = "";
private string notificationType = "success"; // success, error, info

private void ShowNotification(string message, string type)
{
    notificationMessage = message;
    notificationType = type;
    StateHasChanged();
}
```

---

## 9?? Testing Readiness

To make code easier to test:
- Extract logic to services
- Inject dependencies
- Avoid tight coupling
- Use interfaces

```csharp
// ? Hard to test
public class HomeComponent
{
    private void LoadData()
    {
        var context = new SoSuSaFsdContext();
        var posts = context.Posts.ToList();
    }
}

// ? Easy to test
public class PostService
{
    private readonly IDbContextFactory<SoSuSaFsdContext> _dbFactory;
    
    public async Task<List<Posts>> GetPostsAsync() { }
}

public class HomeComponent
{
    [Inject] public PostService PostService { get; set; } = null!;
    
    protected override async Task OnInitializedAsync()
    {
        var posts = await PostService.GetPostsAsync();
    }
}
```

---

## ?? Next Steps

1. **Immediate** (This week)
   - [ ] Create `PostCard.razor` component
   - [ ] Create `CommentSection.razor` component
   - [ ] Update Home.razor to use new components

2. **Short-term** (Next 2 weeks)
   - [ ] Create `PostService.cs`
   - [ ] Extract database logic from pages
   - [ ] Create other services (Category, Comment, User, Report)

3. **Medium-term** (Next month)
   - [ ] Refactor CategoryDetails.razor using new components
   - [ ] Add error handling throughout
   - [ ] Add loading states
   - [ ] Consider state container for complex state

4. **Long-term**
   - [ ] Add unit tests
   - [ ] Add integration tests
   - [ ] Consider advanced state management library
   - [ ] Performance optimization

---

## ?? Resources

- [Blazor Documentation](https://docs.microsoft.com/en-us/aspnet/core/blazor/)
- [Entity Framework Core](https://docs.microsoft.com/en-us/ef/core/)
- [Clean Code Principles](https://en.wikipedia.org/wiki/SOLID)
- [Dependency Injection in .NET](https://docs.microsoft.com/en-us/dotnet/core/extensions/dependency-injection)

---

**Last Updated:** January 2025
**Version:** 1.0
**Status:** ? Active
