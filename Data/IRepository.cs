using Entities;
using System.Linq.Expressions;

namespace Data {
    /// <summary>
    /// Defines a generic repository interface for performing data access operations on entities.
    /// This interface provides a consistent abstraction over Entity Framework Core operations,
    /// enabling testability and reducing code duplication across different entity types.
    /// </summary>
    /// <typeparam name="T">The entity type that inherits from <see cref="EntityBase"/>.</typeparam>
    public interface IRepository<T> where T : EntityBase {
        /// <summary>
        /// Determines whether the repository contains any entities.
        /// </summary>
        /// <returns>
        /// A task that represents the asynchronous operation.
        /// The task result contains <c>true</c> if the repository contains any entities; otherwise, <c>false</c>.
        /// </returns>
        Task<bool> AnyAsync();

        /// <summary>
        /// Determines whether any entities in the repository satisfy the specified predicate.
        /// </summary>
        /// <param name="predicate">An expression to filter entities.</param>
        /// <returns>
        /// A task that represents the asynchronous operation.
        /// The task result contains <c>true</c> if any entities satisfy the condition; otherwise, <c>false</c>.
        /// </returns>
        Task<bool> AnyAsync(Expression<Func<T, bool>> predicate);

        /// <summary>
        /// Retrieves an entity by its unique identifier.
        /// </summary>
        /// <param name="id">The primary key value of the entity to retrieve.</param>
        /// <returns>
        /// A task that represents the asynchronous operation.
        /// The task result contains the entity with the specified ID, or <c>null</c> if not found.
        /// </returns>
        Task<T?> GetByIdAsync(int id);

        /// <summary>
        /// Retrieves all entities from the repository.
        /// </summary>
        /// <returns>
        /// A task that represents the asynchronous operation.
        /// The task result contains a collection of all entities in the repository.
        /// </returns>
        Task<IEnumerable<T>> GetAllAsync();

        /// <summary>
        /// Retrieves all entities that satisfy the specified predicate.
        /// This method accepts an expression tree to allow flexible filtering while maintaining
        /// abstraction over the underlying data source.
        /// </summary>
        /// <param name="predicate">An expression that defines the criteria for filtering entities.</param>
        /// <returns>
        /// A task that represents the asynchronous operation.
        /// The task result contains a collection of entities that match the predicate.
        /// </returns>
        /// <example>
        /// <code>
        /// var activeUsers = await repository.FindAsync(u => u.IsActive);
        /// var recentNotes = await repository.FindAsync(n => n.Updated > DateTime.Now.AddDays(-7));
        /// </code>
        /// </example>
        Task<IEnumerable<T>> FindAsync(Expression<Func<T, bool>> predicate);

        /// <summary>
        /// Retrieves the first entity that satisfies the specified predicate, or <c>null</c> if no such entity exists.
        /// </summary>
        /// <param name="predicate">An expression that defines the criteria for finding the entity.</param>
        /// <returns>
        /// A task that represents the asynchronous operation.
        /// The task result contains the first matching entity, or <c>null</c> if no match is found.
        /// </returns>
        Task<T?> FirstOrDefaultAsync(Expression<Func<T, bool>> predicate);

        /// <summary>
        /// Adds a new entity to the repository.
        /// Note: Changes are not persisted to the database until <see cref="IUnitOfWork.SaveChangesAsync"/> is called.
        /// </summary>
        /// <param name="entity">The entity to add.</param>
        /// <returns>A task that represents the asynchronous operation.</returns>
        Task AddAsync(T entity);

        /// <summary>
        /// Marks an existing entity as modified.
        /// Note: Changes are not persisted to the database until <see cref="IUnitOfWork.SaveChangesAsync"/> is called.
        /// </summary>
        /// <param name="entity">The entity to update with modified values.</param>
        void Update(T entity);

        /// <summary>
        /// Marks an entity for deletion from the repository.
        /// Note: Changes are not persisted to the database until <see cref="IUnitOfWork.SaveChangesAsync"/> is called.
        /// </summary>
        /// <param name="entity">The entity to remove.</param>
        void Remove(T entity);
    }
}
