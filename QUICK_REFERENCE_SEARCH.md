# Quick Reference - Page-Specific Search

## At a Glance

| Page | Search Location | Search Type | What It Searches | Limit |
|------|---|---|---|---|
| **Home** | Header bar | Posts | Post content + author username | 10 posts |
| **Profile** | Left sidebar | Users | Username + display name | 10 users |
| **Settings** | N/A | None | N/A | N/A |
| **Admin** | N/A | Tabs (not search) | N/A | N/A |

---

## Feature Comparison

### Home Page Search
```
Input: "python"
Results:
? "Learning python in 2024" by @dev_john
? "Python best practices" by @tech_guru  
? Posts mentioning "python" anywhere in content
? Posts by users with "python" in their username
? Ordered by newest first
? Real-time as you type
```

### Profile Page Search
```
Input: "john"
Results:
? @john_dev
? @john_codes
? @johnny_smith
? Anyone with "john" in username or display name
? Alphabetically ordered
? Real-time + button click triggers
```

### Settings (NO Search)
```
Menu Items Only:
?? Edit Profile
?? Change Password
?? Request Verification
```

---

## Code Changes Made

### Home.razor - Key Modifications
```csharp
// Added
private List<Posts> PostSearchResults = new();
private bool HasSearchedPosts = false;

// Modified
private async Task HandleHeaderSearch(KeyboardEventArgs e)
{
    // Now searches Posts instead of navigating to user
}

// Placeholder text
"Search posts..." // was "Search users..."
```

### Profile.razor - No Changes
```csharp
// Already had correct user search
// Left untouched - working as intended
```

---

## How It Works

### Step 1: User Types
```
Home page: [?? Search posts...] ? User types here
Profile page: [?? Search by username...] ? User types here
```

### Step 2: Real-Time Search Triggers
```
@onkeyup="HandleHeaderSearch"  // Home page
@onkeyup="HandleSearchKeypress" // Profile page
```

### Step 3: Database Query Executes
```csharp
// Home: Searches Posts table
WHERE Content.Contains("term") OR User.UserName.Contains("term")

// Profile: Searches Users table  
WHERE UserName.Contains("term") OR DisplayName.Contains("term")
```

### Step 4: Results Rendered
```
Home: [PostCard components for each result]
Profile: [User profile cards in sidebar]
```

---

## Testing Checklist

- [ ] Go to Home page, search "test" ? See posts about test
- [ ] Go to Profile page, search "john" ? See users named john
- [ ] Home search returns max 10 results
- [ ] Profile search returns max 10 results
- [ ] Settings page has NO search bar
- [ ] Results are in correct order (newest for posts, alphabetical for users)
- [ ] Can click Home post results (if implemented)
- [ ] Can click Profile user results to navigate

---

## Implementation Details

### Files Changed
- ? `Home.razor` - Post search added
- ? `Profile.razor` - User search verified
- ? Build - Successful

### Lines of Code Added
- ~25 lines for post search logic
- ~15 lines for state variables
- 0 breaking changes

### Database Calls
- Post search: 1 query per keystroke
- User search: 1 query per keystroke/click
- Both include related data efficiently

---

## Common Questions

**Q: Can I search posts on Profile page?**
A: No, Profile page is dedicated to user search only.

**Q: Can I search users on Home page?**
A: No, Home page is dedicated to post search.

**Q: Why 10 result limit?**
A: Prevents performance issues and keeps UI clean.

**Q: Is search case-sensitive?**
A: No, all searches are case-insensitive.

**Q: Does search happen immediately?**
A: Yes, real-time as you type.

**Q: What if no results found?**
A: Message displays "No [posts/users] found matching..."

---

## Visual Examples

### Home Page Search
```
???????????????????????????????????????????
? SoSuSa          [?? Search posts...] ?? ?
???????????????????????????????????????????
? Home  Profile  Settings                 ?
?                                         ?
? Searching: "blazor"                     ?
?                                         ?
? ??????????????????????????????????????? ?
? ? Getting Started with Blazor         ? ?
? ? By @john_dev in #webdev             ? ?
? ? 2 hours ago • ?? 23                  ? ?
? ??????????????????????????????????????? ?
?                                         ?
? ??????????????????????????????????????? ?
? ? Blazor Performance Tips              ? ?
? ? By @tech_guru in #coding            ? ?
? ? 5 hours ago • ?? 45                  ? ?
? ??????????????????????????????????????? ?
?                                         ?
???????????????????????????????????????????
```

### Profile Page Search
```
???????????????????????????????????????
? SoSuSa              Back to Home     ?
???????????????????????????????????????
? Search Users                        ?
? [?? Search by username...  ??]     ?
?                                     ?
? Results for "john"                  ?
?                                     ?
? ?? John Developer                   ?
?    @john_dev                        ?
?    123 posts | 456 followers        ?
?                                     ?
? ?? Johnny Smith                     ?
?    @johnny_smith                    ?
?    45 posts | 123 followers         ?
?                                     ?
? ?? John's Web Dev                   ?
?    @john_webdev                     ?
?    78 posts | 234 followers         ?
?                                     ?
???????????????????????????????????????
```

---

## Deployment Checklist

Before going live:
- [ ] Build runs without errors
- [ ] Manual testing complete
- [ ] Database queries perform well
- [ ] No console errors in browser
- [ ] Mobile responsive (if applicable)
- [ ] Accessibility tested
- [ ] Documentation updated

---

*This is the definitive reference for the page-specific search implementation.*
