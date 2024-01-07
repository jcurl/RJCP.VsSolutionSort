namespace RJCP.VsSolutionSort.Parser
{
    using System.Text.RegularExpressions;

    /// <summary>
    /// Parses for <c>EndProject</c>.
    /// </summary>
    internal class EndProjectParse : IParseLine<Line>
    {
        private readonly Regex Regex =
            new(@"^\s*EndProject\s*$",
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
