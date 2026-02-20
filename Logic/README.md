# Logic Project - Business Logic Layer

The business logic layer containing controllers that coordinate between the UI and Data layers.

## Overview

The Logic project implements the business logic of the Notebook application, acting as an intermediary between the presentation layer (UI) and the data access layer (Data). It follows the principle of separation of concerns and uses dependency injection for loose coupling.

## Architecture

### Project Structure

```
Logic/
├── interfaces/                    # Controller interfaces
│   ├── ILoginController.cs        # Authentication interface
│   ├── IMenuController.cs         # Menu operations interface
│   ├── IAddNoteController.cs      # Add note interface
│   └── IShowNotesController.cs    # View notes interface
├── LoginController.cs             # Authentication implementation
├── MenuController.cs              # Menu operations implementation
├── AddNoteController.cs           # Add note implementation
└── ShowNotesController.cs         # View notes implementation
```

### Layer Responsibilities

**Logic Layer (This Project):**
- Orchestrates business operations
- Validates business rules
- Coordinates between UI and Data
- Independent of presentation concerns
- Uses Unit of Work for data access

**Dependencies:**
- ⬇️ **Entities** - Domain models (User, Note)
- ⬇️ **Data** - Data access (IUnitOfWork, repositories)
- ⬆️ **UI** - Depends on this layer's interfaces

## Design Patterns

### 1. **Controller Pattern**

Each controller handles a specific business concern:

```
ILoginController → Login/Authentication
IMenuController → Menu-related queries
IAddNoteController → Note creation
IShowNotesController → Note retrieval
```

**Benefits:**
- Single Responsibility Principle
- Clear separation of concerns
- Easy to test independently
- Simple to extend

### 2. **Dependency Injection**

Controllers depend on abstractions, not implementations:

```csharp
public class AddNoteController : IAddNoteController {
    private readonly IUnitOfWork _uow;  // Injected dependency
    
    public AddNoteController(IUnitOfWork uow) {
        _uow = uow;
    }
}
```

**Benefits:**
- Loose coupling
- Easy to mock for testing
- Flexible implementation swapping
- Clear dependencies

### 3. **Interface Segregation**

Each controller has its own focused interface:

```csharp
public interface IAddNoteController {
    Task AddNote(Note note);  // Single, focused method
}
```

**Benefits:**
- Clients only depend on what they need
- Small, focused interfaces
- Easy to understand and implement
- Reduces coupling

## Controllers

### LoginController

**Purpose:** Handle user authentication

**Interface:**
```csharp
public interface ILoginController {
    Task<User?> Login(string username, string password);
}
```

**Implementation:**
```csharp
public async Task<User?> Login(string username, string password) 
    => await _uow.Users.FirstOrDefaultAsync(
        u => u.Username == username && u.Password == password
    );
```

**Usage:**
```csharp
var user = await _loginController.Login("kalle", "password");
if (user != null) {
    AppState.Instance.CurrentUser = user;
}
```

**Security Warning:**

⚠️ **CRITICAL:** Uses plaintext password comparison - **NOT secure for production**

**Production Implementation:**
```csharp
public async Task<User?> Login(string username, string password) {
    var user = await _uow.Users.FirstOrDefaultAsync(u => u.Username == username);
    if (user == null) return null;
    
    // Use proper password hashing (BCrypt, PBKDF2, Argon2)
    return PasswordHasher.VerifyPassword(password, user.PasswordHash, user.PasswordSalt)
        ? user
        : null;
}
```

### MenuController

**Purpose:** Provide menu-related business logic

**Interface:**
```csharp
public interface IMenuController {
    Task<bool> HasNotes(User user);
}
```

**Implementation:**
```csharp
public async Task<bool> HasNotes(User user) 
    => await _uow.Notes.AnyAsync(n => n.UserId == user.Id);
```

**Usage:**
```csharp
var hasNotes = await _menuController.HasNotes(currentUser);
if (!hasNotes) {
    menu.Items.Remove("Show notes");
}
```

**Purpose:** Enables dynamic UI behavior based on user data

**Benefits:**
- UI doesn't directly access data layer
- Business logic centralized
- Easy to add validation rules
- Testable without database

### AddNoteController

**Purpose:** Handle note creation

**Interface:**
```csharp
public interface IAddNoteController {
    Task AddNote(Note note);
}
```

**Implementation:**
```csharp
public async Task AddNote(Note note) {
    await _uow.Notes.AddAsync(note);
    await _uow.SaveChangesAsync();
}
```

**Usage:**
```csharp
var note = new Note { 
    Title = title, 
    Text = text, 
    UserId = currentUser.Id 
};
await _addNoteController.AddNote(note);
```

**Transaction Management:**
- Uses Unit of Work pattern
- Changes committed atomically
- Rollback on failure

**Potential Enhancements:**
```csharp
public async Task AddNote(Note note) {
    // Validation
    if (string.IsNullOrWhiteSpace(note.Title)) {
        throw new ValidationException("Title is required");
    }
    
    // Business rules
    note.Updated = DateTime.Now;
    
    // Data access
    await _uow.Notes.AddAsync(note);
    await _uow.SaveChangesAsync();
    
    // Post-processing (logging, notifications, etc.)
    _logger.LogInformation($"Note created: {note.Id}");
}
```

### ShowNotesController

**Purpose:** Retrieve user's notes

**Interface:**
```csharp
public interface IShowNotesController {
    Task<IEnumerable<Note>> GetNotesForUser(User user);
}
```

**Implementation:**
```csharp
public async Task<IEnumerable<Note>> GetNotesForUser(User user)
    => await _uow.Notes.FindAsync(n => n.UserId == user.Id);
```

**Usage:**
```csharp
var notes = await _showNotesController.GetNotesForUser(currentUser);
foreach (var note in notes) {
    DisplayNote(note);
}
```

**Data Isolation:**
- Filters by user ID
- Ensures users only see their own notes
- Business-level security

**Potential Enhancements:**
```csharp
public async Task<IEnumerable<Note>> GetNotesForUser(
    User user, 
    string? searchTerm = null,
    string? sortBy = null) {
    
    var notes = await _uow.Notes.FindAsync(n => n.UserId == user.Id);
    
    // Apply search filter
    if (!string.IsNullOrWhiteSpace(searchTerm)) {
        notes = notes.Where(n => 
            n.Title.Contains(searchTerm) || 
            n.Text.Contains(searchTerm));
    }
    
    // Apply sorting
    return sortBy?.ToLower() switch {
        "title" => notes.OrderBy(n => n.Title),
        "date" => notes.OrderByDescending(n => n.Updated),
        _ => notes
    };
}
```

## Layer Interaction

### Complete Flow Example

```
┌─────────────┐
│  UI Layer   │
└──────┬──────┘
       │ 1. User action
       ↓
┌─────────────────┐
│ Logic Layer     │
│ (Controller)    │
└──────┬──────────┘
       │ 2. Business logic
       ↓
┌─────────────────┐
│ Data Layer      │
│ (Unit of Work)  │
└──────┬──────────┘
       │ 3. Data access
       ↓
┌─────────────────┐
│  Database       │
└─────────────────┘
```

### Detailed Example: Adding a Note

```csharp
// 1. UI Layer (AddNotePage)
var note = new Note { Title = "Test", Text = "Content", UserId = user.Id };
await _controller.AddNote(note);

// 2. Logic Layer (AddNoteController)
public async Task AddNote(Note note) {
    await _uow.Notes.AddAsync(note);
    await _uow.SaveChangesAsync();
}

// 3. Data Layer (UnitOfWork → Repository)
public async Task AddAsync(T entity)
    => await _dbSet.AddAsync(entity);

// 4. EF Core persists to database
```

## Benefits of This Architecture

### Separation of Concerns

✅ **UI Layer:**
- Focused on presentation
- No database knowledge
- Uses controller interfaces

✅ **Logic Layer:**
- Focused on business rules
- Coordinates operations
- Technology-agnostic

✅ **Data Layer:**
- Focused on data access
- Isolated from business logic
- Reusable repositories

### Testability

**Unit Testing Controllers:**
```csharp
[Test]
public async Task AddNote_CallsRepositoryAndSaves() {
    // Arrange
    var mockUow = new Mock<IUnitOfWork>();
    var mockRepo = new Mock<IRepository<Note>>();
    mockUow.Setup(u => u.Notes).Returns(mockRepo.Object);
    
    var controller = new AddNoteController(mockUow.Object);
    var note = new Note { Title = "Test" };
    
    // Act
    await controller.AddNote(note);
    
    // Assert
    mockRepo.Verify(r => r.AddAsync(note), Times.Once);
    mockUow.Verify(u => u.SaveChangesAsync(), Times.Once);
}
```

**Integration Testing:**
```csharp
[Test]
public async Task Login_ValidCredentials_ReturnsUser() {
    // Arrange
    var context = CreateTestContext();
    var uow = new UnitOfWork(context);
    var controller = new LoginController(uow);
    
    // Act
    var user = await controller.Login("kalle", "password");
    
    // Assert
    Assert.IsNotNull(user);
    Assert.AreEqual("kalle", user.Username);
}
```

### Maintainability

✅ **Single Responsibility:**
- Each controller has one job
- Easy to understand
- Simple to modify

✅ **Open/Closed Principle:**
- Open for extension (add new controllers)
- Closed for modification (existing controllers stable)

✅ **Dependency Inversion:**
- Depends on abstractions (IUnitOfWork)
- Not tied to implementations
- Flexible architecture

## Usage Patterns

### Dependency Injection Setup

**Manual (Current Implementation):**
```csharp
var uow = new UnitOfWork(new NotebookDbContext());
var loginController = new LoginController(uow);
var menuController = new MenuController(uow);
```

**Recommended (DI Container):**
```csharp
services.AddScoped<IUnitOfWork, UnitOfWork>();
services.AddScoped<ILoginController, LoginController>();
services.AddScoped<IMenuController, MenuController>();
services.AddScoped<IAddNoteController, AddNoteController>();
services.AddScoped<IShowNotesController, ShowNotesController>();
```

### Error Handling

**Current:**
```csharp
// Exceptions bubble up to UI
await controller.AddNote(note);
```

**Recommended:**
```csharp
public async Task<Result> AddNote(Note note) {
    try {
        // Validation
        if (string.IsNullOrWhiteSpace(note.Title)) {
            return Result.Failure("Title is required");
        }
        
        // Business logic
        await _uow.Notes.AddAsync(note);
        await _uow.SaveChangesAsync();
        
        return Result.Success();
    }
    catch (DbUpdateException ex) {
        _logger.LogError(ex, "Failed to add note");
        return Result.Failure("Failed to save note");
    }
}
```

### Validation

**Add Validation Layer:**
```csharp
public async Task AddNote(Note note) {
    // Input validation
    ValidateNote(note);
    
    // Business rules
    if (await IsDuplicateTitle(note)) {
        throw new BusinessException("Note with this title already exists");
    }
    
    // Data access
    await _uow.Notes.AddAsync(note);
    await _uow.SaveChangesAsync();
}

private void ValidateNote(Note note) {
    if (string.IsNullOrWhiteSpace(note.Title)) {
        throw new ValidationException("Title is required");
    }
    if (note.Title.Length > 200) {
        throw new ValidationException("Title too long");
    }
    if (string.IsNullOrWhiteSpace(note.Text)) {
        throw new ValidationException("Text is required");
    }
}
```

## Technology Stack

- **.NET 8** - Target framework
- **C# 12** - Language version
- **Async/Await** - Asynchronous programming
- **Dependency Injection** - Inversion of Control

## Dependencies

### Project References

```xml
<ProjectReference Include="..\Data\Data.csproj" />
<ProjectReference Include="..\Entities\Entities.csproj" />
```

### NuGet Packages

None - Uses only project references and .NET BCL

## Integration Examples

### From UI Layer

**Login Page:**
```csharp
public class LoginPage : IPage {
    private readonly ILoginController _controller;
    
    public async Task<PageState> Render() {
        var username = IO.Instance.ReadString("Username: ");
        var password = IO.Instance.ReadPassword("Password: ");
        
        var user = await _controller.Login(username, password);
        
        if (user != null) {
            AppState.Instance.CurrentUser = user;
            return PageState.Menu;
        }
        
        return PageState.Login;
    }
}
```

**Menu Page:**
```csharp
public class MenuPage : IPage {
    private readonly IMenuController _controller;
    
    public async Task<PageState> Render() {
        var menu = new Menu<PageState>() {
            Items = new() {
                ["Show notes"] = () => PageState.ShowNotes,
                ["Add note"] = () => PageState.AddNote,
                ["Quit"] = () => PageState.End
            }
        };
        
        var hasNotes = await _controller.HasNotes(AppState.Instance.CurrentUser!);
        if (!hasNotes) {
            menu.Items.Remove("Show notes");
        }
        
        return IO.Instance.ShowMenu(menu, Title);
    }
}
```

## Testing Strategies

### Unit Tests

**Test Controller Logic:**
```csharp
[TestFixture]
public class AddNoteControllerTests {
    [Test]
    public async Task AddNote_ValidNote_SavesSuccessfully() {
        // Arrange
        var mockUow = new Mock<IUnitOfWork>();
        var mockNoteRepo = new Mock<IRepository<Note>>();
        mockUow.Setup(u => u.Notes).Returns(mockNoteRepo.Object);
        mockUow.Setup(u => u.SaveChangesAsync()).ReturnsAsync(1);
        
        var controller = new AddNoteController(mockUow.Object);
        var note = new Note { Title = "Test", Text = "Content", UserId = 1 };
        
        // Act
        await controller.AddNote(note);
        
        // Assert
        mockNoteRepo.Verify(r => r.AddAsync(note), Times.Once);
        mockUow.Verify(u => u.SaveChangesAsync(), Times.Once);
    }
}
```

### Integration Tests

**Test with Real Database:**
```csharp
[TestFixture]
public class LoginControllerIntegrationTests {
    private DbContextOptions<NotebookDbContext> _options;
    
    [SetUp]
    public void Setup() {
        _options = new DbContextOptionsBuilder<NotebookDbContext>()
            .UseInMemoryDatabase(databaseName: $"TestDb_{Guid.NewGuid()}")
            .Options;
        
        using var context = new NotebookDbContext(_options);
        context.Users.Add(new User { 
            Id = 1, 
            Username = "testuser", 
            Password = "password" 
        });
        context.SaveChanges();
    }
    
    [Test]
    public async Task Login_ValidCredentials_ReturnsUser() {
        // Arrange
        using var context = new NotebookDbContext(_options);
        var uow = new UnitOfWork(context);
        var controller = new LoginController(uow);
        
        // Act
        var user = await controller.Login("testuser", "password");
        
        // Assert
        Assert.IsNotNull(user);
        Assert.AreEqual("testuser", user.Username);
    }
    
    [Test]
    public async Task Login_InvalidCredentials_ReturnsNull() {
        // Arrange
        using var context = new NotebookDbContext(_options);
        var uow = new UnitOfWork(context);
        var controller = new LoginController(uow);
        
        // Act
        var user = await controller.Login("wrong", "credentials");
        
        // Assert
        Assert.IsNull(user);
    }
}
```

## Known Issues

### 1. No Input Validation

**Issue:** Controllers accept any input without validation

**Current:**
```csharp
public async Task AddNote(Note note) {
    await _uow.Notes.AddAsync(note);
    await _uow.SaveChangesAsync();
}
```

**Recommended:**
```csharp
public async Task AddNote(Note note) {
    if (note == null) throw new ArgumentNullException(nameof(note));
    if (string.IsNullOrWhiteSpace(note.Title)) 
        throw new ValidationException("Title is required");
    if (string.IsNullOrWhiteSpace(note.Text)) 
        throw new ValidationException("Text is required");
    
    await _uow.Notes.AddAsync(note);
    await _uow.SaveChangesAsync();
}
```

### 2. No Error Handling

**Issue:** Database exceptions bubble to UI

**Solution:** Implement try-catch and return Result types

### 3. Plaintext Password Authentication

**Issue:** LoginController compares plaintext passwords

**Solution:** Implement password hashing (see LoginController section above)

### 4. No Authorization

**Issue:** Controllers don't verify user permissions

**Solution:**
```csharp
public async Task<IEnumerable<Note>> GetNotesForUser(User requestingUser, int userId) {
    if (requestingUser.Id != userId && !requestingUser.IsAdmin) {
        throw new UnauthorizedException("Cannot access other users' notes");
    }
    
    return await _uow.Notes.FindAsync(n => n.UserId == userId);
}
```

## Future Enhancements

### Short Term
- ✨ Add input validation
- ✨ Implement password hashing
- ✨ Add error handling and logging
- ✨ Return Result types instead of throwing exceptions

### Medium Term
- ✨ Add authorization checks
- ✨ Implement FluentValidation
- ✨ Add pagination support
- ✨ Support for sorting and filtering
- ✨ Add caching layer

### Long Term
- ✨ Implement CQRS pattern
- ✨ Add domain events
- ✨ Implement MediatR
- ✨ Add background job processing
- ✨ Support for transactions across multiple aggregates

## Best Practices

### ✅ DO

1. **Keep controllers thin** - Delegate to services if logic grows
2. **Validate input** - Check parameters before processing
3. **Use async/await** - All database operations are async
4. **Handle exceptions** - Catch and log errors appropriately
5. **Follow SOLID principles** - Single responsibility, interface segregation

### ❌ DON'T

1. **Access database directly** - Always use Unit of Work
2. **Put UI logic in controllers** - Keep presentation separate
3. **Create tight coupling** - Depend on abstractions
4. **Ignore validation** - Validate all inputs
5. **Swallow exceptions** - Log and handle appropriately

## Related Documentation

- **[Solution README](../README.md)** - Overall architecture
- **[Entities README](../Entities/README.md)** - Domain models
- **[Data README](../Data/README.md)** - Data access layer
- **[UI README](../UI/README.md)** - User interface

## Contributing

When adding new controllers:

1. ✅ Create interface first
2. ✅ Implement with Unit of Work
3. ✅ Add XML documentation
4. ✅ Write unit tests
5. ✅ Add validation logic
6. ✅ Handle errors appropriately
7. ✅ Follow naming conventions
8. ✅ Keep methods focused and simple
