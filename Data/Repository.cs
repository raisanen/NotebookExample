using Entities;
using Microsoft.EntityFrameworkCore;
using System.Linq.Expressions;

namespace Data {
    /// <summary>
    /// Provides a generic implementation of the repository pattern for data access operations.
    /// This class encapsulates Entity Framework Core operations and provides a consistent
    /// interface for working with entities that inherit from <see cref="EntityBase"/>.
    /// </summary>
    /// <typeparam name="T">The entity type that inherits from <see cref="EntityBase"/>.</typeparam>
    /// <remarks>
    /// This implementation uses a shared <see cref="NotebookDbContext"/> instance, typically
    /// managed by the <see cref="UnitOfWork"/> class. Changes made through this repository
    /// are tracked by Entity Framework Core but not persisted until SaveChanges is called
    /// on the context.
    /// </remarks>
    public class Repository<T> : IRepository<T> where T: EntityBase {
        /// <summary>
        /// The database context used for data access operations.
        /// </summary>
        protected readonly NotebookDbContext _context;

        /// <summary>
        /// The Entity Framework DbSet for the entity type T.
        /// </summary>
        protected readonly DbSet<T> _dbSet;

        /// <summary>
        /// Initializes a new instance of the <see cref="Repository{T}"/> class.
        /// </summary>
        /// <param name="context">The database context to use for data operations.</param>
        public Repository(NotebookDbContext context) {
            _context = context;
            _dbSet = context.Set<T>();
        }

        /// <inheritdoc />
        public async Task<bool> AnyAsync(Expression<Func<T, bool>> predicate)
            => await _dbSet.AnyAsync(predicate);

        /// <inheritdoc />
        public async Task<bool> AnyAsync()
            => await _dbSet.AnyAsync();

        /// <inheritdoc />
        /// <remarks>
        /// This method uses Entity Framework's <c>FindAsync</c>, which first checks the
        /// change tracker for the entity before querying the database, providing better
        /// performance for entities already loaded in the context.
        /// </remarks>
        public async Task<T?> GetByIdAsync(int id)
            => await _dbSet.FindAsync(id);

        /// <inheritdoc />
        /// <remarks>
        /// Warning: This method loads all entities into memory. For large datasets,
        /// consider using <see cref="FindAsync"/> with appropriate filtering predicates.
        /// </remarks>
        public async Task<IEnumerable<T>> GetAllAsync()
            => await _dbSet.ToListAsync();

        /// <inheritdoc />
        /// <remarks>
        /// The predicate is translated to SQL by Entity Framework Core, allowing efficient
        /// server-side filtering. The results are materialized into a list before returning.
        /// </remarks>
        public async Task<IEnumerable<T>> FindAsync(Expression<Func<T, bool>> predicate)
            => await _dbSet.Where(predicate).ToListAsync();

        /// <inheritdoc />
        public async Task<T?> FirstOrDefaultAsync(Expression<Func<T, bool>> predicate)
            => await _dbSet.FirstOrDefaultAsync(predicate);

        /// <inheritdoc />
        /// <remarks>
        /// The entity is added to the change tracker in the Added state.
        /// The entity will be inserted into the database when SaveChanges is called.
        /// </remarks>
        public async Task AddAsync(T entity)
            => await _dbSet.AddAsync(entity);

        /// <inheritdoc />
        /// <remarks>
        /// This method marks all properties of the entity as modified.
        /// The entity will be updated in the database when SaveChanges is called.
        /// If you need to update only specific properties, consider using the
        /// context's Entry method directly or attach the entity first.
        /// </remarks>
        public void Update(T entity)
            => _dbSet.Update(entity);

        /// <inheritdoc />
        /// <remarks>
        /// The entity is marked for deletion in the change tracker.
        /// The entity will be deleted from the database when SaveChanges is called.
        /// </remarks>
        public void Remove(T entity)
            => _dbSet.Remove(entity);
    }
}
