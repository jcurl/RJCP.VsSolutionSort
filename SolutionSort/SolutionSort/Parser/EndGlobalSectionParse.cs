namespace RJCP.VsSolutionSort.Parser
{
    using System.Text.RegularExpressions;

    /// <summary>
    /// Parses for <c>EndGlobalSection</c>.
    /// </summary>
    internal class EndGlobalSectionParse : IParseLine<Line>
    {
        private readonly Regex Regex =
            new(@"^\s*EndGlobalSection\s*$",
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
