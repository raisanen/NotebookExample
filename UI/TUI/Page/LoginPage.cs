using Logic;

namespace UI.TUI.Page {
    /// <summary>
    /// Implements the login page for user authentication.
    /// </summary>
    /// <remarks>
    /// This page prompts the user for credentials and attempts to authenticate them.
    /// Upon successful login, the user is stored in the <see cref="AppState"/> and
    /// navigation proceeds to the main menu. Failed login attempts loop back to the login page.
    /// </remarks>
    public class LoginPage : IPage {
        /// <summary>
        /// The controller responsible for authentication.
        /// </summary>
        private readonly ILoginController _controller;

        /// <summary>
        /// Initializes a new instance of the <see cref="LoginPage"/> class.
        /// </summary>
        /// <param name="controller">The controller for authentication operations.</param>
        public LoginPage(ILoginController controller) {
            _controller = controller;
        }

        /// <inheritdoc />
        public string Title { get; protected set; } = "Log in";

        /// <inheritdoc />
        /// <remarks>
        /// <para>
        /// This method:
        /// <list type="number">
        /// <item><description>Prompts for username (visible input)</description></item>
        /// <item><description>Prompts for password (masked input)</description></item>
        /// <item><description>Attempts authentication via the controller</description></item>
        /// <item><description>Stores the authenticated user in <see cref="AppState"/> if successful</description></item>
        /// <item><description>Returns <see cref="PageState.Menu"/> on success or loops back to <see cref="PageState.Login"/> on failure</description></item>
        /// </list>
        /// </para>
        /// </remarks>
        public async Task<PageState> Render() {
            var username = IO.Instance.ReadString  ("Username: ");
            var password = IO.Instance.ReadPassword("Password: ");

            Console.WriteLine($"Trying to log in {username}...");

            AppState.Instance.CurrentUser = await _controller.Login(username, password);

            if (!AppState.Instance.IsLoggedIn) {
                return PageState.Login;
            }

            return PageState.Menu;
        }
    }
}
