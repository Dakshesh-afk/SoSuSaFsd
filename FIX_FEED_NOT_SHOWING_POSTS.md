# Fix: Feed Not Showing Posts from Followed Categories

## Problem
When users followed a category on the CategoryDetails page and navigated back to Home, the feed was not displaying posts from the newly followed categories. The feed appeared empty even though posts existed in those categories.

## Root Cause
The Home.razor page was only loading the feed data once during initial component initialization (`OnInitializedAsync`). When a user navigated to another page (like CategoryDetails) and followed a category there, the Home page didn't reload to fetch the updated list of followed categories and their posts.

In Blazor Server, when navigating back to a previously visited page, the component lifecycle methods may not be called again, causing the page to display stale data.

## Solution
Implemented proper lifecycle management by:

### 1. **Added `OnParametersSetAsync` Override**
```csharp
protected override async Task OnParametersSetAsync()
{
    // Reload data when returning to this page
    await base.OnParametersSetAsync();
    await LoadUserAndCategories();
}
```

This ensures that whenever the component parameters are set (which includes when navigating to the page), the data is reloaded.

### 2. **Enhanced `LoadUserAndCategories` Method**
- Added explicit null checking: `.Where(c => c != null)` after selecting categories
- Made the followed categories query more robust with explicit includes
- Properly filters posts by followed category IDs

```csharp
FollowedCategories = await context.CategoryFollows
    .Where(cf => cf.UserId == userId)
    .Include(cf => cf.Category)
    .Select(cf => cf.Category!)
    .Where(c => c != null)  // Ensure null categories are filtered
    .ToListAsync();

var followedCategoryIds = FollowedCategories.Select(c => c.Id).ToList();

if (followedCategoryIds.Any())
{
    FeedPosts = await context.Posts
        .Where(p => followedCategoryIds.Contains(p.CategoryId))
        .Include(p => p.User)
        .Include(p => p.Category)
        .Include(p => p.Media)
        .Include(p => p.Likes)
        .OrderByDescending(p => p.DateCreated)
        .ToListAsync();
}
```

### 3. **Added Optional Visibility Change Detection**
Added `OnAfterRenderAsync` and `OnPageVisibilityChanged` methods to optionally refresh the feed when the page comes back into focus (browser tab visibility change).

## Files Modified
- `SoSuSaFsd/Components/Pages/Home.razor`

## Testing
1. ? Build passes without errors
2. ? Feed correctly loads followed categories
3. ? Posts from followed categories now appear in the home feed
4. ? Following/unfollowing categories updates the feed properly

## How It Works Now
1. User navigates to Home page ? `OnParametersSetAsync` loads all data
2. User follows a category in CategoryDetails ? followed category is saved to database
3. User navigates back to Home ? `OnParametersSetAsync` is called again
4. Feed is reloaded with the new followed categories
5. Posts from all followed categories now display in the home feed

## Additional Benefits
- More robust data loading
- Better handling of navigation between pages
- Feed stays in sync with user's category follows
- No duplicate code - reuses existing `LoadUserAndCategories` method
