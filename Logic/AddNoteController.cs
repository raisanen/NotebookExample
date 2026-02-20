using Data;
using Entities;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Logic {
    /// <summary>
    /// Implements business logic for adding new notes to the system.
    /// </summary>
    /// <remarks>
    /// <para>
    /// This controller uses the Unit of Work pattern to coordinate database operations,
    /// ensuring that the note is added and persisted in a single transaction.
    /// </para>
    /// <para>
    /// This class demonstrates the separation between business logic (Logic layer)
    /// and data access (Data layer) through the <see cref="IUnitOfWork"/> abstraction.
    /// </para>
    /// </remarks>
    public class AddNoteController : IAddNoteController {
        /// <summary>
        /// The Unit of Work instance used to coordinate repository operations.
        /// </summary>
        private readonly IUnitOfWork _uow;

        /// <summary>
        /// Initializes a new instance of the <see cref="AddNoteController"/> class.
        /// </summary>
        /// <param name="uow">The Unit of Work instance for managing data operations.</param>
        /// <remarks>
        /// This constructor is typically called by the dependency injection container.
        /// </remarks>
        public AddNoteController(IUnitOfWork uow) {
            _uow = uow;
        }

        /// <inheritdoc />
        /// <remarks>
        /// This implementation:
        /// <list type="number">
        /// <item><description>Adds the note to the Notes repository</description></item>
        /// <item><description>Commits the changes to the database via <see cref="IUnitOfWork.SaveChangesAsync"/></description></item>
        /// </list>
        /// Both operations are part of a single transaction, ensuring atomicity.
        /// </remarks>
        public async Task AddNote(Note note) {
            await _uow.Notes.AddAsync(note);
            await _uow.SaveChangesAsync();
        }
    }
}
