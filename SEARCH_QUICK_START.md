# ?? Search Features - Quick Reference

## What You Can Now Do

### 1?? Search Posts in a Category
**Where:** Any category page (`/category/1`)  
**How:** Type in the search box under the category title  
**Searches:** Post content and author username  
**Result:** Posts filter in real-time

```
Example:
Category: #Technology
Search: "python"
Result: All posts mentioning "python" or by user "python_dev"
```

---

### 2?? Search Users from Anywhere
**Where:** Header search bar (top of page)  
**How:** Type username, press Enter  
**Searches:** Usernames and display names  
**Result:** Navigate to user's profile page

```
Example:
Header Search: "john"
Press Enter
Result: Navigate to /profile/john
```

---

### 3?? View User Profiles
**URL:** `/profile/{username}`  
**Features:**
- User info (name, bio, avatar)
- Post count
- Follower count
- All user's posts
- Like posts
- Search for other users

```
Examples:
/profile/john_doe
/profile/alice
/profile/admin
```

---

## Page URLs

| Page | URL | Purpose |
|------|-----|---------|
| **Category** | `/category/{id}` | View category & search posts |
| **User Profile** | `/profile/{username}` | View user & search users |
| **Home** | `/` | Main feed |
| **Admin** | `/admin` | Admin dashboard |

---

## Search Capabilities

### Post Search
- ? Search by content text
- ? Search by author username
- ? Case-insensitive
- ? Real-time filtering
- ? Partial matching

### User Search
- ? Search by username
- ? Search by display name
- ? Case-insensitive
- ? Real-time suggestions
- ? Shows top 10 results

---

## How Search Works

### Posts (Category Page)
1. Type in search box
2. Results update instantly
3. Shows posts matching content or author
4. Clear search to see all posts

### Users (Profile Page)
1. Type in sidebar search box
2. Results appear in real-time
3. Click a user to view their profile
4. User's posts display automatically

### Users (Header)
1. Type username in header
2. Press Enter
3. Navigates to `/profile/{username}`
4. Shows user's profile page

---

## Features

### Category Post Search
```
Search Box Location: Below category title
Input: Any text
Searches: Post content + author name
Updates: Real-time (as you type)
Results: Filtered post list
```

### Profile User Search
```
Search Box Location: Left sidebar
Input: Username or display name
Searches: Usernames + display names
Updates: Real-time (as you type)
Results: User list with avatars
```

### Header User Search
```
Search Box Location: Top header bar
Input: Username
Trigger: Press Enter key
Results: Redirect to /profile/{username}
Shows: Full user profile page
```

---

## User Profile Page Features

- ?? **Profile Header**: Name, username, bio, avatar
- ?? **Statistics**: Posts count, followers count
- ?? **Posts Feed**: All user's posts with media
- ?? **Interactions**: Like/comment on posts
- ?? **Search Users**: Find other users from sidebar

---

## Examples

### Example 1: Search Posts
```
1. Go to: /category/1 (Technology category)
2. See search box: "Search posts in this category..."
3. Type: "javascript"
4. See: All posts with "javascript" or by user "javascript_dev"
5. Clear search: See all posts again
```

### Example 2: Search Users
```
1. Go to: /profile/john
2. See left sidebar with search
3. Type: "al"
4. See: Results like "alice", "alan", "alias"
5. Click: "alice" ? Navigate to /profile/alice
```

### Example 3: Header Search
```
1. From any page, see header search
2. Type: "sarah"
3. Press: Enter
4. Go to: /profile/sarah
5. See: Sarah's profile, posts, stats
```

---

## Tips & Tricks

- ?? Searches are **case-insensitive** (search "JOHN" = "john")
- ?? Use **partial text** ("joh" finds "john")
- ?? Searches update **in real-time** (no button needed)
- ?? Non-existent users show "User not found" message
- ?? Works on mobile and desktop
- ?? Press Enter to confirm user search

---

## Keyboard Shortcuts

| Action | How |
|--------|-----|
| Search posts | Start typing in category page |
| Search users (header) | Type, press Enter |
| Search users (profile) | Type in left sidebar |
| Navigate user profile | Click result or press Enter |
| Clear search | Delete all text |

---

## Navigation Flow

```
Home (/
  ?
Category (/category/1)
  ?? Search posts (same page)
  ?? Click post author ? /profile/{username}
  
Header (any page)
  ?? Search user (press Enter) ? /profile/{username}

Profile (/profile/{username})
  ?? View user posts
  ?? View user stats
  ?? Like posts
  ?? Search other users ? /profile/{other-username}
```

---

## Performance Notes

- **Post Search**: Very fast (filters in browser)
- **User Search**: Fast (limited to 10 results)
- **No page refresh** (Blazor interactive)
- **No browser reload** (navigation is smooth)

---

## Troubleshooting

**Problem:** Search not updating
- **Solution:** Make sure you're typing in the right search box

**Problem:** User not found
- **Solution:** Check spelling of username (case-insensitive but spelling matters)

**Problem:** Posts not filtering
- **Solution:** Make sure you're on a category page with posts

**Problem:** Navigation not working
- **Solution:** Check if user exists before pressing Enter

---

## Summary

You can now:
? Search posts in categories  
? Search users from anywhere  
? View detailed user profiles  
? Navigate between users easily  
? Find content quickly  

**Happy searching! ??**
