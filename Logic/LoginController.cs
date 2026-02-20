using Data;
using Entities;

namespace Logic {
    /// <summary>
    /// Implements business logic for user authentication.
    /// </summary>
    /// <remarks>
    /// <para>
    /// This controller uses the Unit of Work pattern to query the Users repository
    /// and verify credentials.
    /// </para>
    /// <para>
    /// Warning: This implementation performs plaintext password comparison, which is
    /// insecure for production environments. Proper password hashing (e.g., BCrypt, PBKDF2,
    /// or Argon2) should be implemented before production deployment.
    /// </para>
    /// </remarks>
    public class LoginController : ILoginController {
        /// <summary>
        /// The Unit of Work instance used to coordinate repository operations.
        /// </summary>
        private readonly IUnitOfWork _uow;

        /// <summary>
        /// Initializes a new instance of the <see cref="LoginController"/> class.
        /// </summary>
        /// <param name="uow">The Unit of Work instance for managing data operations.</param>
        /// <remarks>
        /// This constructor is typically called by the dependency injection container.
        /// </remarks>
        public LoginController(IUnitOfWork uow) {
            _uow = uow;
        }

        /// <inheritdoc />
        /// <remarks>
        /// This implementation queries the Users repository using a predicate that matches
        /// both username and password. Returns the first matching user or <c>null</c> if
        /// no match is found.
        /// </remarks>
        public async Task<User?> Login(string username, string password) 
            => await _uow.Users.FirstOrDefaultAsync(u => u.Username == username && u.Password == password);
    }
}
