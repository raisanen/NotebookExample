using Logic;

namespace UI.TUI.Page {
    /// <summary>
    /// Implements the main menu page of the application.
    /// </summary>
    /// <remarks>
    /// <para>
    /// This page displays a navigable menu with options to show notes, add notes, or quit.
    /// The menu dynamically adjusts based on whether the user has any notes.
    /// </para>
    /// <para>
    /// Requires the user to be logged in before displaying.
    /// </para>
    /// </remarks>
    public class MenuPage : IPage {
        /// <summary>
        /// The controller responsible for menu-related operations.
        /// </summary>
        private readonly IMenuController _controller;

        /// <summary>
        /// Initializes a new instance of the <see cref="MenuPage"/> class.
        /// </summary>
        /// <param name="controller">The controller for menu operations.</param>
        public MenuPage(IMenuController controller) {
            _controller = controller;
        }

        /// <inheritdoc />
        public string Title { get; private set; } = "Notebook menu";

        /// <inheritdoc />
        /// <remarks>
        /// <para>
        /// This method:
        /// <list type="number">
        /// <item><description>Checks if the user is logged in, redirecting to login if not</description></item>
        /// <item><description>Creates a menu with options for showing notes, adding notes, and quitting</description></item>
        /// <item><description>Checks if the user has notes and removes "Show notes" option if they don't</description></item>
        /// <item><description>Displays the menu and returns the selected page state</description></item>
        /// </list>
        /// </para>
        /// <para>
        /// The menu is dynamically generated to improve user experience by hiding
        /// unavailable options.
        /// </para>
        /// </remarks>
        public async Task<PageState> Render() {
            if (!AppState.Instance.IsLoggedIn) {
                return PageState.Login;
            }

            var menu = new Menu<PageState>() { 
                Items = new() {
                    ["Show notes"] = () => PageState.ShowNotes,
                    ["Add note"]   = () => PageState.AddNote,
                    ["Quit"]       = () => PageState.End
                }
            };
            var hasNotes = await _controller.HasNotes(AppState.Instance.CurrentUser!);

            if (!hasNotes) {
                menu.Items.Remove("Show notes");
            }

            return IO.Instance.ShowMenu(menu, Title);
        }
    }
}
