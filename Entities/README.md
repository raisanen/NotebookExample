# Entities Project - Domain Models

The domain model layer containing entity classes and base types for the Notebook application.

## Overview

The Entities project defines the core domain models used throughout the application. It follows Domain-Driven Design (DDD) principles with a focus on encapsulating business data and relationships.

## Architecture

### Project Structure

```
Entities/
├── EntityBase.cs    # Abstract base class for all entities
├── User.cs          # User entity
└── Note.cs          # Note entity
```

### Design Principles

- **Separation of Concerns**: Pure domain models without infrastructure dependencies
- **Single Responsibility**: Each entity represents a single domain concept
- **Inheritance**: Common properties abstracted to `EntityBase`
- **Navigation Properties**: EF Core relationships defined for bi-directional navigation

## Entities

### EntityBase

**Purpose:** Abstract base class providing common identity for all entities

**Properties:**
- `Id` (int) - Primary key for all entities

**Key Features:**
- Required by the generic repository pattern's constraint `where T : EntityBase`
- Ensures consistent identity model across all domain objects
- Simplifies generic CRUD operations

**Usage:**
```csharp
public class MyEntity : EntityBase {
    // Id property inherited
    public string Name { get; set; }
}
```

### User

**Purpose:** Represents a user account in the system

**Properties:**
- `Id` (int) - Inherited from `EntityBase`
- `Username` (string) - User's login name
- `Password` (string) - User's password (⚠️ currently plaintext)
- `Notes` (ICollection<Note>) - Navigation property to user's notes

**Relationships:**
- **One-to-Many** with `Note` (one user has many notes)
- **Cascade Delete** configured in `NotebookDbContext`

**Security Considerations:**

⚠️ **Critical Security Issue:** The `Password` property stores passwords in plaintext, which is **not secure** for production use.

**Recommended for Production:**
```csharp
public class User : EntityBase {
    public string Username { get; set; } = string.Empty;
    public string PasswordHash { get; set; } = string.Empty; // Store hash instead
    public string PasswordSalt { get; set; } = string.Empty; // Store salt
    public ICollection<Note> Notes { get; set; } = new List<Note>();
}
```

**Implement Password Hashing:**
- Use BCrypt, PBKDF2, or Argon2
- Never store plaintext passwords
- Store salt separately from hash
- Use proper key derivation functions

### Note

**Purpose:** Represents a note created by a user

**Properties:**
- `Id` (int) - Inherited from `EntityBase`
- `Title` (string) - Note title
- `Text` (string) - Note content
- `Updated` (DateTime) - Timestamp of last update (auto-set in constructor)
- `UserId` (int) - Foreign key to owning user
- `User` (User) - Navigation property to owning user

**Relationships:**
- **Many-to-One** with `User` (many notes belong to one user)
- **Required Relationship** - A note must have a user

**Auto-Initialization:**
```csharp
public Note() {
    Updated = DateTime.Now; // Automatically set on creation
}
```

**Note:** The `Updated` timestamp is set in the constructor but not automatically updated on modifications. Consider implementing proper change tracking or update triggers for production use.

## Entity Relationships

### Database Schema

```
┌─────────────────┐
│      User       │
├─────────────────┤
│ Id (PK)         │◄─────┐
│ Username        │      │
│ Password        │      │
└─────────────────┘      │
                         │ 1:N
                         │
                    ┌────┴────────┐
                    │    Note     │
                    ├─────────────┤
                    │ Id (PK)     │
                    │ Title       │
                    │ Text        │
                    │ Updated     │
                    │ UserId (FK) │
                    └─────────────┘
```

### Cascade Behavior

**Delete User:**
- ✅ Automatically deletes all associated notes (cascade delete)
- Configured in `NotebookDbContext.OnModelCreating()`

**Delete Note:**
- ✅ Does not affect the user
- Standard delete behavior

## Design Patterns

### 1. **Inheritance Hierarchy**

All entities inherit from `EntityBase`:
```
EntityBase (abstract)
    ├── User
    └── Note
```

**Benefits:**
- Generic repository can work with any `EntityBase` type
- Consistent identity model
- Type-safe generic constraints

### 2. **Navigation Properties**

EF Core navigation properties enable lazy/eager loading:

```csharp
// From User, access notes
user.Notes // ICollection<Note>

// From Note, access user
note.User // User
```

### 3. **Value Initialization**

Properties use C# 8+ null-forgiving operators and default values:

```csharp
public string Username { get; set; } = string.Empty; // Never null
public ICollection<Note> Notes { get; set; } = new List<Note>(); // Always initialized
public User User { get; set; } = null!; // EF Core will populate
```

## Usage Examples

### Creating a New User

```csharp
var user = new User {
    Username = "johndoe",
    Password = "password123" // ⚠️ Should be hashed in production!
};
```

### Creating a New Note

```csharp
var note = new Note {
    Title = "My First Note",
    Text = "This is the content of my note.",
    UserId = user.Id
    // Updated is set automatically in constructor
};
```

### Accessing Relationships

```csharp
// Get all notes for a user (requires Include in query)
var userWithNotes = await context.Users
    .Include(u => u.Notes)
    .FirstOrDefaultAsync(u => u.Id == userId);

foreach (var note in userWithNotes.Notes) {
    Console.WriteLine($"{note.Title}: {note.Text}");
}

// Get user from note (requires Include in query)
var noteWithUser = await context.Notes
    .Include(n => n.User)
    .FirstOrDefaultAsync(n => n.Id == noteId);

Console.WriteLine($"Created by: {noteWithUser.User.Username}");
```

## Validation Considerations

### Current State

❌ **No built-in validation** - The entities accept any values without validation

### Recommended for Production

Implement validation using:

**1. Data Annotations:**
```csharp
public class User : EntityBase {
    [Required]
    [MaxLength(50)]
    public string Username { get; set; } = string.Empty;
    
    [Required]
    [MinLength(8)]
    public string Password { get; set; } = string.Empty;
}
```

**2. Fluent Validation:**
```csharp
public class UserValidator : AbstractValidator<User> {
    public UserValidator() {
        RuleFor(u => u.Username)
            .NotEmpty()
            .MaximumLength(50)
            .Matches("^[a-zA-Z0-9_]+$");
            
        RuleFor(u => u.Password)
            .NotEmpty()
            .MinimumLength(8);
    }
}
```

**3. EF Core Fluent API:**
```csharp
modelBuilder.Entity<User>(entity => {
    entity.Property(u => u.Username)
        .IsRequired()
        .HasMaxLength(50);
});
```

## Integration with Other Layers

### Data Layer (EF Core)

The Data layer uses these entities with Entity Framework Core:

```csharp
public class NotebookDbContext : DbContext {
    public DbSet<User> Users => Set<User>();
    public DbSet<Note> Notes => Set<Note>();
}
```

### Repository Pattern

The generic repository works with any `EntityBase`:

```csharp
IRepository<User> userRepo = new Repository<User>(context);
IRepository<Note> noteRepo = new Repository<Note>(context);
```

### Logic Layer

Business logic controllers receive and return entity instances:

```csharp
public async Task<User?> Login(string username, string password) 
    => await _uow.Users.FirstOrDefaultAsync(
        u => u.Username == username && u.Password == password
    );
```

### UI Layer

The presentation layer works with entity instances:

```csharp
var note = new Note { 
    Title = title, 
    Text = text, 
    UserId = AppState.Instance.CurrentUser!.Id 
};
await _controller.AddNote(note);
```

## Entity Lifecycle

### Creation Flow

```
1. UI creates entity instance
   ↓
2. Passes to Logic layer controller
   ↓
3. Controller passes to Repository
   ↓
4. Repository adds to DbSet
   ↓
5. UnitOfWork.SaveChangesAsync() persists to database
```

### Update Flow

```
1. Repository retrieves entity from database
   ↓
2. Returned to Logic layer
   ↓
3. Passed to UI layer for display/editing
   ↓
4. Modified entity returned through Logic layer
   ↓
5. Repository.Update() marks as modified
   ↓
6. UnitOfWork.SaveChangesAsync() persists changes
```

## Technology Stack

- **.NET 8** - Target framework
- **C# 12** - Language version
- **Nullable Reference Types** - Enabled for null safety
- **Init-only Properties** - Used where appropriate

## Dependencies

### NuGet Packages
None - Pure POCO (Plain Old CLR Objects)

### Project References
None - Independent domain model layer

**Key Benefit:** Zero infrastructure dependencies make entities:
- Easy to test
- Easy to serialize
- Portable across different data access strategies
- Clear separation of concerns

## Best Practices

### ✅ DO

1. **Keep entities simple** - Focus on data and relationships
2. **Use meaningful names** - Entity and property names should be self-explanatory
3. **Initialize collections** - Prevent null reference exceptions
4. **Document with XML comments** - All entities are fully documented
5. **Follow naming conventions** - PascalCase for properties

### ❌ DON'T

1. **Add business logic** - Keep entities as data containers
2. **Reference infrastructure** - No dependencies on EF Core, JSON libraries, etc.
3. **Include data access code** - No database queries in entities
4. **Store sensitive data** - Hash passwords, encrypt sensitive fields
5. **Use mutable collections** - Consider `IReadOnlyCollection<T>` for navigation properties

## Known Issues

### 1. Plaintext Passwords

⚠️ **Security Vulnerability**

**Issue:** User passwords stored in plaintext  
**Impact:** Complete password exposure if database is compromised  
**Severity:** CRITICAL

**Solution:**
```csharp
// Add to User entity
public string PasswordHash { get; set; } = string.Empty;
public string PasswordSalt { get; set; } = string.Empty;

// Use in LoginController
public async Task<User?> Login(string username, string password) {
    var user = await _uow.Users.FirstOrDefaultAsync(u => u.Username == username);
    if (user == null) return null;
    
    return PasswordHasher.VerifyPassword(password, user.PasswordHash, user.PasswordSalt) 
        ? user 
        : null;
}
```

### 2. Auto-Update Timestamp

**Issue:** `Note.Updated` only set on creation, not on updates  
**Impact:** Timestamp doesn't reflect actual last modification  
**Severity:** LOW

**Solution:**
```csharp
// Override SaveChanges in DbContext
public override int SaveChanges() {
    UpdateTimestamps();
    return base.SaveChanges();
}

private void UpdateTimestamps() {
    var entries = ChangeTracker.Entries<Note>()
        .Where(e => e.State == EntityState.Modified);
    
    foreach (var entry in entries) {
        entry.Entity.Updated = DateTime.Now;
    }
}
```

### 3. No Soft Delete

**Issue:** Deleted entities permanently removed from database  
**Impact:** No audit trail or recovery possible  
**Severity:** MEDIUM

**Solution:**
```csharp
// Add to EntityBase
public bool IsDeleted { get; set; }
public DateTime? DeletedAt { get; set; }

// Add global query filter
modelBuilder.Entity<User>().HasQueryFilter(u => !u.IsDeleted);
modelBuilder.Entity<Note>().HasQueryFilter(n => !n.IsDeleted);
```

## Future Enhancements

### Short Term
- ✨ Add data validation attributes
- ✨ Implement password hashing
- ✨ Add created/modified timestamps to EntityBase
- ✨ Add soft delete support

### Medium Term
- ✨ Add value objects (Email, Password, etc.)
- ✨ Implement domain events
- ✨ Add audit properties (CreatedBy, ModifiedBy)
- ✨ Support for note categories/tags

### Long Term
- ✨ Rich domain model with business logic
- ✨ Aggregate roots pattern
- ✨ Domain services
- ✨ Specification pattern for queries

## Testing Considerations

### Unit Testing Entities

```csharp
[Test]
public void Note_Constructor_SetsUpdatedTimestamp() {
    // Arrange & Act
    var note = new Note();
    
    // Assert
    Assert.IsTrue((DateTime.Now - note.Updated).TotalSeconds < 1);
}

[Test]
public void User_Notes_InitializedAsEmptyCollection() {
    // Arrange & Act
    var user = new User();
    
    // Assert
    Assert.IsNotNull(user.Notes);
    Assert.AreEqual(0, user.Notes.Count);
}
```

## Related Documentation

- **[Solution README](../README.md)** - Overall architecture and patterns
- **[Data Layer README](../Data/README.md)** - Repository and Unit of Work implementation
- **[Logic Layer README](../Logic/README.md)** - Business logic controllers
- **[UI Layer README](../UI/README.md)** - User interface implementation

## Contributing

When modifying entities:

1. ✅ Keep entities as pure data models
2. ✅ Update XML documentation
3. ✅ Consider database migration impact
4. ✅ Update relationships in `NotebookDbContext`
5. ✅ Maintain backward compatibility
6. ✅ Add validation attributes where appropriate
7. ✅ Initialize collection properties
