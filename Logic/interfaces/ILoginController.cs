using Entities;

namespace Logic {
    /// <summary>
    /// Defines the contract for user authentication operations.
    /// </summary>
    /// <remarks>
    /// This interface is part of the business logic layer and abstracts the authentication
    /// process, making the implementation testable and decoupled from specific data access concerns.
    /// </remarks>
    public interface ILoginController {
        /// <summary>
        /// Asynchronously authenticates a user with the provided credentials.
        /// </summary>
        /// <param name="username">The username to authenticate.</param>
        /// <param name="password">The password to verify.</param>
        /// <returns>
        /// A task that represents the asynchronous operation.
        /// The task result contains the authenticated <see cref="User"/> if credentials are valid,
        /// or <c>null</c> if authentication fails.
        /// </returns>
        /// <remarks>
        /// Warning: The current implementation uses plaintext password comparison, which is
        /// not secure for production use. Consider implementing proper password hashing.
        /// </remarks>
        Task<User?> Login(string username, string password);
    }
}
