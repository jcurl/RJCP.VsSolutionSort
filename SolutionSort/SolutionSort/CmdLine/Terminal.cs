namespace RJCP.VsSolutionSort.CmdLine
{
    using System;

    /// <summary>
    /// A convenience class for writing formatted information to the console.
    /// </summary>
    internal static class Terminal
    {
        /// <summary>
        /// Writes an empty line to the console.
        /// </summary>
        public static void WriteLine()
        {
            Console.WriteLine();
        }

        /// <summary>
        /// Writes a wrapped line to the console.
        /// </summary>
        /// <param name="message">The message to write.</param>
        public static void WriteLine(string message)
        {
            string[] lines = Format.Wrap(Console.WindowWidth - 1, message);
            foreach (string line in lines) {
                Console.WriteLine(line);
            }
        }

        /// <summary>
        /// Writes a wrapped line to the console.
        /// </summary>
        /// <param name="format">The format string.</param>
        /// <param name="args">The arguments to be formatted.</param>
        public static void WriteLine(string format, params object[] args)
        {
            string message = string.Format(format, args);
            WriteLine(message);
        }

        /// <summary>
        /// Writes a wrapped line to the console.
        /// </summary>
        /// <param name="indent">The indent.</param>
        /// <param name="hangingIndent">The hanging indent.</param>
        /// <param name="message">The message to write.</param>
        public static void WriteLine(int indent, int hangingIndent, string message)
        {
            string[] output = Format.Wrap(Console.WindowWidth - 1, indent, hangingIndent, message);

            if (output is not null) {
                foreach (string line in output) {
                    Console.WriteLine(line);
                }
            }
        }

        /// <summary>
        /// Writes a wrapped line to the console.
        /// </summary>
        /// <param name="indent">The indent.</param>
        /// <param name="hangingIndent">The hanging indent.</param>
        /// <param name="format">The format string.</param>
        /// <param name="args">The arguments to be formatted.</param>
        public static void WriteLine(int indent, int hangingIndent, string format, params object[] args)
        {
            string message = string.Format(format, args);
            WriteLine(indent, hangingIndent, message);
        }

        /// <summary>
        /// Writes the line directly to the console without wrapping.
        /// </summary>
        /// <param name="message">The message.</param>
        public static void WriteDirect(string message)
        {
            Console.WriteLine(message);
        }

        /// <summary>
        /// Writes the line directly to the console without wrapping.
        /// </summary>
        /// <param name="format">The format string.</param>
        /// <param name="args">The arguments to be formatted.</param>
        public static void WriteDirect(string format, params object[] args)
        {
            string message = string.Format(format, args);
            WriteDirect(message);
        }
    }
}
