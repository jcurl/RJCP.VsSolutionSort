namespace RJCP.VsSolutionSort.Parser
{
    using System.Text.RegularExpressions;

    /// <summary>
    /// Parse the line looking for <c>EndGlobal</c>.
    /// </summary>
    internal class EndGlobalParse : IParseLine<Line>
    {
        private readonly Regex Regex =
            new(@"^\s*EndGlobal\s*$",
                RegexOptions.CultureInvariant | RegexOptions.Compiled);

        /// <inheritdoc/>
        public Line Parse(string line)
        {
            Match m = Regex.Match(line);
            if (!m.Success) return null;

            return new Line(line);
        }

        object IParseLine.Parse(string line)
        {
            return Parse(line);
        }
    }
}
