using Data;
using Entities;

namespace Logic {
    /// <summary>
    /// Implements business logic for menu-related operations.
    /// </summary>
    /// <remarks>
    /// This controller uses the Unit of Work pattern to query user data and
    /// determine what menu options should be available.
    /// </remarks>
    public class MenuController : IMenuController {
        /// <summary>
        /// The Unit of Work instance used to coordinate repository operations.
        /// </summary>
        private readonly IUnitOfWork _uow;

        /// <summary>
        /// Initializes a new instance of the <see cref="MenuController"/> class.
        /// </summary>
        /// <param name="uow">The Unit of Work instance for managing data operations.</param>
        /// <remarks>
        /// This constructor is typically called by the dependency injection container.
        /// </remarks>
        public MenuController(IUnitOfWork uow) {
            _uow = uow;
        }

        /// <inheritdoc />
        /// <remarks>
        /// This implementation uses the <see cref="IRepository{T}.AnyAsync"/> method
        /// with a predicate to efficiently check for the existence of notes without
        /// loading the actual note data.
        /// </remarks>
        public async Task<bool> HasNotes(User user) => await _uow.Notes.AnyAsync(n => n.UserId == user.Id);
    }
}
