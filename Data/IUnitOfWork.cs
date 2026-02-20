using Entities;

namespace Data {
    /// <summary>
    /// Defines the Unit of Work pattern for coordinating multiple repository operations
    /// within a single database transaction context.
    /// </summary>
    /// <remarks>
    /// <para>
    /// The Unit of Work pattern maintains a list of objects affected by a business transaction
    /// and coordinates the writing of changes to the database. All repositories accessed through
    /// this interface share the same <see cref="NotebookDbContext"/> instance, ensuring that
    /// changes across multiple repositories are committed atomically.
    /// </para>
    /// <para>
    /// This interface extends <see cref="IDisposable"/> to properly manage the lifetime of the
    /// underlying database context.
    /// </para>
    /// </remarks>
    /// <example>
    /// <code>
    /// public class AddNoteController {
    ///     private readonly IUnitOfWork _uow;
    ///     
    ///     public AddNoteController(IUnitOfWork uow) {
    ///         _uow = uow;
    ///     }
    ///     
    ///     public async Task AddNote(Note note) {
    ///         // Add note using the repository
    ///         await _uow.Notes.AddAsync(note);
    ///         
    ///         // Could perform other operations on Users or Notes here
    ///         
    ///         // Commit all changes in a single transaction
    ///         await _uow.SaveChangesAsync();
    ///     }
    /// }
    /// </code>
    /// </example>
    public interface IUnitOfWork : IDisposable {
        /// <summary>
        /// Gets the repository for <see cref="User"/> entities.
        /// </summary>
        /// <value>
        /// An <see cref="IRepository{T}"/> instance for managing User entities.
        /// </value>
        /// <remarks>
        /// This property uses lazy initialization - the repository is created only when first accessed.
        /// </remarks>
        IRepository<User> Users{ get; }

        /// <summary>
        /// Gets the repository for <see cref="Note"/> entities.
        /// </summary>
        /// <value>
        /// An <see cref="IRepository{T}"/> instance for managing Note entities.
        /// </value>
        /// <remarks>
        /// This property uses lazy initialization - the repository is created only when first accessed.
        /// </remarks>
        IRepository<Note> Notes { get; }

        /// <summary>
        /// Asynchronously saves all changes made through the repositories to the database
        /// as a single atomic transaction.
        /// </summary>
        /// <returns>
        /// A task that represents the asynchronous save operation.
        /// The task result contains the number of state entries written to the database.
        /// </returns>
        /// <remarks>
        /// <para>
        /// This method should be called after all repository operations (Add, Update, Remove)
        /// have been performed. All changes tracked by the underlying <see cref="NotebookDbContext"/>
        /// will be persisted to the database in a single transaction.
        /// </para>
        /// <para>
        /// If any operation fails, the entire transaction is rolled back, ensuring data consistency.
        /// </para>
        /// </remarks>
        /// <exception cref="DbUpdateException">
        /// Thrown when an error occurs while saving changes to the database, such as constraint violations.
        /// </exception>
        /// <exception cref="DbUpdateConcurrencyException">
        /// Thrown when a concurrency violation occurs during the save operation.
        /// </exception>
        Task<int> SaveChangesAsync();
    }
}
