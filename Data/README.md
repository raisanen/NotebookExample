# Data Project - Data Access Layer

The data access layer implementing the Repository and Unit of Work patterns with Entity Framework Core.

## Overview

The Data project provides a clean abstraction over Entity Framework Core, implementing the Repository and Unit of Work patterns to decouple business logic from data access concerns.

## Architecture

### Project Structure

```
Data/
├── IRepository.cs         # Generic repository interface
├── Repository.cs          # Generic repository implementation
├── IUnitOfWork.cs         # Unit of Work interface
├── UnitOfWork.cs          # Unit of Work implementation
├── NotebookDbContext.cs   # EF Core database context
└── Migrations/            # EF Core migrations
```

### Design Patterns

This layer demonstrates three key patterns:

1. **Repository Pattern** - Abstracts data access operations
2. **Unit of Work Pattern** - Coordinates transactions across repositories
3. **Generic Repository** - Reusable implementation for all entity types

## Repository Pattern

### IRepository<T>

**Purpose:** Generic interface defining standard data access operations

**Type Constraint:** `where T : EntityBase` - Ensures all entities have an `Id` property

**Operations:**

**Query Operations:**
- `AnyAsync()` - Check if any entities exist
- `AnyAsync(predicate)` - Check if any entities match criteria
- `GetByIdAsync(id)` - Retrieve entity by primary key
- `GetAllAsync()` - Retrieve all entities
- `FindAsync(predicate)` - Filter entities by criteria
- `FirstOrDefaultAsync(predicate)` - Get first matching entity or null

**Modification Operations:**
- `AddAsync(entity)` - Add new entity to context
- `Update(entity)` - Mark entity as modified
- `Remove(entity)` - Mark entity for deletion

**Key Design Decision:**

Returns `IEnumerable<T>` instead of `IQueryable<T>` to:
- ✅ Hide EF Core implementation details
- ✅ Prevent query composition in business logic
- ✅ Maintain clear separation of concerns
- ⚠️ Trade-off: Less flexible querying

### Repository<T>

**Purpose:** Generic implementation working with any `EntityBase` entity

**Key Features:**

1. **Generic Type Parameter:**
```csharp
public class Repository<T> : IRepository<T> where T : EntityBase
```

2. **Shared DbContext:**
```csharp
protected readonly NotebookDbContext _context;
protected readonly DbSet<T> _dbSet;
```

3. **Expression Trees:**
```csharp
public async Task<IEnumerable<T>> FindAsync(Expression<Func<T, bool>> predicate)
    => await _dbSet.Where(predicate).ToListAsync();
```

**Benefits:**
- Write data access logic once
- Type-safe operations
- Consistent API across all entities
- Easy to mock for testing

**Example Usage:**
```csharp
var userRepo = new Repository<User>(context);
var users = await userRepo.FindAsync(u => u.Username.StartsWith("john"));
```

## Unit of Work Pattern

### IUnitOfWork

**Purpose:** Coordinate operations across multiple repositories

**Responsibilities:**
- Provide access to repositories
- Manage single `DbContext` instance
- Control transaction lifecycle
- Implement `IDisposable` for cleanup

**Interface:**
```csharp
public interface IUnitOfWork : IDisposable {
    IRepository<User> Users { get; }
    IRepository<Note> Notes { get; }
    Task<int> SaveChangesAsync();
}
```

### UnitOfWork

**Purpose:** Implementation coordinating repositories and transactions

**Key Features:**

1. **Lazy Initialization:**
```csharp
private IRepository<User>? _users;
public IRepository<User> Users => _users ??= new Repository<User>(_context);
```

2. **Shared Context:**
```csharp
private readonly NotebookDbContext _context;
```

3. **Single Save Point:**
```csharp
public async Task<int> SaveChangesAsync()
    => await _context.SaveChangesAsync();
```

**Benefits:**
- All repositories share same context
- Changes committed atomically
- Single transaction per operation
- Proper resource cleanup

**Example Usage:**
```csharp
using var uow = new UnitOfWork(context);

// Multiple operations on different repositories
await uow.Users.AddAsync(newUser);
await uow.Notes.AddAsync(newNote);

// Single commit point
await uow.SaveChangesAsync();
```

## NotebookDbContext

### Purpose

EF Core database context managing entity relationships and database configuration.

### Configuration

**Database Provider:**
```csharp
protected override void OnConfiguring(DbContextOptionsBuilder optionsBuilder) {
    optionsBuilder.UseSqlServer(@"Server=(localdb)\mssqllocaldb;Database=NotebookDB");
}
```

**Entity Configuration:**
```csharp
protected override void OnModelCreating(ModelBuilder modelBuilder) {
    // User configuration
    modelBuilder.Entity<User>(u => {
        u.HasKey(x => x.Id);
    });
    
    // Note configuration with relationship
    modelBuilder.Entity<Note>(n => {
        n.HasKey(x => x.Id);
        n.HasOne(x => x.User)
            .WithMany(u => u.Notes)
            .HasForeignKey(x => x.UserId);
    });
    
    Seed(modelBuilder);
}
```

### Relationships

**User ← Notes (One-to-Many):**
- One user can have many notes
- Cascade delete enabled (deleting user deletes notes)
- Required relationship (note must have user)

### Seed Data

**Purpose:** Initial data for development/testing

⚠️ **Security Warning:** Contains plaintext password - **NOT for production use**

```csharp
modelBuilder.Entity<User>().HasData(
    new User { Id = 1, Username = "kalle", Password = "password" }
);
modelBuilder.Entity<Note>().HasData(
    new Note { Id = 1, Title = "test", Text = "This is a test", UserId = 1 }
);
```

## How the Patterns Work Together

### Complete Flow Example

```csharp
// 1. Create Unit of Work (manages context)
using var uow = new UnitOfWork(new NotebookDbContext());

// 2. Use repositories (share same context)
var user = await uow.Users.FirstOrDefaultAsync(u => u.Username == "kalle");
var note = new Note { 
    Title = "New Note", 
    Text = "Content", 
    UserId = user.Id 
};
await uow.Notes.AddAsync(note);

// 3. Save changes (single transaction)
await uow.SaveChangesAsync();

// 4. Context disposed automatically
```

### Transaction Guarantees

**Scenario: Adding User and Note**
```csharp
using var uow = new UnitOfWork(context);

var user = new User { Username = "newuser", Password = "password" };
await uow.Users.AddAsync(user);
await uow.SaveChangesAsync(); // User now has ID

var note = new Note { Title = "First Note", UserId = user.Id };
await uow.Notes.AddAsync(note);
await uow.SaveChangesAsync(); // Both committed

// If SaveChangesAsync() fails, transaction rolls back
```

## Pattern Benefits

### Repository Pattern Benefits

✅ **Abstraction**
- Business logic doesn't depend on EF Core
- Easy to switch data access technology
- Clear separation of concerns

✅ **Testability**
- Mock `IRepository<T>` in unit tests
- No database required for testing
- Fast test execution

✅ **Consistency**
- Same API for all entities
- Standardized data access patterns
- Reduced code duplication

✅ **Type Safety**
- Generic constraints enforce EntityBase
- Compile-time type checking
- IntelliSense support

### Unit of Work Benefits

✅ **Transaction Management**
- Multiple operations in single transaction
- All succeed or all fail (atomicity)
- Data consistency guaranteed

✅ **Context Management**
- Single DbContext instance
- Proper resource disposal
- No context leaks

✅ **Simplified API**
- One SaveChanges call
- Clear transaction boundaries
- Easy to reason about

## Pattern Trade-offs

### Advantages

✅ **Clean Architecture**
- Clear layer boundaries
- Testable business logic
- Maintainable codebase

✅ **Flexibility**
- Easy to add new entities
- Simple to extend functionality
- Mock-friendly interfaces

✅ **Safety**
- Type-safe operations
- Compile-time checks
- Controlled data access

### Considerations

⚠️ **Abstraction Cost**
- Additional layer of indirection
- Potential performance overhead
- Learning curve for developers

⚠️ **Limited Composability**
- Returns `IEnumerable<T>` not `IQueryable<T>`
- Complex queries require new methods
- Less flexible than direct EF Core

⚠️ **Advanced Features**
- Eager loading requires specific methods
- Projections need additional support
- Complex queries may need custom implementations

### When to Use This Pattern

**Good Fit:**
- ✅ CRUD-focused applications
- ✅ Clear transaction boundaries
- ✅ Need for testability
- ✅ Multiple data access points
- ✅ Team needs consistency

**Consider Alternatives:**
- ⚠️ Complex query requirements
- ⚠️ Performance-critical applications
- ⚠️ Heavy use of EF Core features
- ⚠️ Simple applications (might be overkill)

## Usage Examples

### Basic CRUD Operations

**Create:**
```csharp
var user = new User { Username = "johndoe", Password = "password" };
await uow.Users.AddAsync(user);
await uow.SaveChangesAsync();
```

**Read:**
```csharp
// By ID
var user = await uow.Users.GetByIdAsync(1);

// By criteria
var users = await uow.Users.FindAsync(u => u.Username.Contains("john"));

// First or default
var user = await uow.Users.FirstOrDefaultAsync(u => u.Username == "johndoe");
```

**Update:**
```csharp
var user = await uow.Users.GetByIdAsync(1);
user.Password = "newpassword";
uow.Users.Update(user);
await uow.SaveChangesAsync();
```

**Delete:**
```csharp
var user = await uow.Users.GetByIdAsync(1);
uow.Users.Remove(user);
await uow.SaveChangesAsync();
```

### Complex Queries

**Multiple Conditions:**
```csharp
var recentNotes = await uow.Notes.FindAsync(n => 
    n.UserId == userId && 
    n.Updated > DateTime.Now.AddDays(-7)
);
```

**Check Existence:**
```csharp
var hasNotes = await uow.Notes.AnyAsync(n => n.UserId == userId);
```

**Get All:**
```csharp
var allUsers = await uow.Users.GetAllAsync();
```

### Transaction Management

**Multiple Operations:**
```csharp
using var uow = new UnitOfWork(context);

// Add user
var user = new User { Username = "newuser", Password = "password" };
await uow.Users.AddAsync(user);

// Add multiple notes
foreach (var noteData in notes) {
    await uow.Notes.AddAsync(new Note { 
        Title = noteData.Title,
        Text = noteData.Text,
        UserId = user.Id
    });
}

// Single commit
await uow.SaveChangesAsync();
```

**Error Handling:**
```csharp
using var uow = new UnitOfWork(context);

try {
    await uow.Users.AddAsync(user);
    await uow.Notes.AddAsync(note);
    await uow.SaveChangesAsync();
} catch (DbUpdateException ex) {
    // Handle database errors
    // Transaction automatically rolled back
}
```

## Database Migrations

### Creating Migrations

```bash
# Add new migration
dotnet ef migrations add MigrationName

# Update database
dotnet ef database update

# Remove last migration
dotnet ef migrations remove
```

### Migration Files

Located in `Data/Migrations/`:
- `YYYYMMDDHHMMSS_MigrationName.cs` - Migration logic
- `YYYYMMDDHHMMSS_MigrationName.Designer.cs` - Metadata
- `NotebookDbContextModelSnapshot.cs` - Current model state

## Technology Stack

- **.NET 8** - Target framework
- **C# 12** - Language version
- **Entity Framework Core 8** - ORM
- **SQL Server** - Database provider
- **LocalDB** - Development database

## Dependencies

### NuGet Packages

```xml
<PackageReference Include="Microsoft.EntityFrameworkCore" Version="8.0.*" />
<PackageReference Include="Microsoft.EntityFrameworkCore.SqlServer" Version="8.0.*" />
<PackageReference Include="Microsoft.EntityFrameworkCore.Tools" Version="8.0.*" />
```

### Project References

```xml
<ProjectReference Include="..\Entities\Entities.csproj" />
```

## Integration with Other Layers

### From Logic Layer

```csharp
public class AddNoteController : IAddNoteController {
    private readonly IUnitOfWork _uow;
    
    public async Task AddNote(Note note) {
        await _uow.Notes.AddAsync(note);
        await _uow.SaveChangesAsync();
    }
}
```

### From UI Layer

```csharp
// Program.cs
var uow = new UnitOfWork(new NotebookDbContext());
var controller = new AddNoteController(uow);
```

## Testing Strategies

### Unit Testing Repositories

```csharp
[Test]
public async Task AddAsync_AddsEntityToContext() {
    // Arrange
    var options = new DbContextOptionsBuilder<NotebookDbContext>()
        .UseInMemoryDatabase(databaseName: "TestDb")
        .Options;
    
    using var context = new NotebookDbContext(options);
    var repository = new Repository<User>(context);
    var user = new User { Username = "test", Password = "password" };
    
    // Act
    await repository.AddAsync(user);
    await context.SaveChangesAsync();
    
    // Assert
    var saved = await repository.GetByIdAsync(user.Id);
    Assert.IsNotNull(saved);
    Assert.AreEqual("test", saved.Username);
}
```

### Mocking for Business Logic Tests

```csharp
[Test]
public async Task AddNote_CallsRepository() {
    // Arrange
    var mockUow = new Mock<IUnitOfWork>();
    var mockNoteRepo = new Mock<IRepository<Note>>();
    mockUow.Setup(u => u.Notes).Returns(mockNoteRepo.Object);
    
    var controller = new AddNoteController(mockUow.Object);
    var note = new Note { Title = "Test", Text = "Content" };
    
    // Act
    await controller.AddNote(note);
    
    // Assert
    mockNoteRepo.Verify(r => r.AddAsync(note), Times.Once);
    mockUow.Verify(u => u.SaveChangesAsync(), Times.Once);
}
```

## Performance Considerations

### Query Performance

**Good:**
```csharp
// Filtered query - only retrieves matching records
var users = await uow.Users.FindAsync(u => u.Username == "john");
```

**Avoid:**
```csharp
// Gets ALL users, then filters in memory
var allUsers = await uow.Users.GetAllAsync();
var filtered = allUsers.Where(u => u.Username == "john");
```

### Change Tracking

EF Core tracks all retrieved entities. For read-only operations:

```csharp
// Consider adding AsNoTracking support
public async Task<IEnumerable<T>> GetAllReadOnlyAsync()
    => await _dbSet.AsNoTracking().ToListAsync();
```

## Known Limitations

### 1. No Eager Loading Support

**Issue:** Cannot easily include related entities

**Current:**
```csharp
// Doesn't load Notes
var user = await uow.Users.GetByIdAsync(1);
// user.Notes is not loaded
```

**Solution:**
```csharp
// Add to IRepository<T>
Task<T?> GetByIdWithIncludesAsync(int id, params Expression<Func<T, object>>[] includes);

// Implementation
public async Task<T?> GetByIdWithIncludesAsync(
    int id, 
    params Expression<Func<T, object>>[] includes) {
    
    IQueryable<T> query = _dbSet;
    foreach (var include in includes) {
        query = query.Include(include);
    }
    return await query.FirstOrDefaultAsync(e => e.Id == id);
}
```

### 2. No Pagination Support

**Issue:** `GetAllAsync()` loads all entities into memory

**Solution:**
```csharp
// Add to IRepository<T>
Task<IEnumerable<T>> GetPagedAsync(int pageNumber, int pageSize);
Task<int> CountAsync(Expression<Func<T, bool>>? predicate = null);
```

### 3. No Sorting Support

**Issue:** Results returned in database order

**Solution:**
```csharp
// Add to IRepository<T>
Task<IEnumerable<T>> FindAsync(
    Expression<Func<T, bool>> predicate,
    Func<IQueryable<T>, IOrderedQueryable<T>>? orderBy = null);
```

## Future Enhancements

### Short Term
- ✨ Add pagination support
- ✨ Add sorting capabilities
- ✨ Implement eager loading
- ✨ Add AsNoTracking for read-only queries

### Medium Term
- ✨ Implement specification pattern
- ✨ Add bulk operations
- ✨ Support for projections
- ✨ Query result caching

### Long Term
- ✨ Multi-tenancy support
- ✨ Audit logging
- ✨ Soft delete implementation
- ✨ Database sharding support

## Best Practices

### ✅ DO

1. **Use Unit of Work for transactions**
2. **Dispose contexts properly** (using statement)
3. **Async operations** for all database calls
4. **Filter at database level** with predicates
5. **Handle DbUpdateException** for constraint violations

### ❌ DON'T

1. **Create multiple DbContext instances** per operation
2. **Use GetAllAsync()** for large tables
3. **Modify entities without Update()** call
4. **Forget to call SaveChangesAsync()**
5. **Expose IQueryable** to business logic

## Related Documentation

- **[Solution README](../README.md)** - Overall architecture
- **[Entities README](../Entities/README.md)** - Domain models
- **[Logic README](../Logic/README.md)** - Business logic
- **[UI README](../UI/README.md)** - User interface

## Contributing

When modifying the Data layer:

1. ✅ Maintain interface contracts
2. ✅ Update XML documentation
3. ✅ Create EF Core migrations
4. ✅ Test with unit tests
5. ✅ Consider backward compatibility
6. ✅ Document breaking changes
7. ✅ Follow async/await patterns
