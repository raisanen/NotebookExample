using Entities;
using Logic;

namespace UI.TUI.Page {
    /// <summary>
    /// Implements the page for displaying all notes belonging to the current user.
    /// </summary>
    /// <remarks>
    /// This page retrieves and displays the user's notes in a formatted view.
    /// Requires the user to be logged in before displaying.
    /// </remarks>
    public class ShowNotesPage : IPage {
        /// <summary>
        /// The controller responsible for retrieving notes.
        /// </summary>
        private readonly IShowNotesController _controller;

        /// <summary>
        /// Initializes a new instance of the <see cref="ShowNotesPage"/> class.
        /// </summary>
        /// <param name="controller">The controller for retrieving notes.</param>
        public ShowNotesPage(IShowNotesController controller) {
            _controller = controller;
        }

        /// <inheritdoc />
        public string Title { get; private set; } = "Notebook";

        /// <summary>
        /// Prints a single note to the console with formatting.
        /// </summary>
        /// <param name="note">The note to display.</param>
        /// <remarks>
        /// Formats the note with a header showing the title and the note text,
        /// separated by decorative lines.
        /// </remarks>
        private void PrintNote(Note note) {
            Console.WriteLine();
            IO.Instance.PrintAltLine();
            IO.Instance.PrintSubHeader(note.Title);
            IO.Instance.PrintFullWidth(note.Text);
            IO.Instance.PrintAltLine();
            Console.WriteLine();
        }

        /// <inheritdoc />
        /// <remarks>
        /// <para>
        /// This method:
        /// <list type="number">
        /// <item><description>Checks if the user is logged in, redirecting to login if not</description></item>
        /// <item><description>Retrieves all notes for the current user</description></item>
        /// <item><description>Displays each note in a formatted view</description></item>
        /// <item><description>Waits for user input before returning to the menu</description></item>
        /// </list>
        /// </para>
        /// </remarks>
        public async Task<PageState> Render() {
            if (!AppState.Instance.IsLoggedIn) {
                return PageState.Login;
            }
            IO.Instance.PrintHeader(Title);

            var notes = await _controller.GetNotesForUser(AppState.Instance.CurrentUser!);
            foreach (var note in notes) {
                PrintNote(note);
            }
            IO.Instance.WaitForAny();

            return PageState.Menu;
        }
    }
}
