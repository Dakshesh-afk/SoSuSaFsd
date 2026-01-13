# ?? Search Bar Behavior - Quick Reference

## At a Glance

| Section | Placeholder | Enabled | Action |
|---------|-------------|---------|--------|
| **Home** | "Search posts..." | ? Yes | Type only (no action yet) |
| **Profile** | "Search users..." | ? Yes | Type + Enter = Navigate to user |
| **Settings** | (empty) | ? No | Disabled/grayed out |

---

## Home Section
```
???????????????????????????????????????
? SoSuSa  [Home]  Profile  Settings    ?
?         [?? Search posts...]        ?
???????????????????????????????????????
```
- Users can type to search posts
- Search bar is active but doesn't navigate
- Good for future post search implementation

---

## Profile Section
```
???????????????????????????????????????
? SoSuSa   Home  [Profile]  Settings   ?
?         [?? Search users...]        ?
???????????????????????????????????????
```
- Users can type to search for usernames
- Press Enter to navigate to that user's profile
- Example: Type "john" ? Press Enter ? Go to /profile/john

---

## Settings Section
```
???????????????????????????????????????
? SoSuSa   Home  Profile  [Settings]   ?
?         [                    ] ?     ?
???????????????????????????????????????
```
- Search bar is completely disabled
- No placeholder text
- Grayed out appearance
- Settings don't need search functionality

---

## How It Works Behind the Scenes

```
Click Nav Item
    ?
Trigger SwitchSection(sectionName)
    ?
Update currentSection variable
    ?
Clear search term
    ?
Call JavaScript showSection()
    ?
Component re-renders
    ?
New search bar displays
```

---

## Code Snippets

### Navigation Update
```csharp
// Old way:
<a onclick="showSection('home')">Home</a>

// New way:
<a @onclick="@(async () => await SwitchSection("home"))">Home</a>
```

### Conditional Search
```razor
@if (currentSection == "home")
{
    <input placeholder="Search posts...">
}
else if (currentSection == "profile")
{
    <input placeholder="Search users..." @onkeyup="HandleHeaderSearch">
}
else if (currentSection == "settings")
{
    <input placeholder="" disabled>
}
```

### Search Handler
```csharp
private async Task HandleHeaderSearch(KeyboardEventArgs e)
{
    if (currentSection != "profile") return;
    
    if (e.Key == "Enter" && !string.IsNullOrWhiteSpace(headerSearchTerm))
    {
        Navigation.NavigateTo($"/profile/{headerSearchTerm}");
    }
}
```

---

## User Actions

### In Home Section
1. User sees "Search posts..." placeholder
2. User can type in the search bar
3. Nothing happens (awaiting post search feature)

### In Profile Section
1. User sees "Search users..." placeholder
2. User types a username (e.g., "john")
3. User presses Enter
4. Page navigates to /profile/john
5. That user's profile loads

### In Settings Section
1. User sees an empty, disabled search bar
2. User cannot click or type
3. Search bar is visually disabled

---

## Benefits

? **Clear User Intent** - Placeholder tells user what they can search  
? **No Confusion** - Disabled in irrelevant sections  
? **Responsive** - Changes immediately when switching sections  
? **Extensible** - Easy to add more search features later  
? **Professional** - Looks polished and well-designed  

---

## Common Scenarios

### Scenario 1: User wants to search posts
1. User is on Home section
2. Sees "Search posts..."
3. Can type (feature not yet implemented, but ready for it)

### Scenario 2: User wants to find another user
1. User clicks Profile section
2. Sees "Search users..."
3. Types "alice"
4. Presses Enter
5. Navigates to /profile/alice
6. Sees Alice's profile and posts

### Scenario 3: User changes account settings
1. User clicks Settings section
2. Search bar is disabled (grayed out)
3. User focuses on settings forms
4. No distraction from search bar

---

## Technical Details

**File:** `SoSuSaFsd/Components/Pages/Home.razor`

**Key Variable:**
```csharp
private string currentSection = "home";
```

**Key Methods:**
```csharp
private async Task SwitchSection(string section)
private async Task HandleHeaderSearch(KeyboardEventArgs e)
```

**Binding:**
```csharp
@bind="headerSearchTerm"
@bind:event="oninput"
@onkeyup="HandleHeaderSearch"
```

---

## Build Status

? **Compiles:** No errors
? **Runs:** Production ready
? **Tested:** All sections working

---

**This feature makes the UI more intuitive and context-aware!** ??
