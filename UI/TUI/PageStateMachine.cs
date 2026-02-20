

using UI.TUI.Page;

namespace UI.TUI {
    /// <summary>
    /// Defines the available page states in the application's navigation flow.
    /// </summary>
    /// <remarks>
    /// Each state represents a specific page or screen that can be navigated to.
    /// The <see cref="End"/> state indicates the application should terminate.
    /// </remarks>
    public enum PageState {
        /// <summary>
        /// The login page state.
        /// </summary>
        Login,

        /// <summary>
        /// The main menu page state.
        /// </summary>
        Menu,

        /// <summary>
        /// The show notes page state.
        /// </summary>
        ShowNotes,

        /// <summary>
        /// The add note page state.
        /// </summary>
        AddNote,

        /// <summary>
        /// The end state, indicating the application should terminate.
        /// </summary>
        End
    }

    /// <summary>
    /// Manages the application's page navigation flow using a state machine pattern.
    /// </summary>
    /// <remarks>
    /// <para>
    /// This class coordinates page transitions by maintaining the current state and
    /// invoking the appropriate page's render method. Pages return the next state to
    /// navigate to, creating a simple but effective navigation system.
    /// </para>
    /// <para>
    /// The state machine runs until it reaches the <see cref="PageState.End"/> state
    /// or encounters an invalid state.
    /// </para>
    /// </remarks>
    public class PageStateMachine {
        /// <summary>
        /// The current page state.
        /// </summary>
        private PageState _currState = PageState.Login;

        /// <summary>
        /// Gets or sets the dictionary mapping page states to page factory functions.
        /// </summary>
        /// <value>
        /// A dictionary where keys are <see cref="PageState"/> values and values are
        /// factory functions that create <see cref="IPage"/> instances.
        /// </value>
        /// <remarks>
        /// This property must be initialized before calling <see cref="Run"/>.
        /// Factory functions are used instead of instances to ensure fresh page objects
        /// are created on each navigation.
        /// </remarks>
        public required Dictionary<PageState, Func<IPage>> Pages;

        /// <summary>
        /// Asynchronously runs the state machine, navigating through pages until termination.
        /// </summary>
        /// <returns>A task that represents the asynchronous operation.</returns>
        /// <remarks>
        /// <para>
        /// The state machine loop:
        /// <list type="number">
        /// <item><description>Creates the page for the current state</description></item>
        /// <item><description>Displays the page header</description></item>
        /// <item><description>Renders the page and captures the next state</description></item>
        /// <item><description>Validates the next state exists or terminates</description></item>
        /// </list>
        /// </para>
        /// <para>
        /// The loop continues until <see cref="PageState.End"/> is reached or an
        /// invalid state is encountered.
        /// </para>
        /// </remarks>
        public async Task Run() {
            while (_currState != PageState.End) {
                var page = Pages[_currState].Invoke();

                IO.Instance.PrintHeader(page.Title);

                _currState = await page.Render();

                if (!Pages.ContainsKey(_currState)) {
                    _currState = PageState.End;
                }
            }
        }
    }
}
