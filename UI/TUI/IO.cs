namespace UI.TUI {
/// <summary>
/// Provides a singleton for console input/output operations with enhanced formatting capabilities.
/// </summary>
/// <remarks>
/// <para>
/// This class encapsulates console I/O functionality with features including:
/// <list type="bullet">
/// <item><description>Masked password input</description></item>
/// <item><description>Formatted headers and lines</description></item>
/// <item><description>Inverted color schemes for highlighted text</description></item>
/// <item><description>Interactive menu display</description></item>
/// </list>
/// </para>
/// <para>
/// Uses the Singleton pattern to provide consistent console state management throughout the application.
/// </para>
/// </remarks>
public class IO {
    /// <summary>
    /// The default prompt string displayed when requesting user input.
    /// </summary>
    public const string DefaultPrompt = ": ";

    /// <summary>
    /// The character used to mask password input.
    /// </summary>
    public const char PasswordCharacter = '*';

    /// <summary>
    /// The character used for primary decorative lines.
    /// </summary>
    public const char LineCharacter = '=';

    /// <summary>
    /// The character used for secondary decorative lines.
    /// </summary>
    public const char AltLineCharacter = '-';

    /// <summary>
    /// The singleton instance backing field.
    /// </summary>
    private static IO? _instance = null;

    /// <summary>
    /// Gets the singleton instance of <see cref="IO"/>.
    /// </summary>
    /// <value>
    /// The single <see cref="IO"/> instance for the application.
    /// </value>
    public static IO Instance => _instance ??= new IO();

    /// <summary>
    /// Indicates whether the console colors are currently inverted.
    /// </summary>
    private bool _inverted = false;

    /// <summary>
    /// Indicates whether the cursor should be visible.
    /// </summary>
    private bool _cursorVisible = false;

    /// <summary>
    /// Prevents external instantiation, enforcing the singleton pattern.
    /// </summary>
    /// <remarks>
    /// Initializes the console by clearing it on first use.
    /// </remarks>
    private IO() {
        Clear();
    }
        /// <summary>
        /// Reads input from the console with optional echoing and character masking.
        /// </summary>
        /// <param name="echo">Indicates whether to echo the input to the console.</param>
        /// <param name="echoChar">Optional character to display instead of the actual input (for masking).</param>
        /// <returns>The string entered by the user.</returns>
        /// <remarks>
        /// <para>
        /// This method provides low-level input handling with support for:
        /// <list type="bullet">
        /// <item><description>Backspace to delete characters</description></item>
        /// <item><description>Enter or Escape to complete input</description></item>
        /// <item><description>Character masking for password fields</description></item>
        /// <item><description>Filtering of control and alt key combinations</description></item>
        /// </list>
        /// </para>
        /// <para>
        /// Input is displayed with inverted colors to indicate the active input field.
        /// </para>
        /// </remarks>
        private string Read(bool echo = true, char? echoChar = null) {
            string result = "";
            bool keepReading = true;
            Invert(true);
            while (keepReading) {
                var keyInfo = Console.ReadKey(intercept: true);

                if ((keyInfo.Modifiers & ConsoleModifiers.Alt) == ConsoleModifiers.Alt)
                    continue;
                if ((keyInfo.Modifiers & ConsoleModifiers.Control) == ConsoleModifiers.Control)
                    continue;
                // Ignore if KeyChar value is \u0000.
                if (keyInfo.KeyChar == '\u0000') continue;

                switch (keyInfo.Key) {
                    case ConsoleKey.Tab:
                        break;
                    case ConsoleKey.Backspace:
                        // Are there any characters to erase?
                        if (result.Length >= 1 && echo) {
                            // Determine where we are in the console buffer.
                            int cursorCol = Console.CursorLeft - 1;
                            int oldLength = result.Length;
                            int extraRows = oldLength / 80;

                            result = result.Substring(0, oldLength - 1);
                            Console.CursorLeft = 0;
                            Console.CursorTop = Console.CursorTop - extraRows;
                            Console.Write(result + new string(' ', oldLength - result.Length));
                            Console.CursorLeft = cursorCol;
                        }
                        break;
                    case ConsoleKey.Enter:
                    case ConsoleKey.Escape:
                        keepReading = false;
                        break;
                    default:
                        result += keyInfo.KeyChar;
                        if (echo) {
                            Console.Write(echoChar ?? keyInfo.KeyChar);
                        }
                        break;
                }
            }
            Invert(false);

            Console.WriteLine();
            return result;
        }

        /// <summary>
        /// Updates the console appearance based on the current state flags.
        /// </summary>
        /// <remarks>
        /// Applies color inversion and cursor visibility settings to the console.
        /// </remarks>
        private void Update() {
            if (_inverted) {
                Console.BackgroundColor = ConsoleColor.Gray;
                Console.ForegroundColor = ConsoleColor.Black;
            } else {
                Console.BackgroundColor = ConsoleColor.Black;
                Console.ForegroundColor = ConsoleColor.White;
            }
            Console.CursorVisible = _cursorVisible;
        }

        /// <summary>
        /// Resets the console to default colors and makes the cursor visible.
        /// </summary>
        public void Reset() {
            _inverted = false;
            _cursorVisible = true;
            Update();
        }

        /// <summary>
        /// Toggles or sets the color inversion state of the console.
        /// </summary>
        /// <param name="inverted">
        /// Optional boolean to explicitly set the inverted state.
        /// If <c>null</c>, the current state is maintained.
        /// </param>
        /// <remarks>
        /// Inverted colors use gray background with black text instead of
        /// the default black background with white text.
        /// </remarks>
        public void Invert(bool? inverted = null) {
            _inverted = inverted ?? _inverted;
            Update();
        }

        /// <summary>
        /// Toggles or sets the cursor visibility.
        /// </summary>
        /// <param name="visible">
        /// Optional boolean to explicitly set the cursor visibility.
        /// If <c>null</c>, the current state is maintained.
        /// </param>
        public void CursorVisible(bool? visible = null) {
            _cursorVisible = visible ?? _cursorVisible;
            Update();
        }

        /// <summary>
        /// Clears the console and resets to default colors.
        /// </summary>
        public void Clear() {
            Reset();
            Console.Clear();
        }

        /// <summary>
        /// Reads a string from the console after displaying a prompt, with optional input echoing and masking.
        /// </summary>
        /// <param name="prompt">The prompt message displayed to the user.</param>
        /// <param name="echo">Indicates whether the input should be echoed to the console.</param>
        /// <param name="echoChar">The character used to mask the input if echoing is enabled.</param>
        /// <returns>The string entered by the user.</returns>
        public string ReadString(string prompt = DefaultPrompt, bool echo = true, char? echoChar = null) {
            Console.Write(prompt);
            return Read(true, echoChar);
        }

        /// <summary>
        /// Reads a password from the console, masking the input.
        /// </summary>
        /// <param name="prompt">The prompt message displayed to the user.</param>
        /// <returns>The entered password as a string.</returns>
        public string ReadPassword(string prompt = DefaultPrompt) {
            return ReadString(prompt, true, PasswordCharacter);
        }

        /// <summary>
        /// Reads a string from the user, attempts to parse it as an integer, and returns the result or -1 if parsing
        /// fails.
        /// </summary>
        /// <param name="prompt">The prompt message displayed to the user.</param>
        /// <param name="echo">Indicates whether the input should be echoed to the console.</param>
        /// <returns>The parsed integer value, or -1 if parsing fails.</returns>
        public int ReadNumber(string prompt = DefaultPrompt, bool echo = true) {
            var input = ReadString(prompt, echo);

            if (input != null && int.TryParse(input, out int result)) {
                return result;
            }

            return -1;
        }

        /// <summary>
        /// Writes a horizontal line to the console using the specified character and width.
        /// </summary>
        /// <param name="width">The number of characters in the line.</param>
        /// <param name="lineChar">The character to use for the line.</param>
        public void PrintLine(int width = 80, char lineChar = LineCharacter) {
            string line = "";
            for (int i = 0; i < width; i++) {
                line += lineChar;
            }
            Console.WriteLine(line);
        }

        /// <summary>
        /// Writes a horizontal line to the console using the specified character and width.
        /// </summary>
        /// <param name="width">The number of characters in the line.</param>
        /// <param name="lineChar">The character to use for the line.</param>
        public void PrintAltLine(int width = 80, char lineChar = AltLineCharacter) {
            PrintLine(width, lineChar);
        }

        /// <summary>
        /// Clears the console and prints a formatted header with the specified title and optional subtitle.
        /// </summary>
        /// <param name="title">The main title to display in the header.</param>
        /// <param name="subtitle">An optional subtitle to display alongside the title.</param>
        public void PrintHeader(string title, string? subtitle = null) {
            var printTitle = title;
            Console.Clear();
            PrintLine();
            if (!string.IsNullOrWhiteSpace(subtitle)) {
                printTitle += $"- {subtitle}";
            }
            PrintFullWidth(printTitle, LineCharacter);
            PrintLine();
            Console.WriteLine();
        }

        /// <summary>
        /// Prints a sub-header with the specified title and an underline.
        /// </summary>
        /// <param name="title">The sub-header text to display.</param>
        public void PrintSubHeader(string title) {
            Console.WriteLine("  {0}", title);
            PrintAltLine();
        }

        /// <summary>
        /// Prints text formatted to fill the full width of the console (80 characters).
        /// </summary>
        /// <param name="text">The text to display.</param>
        /// <param name="prefix">Optional prefix character (defaults to space).</param>
        /// <param name="suffix">Optional suffix character (defaults to prefix or space).</param>
        /// <remarks>
        /// The text is left-aligned with padding to ensure consistent 80-character lines.
        /// </remarks>
        public void PrintFullWidth(string text, char? prefix=null, char? suffix=null) {
            Console.WriteLine("{0,-2}{1,-76}{2,2}", prefix ?? ' ', text, suffix ?? prefix ?? ' ');
        }

        /// <summary>
        /// Prints text formatted to fill the full width with inverted colors.
        /// </summary>
        /// <param name="text">The text to display.</param>
        /// <param name="prefix">Optional prefix character (defaults to space).</param>
        /// <param name="suffix">Optional suffix character (defaults to prefix or space).</param>
        /// <remarks>
        /// This method is useful for highlighting selected menu items or emphasized content.
        /// Colors are automatically restored after printing.
        /// </remarks>
        public void PrintFullWidthInverted(string text, char? prefix = null, char? suffix = null) {
            Invert(true);
            PrintFullWidth(text, prefix, suffix);
            Invert(false);
        }

        /// <summary>
        /// Displays a prompt and waits for the user to press any key.
        /// </summary>
        /// <param name="prompt">The message to display before waiting for a key press.</param>
        public ConsoleKeyInfo WaitForAny(string prompt = "Press any key to continue") {
            Console.WriteLine("\t[{0}]", prompt);
            return Console.ReadKey();
        }

        /// <summary>
        /// Displays an interactive menu and returns the selected value.
        /// </summary>
        /// <typeparam name="T">The type of value returned by menu items.</typeparam>
        /// <param name="menu">The menu to display.</param>
        /// <param name="title">Optional title to display above the menu.</param>
        /// <returns>The value returned by the selected menu item's function.</returns>
        /// <remarks>
        /// <para>
        /// Users can navigate the menu using:
        /// <list type="bullet">
        /// <item><description>Up Arrow: Move to previous item</description></item>
        /// <item><description>Down Arrow: Move to next item</description></item>
        /// <item><description>Space or Enter: Select current item</description></item>
        /// </list>
        /// </para>
        /// <para>
        /// The currently selected item is highlighted with inverted colors and arrow indicators.
        /// </para>
        /// </remarks>
        public T ShowMenu<T>(Menu<T> menu, string? title = null) {
            var print = true;

            while (print) {
                Console.Clear();
                if (!string.IsNullOrWhiteSpace(title)) {
                    PrintHeader(title);
                }

                for (int i = 0; i < menu.NumItems; i++) {
                    if (menu.Index == i) {
                        PrintFullWidthInverted(menu.KeyAt(i), '>', '<');
                    } else {
                        PrintFullWidth(menu.KeyAt(i));
                    }
                }
                var keyInfo = Console.ReadKey();

                switch (keyInfo.Key) {
                    case ConsoleKey.Spacebar:
                    case ConsoleKey.Enter:
                        print = false;
                        break;
                    case ConsoleKey.UpArrow:
                        menu.PrevIndex();
                        break;
                    case ConsoleKey.DownArrow:
                        menu.NextIndex();
                        break;
                }
            }
            Reset();
            return menu.CurrValue.Invoke();
        }
    }
}
