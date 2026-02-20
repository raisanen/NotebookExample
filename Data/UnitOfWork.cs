using Entities;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Data {
    /// <summary>
    /// Implements the Unit of Work pattern to coordinate operations across multiple repositories
    /// and manage database transactions through a single <see cref="NotebookDbContext"/> instance.
    /// </summary>
    /// <remarks>
    /// <para>
    /// This class ensures that all repository operations share the same database context,
    /// allowing multiple data modifications to be committed atomically. Changes made through
    /// any repository are tracked by the context but not persisted until <see cref="SaveChangesAsync"/>
    /// is explicitly called.
    /// </para>
    /// <para>
    /// Repositories are lazily initialized when first accessed to optimize resource usage.
    /// The same repository instance is returned on subsequent accesses.
    /// </para>
    /// </remarks>
    public class UnitOfWork : IUnitOfWork {
        /// <summary>
        /// The shared database context used by all repositories in this unit of work.
        /// </summary>
        private readonly NotebookDbContext _context;

        /// <summary>
        /// Backing field for the <see cref="Users"/> repository, used for lazy initialization.
        /// </summary>
        private IRepository<User>? _users;

        /// <summary>
        /// Backing field for the <see cref="Notes"/> repository, used for lazy initialization.
        /// </summary>
        private IRepository<Note>? _notes;

        /// <inheritdoc />
        /// <remarks>
        /// Uses the null-coalescing assignment operator (??=) to lazily create the repository
        /// only when first accessed. Subsequent accesses return the cached instance.
        /// </remarks>
        public IRepository<User> Users => _users ??= new Repository<User>(_context);

        /// <inheritdoc />
        /// <remarks>
        /// Uses the null-coalescing assignment operator (??=) to lazily create the repository
        /// only when first accessed. Subsequent accesses return the cached instance.
        /// </remarks>
        public IRepository<Note> Notes => _notes ??= new Repository<Note>(_context);

        /// <summary>
        /// Initializes a new instance of the <see cref="UnitOfWork"/> class.
        /// </summary>
        /// <param name="context">The database context to be shared across all repositories.</param>
        /// <remarks>
        /// This constructor is typically called by the dependency injection container,
        /// which manages the lifetime of the <see cref="NotebookDbContext"/>.
        /// </remarks>
        public UnitOfWork(NotebookDbContext context) {
            _context = context;
        }

        /// <inheritdoc />
        /// <remarks>
        /// This method delegates to the underlying <see cref="NotebookDbContext.SaveChangesAsync()"/>
        /// method, which persists all tracked changes to the database in a single transaction.
        /// If any error occurs, the entire transaction is rolled back.
        /// </remarks>
        public async Task<int> SaveChangesAsync()
            => await _context.SaveChangesAsync();

        /// <inheritdoc />
        /// <remarks>
        /// Disposes the underlying <see cref="NotebookDbContext"/>, releasing database connections
        /// and other resources. This method is typically called automatically by the dependency
        /// injection container at the end of the request scope.
        /// </remarks>
        public void Dispose()
            => _context.Dispose();
    }
}
