using Data;
using Logic;
using UI.TUI;
using UI.TUI.Page;

namespace UI {
/// <summary>
/// The entry point class for the Notebook text-based user interface application.
/// </summary>
/// <remarks>
/// This class bootstraps the application by manually instantiating dependencies and
/// configuring the page state machine for navigation.
/// Note: This uses manual dependency instantiation rather than a DI container for simplicity.
/// </remarks>
internal class Program {
    /// <summary>
    /// The main entry point for the application.
    /// </summary>
    /// <param name="args">Command-line arguments (not currently used).</param>
    /// <returns>A task representing the asynchronous operation.</returns>
    /// <remarks>
    /// <para>
    /// This method:
    /// <list type="number">
    /// <item><description>Creates the database context and Unit of Work</description></item>
    /// <item><description>Configures the page state machine with all available pages</description></item>
    /// <item><description>Starts the application loop via <see cref="PageStateMachine.Run"/></description></item>
    /// </list>
    /// </para>
    /// <para>
    /// All pages share the same <see cref="IUnitOfWork"/> instance, ensuring consistent
    /// database context throughout the application session.
    /// </para>
    /// </remarks>
    static async Task Main(string[] args) {
            var uow = new UnitOfWork(new NotebookDbContext());

            var sm = new PageStateMachine {
                Pages = new() {
                    [PageState.Login] = () => new LoginPage(new LoginController(uow)),
                    [PageState.Menu]  = () => new MenuPage(new MenuController(uow)),
                    [PageState.ShowNotes] = () => new ShowNotesPage(new ShowNotesController(uow)),
                    [PageState.AddNote] = () => new AddNotePage(new AddNoteController(uow))
                }
            };
            await sm.Run();
        }
    }
}
