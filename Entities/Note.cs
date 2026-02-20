using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Entities {
    /// <summary>
    /// Represents a note in the Notebook application.
    /// </summary>
    /// <remarks>
    /// Each note belongs to a single <see cref="User"/> and contains a title, text content,
    /// and timestamp. The <see cref="Updated"/> property is automatically set to the current
    /// time when a note is created.
    /// </remarks>
    public class Note : EntityBase {
        /// <summary>
        /// Gets or sets the title of the note.
        /// </summary>
        /// <value>
        /// A string representing the note's title. Defaults to an empty string.
        /// </value>
        public string Title { get; set; } = string.Empty;

        /// <summary>
        /// Gets or sets the text content of the note.
        /// </summary>
        /// <value>
        /// A string containing the main content of the note. Defaults to an empty string.
        /// </value>
        public string Text { get; set; } = string.Empty;

        /// <summary>
        /// Gets or sets the timestamp when the note was last updated.
        /// </summary>
        /// <value>
        /// A <see cref="DateTime"/> value representing the last update time.
        /// Automatically initialized to the current time in the constructor.
        /// </value>
        public DateTime Updated { get; set; }

        /// <summary>
        /// Gets or sets the foreign key identifier for the user who owns this note.
        /// </summary>
        /// <value>
        /// An integer that corresponds to the <see cref="EntityBase.Id"/> of the owning <see cref="User"/>.
        /// </value>
        public int UserId { get; set; }

        /// <summary>
        /// Gets or sets the navigation property to the user who owns this note.
        /// </summary>
        /// <value>
        /// The <see cref="User"/> entity that owns this note.
        /// </value>
        /// <remarks>
        /// This navigation property represents the "one" side of the one-to-many
        /// relationship between users and notes. The null-forgiving operator (!) is used
        /// because EF Core will ensure this property is populated when loading notes.
        /// </remarks>
        public User User { get; set; } = null!;

        /// <summary>
        /// Initializes a new instance of the <see cref="Note"/> class.
        /// </summary>
        /// <remarks>
        /// The constructor automatically sets the <see cref="Updated"/> property to the current date and time.
        /// </remarks>
        public Note() {
            Updated = DateTime.Now;
        }
    }
}
