using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace UI.TUI.Page {
    /// <summary>
    /// Defines the contract for a page in the text-based user interface.
    /// </summary>
    /// <remarks>
    /// Pages are managed by the <see cref="PageStateMachine"/> and represent different
    /// screens or views in the application (e.g., login, menu, add note).
    /// </remarks>
    public interface IPage {
        /// <summary>
        /// Gets the title of the page to be displayed in the header.
        /// </summary>
        /// <value>
        /// A string representing the page title.
        /// </value>
        string Title { get; }

        /// <summary>
        /// Asynchronously renders the page content and handles user interaction.
        /// </summary>
        /// <returns>
        /// A task that represents the asynchronous operation.
        /// The task result contains the <see cref="PageState"/> to navigate to after
        /// the page completes its rendering and interaction.
        /// </returns>
        /// <remarks>
        /// This method is called by the <see cref="PageStateMachine"/> to display the page
        /// and determine the next page to navigate to based on user actions.
        /// </remarks>
        Task<PageState> Render();
    }
}
