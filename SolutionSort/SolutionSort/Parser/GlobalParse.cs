namespace RJCP.VsSolutionSort.Parser
{
    using System.Text.RegularExpressions;

    /// <summary>
    /// Parses for <c>Global</c>.
    /// </summary>
    internal class GlobalParse : IParseLine<Global>
    {
        private readonly Regex Regex =
            new(@"^\s*Global\s*$",
                RegexOptions.CultureInvariant | RegexOptions.Compiled);

        /// <inheritdoc/>
        public Global Parse(string line)
        {
            Match m = Regex.Match(line);
            if (!m.Success) return null;

            return new Global(line);
        }

        object IParseLine.Parse(string line)
        {
            return Parse(line);
        }
    }
}
