using Entities;

namespace Logic {
    /// <summary>
    /// Defines the contract for adding new notes to the system.
    /// </summary>
    /// <remarks>
    /// This interface is part of the business logic layer and abstracts the operation
    /// of adding notes, making the implementation testable and decoupled from specific
    /// data access concerns.
    /// </remarks>
    public interface IAddNoteController {
        /// <summary>
        /// Asynchronously adds a new note to the system.
        /// </summary>
        /// <param name="note">The note entity to add. Must have a valid <see cref="Note.UserId"/>.</param>
        /// <returns>A task that represents the asynchronous operation.</returns>
        /// <remarks>
        /// The note will be persisted to the database as part of this operation.
        /// The <see cref="Note.Updated"/> timestamp should be set before calling this method.
        /// </remarks>
        Task AddNote(Note note);
    }
}