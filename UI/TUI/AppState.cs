using Entities;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace UI.TUI {
    /// <summary>
    /// Manages the global application state using the Singleton pattern.
    /// </summary>
    /// <remarks>
    /// <para>
    /// This class maintains session-level information such as the currently logged-in user.
    /// It uses lazy initialization to ensure only one instance exists throughout the
    /// application lifetime.
    /// </para>
    /// <para>
    /// Note: In a multi-user or web application, this pattern would need to be replaced
    /// with proper session management.
    /// </para>
    /// </remarks>
    public class AppState {
        /// <summary>
        /// The singleton instance backing field.
        /// </summary>
        private static AppState? _instance;

        /// <summary>
        /// Gets the singleton instance of <see cref="AppState"/>.
        /// </summary>
        /// <value>
        /// The single <see cref="AppState"/> instance for the application.
        /// </value>
        /// <remarks>
        /// Uses the null-coalescing assignment operator (??=) for thread-safe lazy initialization.
        /// </remarks>
        public static AppState Instance => _instance ??= new AppState();

        /// <summary>
        /// Gets or sets the currently logged-in user.
        /// </summary>
        /// <value>
        /// The <see cref="User"/> entity representing the current user,
        /// or <c>null</c> if no user is logged in.
        /// </value>
        public User? CurrentUser { get; set; }

        /// <summary>
        /// Gets a value indicating whether a user is currently logged in.
        /// </summary>
        /// <value>
        /// <c>true</c> if <see cref="CurrentUser"/> is not <c>null</c>; otherwise, <c>false</c>.
        /// </value>
        /// <remarks>
        /// This is a convenience property to simplify authentication checks throughout the application.
        /// </remarks>
        public bool IsLoggedIn => CurrentUser != null;

        /// <summary>
        /// Prevents external instantiation, enforcing the singleton pattern.
        /// </summary>
        private AppState() { }
    }
}
