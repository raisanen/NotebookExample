using Data;
using Entities;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Logic {
    /// <summary>
    /// Implements business logic for retrieving notes from the system.
    /// </summary>
    /// <remarks>
    /// <para>
    /// This controller uses the Unit of Work pattern to access the Notes repository
    /// and filter notes by user, ensuring proper data isolation.
    /// </para>
    /// <para>
    /// This class demonstrates the separation between business logic (Logic layer)
    /// and data access (Data layer) through the <see cref="IUnitOfWork"/> abstraction.
    /// </para>
    /// </remarks>
    public class ShowNotesController : IShowNotesController {
        /// <summary>
        /// The Unit of Work instance used to coordinate repository operations.
        /// </summary>
        private readonly IUnitOfWork _uow;

        /// <summary>
        /// Initializes a new instance of the <see cref="ShowNotesController"/> class.
        /// </summary>
        /// <param name="uow">The Unit of Work instance for managing data operations.</param>
        /// <remarks>
        /// This constructor is typically called by the dependency injection container.
        /// </remarks>
        public ShowNotesController(IUnitOfWork uow) {
            _uow = uow;
        }

        /// <inheritdoc />
        /// <remarks>
        /// This implementation uses the <see cref="IRepository{T}.FindAsync"/> method
        /// with a predicate to filter notes by the user's ID, ensuring that users can
        /// only see their own notes.
        /// </remarks>
        public async Task<IEnumerable<Note>> GetNotesForUser(User user)
            => await _uow.Notes.FindAsync(n => n.UserId == user.Id);
    }
}
