# UI Project - Text-Based User Interface

A console-based user interface for the Notebook application, implementing a page-based navigation system with state machine pattern.

## Overview

The UI project provides a text-based interactive interface for the Notebook application. It demonstrates clean separation between presentation logic and business logic, using dependency injection principles and the state machine pattern for navigation.

## Architecture

### Project Structure

```
UI/
??? Program.cs              # Application entry point
??? TUI/                    # Text User Interface components
    ??? AppState.cs         # Singleton session state manager
    ??? IO.cs               # Singleton console I/O utilities
    ??? Menu.cs             # Generic navigable menu component
    ??? PageStateMachine.cs # State machine for page navigation
    ??? Page/               # Individual page implementations
        ??? IPage.cs        # Page interface
        ??? LoginPage.cs    # User authentication page
        ??? MenuPage.cs     # Main menu page
        ??? AddNotePage.cs  # Note creation page
        ??? ShowNotesPage.cs # Note display page
```

### Key Design Patterns

#### 1. **State Machine Pattern**

The UI uses a state machine to manage page navigation:

```csharp
public enum PageState {
    Login,      // Authentication page
    Menu,       // Main menu
    ShowNotes,  // Display user's notes
    AddNote,    // Create new note
    End         // Application termination
}
```

**How it works:**
- Each page implements `IPage` and returns the next `PageState` after rendering
- `PageStateMachine` coordinates the navigation flow
- Pages are created using factory functions for fresh instances

**Benefits:**
- Clear navigation flow
- Easy to add new pages
- Centralized state management
- Type-safe transitions

#### 2. **Singleton Pattern**

Two key singletons manage shared resources:

**`AppState`** - Session State Management

- Maintains currently logged-in user
- Provides authentication status across all pages
- Simple session management for console application

**`IO`** - Console I/O Utilities

- Consistent console formatting across the application
- Centralized color and cursor management
- Password masking functionality
- Menu display utilities

#### 3. **Page Pattern**

Each screen implements the `IPage` interface:

```csharp
public interface IPage {
    string Title { get; }
    Task<PageState> Render();
}
```

**Page Lifecycle:**
1. `PageStateMachine` creates page instance
2. Displays page title header
3. Calls `Render()` method
4. Page performs its logic and returns next state
5. Machine navigates to the next page

## Component Details

### PageStateMachine

Manages the application's navigation flow.

**Responsibilities:**
- Maintains current page state
- Invokes page factory functions
- Displays page headers
- Handles state transitions
- Terminates on `End` state or invalid states

**Configuration Example:**
```csharp
var sm = new PageStateMachine {
    Pages = new() {
        [PageState.Login] = () => new LoginPage(loginController),
        [PageState.Menu] = () => new MenuPage(menuController),
        [PageState.ShowNotes] = () => new ShowNotesPage(showNotesController),
        [PageState.AddNote] = () => new AddNotePage(addNoteController)
    }
};
await sm.Run();
```

### Pages

#### LoginPage
**Purpose:** User authentication

**Flow:**
1. Prompts for username (visible input)
2. Prompts for password (masked with `*`)
3. Attempts authentication via `ILoginController`
4. On success: stores user in `AppState`, navigates to Menu
5. On failure: loops back to Login

**Dependencies:** `ILoginController`

#### MenuPage
**Purpose:** Main navigation hub

**Flow:**
1. Checks authentication status
2. Queries if user has notes via `IMenuController`
3. Dynamically builds menu:
   - "Show notes" (only if user has notes)
   - "Add note"
   - "Quit"
4. Displays interactive menu
5. Returns selected page state

**Dependencies:** `IMenuController`

**Key Feature:** Dynamic menu options based on user data

#### AddNotePage
**Purpose:** Create new notes

**Flow:**
1. Checks authentication status
2. Prompts for note title
3. Prompts for note text
4. Creates `Note` entity with current user ID
5. Saves via `IAddNoteController`
6. Returns to Menu

**Dependencies:** `IAddNoteController`

#### ShowNotesPage
**Purpose:** Display user's notes

**Flow:**
1. Checks authentication status
2. Retrieves notes via `IShowNotesController`
3. Displays each note with formatting:
   - Decorative separator lines
   - Note title as sub-header
   - Note text content
4. Waits for user keypress
5. Returns to Menu

**Dependencies:** `IShowNotesController`

### IO Utilities

The `IO` class provides rich console functionality:

#### Input Methods
- **`ReadString()`** - Standard text input with prompt
- **`ReadPassword()`** - Masked password input (displays `*`)
- **`ReadNumber()`** - Integer input with validation
- **`WaitForAny()`** - Wait for any keypress

#### Output Methods
- **`PrintHeader()`** - Full-width header with title
- **`PrintSubHeader()`** - Sub-header with underline
- **`PrintLine()`** - Decorative line using `=` character
- **`PrintAltLine()`** - Decorative line using `-` character
- **`PrintFullWidth()`** - 80-character formatted line
- **`PrintFullWidthInverted()`** - Highlighted line with inverted colors

#### Visual Features
- **Color inversion** for highlighting (gray background, black text)
- **Cursor control** for cleaner input experience
- **Password masking** using asterisks
- **Consistent 80-character formatting**

#### Menu Display
- **`ShowMenu<T>()`** - Interactive menu with keyboard navigation
  - Up/Down arrows to navigate
  - Space/Enter to select
  - Selected item highlighted with inverted colors and arrows

### Menu Component

Generic navigable menu system:

```csharp
var menu = new Menu<PageState>() {
    Items = new() {
        ["Show notes"] = () => PageState.ShowNotes,
        ["Add note"] = () => PageState.AddNote,
        ["Quit"] = () => PageState.End
    }
};
```

**Features:**
- Generic return type `T` for flexibility
- Keyboard navigation (Up/Down arrows)
- Visual selection indicator
- Wrapping navigation (top ? bottom)
- Dynamic item addition/removal

## Dependency Flow

```
Program.cs
    ?? Creates NotebookDbContext
    ?? Creates UnitOfWork
    ?? Configures PageStateMachine
    ?? Injects controllers into pages

Pages
    ?? Depend on IXxxController interfaces (Logic layer)
    ?? Use AppState for session management
    ?? Use IO for console operations

Controllers (from Logic layer)
    ?? Depend on IUnitOfWork (Data layer)
```

**Separation of Concerns:**
- ? UI layer only handles presentation
- ? Business logic delegated to Logic layer controllers
- ? Data access through Repository/UoW patterns
- ? Easy to mock dependencies for testing

## Application Flow

### Startup Sequence

1. **`Program.Main()`** executes
2. Database context and Unit of Work created
3. PageStateMachine configured with page factories
4. State machine starts at `PageState.Login`

### Session Flow

```
???????????????
? Login Page  ????????? Authentication
???????????????
      ? ? Success
      ?
???????????????
?  Menu Page  ????????? Navigation Hub
???????????????
      ?
      ??? Show Notes ??? Display ??? Back to Menu
      ?
      ??? Add Note ??? Create ??? Back to Menu
      ?
      ??? Quit ??? End State ??? Application Exits
```

## Authentication & Security

### Current Implementation

?? **Security Warning:** The current implementation has significant security limitations:

- **Plaintext passwords** stored in database
- **No password hashing** (BCrypt, PBKDF2, Argon2)
- **No session timeout** mechanism
- **No brute-force protection**

### Session Management

- Single-user console application
- `AppState.CurrentUser` maintains logged-in user
- All pages check `AppState.IsLoggedIn` before rendering
- Failed authentication loops back to login page

### Production Considerations

For production deployment, implement:
1. **Password Hashing**: Use BCrypt, PBKDF2, or Argon2
2. **Session Expiration**: Auto-logout after inactivity
3. **Account Lockout**: After multiple failed attempts
4. **Audit Logging**: Track authentication events
5. **Multi-user Support**: Replace singleton with proper session management

## Extending the UI

### Adding a New Page

1. Create page class implementing `IPage`
2. Add state to `PageState` enum
3. Register in `PageStateMachine.Pages` dictionary
4. Navigate from other pages by returning the new state

### Adding Menu Options

Add new items to the `Menu<PageState>.Items` dictionary with appropriate state values.

## Technology Stack

- **.NET 8** - Target framework
- **C# 12** - Language version
- **System.Console** - Console I/O
- **Async/Await** - Asynchronous operations

## Dependencies

### Project References
- **Entities** - Domain models (`User`, `Note`, `EntityBase`)
- **Logic** - Business logic controllers
- **Data** - Data access (`IUnitOfWork`, repositories)

### NuGet Packages
None - Uses only .NET BCL (Base Class Library)

## Known Limitations

1. **Console-only** - No web or desktop GUI
2. **Single-user** - No concurrent user support
3. **No data validation** - Accepts any input without validation
4. **Limited error handling** - Minimal exception handling
5. **No confirmation dialogs** - Actions execute immediately
6. **No edit/delete** - Can only add and view notes
7. **No search/filter** - Displays all notes without filtering
8. **Fixed 80-character width** - Assumes standard console width

## Running the Application

### Prerequisites
- .NET 8 SDK
- SQL Server LocalDB

### Steps

1. **Ensure database is set up**:
```bash
cd Data
dotnet ef database update
```

2. **Run the application**:
```bash
cd UI
dotnet run
```

3. **Default credentials**:
   - Username: `kalle`
   - Password: `password`

### Navigation

- **Login Screen**: Enter credentials
- **Main Menu**: Use Up/Down arrows to select, Space/Enter to confirm
- **Add Note**: Type title and text, press Enter
- **Show Notes**: View all notes, press any key to return
- **Quit**: Exit the application

## Contributing

When adding new features to the UI:

1. ? Follow existing page patterns
2. ? Check authentication before rendering
3. ? Use `IO.Instance` for console operations
4. ? Return appropriate `PageState` for navigation
5. ? Add XML documentation to all public members
6. ? Consider testability in design
7. ? Maintain consistent 80-character formatting

## Related Documentation

- **[Solution README](../README.md)** - Overall architecture and patterns
- **[Data Layer](../Data/)** - Repository and Unit of Work patterns
- **[Logic Layer](../Logic/)** - Business logic controllers
- **[Entities Layer](../Entities/)** - Domain models
