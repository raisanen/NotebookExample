# Notebook Application

A .NET 8 application demonstrating clean architecture principles with Entity Framework Core, implementing the **Generic Repository** and **Unit of Work** patterns for data access.

## Architecture Overview

This solution follows a layered architecture with clear separation of concerns:

### Project Structure

- **Entities**: Domain models and base classes
- **Data**: Data access layer with Repository and Unit of Work implementations
- **Logic**: Business logic and controllers

## Generic Repository Pattern

The Generic Repository pattern provides a reusable abstraction over data access operations, eliminating repetitive CRUD code.

### Key Components

#### `EntityBase`
All entities inherit from this base class, ensuring they have a common `Id` property:

#### `IRepository<T>`
The repository interface defines standard data operations:

#### `Repository<T>`
Generic implementation that works with any entity inheriting from `EntityBase`:

### Benefits

✅ **Eliminates code duplication** - Write data access logic once  
✅ **Type-safe operations** - Generic constraints ensure proper entity types  
✅ **Testability** - Easy to mock `IRepository<T>` for unit tests  
✅ **Consistency** - All entities use the same data access patterns  

## Unit of Work Pattern

The Unit of Work pattern manages transactions and coordinates changes across multiple repositories.

### Key Components

#### `IUnitOfWork`
Exposes repositories and transaction control:

#### `UnitOfWork`
Coordinates repositories and manages the `DbContext`:

### How It Works

1. **Single DbContext Instance**: All repositories share the same `NotebookDbContext`
2. **Lazy Initialization**: Repositories are created only when accessed
3. **Atomic Transactions**: Changes across multiple repositories are saved together
4. **Resource Management**: Implements `IDisposable` for proper cleanup

### Benefits

✅ **Transaction Management** - Multiple operations succeed or fail together  
✅ **Single Point of Commit** - Call `SaveChangesAsync()` once for all changes  
✅ **Reduced Coupling** - Business logic doesn't directly depend on `DbContext`  
✅ **Prevents Inconsistencies** - Ensures data integrity across related operations  

## Pattern Trade-offs

### Advantages
- **Abstraction**: Business logic is decoupled from Entity Framework
- **Testability**: Easy to mock and unit test
- **Maintainability**: Centralized data access logic
- **Consistency**: Standardized approach across all entities

### Considerations
- **Expression Trees**: The repository accepts `Expression<Func<T, bool>>` predicates, allowing flexible queries while hiding EF Core's `IQueryable`
- **Limited Composability**: By returning `IEnumerable<T>` instead of `IQueryable<T>`, complex query composition is traded for better abstraction
- **Include Operations**: Advanced EF Core features (eager loading, projections) may require specific repository methods

## Domain Model

### Entities

**User**

**Note**

### Relationships
- One `User` can have many `Notes` (one-to-many)
- Cascade delete is configured (deleting a user deletes their notes)

## Technology Stack

- **.NET 8** - Target framework
- **C# 12** - Language version
- **Entity Framework Core 8** - ORM
- **SQL Server (LocalDB)** - Database

## Database Setup

Connection string configured in `NotebookDbContext`:

```
Server=(localdb)\mssqllocaldb;Database=NotebookDB
```

Initial seed data includes:
- User: `kalle` / `password`
- Note: "test" / "This is a test"

## Getting Started

### Prerequisites
- .NET 8 SDK
- SQL Server LocalDB

### Running Migrations

## Further Reading

- [Repository Pattern - Martin Fowler](https://martinfowler.com/eaaCatalog/repository.html)
- [Unit of Work Pattern - Martin Fowler](https://martinfowler.com/eaaCatalog/unitOfWork.html)
- [EF Core - Generic Repository Considerations](https://learn.microsoft.com/en-us/ef/core/)
