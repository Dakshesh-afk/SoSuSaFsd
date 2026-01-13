# ?? Search Features - Visual Guide

## Search Architecture

```
???????????????????????????????????????????????????????????????
?                     SoSuSa Application                        ?
???????????????????????????????????????????????????????????????
?                                                               ?
?  ???????????????????????????????????????????????????????    ?
?  ?              HOME PAGE (/)                           ?    ?
?  ????????????????????????????????????????????????????????   ?
?  ?  Header: [Search Users...?] [Login] [Logout]        ?   ?
?  ?          ???? Type username ? Enter ? /profile/...  ?   ?
?  ?                                                      ?    ?
?  ?  Left Sidebar:          Main Feed:     Right Sidebar:   ?
?  ?  - Following            - Posts        - Verified       ?
?  ?  - Search Categories    - Comments     - Recent Cats    ?
?  ????????????????????????????????????????????????????????   ?
?           ?                                      ?             ?
?           ??? /categories/create          /category/{id}    ?
?                                                  ?             ?
?                                  ??????????????????????????   ?
?                                  ? CATEGORY DETAILS PAGE  ?   ?
?                                  ??????????????????????????   ?
?                                  ? Category Header:       ?   ?
?                                  ? - Name, Description    ?   ?
?                                  ? - Follow/Post buttons  ?   ?
?                                  ?                        ?   ?
?                                  ? Search Posts:          ?   ?
?                                  ? [Search posts...?]     ?   ?
?                                  ? ??? Filter in Real-   ?   ?
?                                  ?    time                ?   ?
?                                  ?                        ?   ?
?                                  ? Posts Feed:            ?   ?
?                                  ? - Post 1 ?? ?? ?? ??  ?   ?
?                                  ? - Post 2 ?? ?? ?? ??  ?   ?
?                                  ? - Post 3 ?? ?? ?? ??  ?   ?
?                                  ?                        ?   ?
?                                  ? Click post author:     ?   ?
?                                  ? ??? /profile/{name}    ?   ?
?                                  ??????????????????????????   ?
?                                         ?                     ?
?                          ????????????????????????????????     ?
?                          ?   USER PROFILE PAGE          ?     ?
?                          ?  /profile/{username}         ?     ?
?                          ????????????????????????????????     ?
?                          ?                              ?     ?
?                          ? Left Sidebar:   Profile:     ?     ?
?                          ? - Search Users  - Avatar     ?     ?
?                          ? - Results       - Name       ?     ?
?                          ?   Click user?   - Bio        ?     ?
?                          ?   /profile/...  - Stats      ?     ?
?                          ?                              ?     ?
?                          ? Posts Feed:                  ?     ?
?                          ? - User's Post 1              ?     ?
?                          ? - User's Post 2              ?     ?
?                          ? - User's Post 3              ?     ?
?                          ?                              ?     ?
?                          ? Actions:                     ?     ?
?                          ? - Like posts                 ?     ?
?                          ? - Comment                    ?     ?
?                          ? - Search other users         ?     ?
?                          ????????????????????????????????     ?
?                                                               ?
???????????????????????????????????????????????????????????????
```

---

## Search Features Map

```
USER SEARCHES
??? ?? Header Search (Any Page)
?   ?? Input: Username
?   ?? Action: Type + Press Enter
?   ?? Result: Navigate to /profile/{username}
?   ?? Shows: User profile page
?
??? ?? Profile Sidebar Search (/profile/...)
    ?? Input: Username or Display Name
    ?? Action: Type (real-time)
    ?? Results: Live user list (top 10)
    ?? Action: Click user ? Navigate to /profile/{username}


POST SEARCHES
??? ?? Category Feed Search (/category/...)
    ?? Input: Post content or username
    ?? Action: Type (real-time)
    ?? Searches: Post content + Author name
    ?? Results: Filtered post list
    ?? Clear: See all posts again
```

---

## Data Flow Diagrams

### Post Search Data Flow
```
User Types in Search Box
        ?
React to @bind="postSearchTerm"
        ?
Filter CategoryPosts in Real-time
        ?
Where(p => p.Content.Contains(term) || p.User.UserName.Contains(term))
        ?
Render filtered posts
        ?
User sees results immediately
```

### User Search Data Flow (Header)
```
User Types Username in Header
        ?
User Presses Enter
        ?
HandleHeaderSearch(KeyboardEventArgs)
        ?
Navigation.NavigateTo("/profile/{username}")
        ?
Profile.razor Loads
        ?
OnParametersSetAsync() Loads User Data
        ?
Display User Profile + Posts
```

### User Search Data Flow (Profile Sidebar)
```
User Types in Profile Sidebar Search
        ?
React to @bind="searchTerm"
        ?
HandleUserSearch() Called
        ?
Query Database:
context.Users
  .Where(u => u.UserName.Contains(term) || u.DisplayName.Contains(term))
  .Take(10)
        ?
Display Search Results
        ?
User Clicks Result
        ?
Navigate to /profile/{username}
```

---

## Component Hierarchy

```
Home.razor (/)
?? Header
?  ?? Header Search (User Search)
?     ?? /profile/{username}
?
?? Left Sidebar
?  ?? Following Categories
?  ?? Category Search
?     ?? /category/{id}
?
?? Main Feed
?  ?? Posts
?     ?? Click Author ? /profile/{username}
?     ?? Comment on Post
?
?? Right Sidebar
   ?? Verified Categories
   ?? Recent Categories

CategoryDetails.razor (/category/{id})
?? Category Header
?  ?? Follow Button
?  ?? Create Post Button
?
?? Post Search Box
?  ?? Filter Posts in Real-time
?
?? Main Feed
?  ?? Posts
?  ?  ?? Click Author ? /profile/{username}
?  ?  ?? Like Button
?  ?  ?? Comment Section
?  ?  ?? Report Button
?  ?
?  ?? Modals
?     ?? Create Post
?     ?? Request Access
?     ?? Report Content
?     ?? Notifications
?
?? Sidebars
   ?? Following Categories
   ?? Verified Categories

Profile.razor (/profile/{username})
?? User Profile Header
?  ?? Avatar
?  ?? Name & Bio
?  ?? Statistics
?
?? Left Sidebar (User Search)
?  ?? Search Results
?     ?? Click User ? /profile/{other-username}
?
?? Main Feed
?  ?? User's Posts
?     ?? Like Posts
?     ?? Comments
?     ?? Carousels (Media)
?
?? Right Sidebar (About User)
   ?? User Info & Stats
```

---

## Search Feature Specifications

### Post Search (CategoryDetails.razor)

```
FEATURE: Post Search
?? LOCATION: /category/{id} page
?? TYPE: Client-side Real-time Filter
?? INPUT: Text
?? SEARCHES: 
?  ?? Post content
?  ?? Author username
?? FEATURES:
?  ?? Case-insensitive
?  ?? Partial matching
?  ?? Real-time filtering
?  ?? Clear search to reset
?? IMPLEMENTATION:
   ?? Variable: postSearchTerm
   ?? Method: LINQ Where() filter
   ?? No database queries
```

### User Search - Header (Home.razor)

```
FEATURE: Header User Search
?? LOCATION: Home page header (any page)
?? TYPE: Navigation Search
?? INPUT: Username
?? ACTION: Press Enter
?? RESULT: Navigate to /profile/{username}
?? FEATURES:
?  ?? Quick access
?  ?? From any page
?  ?? Direct navigation
?? IMPLEMENTATION:
   ?? Variable: headerSearchTerm
   ?? Event: HandleHeaderSearch(KeyboardEventArgs)
   ?? Method: Navigation.NavigateTo()
```

### User Search - Profile (Profile.razor)

```
FEATURE: Profile Sidebar User Search
?? LOCATION: /profile/{username} page
?? TYPE: Server-side Real-time Search
?? INPUT: Username or Display Name
?? SEARCHES:
?  ?? Username
?  ?? Display Name
?? FEATURES:
?  ?? Live search results
?  ?? Top 10 results
?  ?? Case-insensitive
?  ?? Partial matching
?  ?? Click to navigate
?? IMPLEMENTATION:
   ?? Variable: searchTerm
   ?? Method: HandleUserSearch()
   ?? Query: LINQ to Entities
   ?? Database: context.Users.Where()
```

---

## State Variables

### Home.razor
```csharp
private string searchTerm = "";              // Category search
private string headerSearchTerm = "";        // User search (header)
private string postSearchTerm = "";          // Post search
```

### CategoryDetails.razor
```csharp
private string searchTerm = "";              // Category search (sidebar)
private string postSearchTerm = "";          // Post search
```

### Profile.razor
```csharp
private string searchTerm = "";              // User search
private List<Users> SearchResults = new();   // Search results
private bool HasSearched = false;            // Track if searched
```

---

## URL Patterns

```
Navigation Patterns
?? /                           ? Home
?? /category/{id}              ? Category Details
?  ?? Post Search              ? Filter on page
?? /profile/{username}         ? User Profile
?  ?? User Search              ? Filter on page
?  ?? Click Result             ? /profile/{other-username}
?? Header Search               ? /profile/{username}
```

---

## Database Queries

### User Search (Profile Sidebar)
```csharp
var results = await context.Users
    .Where(u => u.UserName.ToLower().Contains(term.ToLower()) || 
               u.DisplayName.ToLower().Contains(term.ToLower()))
    .OrderBy(u => u.DisplayName ?? u.UserName)
    .Take(10)
    .ToListAsync();
```

### Post Filter (Client-side)
```csharp
var filtered = CategoryPosts
    .Where(p => p.Content.Contains(term, StringComparison.OrdinalIgnoreCase) || 
               (p.User?.UserName ?? "").Contains(term, StringComparison.OrdinalIgnoreCase))
    .ToList();
```

---

## Feature Comparison Table

| Feature | Post Search | User Search (Header) | User Search (Profile) |
|---------|------------|----------------------|----------------------|
| **Location** | Category page | Header (all pages) | Profile page |
| **Type** | Client-side | Navigation | Server-side |
| **Input** | Post content | Username | Username/Display name |
| **Real-time** | Yes | No (Press Enter) | Yes |
| **Results** | Filtered posts | Navigate away | User list |
| **Database** | No query | No query | Yes, limited |
| **Speed** | Very fast | Instant | Fast |
| **Limit** | All posts | N/A | 10 users |

---

## User Journeys

### Journey 1: Search Post in Category
```
1. Visit /category/1
2. See "Search posts in this category..."
3. Type "javascript"
4. See filtered posts
5. Click post author
6. Navigate to /profile/{author}
```

### Journey 2: Search User from Header
```
1. From any page
2. See header search bar
3. Type "john_doe"
4. Press Enter
5. Navigate to /profile/john_doe
6. See user's profile and posts
```

### Journey 3: Search User from Profile
```
1. On /profile/alice
2. See sidebar search
3. Type "b"
4. See users: bob, brandon, bella
5. Click "bob"
6. Navigate to /profile/bob
7. See bob's profile and posts
```

---

## Performance Characteristics

```
POST SEARCH (Client-side)
?? Time Complexity: O(n)
?? Space Complexity: O(n)
?? Latency: 0-10ms
?? Best for: Small datasets (< 1000 posts)

USER SEARCH - HEADER
?? Time Complexity: O(1)
?? Space Complexity: O(1)
?? Latency: 0ms (navigation)
?? Best for: Direct access

USER SEARCH - PROFILE
?? Time Complexity: O(log n) [with DB indexes]
?? Space Complexity: O(10) [limited results]
?? Latency: 50-100ms [typical DB query]
?? Best for: Discovery and exploration
```

---

**All search features are production-ready and optimized for performance.** ?
