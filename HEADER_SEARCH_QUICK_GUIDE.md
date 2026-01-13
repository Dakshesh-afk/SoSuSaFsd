# Header Search - Quick Guide

## Visual Layout

```
???????????????????????????????????????????????????????????????????????
?  SoSuSa    Home  Profile  Settings    [Search posts... ??]  Admin... ?
???????????????????????????????????????????????????????????????????????
                                          ?
                        ????????????????????????????????
                        ? Search Results (5)           ?
                        ????????????????????????????????
                        ? ?? John Dev                  ?
                        ? "Getting started with..."   ?
                        ? #webdev • Mar 15             ?
                        ????????????????????????????????
                        ? ?? Tech Guru                 ?
                        ? "Best practices for..."      ?
                        ? #coding • Mar 12             ?
                        ????????????????????????????????
```

---

## Component Structure

```
Header
??? .header-right
    ??? .header-search-wrapper
        ??? <input> Search posts...
        ??? <button> ??
```

---

## CSS Structure

```css
.header-search-wrapper {
    /* Container for input + button */
    display: flex;
    background: #f5f5f5;
    border: 1px solid #ccc;
    border-radius: 20px;
}

.search-bar {
    /* Input field inside wrapper */
    border: none;
    background: transparent;
    padding: 8px 15px;
}

.header-search-btn {
    /* Search button */
    background: transparent;
    border: none;
    color: #666;
    cursor: pointer;
}
```

---

## User Interaction Flow

```
User starts typing in search box
           ?
    oninput event fires
           ?
    HandleHeaderSearch() called
           ?
    Database query executes
           ?
    PostSearchResults populated
           ?
    Dropdown renders with results
           ?
    User can see matching posts
           ?
    User clears search
           ?
    Dropdown auto-hides
```

---

## Search Methods

### 1. Real-Time (As User Types)
```html
@onkeyup="HandleHeaderSearch"
@bind:event="oninput"
```
Triggers search on every keystroke

### 2. Button Click
```html
@onclick="() => HandleHeaderSearch(null)"
```
Click the search icon button

### 3. Enter Key
```html
@onkeyup="HandleHeaderSearch"
```
Pressing Enter triggers search (same handler)

---

## Search Results Display

### Dropdown Structure
```
?? Search Results (Count) ??
????????????????????????????
? [Avatar] Author          ?
?          Content preview  ?
?          #category • date ?
????????????????????????????
? [Avatar] Author 2        ?
?          Content preview  ?
?          #category • date ?
????????????????????????????
```

### Data Shown Per Result
- User avatar (40px)
- User display name or username
- Post content (first 60 chars)
- Category name
- Post date (short format)

---

## Search Behavior

| Action | Behavior |
|--------|----------|
| Type in search | Real-time results appear |
| Press Enter | Results update/displayed |
| Click button | Search executes |
| Clear search | Dropdown hides |
| Empty field | No results shown |
| No matches | "No posts found" message |

---

## Code Entry Points

### React to Search Input
```csharp
private async Task HandleHeaderSearch(KeyboardEventArgs? e = null)
{
    // e is null when button clicked
    // e is KeyboardEventArgs when typing
}
```

### Update Results
```csharp
PostSearchResults = await context.Posts
    .Where(p => p.Content.ToLower().Contains(searchTerm) || 
               p.User.UserName.ToLower().Contains(searchTerm))
    .Take(10)
    .ToListAsync();
```

### Trigger UI Update
```csharp
StateHasChanged();  // Render new results
```

---

## Styling Details

### Colors
- **Background**: #f5f5f5 (light gray)
- **Border**: #ccc (medium gray)
- **Text**: #666 (dark gray)
- **Hover**: #333 (darker)

### Dimensions
- **Height**: 40px (8px padding + icons)
- **Border-radius**: 20px (pill shape)
- **Avatar**: 40x40px (circular)
- **Dropdown width**: 400px
- **Max height**: 400px (scrollable)

### Z-Index
- Dropdown: z-index 100 (above most content)
- Header: z-index 1000 (always on top)

---

## Browser Compatibility

? Chrome/Edge
? Firefox
? Safari
? Mobile browsers

---

## Common Issues & Fixes

| Issue | Cause | Fix |
|-------|-------|-----|
| Search doesn't work | Missing StateHasChanged() | Call StateHasChanged() after query |
| Button doesn't appear | CSS not loaded | Check header.css is linked |
| Results don't show | Query returns empty | Check database has posts |
| Dropdown overlaps | Z-index conflict | Increase z-index value |
| Slow search | N+1 query problem | Use .Include() for relations |

---

## Performance Notes

- **Limit**: 10 results per search (prevents lag)
- **Query**: Executed on every keystroke (real-time)
- **Includes**: Related data loaded (User, Category, Media, Likes)
- **Database**: Posts table searched (should have index on Content, UserName)

### Optimization Recommendations

1. Add database index on Posts.Content
2. Add database index on Users.UserName
3. Implement debouncing (delay search 300ms after typing stops)
4. Cache popular search terms
5. Use full-text search for large datasets

---

*Last Updated: Current Implementation*
