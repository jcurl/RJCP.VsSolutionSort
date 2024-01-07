namespace RJCP.VsSolutionSort.Parser
{
    /// <summary>
    /// Represents a parsed line.
    /// </summary>
    internal interface IParseLine
    {
        /// <summary>
        /// Parses the specified line.
        /// </summary>
        /// <param name="line">The line that should be parsed.</param>
        /// <returns>Returns the parsed object. Returns <see langword="null"/> if the parsing fails.</returns>
        object Parse(string line);
    }

    /// <summary>
    /// Represents a parsed line.
    /// </summary>
    internal interface IParseLine<out T> : IParseLine
    {
        /// <summary>
        /// Parses the specified line.
        /// </summary>
        /// <param name="line">The line that should be parsed.</param>
        /// <returns>Returns the parsed object. Returns <see langword="null"/> if the parsing fails.</returns>
        new T Parse(string line);
    }
}
