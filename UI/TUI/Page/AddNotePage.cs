using Entities;
using Logic;

namespace UI.TUI.Page {
    /// <summary>
    /// Implements the page for adding new notes to the system.
    /// </summary>
    /// <remarks>
    /// This page prompts the user for note title and text, creates a new <see cref="Note"/>
    /// entity, and saves it using the <see cref="IAddNoteController"/>.
    /// Requires the user to be logged in before displaying.
    /// </remarks>
    public class AddNotePage : IPage {
        /// <summary>
        /// The controller responsible for adding notes.
        /// </summary>
        private readonly IAddNoteController _controller;

        /// <summary>
        /// Initializes a new instance of the <see cref="AddNotePage"/> class.
        /// </summary>
        /// <param name="controller">The controller for adding notes.</param>
        public AddNotePage(IAddNoteController controller) {
            _controller = controller;
        }

        /// <inheritdoc />
        public string Title { get; private set; } = "Add note";

        /// <inheritdoc />
        /// <remarks>
        /// <para>
        /// This method:
        /// <list type="number">
        /// <item><description>Checks if the user is logged in, redirecting to login if not</description></item>
        /// <item><description>Prompts for note title and text</description></item>
        /// <item><description>Creates a new note associated with the current user</description></item>
        /// <item><description>Saves the note via the controller</description></item>
        /// <item><description>Returns to the main menu</description></item>
        /// </list>
        /// </para>
        /// <para>
        /// The note's <see cref="Note.Updated"/> timestamp is automatically set by the
        /// <see cref="Note"/> constructor.
        /// </para>
        /// </remarks>
        public async Task<PageState> Render() {
            if (!AppState.Instance.IsLoggedIn) {
                return PageState.Login;
            }

            var title = IO.Instance.ReadString("Title: ");
            var text = IO.Instance.ReadString("Text : ");

            var note = new Note { Title = title, Text = text, UserId = AppState.Instance.CurrentUser!.Id };

            await _controller.AddNote(note);

            return PageState.Menu;
        }
    }
}
