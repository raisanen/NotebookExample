using Entities;

namespace Logic {
    /// <summary>
    /// Defines the contract for menu-related operations.
    /// </summary>
    /// <remarks>
    /// This interface provides functionality to determine what menu options
    /// should be available to the user based on their data.
    /// </remarks>
    public interface IMenuController {
        /// <summary>
        /// Asynchronously determines whether a user has any notes.
        /// </summary>
        /// <param name="user">The user to check for notes.</param>
        /// <returns>
        /// A task that represents the asynchronous operation.
        /// The task result contains <c>true</c> if the user has at least one note;
        /// otherwise, <c>false</c>.
        /// </returns>
        /// <remarks>
        /// This method is typically used to conditionally display menu options,
        /// such as hiding "Show notes" if the user has no notes.
        /// </remarks>
        public Task<bool> HasNotes(User user);
    }
}
