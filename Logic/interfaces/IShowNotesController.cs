using Entities;

namespace Logic {
    /// <summary>
    /// Defines the contract for retrieving notes from the system.
    /// </summary>
    /// <remarks>
    /// This interface is part of the business logic layer and abstracts the operation
    /// of querying notes, making the implementation testable and decoupled from specific
    /// data access concerns.
    /// </remarks>
    public interface IShowNotesController {
        /// <summary>
        /// Asynchronously retrieves all notes belonging to a specific user.
        /// </summary>
        /// <param name="user">The user whose notes should be retrieved.</param>
        /// <returns>
        /// A task that represents the asynchronous operation.
        /// The task result contains a collection of all notes owned by the specified user.
        /// </returns>
        /// <remarks>
        /// This method filters notes by the user's ID to ensure data isolation between users.
        /// </remarks>
        Task<IEnumerable<Note>> GetNotesForUser(User user);
    }
}
