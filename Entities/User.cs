using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Entities {
    /// <summary>
    /// Represents a user in the Notebook application.
    /// </summary>
    /// <remarks>
    /// <para>
    /// A user can have multiple notes associated with their account. The relationship
    /// is configured as one-to-many with cascade delete, meaning deleting a user will
    /// automatically delete all their notes.
    /// </para>
    /// <para>
    /// Warning: This entity stores passwords in plaintext, which is not secure for
    /// production use. Consider implementing proper password hashing (e.g., using BCrypt,
    /// PBKDF2, or Argon2) before deploying to production.
    /// </para>
    /// </remarks>
    public class User : EntityBase {
        /// <summary>
        /// Gets or sets the username for this user.
        /// </summary>
        /// <value>
        /// A string representing the user's login name. Defaults to an empty string.
        /// </value>
        public string Username { get; set; } = string.Empty;

        /// <summary>
        /// Gets or sets the password for this user.
        /// </summary>
        /// <value>
        /// A string representing the user's password. Defaults to an empty string.
        /// </value>
        /// <remarks>
        /// Warning: This property currently stores passwords in plaintext. In production,
        /// this should store a hashed password instead.
        /// </remarks>
        public string Password { get; set; } = string.Empty;

        /// <summary>
        /// Gets or sets the collection of notes associated with this user.
        /// </summary>
        /// <value>
        /// A collection of <see cref="Note"/> entities owned by this user.
        /// Initialized to an empty list to prevent null reference exceptions.
        /// </value>
        /// <remarks>
        /// This navigation property represents the "many" side of the one-to-many
        /// relationship between users and notes.
        /// </remarks>
        public ICollection<Note> Notes { get; set; } = new List<Note>();
    }
}
