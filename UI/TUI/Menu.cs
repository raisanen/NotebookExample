namespace UI.TUI {
    /// <summary>
    /// Represents a navigable menu with selectable items.
    /// </summary>
    /// <typeparam name="T">The type of value returned when a menu item is selected.</typeparam>
    /// <remarks>
    /// This class provides keyboard navigation (up/down arrows) through menu items,
    /// where each item is associated with a function that returns a value of type <typeparamref name="T"/>.
    /// Commonly used to navigate between <see cref="PageState"/> values.
    /// </remarks>
    public class Menu<T> {
        /// <summary>
        /// The current selected index in the menu.
        /// </summary>
        private int _index = 0;

        /// <summary>
        /// Gets or sets the dictionary of menu items.
        /// </summary>
        /// <value>
        /// A dictionary where keys are display strings and values are functions that
        /// return <typeparamref name="T"/> when the item is selected.
        /// </value>
        /// <remarks>
        /// This property must be initialized before using the menu.
        /// </remarks>
        public required Dictionary<string, Func<T>> Items { get; set; }

        /// <summary>
        /// Gets the total number of items in the menu.
        /// </summary>
        /// <value>
        /// The count of items in the <see cref="Items"/> dictionary.
        /// </value>
        public int NumItems => Items.Count;

        /// <summary>
        /// Gets the current selected index.
        /// </summary>
        /// <value>
        /// An integer representing the zero-based index of the currently selected item.
        /// </value>
        public int Index {
            get { return _index; }
        }

        /// <summary>
        /// Moves the selection to the next item, wrapping to the first item if at the end.
        /// </summary>
        public void NextIndex() {
            _index = (_index + 1) % NumItems;
        }

        /// <summary>
        /// Moves the selection to the previous item, wrapping to the last item if at the beginning.
        /// </summary>
        public void PrevIndex() {
            if (_index == 0) {
                _index = NumItems - 1;
            } else {
                _index--;
            }
        }

        /// <summary>
        /// Gets the display text for the item at the specified index.
        /// </summary>
        /// <param name="index">The zero-based index of the item.</param>
        /// <returns>The display string for the menu item.</returns>
        public string KeyAt(int index) => Items.Keys.ElementAt(index);

        /// <summary>
        /// Gets the function associated with the item at the specified index.
        /// </summary>
        /// <param name="index">The zero-based index of the item.</param>
        /// <returns>The function that returns <typeparamref name="T"/> for the menu item.</returns>
        public Func<T> ValueAt(int index) => Items.Values.ElementAt(index);

        /// <summary>
        /// Gets the display text for the currently selected item.
        /// </summary>
        /// <value>
        /// The display string of the selected menu item.
        /// </value>
        public string CurrKey => KeyAt(Index);

        /// <summary>
        /// Gets the function associated with the currently selected item.
        /// </summary>
        /// <value>
        /// The function that returns <typeparamref name="T"/> for the selected menu item.
        /// </value>
        public Func<T> CurrValue => ValueAt(Index);

    }
}
