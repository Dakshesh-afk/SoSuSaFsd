# CategoryDetails.razor - Professional Code Guidelines

## Code Style & Conventions

### 1. Method Organization

**Always organize methods in logical groups with section headers:**
```csharp
@code {
    // ============ SECTION NAME ============
    // Related methods grouped together
    
    private void MethodOne() { }
    private void MethodTwo() { }
}
```

### 2. Variable Naming

**Follow C# naming conventions:**
```csharp
// ? Good
private bool isFollowing;
private string currentUserId;
private List<Categories> FollowedCategories = new();

// ? Avoid
private bool ifollowing;
private string cuserid;
private List<Categories> followedcategories = new();
```

### 3. CSS Classes vs Inline Styles

**Always use CSS classes:**
```razor
<!-- ? Good -->
<div class="alert alert-error">
    <strong>Error:</strong> @errorMessage
</div>

<!-- ? Avoid -->
<div style="color: #721c24; background-color: #f8d7da; border: 1px solid #f5c6cb; padding: 10px;">
    <strong>Error:</strong> @errorMessage
</div>
```

### 4. Method Size

**Keep methods focused and small (< 30 lines ideal):**
```csharp
// ? Good - Clear single purpose
private async Task LoadComments(int postId)
{
    if (!_postComments.ContainsKey(postId) || _postComments[postId] == null)
    {
        try
        {
            using var context = DbFactory.CreateDbContext();
            var comments = await context.Comments
                .Where(c => c.PostID == postId)
                .Include(c => c.User)
                .OrderBy(c => c.DateCreated)
                .ToListAsync();
            _postComments[postId] = comments;
        }
        catch (Exception ex)
        {
            Console.WriteLine("Error loading comments: " + ex.Message);
        }
    }
}

// ? Avoid - Multiple responsibilities mixed
private async Task DoEverything()
{
    // ... 100 lines of mixed logic
}
```

### 5. Comments & Documentation

**Use section headers for clarity:**
```csharp
// ============ INITIALIZATION ============
private async Task InitializePageData() { }

// ============ COMMENTS ============
private async Task ToggleComments(int postId) { }
```

**Avoid excessive inline comments:**
```csharp
// ? Good - Code is self-explanatory
var emailUser = await userManager.FindByEmailAsync(email);

// ? Avoid - Obvious comments clutter
// Find user by email
var emailUser = await userManager.FindByEmailAsync(email);
```

### 6. Error Handling

**Always handle exceptions appropriately:**
```csharp
// ? Good - Specific exception handling
try
{
    using var context = DbFactory.CreateDbContext();
    // ... database operations
    await context.SaveChangesAsync();
}
catch (DbUpdateException ex)
{
    Console.WriteLine($"Database error: {ex.Message}");
    errorMessage = "Failed to save changes.";
}
catch (Exception ex)
{
    Console.WriteLine($"Unexpected error: {ex.Message}");
    errorMessage = "An error occurred.";
}

// ? Avoid - Silent failures
try { /* code */ }
catch { }
```

### 7. Null Checks

**Use consistent null checking patterns:**
```csharp
// ? Good - Explicit and clear
if (string.IsNullOrEmpty(currentUserId))
{
    ShowLoginPopup("action");
    return;
}

if (CurrentCategory != null)
{
    Title = "#" + CurrentCategory.CategoryName;
}

// ? Avoid - Inconsistent patterns
if (currentUserId == null || currentUserId == "")
if (CurrentCategory == null)
```

### 8. LINQ Usage

**Use LINQ fluently and consistently:**
```csharp
// ? Good - Readable LINQ chain
var posts = await context.Posts
    .Where(p => p.CategoryId == Id)
    .Include(p => p.User)
    .Include(p => p.Media)
    .Include(p => p.Likes)
    .OrderByDescending(p => p.DateCreated)
    .ToListAsync();

// ? Avoid - Hard to read
var posts = (from p in context.Posts where p.CategoryId == Id select p).ToList();
```

### 9. Async/Await

**Always use async properly:**
```csharp
// ? Good - Properly awaited
private async Task LoadData()
{
    using var context = DbFactory.CreateDbContext();
    var data = await context.Data.ToListAsync();
    // Use data
}

// ? Avoid - Not awaited
private async Task LoadData()
{
    using var context = DbFactory.CreateDbContext();
    var data = context.Data.ToList();  // Blocks!
}
```

### 10. Constants

**Define magic numbers as named constants:**
```csharp
// ? Good
private const long MaxFileSize = 1024 * 1024 * 200;  // 200MB
private const int MaxAllowedFiles = 10;

// ? Avoid
private const long MaxFileSize = 209715200;
private const int MaxAllowedFiles = 10;
```

## Razor Component Best Practices

### 1. Component Structure

**Follow this order in Razor components:**
```razor
@page "/route"
@rendermode InteractiveServer
@* Using statements *@
@* Inject statements *@

<PageTitle>@Title</PageTitle>
<link rel="stylesheet" href="/styles/component.css" />

@* Template sections *@
<header></header>
<main></main>
<aside></aside>

@* Modals *@
@if (showModal) { }

@code {
    // Code section
}
```

### 2. Modal Pattern

**Use consistent modal structure:**
```razor
@if (showModal) 
{ 
    <div class="modal">
        <div class="modal-content">
            <span class="modal-close" @onclick="() => showModal = false">×</span>
            <h2>Modal Title</h2>
            <div class="modal-subtitle">Subtitle if needed</div>
            
            <!-- Content -->
            
            <div class="modal-actions">
                <button class="login-btn secondary" @onclick="CloseModal">Cancel</button>
                <button class="login-btn" @onclick="HandleSubmit">Submit</button>
            </div>
        </div>
    </div> 
}
```

### 3. Event Handlers

**Use proper event binding syntax:**
```razor
<!-- ? Good -->
<button @onclick="HandleClick">Click me</button>
<button @onclick="() => HandleClickWithArg(123)">Click with arg</button>
<a href="#" @onclick:preventDefault @onclick="NavigateToCategory">Navigate</a>

<!-- ? Avoid -->
<button onclick="javascript:alert('clicked')">Click me</button>
<button @onclick="HandleClickWithArg(123)">Click with arg</button>
```

### 4. Conditional Rendering

**Keep conditions simple and readable:**
```razor
<!-- ? Good -->
@if (isFollowing)
{
    <button class="follow-btn following">Unfollow</button>
}
else
{
    <button class="follow-btn not-following">Follow</button>
}

<!-- ? Avoid -->
<button class="follow-btn @(isFollowing ? "following" : "not-following")">@(isFollowing ? "Unfollow" : "Follow")</button>
```

### 5. Data Binding

**Use proper binding syntax:**
```razor
<!-- ? Good -->
<input type="text" @bind="searchTerm" @bind:event="oninput" />
<select @bind="reportReasonSelection">
    <option value="Spam">Spam</option>
</select>

<!-- ? Avoid -->
<input type="text" value="@searchTerm" @onchange="@(e => searchTerm = e.Value?.ToString())" />
```

## CSS Best Practices

### 1. Variable Usage

**Always use CSS variables for consistent theming:**
```css
/* ? Good */
.button {
    background-color: var(--primary-color);
    color: white;
    transition: background-color 0.2s;
}

.button:hover {
    background-color: var(--primary-hover);
}

/* ? Avoid */
.button {
    background-color: #333;
    color: white;
    transition: background-color 0.2s;
}

.button:hover {
    background-color: #000;
}
```

### 2. Class Naming

**Use descriptive, semantic class names:**
```css
/* ? Good */
.comment-section { }
.comment-avatar { }
.comment-text { }

/* ? Avoid */
.cs { }
.ca { }
.ct { }
```

### 3. Responsive Design

**Design mobile-first approach:**
```css
/* ? Good */
.sidebar {
    width: 280px;
}

@media (max-width: 768px) {
    .sidebar {
        width: 100%;
    }
}

/* ? Avoid */
.sidebar {
    width: 100%;
}

@media (min-width: 768px) {
    .sidebar {
        width: 280px;
    }
}
```

## Testing Recommendations

### 1. Unit Test Methods

```csharp
[Test]
public async Task ToggleLike_WithoutUserId_ShowsLoginPopup()
{
    // Arrange
    currentUserId = null;
    var postId = 1;
    
    // Act
    await component.ToggleLike(postId);
    
    // Assert
    Assert.IsTrue(component.ShowLoginOverlay);
}
```

### 2. Integration Tests

```csharp
[Test]
public async Task LoadData_LoadsAllCategories()
{
    // Arrange
    var component = RenderComponent<CategoryDetails>(
        parameters => parameters.Add(p => p.Id, 1)
    );
    
    // Act
    await component.InvokeAsync(() => component.Instance.InitializePageData());
    
    // Assert
    Assert.IsNotNull(component.Instance.CurrentCategory);
}
```

## Performance Considerations

1. **Minimize Re-renders**: Only call `StateHasChanged()` when necessary
2. **Lazy Load Data**: Load comments/media only when needed
3. **Cache Results**: Store already-loaded data in dictionaries
4. **Use Keys in Loops**: Add `@key` directive for better rendering
5. **Async All the Way**: Use async/await for I/O operations

## Summary

Professional code is:
- ? **Readable**: Others can understand it at a glance
- ? **Maintainable**: Easy to modify and extend
- ? **Tested**: Has comprehensive test coverage
- ? **Documented**: Clear intent and purpose
- ? **Consistent**: Follows established patterns
- ? **Performant**: Optimized for user experience
